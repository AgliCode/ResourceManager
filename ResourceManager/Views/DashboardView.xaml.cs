using ResourceManager.Core.Models;
using ResourceManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ResourceManager.Views
{
    /// <summary>
    /// Logika interakcji dla klasy DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly ResourceReader _resourceReader = new();
        private List<ResourceEntry> allKeys = new();
        private List<ResourceEntry> missingTranslations = new();
        private int allKeys_Count;
        private int missingTranslations_Count;
        private int translatedKeys_Count;

        public DashboardView(ProjectContext projectContext)
        {
            InitializeComponent();
            _projectContext = projectContext;
            UpdateCompletePanelsVisible();
        }

        public void UpdateCompletePanelsVisible()
        {
            TranslatedPanel.Visibility = _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            MissingPanel.Visibility = _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        }

        public async Task LoadStaticticAsync()
        {
            if (!_projectContext.IsProjectLoaded)
            {
                MessageBox.Show("No project folder has been selected.");
                return;
            }
            //Statistics
            allKeys = _resourceReader.ReadResources(_projectContext.BaseFilePath!);
            missingTranslations = _resourceReader.FindMissingTranslations(_projectContext.BaseFilePath!, _projectContext.TargetFilePath!);
            allKeys_Count = allKeys.Count;
            missingTranslations_Count = missingTranslations.Count;
            translatedKeys_Count = (allKeys_Count - missingTranslations_Count);

            TotalKeysText.Text = allKeys_Count.ToString();
            MissingTranslationsText.Text = missingTranslations_Count.ToString();
            TranslatedText.Text = (translatedKeys_Count).ToString();

            //Progress bar
            double completion = allKeys_Count > 0
               ? (double)translatedKeys_Count / allKeys_Count * 100
               : 0;
            CompletionText.Text = $"{completion:F1}%";
            CompletionProgressBar.Value = completion;

            //Project directory
            ProjectNameText.Text = System.IO.Path.GetFileName(_projectContext.ProjectDirectory);
            ProjectPathText.Text = _projectContext.ProjectDirectory;


        }

    }
}
