using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Buhtig.Entities.Git;
using Buhtig.Entities.User;
using Buhtig.Helpers;
using Buhtig.Resources.Strings;
using Buhtig.Storage;
using Buhtig.ViewModels;
using Microsoft.Win32;
using Telerik.Charting;
using Telerik.Windows;
using Telerik.Windows.Controls;

namespace Buhtig.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : RadRibbonWindow
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            _viewModel = new MainViewModel();
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ManualImport_OnClick(object sender, RoutedEventArgs e)
        {
            var manualImportWindow = new ManualImportWindow(_viewModel.TeamStorage);
            manualImportWindow.Show();
        }

        private void TeamView_OnSelected(object sender, RadRoutedEventArgs e)
        {
            var selectedElement = TeamView.SelectedItem;

            CodeLinesSeries.DataPoints.Clear();
            CommitSeries.DataPoints.Clear();
            ContributionSeries.DataPoints.Clear();

            foreach (var dataPoint in ChartHelper.GenerateCodeLinesChart(selectedElement))
            {
                CodeLinesSeries.DataPoints.Add(dataPoint);
            }
            foreach (var dataPoint in ChartHelper.GenerateCommitChart(selectedElement))
            {
                CommitSeries.DataPoints.Add(dataPoint);
            }
            foreach (var dataPoint in ChartHelper.GenerateContributionChart(selectedElement))
            {
                ContributionSeries.DataPoints.Add(dataPoint);
            }
            foreach (var shape in ChartHelper.GenerateCollaborationDiagram(selectedElement))
            {
                CollaborationDiagram.Items.Add(shape);
            }
            CollaborationDiagram.AutoFit();

            if (selectedElement.GetType() == typeof(Student))
            {
                var contributionString = ((Student) selectedElement).ContributionString;
                var selectedDp = ContributionSeries.DataPoints.First(dp => (string) dp.Label == contributionString);
                selectedDp.IsSelected = true;
            }
        }

        private void WorkSpaceSave_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Buhtig File|*.btw",
                Title = Buhtig.Resources.Strings.Language.AppName
            };
            var result = saveDialog.ShowDialog();
            if (result != null && result.Value)
            {
                _viewModel.TeamStorage.Save(saveDialog.OpenFile());
            }
        }

        private void WorkSpaceLoad_OnClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Buhtig File | *.btw",
                Title = Buhtig.Resources.Strings.Language.AppName,
            };
            var result = openDialog.ShowDialog();
            if (result != null && result.Value)
            {
                _viewModel.TeamStorage = new TeamStorage(openDialog.OpenFile());
            }
        }

        private void CollaborationPane_OnActivated(object sender, EventArgs e)
        {
            CollaborationDiagram.AutoFit();
        }
    }
}
