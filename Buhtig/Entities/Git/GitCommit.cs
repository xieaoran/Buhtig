using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Entities.Base;
using Buhtig.Entities.User;
using Buhtig.Resources.Strings;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Buhtig.Entities.Git
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GitCommit : INotifyPropertyChanged, IPostProcessRequired, IDisposable
    {
        private string _sha;

        [JsonProperty(PropertyName = "sha")]
        public string Sha
        {
            get { return _sha; }
            set
            {
                if (_sha == value) return;
                _sha = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(value)) return;
                ShaShort = value.Substring(0, 7);
            }
        }

        private string _shaShort;

        [JsonIgnore]
        public string ShaShort
        {
            get { return _shaShort; }
            set
            {
                if (_shaShort == value) return;
                _shaShort = value;
                OnPropertyChanged();
            }
        }

        private DateTime _time;

        [JsonProperty(PropertyName = "time")]
        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (_time == value) return;
                _time = value;
                OnPropertyChanged();
            }
        }

        private DateTime _timeOriginal;

        [JsonProperty(PropertyName = "time_original")]
        public DateTime TimeOriginal
        {
            get { return _timeOriginal; }
            set
            {
                if (_timeOriginal == value) return;
                _timeOriginal = value;
                OnPropertyChanged();
            }
        }

        private Student _author;

        [JsonIgnore]
        public Student Author
        {
            get { return _author; }
            set
            {
                if (Equals(_author, value)) return;
                _author = value;
                OnPropertyChanged();
                if (value == null) return;
                AuthorId = value.Id;
            }
        }

        private Guid _authorId;

        [JsonProperty(PropertyName = "author_id")]
        public Guid AuthorId
        {
            get { return _authorId; }
            set
            {
                if (_authorId == value) return;
                _authorId = value;
                OnPropertyChanged();
            }
        }

        private string _messageShort;

        [JsonProperty(PropertyName = "message_short")]
        public string MessageShort
        {
            get { return _messageShort; }
            set
            {
                if (_messageShort == value) return;
                _messageShort = value;
                OnPropertyChanged();
            }
        }

        private string _message;

        [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, List<GitChange>> _changeShas;

        [JsonProperty(PropertyName = "changes")]
        public Dictionary<string, List<GitChange>> ChangeShas
        {
            get { return _changeShas; }
            set
            {
                if (_changeShas == value) return;
                _changeShas = value;
                OnPropertyChanged();
            }
        }


        private Dictionary<GitCommit, List<GitChange>> _changes;

        [JsonIgnore]
        public Dictionary<GitCommit, List<GitChange>> Changes
        {
            get { return _changes; }
            set
            {
                if (_changes == value) return;
                _changes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GitChange> _changesList;

        [JsonIgnore]
        public ObservableCollection<GitChange> ChangesList
        {
            get { return _changesList; }
            set
            {
                if (_changesList == value) return;
                _changesList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _childrenSha;

        [JsonProperty(PropertyName = "children_sha")]
        public ObservableCollection<string> ChildrenSha
        {
            get { return _childrenSha; }
            set
            {
                if (_childrenSha == value) return;
                _childrenSha = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GitCommit> _children;

        [JsonIgnore]
        public ObservableCollection<GitCommit> Children
        {
            get { return _children; }
            set
            {
                if (_children == value) return;
                _children = value;
                OnPropertyChanged();
            }
        }

        private Commit _innerCommit;

        [JsonIgnore]
        public Commit InnerCommit
        {
            get { return _innerCommit; }
            set
            {
                if (_innerCommit == value) return;
                _innerCommit = value;
                OnPropertyChanged();
            }
        }

        private bool _timeEstimated;

        [JsonProperty(PropertyName = "time_estimated")]
        public bool TimeEstimated
        {
            get { return _timeEstimated; }
            set
            {
                if (_timeEstimated == value) return;
                _timeEstimated = value;
                OnPropertyChanged();
            }
        }

        private DateTime _minTime;

        [JsonProperty(PropertyName = "min_time")]
        public DateTime MinTime
        {
            get { return _minTime; }
            set
            {
                if (_minTime == value) return;
                _minTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _maxTime;

        [JsonProperty(PropertyName = "max_time")]

        public DateTime MaxTime
        {
            get { return _maxTime; }
            set
            {
                if (_maxTime == value) return;
                _maxTime = value;
                OnPropertyChanged();
            }
        }

        public GitCommit(Commit innerCommit, IEnumerable<Student> members)
        {
            Sha = innerCommit.Sha;
            TimeOriginal = innerCommit.Author.When.DateTime;
            Time = innerCommit.Author.When.DateTime;
            MinTime = Time;
            MaxTime = Time;
            TimeEstimated = false;
            Author =
                members.FirstOrDefault(
                    student => student.GitEmail == innerCommit.Author.Email);
            Author?.Commits.Add(this);
            MessageShort = innerCommit.MessageShort;
            Message = innerCommit.Message;
            InnerCommit = innerCommit;
            Changes = new Dictionary<GitCommit, List<GitChange>>();
            ChangeShas = new Dictionary<string, List<GitChange>>();
            ChangesList = new ObservableCollection<GitChange>();
            Children = new ObservableCollection<GitCommit>();
            ChildrenSha = new ObservableCollection<string>();
        }

        public void AnalyzeChanges(GitRepo repo)
        {
            if (!InnerCommit.Parents.Any())
            {
                var diff = repo.InnerRepo.Diff.Compare<Patch>(null, InnerCommit.Tree);
                var changesList = diff.Select(patchEntry => new GitChange(RuntimeRoot.CommitRoot, this, patchEntry, Author)).ToList();
                Changes.Add(RuntimeRoot.CommitRoot, changesList);
                ChangeShas.Add(RuntimeRoot.CommitRoot.Sha, changesList);
                foreach (var change in changesList)
                {
                    ChangesList.Add(change);
                }
            }
            else
            {
                foreach (var innerParent in InnerCommit.Parents)
                {
                    var parent = repo.Commits.First(c => c.Sha == innerParent.Sha);
                    var diff = repo.InnerRepo.Diff.Compare<Patch>(parent.InnerCommit.Tree, InnerCommit.Tree);
                    var changesList = diff.Select(patchEntry => new GitChange(parent, this, patchEntry, Author)).ToList();
                    Changes.Add(parent, changesList);
                    ChangeShas.Add(parent.Sha, changesList);
                    foreach (var change in changesList)
                    {
                        ChangesList.Add(change);
                    }
                    parent.Children.Add(this);
                    parent.ChildrenSha.Add(Sha);
                }
            }
        }

        public void PostProcess(Team team)
        {
            Author = team.Members.FirstOrDefault(member => member.Id == AuthorId);
            Author?.Commits.Add(this);
            InnerCommit = team.Repo.InnerRepo.Commits.First(c => c.Sha == Sha);
            Changes = new Dictionary<GitCommit, List<GitChange>>();
            ChangesList = new ObservableCollection<GitChange>();
            Children = new ObservableCollection<GitCommit>();
            foreach (var parentSha in ChangeShas.Keys)
            {
                if (parentSha == StaticConfigs.CommitRootSha)
                    Changes.Add(RuntimeRoot.CommitRoot, ChangeShas[parentSha]);
                else
                {
                    var parent = team.Repo.Commits.First(c => c.Sha == parentSha);
                    Changes.Add(parent, ChangeShas[parentSha]);
                }
            }
            foreach (var childSha in ChildrenSha)
            {
                var child = team.Repo.Commits.First(c => c.Sha == childSha);
                Children.Add(child);
            }
            foreach (var changeList in Changes.Values)
                foreach (var change in changeList)
                {
                    change.PostProcess(team);
                    ChangesList.Add(change);
                }
        }

        public GitCommit()
        {
            // Reserved for Serialization
        }

        ~GitCommit()
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
            ChangeShas.Clear();
            Changes.Clear();
        }

        public override bool Equals(object obj)
        {
            var target = obj as GitCommit;
            if (target == null) return false;
            return target.Sha == Sha;
        }

        public override int GetHashCode()
        {
            return Sha.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
