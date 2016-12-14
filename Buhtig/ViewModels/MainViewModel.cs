using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Buhtig.Annotations;
using Buhtig.Entities.User;
using Buhtig.Storage;

namespace Buhtig.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private TeamStorage _teamStorage;

        public TeamStorage TeamStorage
        {
            get { return _teamStorage; }
            set
            {
                if (_teamStorage == value) return;
                _teamStorage = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            TeamStorage = new TeamStorage();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
