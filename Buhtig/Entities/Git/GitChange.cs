using System;
using System.Collections.Generic;
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
    public class GitChange : INotifyPropertyChanged, IPostProcessRequired
    {

        private Guid _id;

        [JsonProperty(PropertyName = "id")]
        public Guid Id
        {
            get { return _id; }
            set
            {
                if(_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _parentSha;

        [JsonProperty(PropertyName = "parent_sha")]
        public string ParentSha
        {
            get { return _parentSha; }
            set
            {
                if (_parentSha == value) return;
                _parentSha = value;
                OnPropertyChanged();
            }
        }

        private GitCommit _parent;

        [JsonIgnore]
        public GitCommit Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value) return;
                _parent = value;
                OnPropertyChanged();
            }
        }

        private string _commitSha;

        [JsonProperty(PropertyName = "commit_sha")]
        public string CommitSha
        {
            get { return _commitSha; }
            set
            {
                if (_commitSha == value) return;
                _commitSha = value;
                OnPropertyChanged();
            }
        }

        private GitCommit _commit;

        [JsonIgnore]
        public GitCommit Commit
        {
            get { return _commit; }
            set
            {
                if (_commit == value) return;
                _commit = value;
                OnPropertyChanged();
            }
        }

        private GitChangeSummary _summary;

        [JsonProperty(PropertyName = "summary")]
        public GitChangeSummary Summary
        {
            get { return _summary; }
            set
            {
                if (_summary == value) return;
                _summary = value;
                OnPropertyChanged();
            }
        }

        private GitChangeBlob _oldBlob;

        [JsonProperty(PropertyName = "old_blob")]
        public GitChangeBlob OldBlob
        {
            get { return _oldBlob; }
            set
            {
                if (_oldBlob == value) return;
                _oldBlob = value;
                OnPropertyChanged();
            }
        }

        private GitChangeBlob _newBlob;

        [JsonProperty(PropertyName = "new_blob")]
        public GitChangeBlob NewBlob
        {
            get { return _newBlob; }
            set
            {
                if (_newBlob == value) return;
                _newBlob = value;
                OnPropertyChanged();
            }
        }

        private bool _framework;

        [JsonProperty(PropertyName = "framework")]
        public bool Framework
        {
            get { return _framework; }
            set
            {
                if (_framework == value) return;
                _framework = value;
                OnPropertyChanged();
            }
        }

        private bool _merge;

        [JsonProperty(PropertyName = "merge")]
        public bool Merge
        {
            get { return _merge; }
            set
            {
                if (_merge == value) return;
                _merge = value;
                OnPropertyChanged();
            }
        }

        public GitChange(GitCommit parent, GitCommit commit, PatchEntryChanges patchEntryChanges, Student author)
        {
            Id = Guid.NewGuid();
            Parent = parent;
            ParentSha = parent.Sha;
            Commit = commit;
            CommitSha = commit.Sha;
            Summary = new GitChangeSummary(patchEntryChanges.Status, patchEntryChanges.LinesAdded,
                patchEntryChanges.LinesDeleted);
            OldBlob = new GitChangeBlob(patchEntryChanges.OldOid, patchEntryChanges.OldPath, patchEntryChanges.OldMode);
            NewBlob = new GitChangeBlob(patchEntryChanges.Oid, patchEntryChanges.Path, patchEntryChanges.Mode);

            Merge = commit.InnerCommit.Parents.Count() > 1;

            var lowerPatch = patchEntryChanges.Patch.ToLower();
            if (lowerPatch.Contains(StaticConfigs.CopiedCriteria))
            {
                if (author == null) Framework = true;
                else if (lowerPatch.Contains(author.GitName.ToLower()) ||
                         lowerPatch.Contains(author.GitEmail.ToLower()) ||
                         lowerPatch.Contains(author.Name.ToLower())) Framework = false;
                else Framework = true;
            }
            else Framework = false;
        }

        public GitChange()
        {
            // Reserved for Serialization
        }

        public void PostProcess(Team team)
        {
            Parent = team.Repo.Commits.FirstOrDefault(c => c.Sha == ParentSha);
            Commit = team.Repo.Commits.FirstOrDefault(c => c.Sha == CommitSha);
        }

        public override bool Equals(object obj)
        {
            var target = obj as GitChange;
            if (target == null) return false;
            return target.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
