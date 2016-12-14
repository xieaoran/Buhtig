using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Buhtig.Entities.Git
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GitChangeBlob : INotifyPropertyChanged
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
            }
        }

        private string _path;

        [JsonProperty(PropertyName = "path")]
        public string Path
        {
            get { return _path; }
            set
            {
                if (_path == value) return;
                _path = value;
                OnPropertyChanged();
            }
        }

        private Mode _mode;

        [JsonProperty(PropertyName = "mode")]
        public Mode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode == value) return;
                _mode = value;
                OnPropertyChanged();
            }
        }

        public GitChangeBlob(ObjectId id, string path, Mode mode)
        {
            Sha = id.Sha;
            Path = path;
            Mode = mode;
        }

        public GitChangeBlob()
        {
            // Reserved for Serialization
        }

        public override bool Equals(object obj)
        {
            var target = obj as GitChangeBlob;
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
