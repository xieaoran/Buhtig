using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Entities.Base;
using Buhtig.Entities.User;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Buhtig.Entities.Git
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GitBranch : INotifyPropertyChanged, IPostProcessRequired, IDisposable
    {
        private string _name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _commitShas;

        [JsonProperty(PropertyName = "commit_shas")]
        public ObservableCollection<string> CommitShas
        {
            get { return _commitShas; }
            set
            {
                if (_commitShas == value) return;
                _commitShas = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GitCommit> _commits;

        [JsonIgnore]
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

        public GitBranch(GitRepo repo, Branch innerBranch)
        {
            Name = innerBranch.FriendlyName;
            Commits = new ObservableCollection<GitCommit>();
            CommitShas = new ObservableCollection<string>();
            foreach (var innerCommit in innerBranch.Commits)
            {
                var commit = repo.Commits.First(c => c.Sha == innerCommit.Sha);
                Commits.Add(commit);
                CommitShas.Add(commit.Sha);
            }
        }

        public void PostProcess(Team team)
        {
            Commits = new ObservableCollection<GitCommit>();
            foreach (var commitSha in CommitShas)
            {
                var commit = team.Repo.Commits.First(c => c.Sha == commitSha);
                Commits.Add(commit);
            }
        }

        public GitBranch()
        {
            // Reserved for Serialization
        }

        ~GitBranch()
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
            if (!disposing) return;
            CommitShas.Clear();
            Commits.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
