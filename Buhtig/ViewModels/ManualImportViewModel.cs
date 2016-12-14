using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Buhtig.Annotations;
using Buhtig.Entities.User;
using Buhtig.Storage;
using Buhtig.Views;

namespace Buhtig.ViewModels
{
    public class ManualImportViewModel : INotifyPropertyChanged
    {

        private TeamStorage _teamStorage;

        private Team _team;

        public Team Team
        {
            get { return _team; }
            set
            {
                if (_team == value) return;
                _team = value;
                OnPropertyChanged();
            }
        }

        public ManualImportViewModel(TeamStorage teamStorage)
        {
            _teamStorage = teamStorage;
        }

        public void Clone(string localPath)
        {
            try
            {
                if (Team == null) return;
                if (!_teamStorage.Teams.Contains(Team))
                    Team.Dispose();
            }
            finally
            {
                Team = new Team(localPath);
            }
        }

        public void Clone(Uri remoteUri, ManualImportWindow.CloneCallback callback)
        {
            try
            {
                if (Team == null) return;
                if (!_teamStorage.Teams.Contains(Team))
                    Team.Dispose();
            }
            finally
            {
                var cloneTask = new Task(() => { Team = new Team(remoteUri); });
                cloneTask.ContinueWith(task =>
                {
                    callback.Invoke(task.Exception);
                });
                cloneTask.Start();
            }
        }

        public void Import()
        {
            _teamStorage.Teams.Add(Team);
            Team = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
