using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Buhtig.Entities.Git
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GitChangeSummary : INotifyPropertyChanged
    {
        private ChangeKind _status;

        [JsonProperty(PropertyName = "status")]
        public ChangeKind Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;
                _status = value;
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

        public GitChangeSummary(ChangeKind status, int linesAdded, int linesDeleted)
        {
            Status = status;
            LinesAdded = linesAdded;
            LinesDeleted = linesDeleted;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
