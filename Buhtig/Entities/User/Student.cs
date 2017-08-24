using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Entities.Base;
using Buhtig.Entities.Git;
using Buhtig.Resources.Strings;
using Newtonsoft.Json;

namespace Buhtig.Entities.User
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Student : INotifyPropertyChanged, IPostProcessRequired
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

        private int _studentNumber;

        [JsonProperty(PropertyName = "student_number")]
        public int StudentNumber
        {
            get { return _studentNumber; }
            set
            {
                if (_studentNumber == value) return;
                _studentNumber = value;
                OnPropertyChanged();
            }
        }

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

        private long _mobile;

        [JsonProperty(PropertyName = "mobile")]
        public long Mobile
        {
            get { return _mobile; }
            set
            {
                if (_mobile == value) return;
                _mobile = value;
                OnPropertyChanged();
            }
        }

        private string _gitName;

        [JsonProperty(PropertyName = "git_name")]
        public string GitName
        {
            get { return _gitName; }
            set
            {
                if (_gitName == value) return;
                _gitName = value;
                OnPropertyChanged();
            }
        }
        private string _gitEmail;

        [JsonProperty(PropertyName = "git_email")]
        public string GitEmail
        {
            get { return _gitEmail; }
            set
            {
                if (_gitEmail == value) return;
                _gitEmail = value;
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

        private double _contribution;

        [JsonProperty(PropertyName = "contribution")]
        public double Contribution
        {
            get { return _contribution; }
            set
            {
                if (Math.Abs(_contribution - value) < 0.01) return;
                _contribution = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string ContributionString => 
            string.Format(StaticConfigs.ContributionFormat, ToString(), Contribution);

        public void CalcContribution()
        {
            var linesAdded = 0.0;
            var linesDeleted = 0.0;
            var totalChanges = (double)(BelongingTeam.Repo.LinesAdded + BelongingTeam.Repo.LinesDeleted);

            foreach (var commit in Commits)
            {
                foreach (var changeList in commit.Changes.Values)
                {
                    foreach (var change in changeList)
                    {
                        if (change.Framework) continue;
                        if (change.Merge) continue;
                        linesAdded += change.Summary.LinesAdded;
                        linesDeleted += change.Summary.LinesDeleted;
                    }
                }
            }

            Contribution = (linesAdded/BelongingTeam.Repo.LinesAdded)*
                           (BelongingTeam.Repo.LinesAdded/totalChanges) +
                           (linesDeleted/BelongingTeam.Repo.LinesDeleted)*
                           (BelongingTeam.Repo.LinesDeleted/totalChanges);
        }

        public override string ToString()
        {
            return StudentNumber == default(int)
                ? string.Format(StaticConfigs.StudentNameFormat, GitName, GitEmail)
                : string.Format(StaticConfigs.StudentNameFormat, Name, StudentNumber);
        }

        public Student(Team team, int studentNumber, string name, long mobile,
            string gitName, string gitEmail)
        {
            Id = Guid.NewGuid();
            BelongingTeam = team;
            StudentNumber = studentNumber;
            Name = name;
            Mobile = mobile;
            GitName = gitName;
            GitEmail = gitEmail;
            Commits = new ObservableCollection<GitCommit>();
        }

        public Student(Team team, string gitName, string gitEmail)
        {
            Id = Guid.NewGuid();
            BelongingTeam = team;
            GitName = gitName;
            Name = gitName;
            GitEmail = gitEmail;
            Commits = new ObservableCollection<GitCommit>();
        }

        public Student()
        {
            // Reserved for Serialization
        }

        public void PostProcess(Team team)
        {
            Commits = new ObservableCollection<GitCommit>();
            BelongingTeam = team;
        }

        public override int GetHashCode()
        {
            return GitEmail.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var targetStudent = obj as Student;
            if (targetStudent == null) return false;
            return GitEmail == targetStudent.GitEmail;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
