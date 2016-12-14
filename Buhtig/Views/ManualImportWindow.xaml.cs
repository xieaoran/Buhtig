using System;
using System.Windows;
using System.Windows.Forms;
using Buhtig.Helpers;
using Buhtig.Storage;
using Buhtig.ViewModels;

namespace Buhtig.Views
{
    /// <summary>
    /// ManualImportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ManualImportWindow : Window
    {
        private ManualImportViewModel _viewModel;

        public delegate void CloneCallback(Exception exception);

        public ManualImportWindow(TeamStorage teamStorage)
        {
            _viewModel = new ManualImportViewModel(teamStorage);
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            var browseDialog = new FolderBrowserDialog { Description = Buhtig.Resources.Strings.Language.Browse };
            if (browseDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.Clone(browseDialog.SelectedPath);
            }
        }

        private void Import_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Import();
        }

        private void Clone_OnClick(object sender, RoutedEventArgs e)
        {
            BusyIndicator.IsBusy = true;
            try
            {
                _viewModel.Clone(new Uri(RepositoryUrl.Text), Clone_Finished);
            }
            catch (Exception exception)
            {
                DialogHelper.PromptException(exception);
                BusyIndicator.IsBusy = false;
            }
        }

        private void Clone_Finished(Exception exception)
        {
            Dispatcher.Invoke(() =>
            {
                if (exception != null) DialogHelper.PromptException(exception);
                BusyIndicator.IsBusy = false;
            });
        }
    }
}
