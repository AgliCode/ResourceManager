using ResourceManager.Core.Models;
using ResourceManager.Core.Services;
using System.Windows;
using System.Windows.Controls;


namespace ResourceManager.Views
{

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
            TranslatedPanel.SetCurrentValue(VisibilityProperty, _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed);

            MissingPanel.SetCurrentValue(VisibilityProperty, _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed);
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

            TotalKeysText.SetCurrentValue(TextBlock.TextProperty, allKeys_Count.ToString());
            MissingTranslationsText.SetCurrentValue(TextBlock.TextProperty, missingTranslations_Count.ToString());
            TranslatedText.SetCurrentValue(TextBlock.TextProperty, (translatedKeys_Count).ToString());

            //Progress bar
            double completion = allKeys_Count > 0
               ? (double)translatedKeys_Count / allKeys_Count * 100
               : 0;
            CompletionText.SetCurrentValue(TextBlock.TextProperty, $"{completion:F1}%");
            CompletionProgressBar.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.ValueProperty, completion);

            //Project directory
            ProjectNameText.SetCurrentValue(TextBlock.TextProperty, System.IO.Path.GetFileName(_projectContext.ProjectDirectory));
            ProjectPathText.SetCurrentValue(TextBlock.TextProperty, _projectContext.ProjectDirectory);
        }

    }
}
