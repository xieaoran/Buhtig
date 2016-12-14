using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Configs;
using Buhtig.Entities.Base;
using Buhtig.Entities.User;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Buhtig.Entities.Git
{
    public enum RepoLocation
    {
        Local,
        Remote
    }
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GitRepo : INotifyPropertyChanged, IPostProcessRequired, IDisposable
    {

        private RepoLocation _repoLocation;

        public RepoLocation RepoLocation
        {
            get { return _repoLocation; }
            set
            {
                if (_repoLocation == value) return;
                _repoLocation = value;
                OnPropertyChanged();
            }
        }

        private Uri _remoteUri;
        [JsonProperty(PropertyName = "remote_uri")]
        public Uri RemoteUri
        {
            get { return _remoteUri; }
            set
            {
                if (_remoteUri == value) return;
                _remoteUri = value;
                OnPropertyChanged();
            }
        }
        private string _localPath;

        [JsonProperty(PropertyName = "local_path")]
        public string LocalPath
        {
            get { return _localPath; }
            set
            {
                if (_localPath == value) return;
                _localPath = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GitCommit> _commits;

        [JsonProperty(PropertyName = "commits")]
        public ObservableCollection<GitCommit> Commits
        {
            get { return _commits; }
            set
            {
                if (_commits == value) return;
                _commits = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GitBranch> _branches;

        [JsonProperty(PropertyName = "branches")]
        public ObservableCollection<GitBranch> Branches
        {
            get { return _branches; }
            set
            {
                if (_branches == value) return;
                _branches = value;
                OnPropertyChanged();
            }
        }

        private IRepository _innerRepo;

        [JsonIgnore]
        public IRepository InnerRepo
        {
            get { return _innerRepo; }
            set
            {
                if (_innerRepo == value) return;
                _innerRepo = value;
                OnPropertyChanged();
            }
        }

        private Team _belongingTeam;

        [JsonIgnore]
        public Team BelongingTeam
        {
            get { return _belongingTeam; }
            set
            {
                if (_belongingTeam == value) return;
                _belongingTeam = value;
                OnPropertyChanged();
            }
        }

        private int _linesAdded;

        [JsonProperty(PropertyName = "lines_added")]
        public int LinesAdded
        {
            get { return _linesAdded; }
            set
            {
                if (_linesAdded == value) return;
                _linesAdded = value;
                OnPropertyChanged();
            }
        }

        private int _linesDeleted;

        [JsonProperty(PropertyName = "lines_deleted")]
        public int LinesDeleted
        {
            get { return _linesDeleted; }
            set
            {
                if (_linesDeleted == value) return;
                _linesDeleted = value;
                OnPropertyChanged();
            }
        }

        public GitRepo(Team team, Uri remoteUri)
        {
            RepoLocation = RepoLocation.Remote;
            BelongingTeam = team;
            RemoteUri = remoteUri;
            var workingDir = string.Format(RuntimeConfigs.LocalWorkSpaceConfig.RepoPathFormat, team.Id);
            LocalPath = Repository.Clone(RemoteUri.AbsoluteUri, workingDir);
            InnerRepo = new Repository(LocalPath);
            Analyze();
        }

        public GitRepo(Team team, string localPath)
        {
            RepoLocation = RepoLocation.Local;
            BelongingTeam = team;
            LocalPath = localPath;
            InnerRepo = new Repository(LocalPath);
            RemoteUri = new Uri(InnerRepo.Branches.First(br => br.IsRemote).Remote.Url);
            Analyze();
        }

        public GitRepo()
        {
            // Reserved for Serialization
        }

        private void Analyze()
        {
            BelongingTeam.InitializeMembers(InnerRepo.Commits.Select(c => c.Author).Distinct());
            Commits = new ObservableCollection<GitCommit>();
            Branches = new ObservableCollection<GitBranch>();

            foreach (var innerCommit in InnerRepo.Commits.OrderBy(c => c.Author.When))
            {
                Commits.Add(new GitCommit(innerCommit, BelongingTeam.Members));
            }
            foreach (var commit in Commits)
            {
                commit.AnalyzeChanges(this);
            }
            foreach (var innerBranch in InnerRepo.Branches.Where(br => !br.IsRemote))
            {
                Branches.Add(new GitBranch(this, innerBranch));
            }

            var initialCommit = Commits.First(c => c.Changes.Keys.Contains(RuntimeRoot.CommitRoot));
            FixMinTime(initialCommit, initialCommit.Time);

            var lastCommit = Commits.First(c => !c.Children.Any());
            FixMaxTime(lastCommit, lastCommit.Time);

            EstimateTime(initialCommit);

            var unsortedCommits = Commits;
            Commits = new ObservableCollection<GitCommit>(unsortedCommits.OrderBy(c => c.Time));
            unsortedCommits.Clear();

            LinesAdded = 0;
            LinesDeleted = 0;

            foreach (var commit in Commits)
                foreach (var changeList in commit.Changes.Values)
                    foreach (var change in changeList)
                    {
                        if (change.Framework) continue;
                        LinesAdded += change.Summary.LinesAdded;
                        LinesDeleted += change.Summary.LinesDeleted;
                    }
        }

        private void FixMinTime(GitCommit currentCommit, DateTime prevMinTime)
        {
            var minTime = currentCommit.TimeEstimated ? prevMinTime : currentCommit.TimeOriginal;
            var currentChildren = currentCommit.Children;
            if (!currentChildren.Any()) return;
            foreach (var child in currentChildren)
            {
                if (!child.TimeEstimated && child.TimeOriginal < minTime)
                {
                    child.TimeEstimated = true;
                    child.MinTime = minTime;
                }
                else if (child.MinTime == child.TimeOriginal || child.MinTime < minTime)
                {
                    child.MinTime = minTime;
                }
                FixMinTime(child, minTime);
            }
        }

        private void FixMaxTime(GitCommit currentCommit, DateTime prevMaxTime)
        {
            var maxTime = currentCommit.TimeEstimated ? prevMaxTime : currentCommit.TimeOriginal;
            var currentParents = currentCommit.Changes.Keys;
            if (currentParents.Contains(RuntimeRoot.CommitRoot)) return;
            foreach (var parent in currentParents)
            {
                if (!parent.TimeEstimated && parent.TimeOriginal > maxTime)
                {
                    parent.TimeEstimated = true;
                    parent.MaxTime = maxTime;
                }
                else if (parent.MaxTime == parent.TimeOriginal || parent.MaxTime > maxTime)
                {
                    parent.MaxTime = maxTime;
                }
                FixMaxTime(parent, maxTime);
            }
        }

        private void EstimateTime(GitCommit initialCommit)
        {
            var timeEstimatedCommits = Commits.Where(c => c.TimeEstimated).ToList();

            var layerInfo = new Dictionary<GitCommit, int>();
            SortEstimatedCommits(ref layerInfo, timeEstimatedCommits, initialCommit, 0);

            var sortedLayerInfo = layerInfo.OrderBy(l => l.Value);
            var sortedCommits = sortedLayerInfo.Select(s => s.Key);

            GitCommit parentCommit = null;
            foreach (var commit in sortedCommits)
            {
                if (parentCommit == null || parentCommit.Time < commit.MinTime)
                {
                    var fixMs = (commit.MaxTime - commit.MinTime).TotalMilliseconds * 0.5;
                    commit.Time = commit.MinTime.AddMilliseconds(fixMs);
                }
                else
                {
                    var fixMs = (commit.MaxTime - parentCommit.Time).TotalMilliseconds * 0.5;
                    commit.Time = parentCommit.Time.AddMilliseconds(fixMs);
                }
                parentCommit = commit;
            }
        }

        public void SortEstimatedCommits(ref Dictionary<GitCommit, int> layerInfo,
            List<GitCommit> unsortedCommits, GitCommit currentCommit, int currentLayer)
        {
            var currentChildren = currentCommit.Children;
            if (!currentChildren.Any()) return;
            foreach (var child in currentChildren)
            {
                if (unsortedCommits.Contains(child))
                {
                    if (!layerInfo.ContainsKey(child)) layerInfo.Add(child, currentLayer);
                    else if (layerInfo[child] < currentLayer) layerInfo[child] = currentLayer;
                }
                var nextLayer = currentLayer++;
                SortEstimatedCommits(ref layerInfo, unsortedCommits, child, nextLayer);
            }
        }

        public void PostProcess(Team team)
        {
            BelongingTeam = team;
            InnerRepo = new Repository(LocalPath);
            foreach (var commit in Commits)
            {
                commit.PostProcess(BelongingTeam);
            }
            foreach (var branch in Branches)
            {
                branch.PostProcess(BelongingTeam);
            }
        }

        ~GitRepo()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var commit in Commits)
                {
                    commit.Dispose();
                }
                foreach (var branch in Branches)
                {
                    branch.Dispose();
                }
                Commits.Clear();
                Branches.Clear();
            }
            InnerRepo.Dispose();
            if (RepoLocation != RepoLocation.Remote) return;
            var localPath = LocalPath;
            if (localPath.Contains(".git"))
            {
                var directoryInfo = Directory.GetParent(localPath).Parent;
                if (directoryInfo != null)
                    localPath = directoryInfo.FullName;
            }
            try
            {
                Directory.Delete(localPath, true);
            }
            catch
            {
                // ignored
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
