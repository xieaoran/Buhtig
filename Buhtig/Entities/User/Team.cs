using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Entities.Git;
using Buhtig.Resources.Strings;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Buhtig.Entities.User
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Team : INotifyPropertyChanged, IDisposable
    {
        private Guid _id;

        [JsonProperty(PropertyName = "id")]
        public Guid Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _teamName;

        [JsonProperty(PropertyName = "team_name")]
        public string TeamName
        {
            get { return _teamName; }
            set
            {
                if (_teamName == value) return;
                _teamName = value;
                OnPropertyChanged();
            }
        }

        private string _projectName;

        [JsonProperty(PropertyName = "project_name")]
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (_projectName == value) return;
                _projectName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Student> _members;

        [JsonProperty(PropertyName = "members")]
        public ObservableCollection<Student> Members
        {
            get { return _members; }
            set
            {
                if (_members == value) return;
                _members = value;
                OnPropertyChanged();
            }
        }

        private GitRepo _repo;

        [JsonProperty(PropertyName = "repo")]
        public GitRepo Repo
        {
            get { return _repo; }
            set
            {
                if (_repo == value) return;
                _repo = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Collaboration> _collaborations;

        [JsonProperty(PropertyName = "collaborations")]
        public ObservableCollection<Collaboration> Collaborations
        {
            get { return _collaborations; }
            set
            {
                if (_collaborations == value) return;
                _collaborations = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return string.Format(StaticConfigs.TeamNameFormat, TeamName, ProjectName);
        }

        public Team(Uri remoteUri)
        {
            Id = Guid.NewGuid();
            Members = new ObservableCollection<Student>();
            Repo = new GitRepo(this, remoteUri);
            Collaborations = new ObservableCollection<Collaboration>();
            Analyze();
        }

        public Team(string localPath)
        {
            Id = Guid.NewGuid();
            Members = new ObservableCollection<Student>();
            Repo = new GitRepo(this, localPath);
            Collaborations = new ObservableCollection<Collaboration>();
            Analyze();
        }

        private void Analyze()
        {
            var uriSegments = Repo.RemoteUri.AbsolutePath.Split('/');
            var segmentsLength = uriSegments.Length;
            TeamName = uriSegments[segmentsLength - 2];
            ProjectName = uriSegments[segmentsLength - 1];
            foreach (var member in Members) member.CalcContribution();
            AnalyzeCollaborations();
        }

        private void AnalyzeCollaborations()
        {
            var changeMembers = new Dictionary<string, List<Student>>();
            var memberChanges = new Dictionary<List<Student>, List<GitChange>>();
            foreach (var member in Members)
                foreach (var commit in member.Commits)
                    foreach (var change in commit.ChangesList)
                    {
                        if (!changeMembers.ContainsKey(change.NewBlob.Path))
                        {
                            changeMembers[change.NewBlob.Path] = new List<Student>();
                        }
                        var members = changeMembers[change.NewBlob.Path];
                        if (!members.Contains(member)) members.Add(member);
                    }
            foreach (var changeMember in changeMembers)
            {
                if (changeMember.Value.Count < 2) continue;
                var target = memberChanges.Keys.FirstOrDefault(
                    k => k.Count == changeMember.Value.Count &&
                    k.TrueForAll(s => changeMember.Value.Contains(s)));
                if (target == null)
                {
                    memberChanges.Add(changeMember.Value, new List<GitChange>());
                    target = changeMember.Value;
                }
                foreach (var member in changeMember.Value)
                    foreach (var commit in member.Commits)
                    {
                        memberChanges[target].AddRange(
                            commit.ChangesList.Where(c => c.NewBlob.Path == changeMember.Key));
                    }
            }
            foreach (var changeMember in changeMembers.Values)
            {
                changeMember.Clear();
            }
            changeMembers.Clear();
            foreach (var change in memberChanges.Values)
            {
                Collaborations.Add(new Collaboration(change));
            }
        }

        public void InitializeMembers(IEnumerable<Signature> innerAuthors)
        {
            foreach (var innerAuthor in innerAuthors)
            {
                if (Members.All(m => m.GitEmail != innerAuthor.Email))
                {
                    Members.Add(new Student(this, innerAuthor.Name, innerAuthor.Email));
                }
            }
        }

        public void PostProcess()
        {
            foreach (var member in Members)
            {
                member.PostProcess(this);
            }
            Repo.PostProcess(this);
            foreach (var collaboration in Collaborations)
            {
                collaboration.PostProcess(this);
            }
        }

        public Team()
        {
            // Reserved for Serialization
        }

        ~Team()
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
            if (disposing) Repo.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
