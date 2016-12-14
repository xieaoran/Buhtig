using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Entities.Base;
using Buhtig.Entities.Git;
using Newtonsoft.Json;

namespace Buhtig.Entities.User
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Collaboration : INotifyPropertyChanged, IPostProcessRequired
    {

        private ObservableCollection<Guid> _memberIds;

        [JsonProperty(PropertyName = "member_Ids")]
        public ObservableCollection<Guid> MemberIds
        {
            get { return _memberIds; }
            set
            {
                if (_memberIds == value) return;
                _memberIds = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Student> _members;

        [JsonIgnore]
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

        private Dictionary<Guid, List<Guid>> _memberIdChangeIds;

        [JsonProperty(PropertyName = "member_id_change_ids")]
        public Dictionary<Guid, List<Guid>> MemberIdChangeIds
        {
            get { return _memberIdChangeIds; }
            set
            {
                if (_memberIdChangeIds == value) return;
                _memberIdChangeIds = value;
                OnPropertyChanged();
            }

        }

        private Dictionary<Student, List<GitChange>> _memberChanges;

        [JsonIgnore]
        public Dictionary<Student, List<GitChange>> MemberChanges
        {
            get { return _memberChanges; }
            set
            {
                if (_memberChanges == value) return;
                _memberChanges = value;
                OnPropertyChanged();
            }
        }

        public Collaboration(IEnumerable<GitChange> memberChanges)
        {
            Members = new ObservableCollection<Student>();
            MemberIds = new ObservableCollection<Guid>(Members.Select(m => m.Id));
            MemberChanges = new Dictionary<Student, List<GitChange>>();
            MemberIdChangeIds = new Dictionary<Guid, List<Guid>>();
            foreach (var memberChange in memberChanges)
            {
                if (!Members.Contains(memberChange.Commit.Author))
                {
                    Members.Add(memberChange.Commit.Author);
                    MemberIds.Add(memberChange.Commit.Author.Id);
                }
                if (!MemberChanges.ContainsKey(memberChange.Commit.Author))
                {
                    MemberChanges.Add(memberChange.Commit.Author, new List<GitChange>());
                    MemberIdChangeIds.Add(memberChange.Commit.Author.Id, new List<Guid>());
                }
                MemberChanges[memberChange.Commit.Author].Add(memberChange);
                MemberIdChangeIds[memberChange.Commit.Author.Id].Add(memberChange.Id);
            }
        }

        public Collaboration()
        {
            // Reserved for Serialization
        }

        public void PostProcess(Team team)
        {
            Members = new ObservableCollection<Student>(team.Members.Where(m => MemberIds.Contains(m.Id)));
            MemberChanges = new Dictionary<Student, List<GitChange>>();
            foreach (var member in Members)
            {
                MemberChanges.Add(member, new List<GitChange>());
            }
            foreach (var commit in team.Repo.Commits)
            {
                if (!MemberIdChangeIds.Keys.Contains(commit.AuthorId)) continue;
                var changeIds = MemberIdChangeIds[commit.AuthorId];
                var changes = commit.ChangesList.Where(c => changeIds.Contains(c.Id));
                MemberChanges[commit.Author].AddRange(changes);
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
