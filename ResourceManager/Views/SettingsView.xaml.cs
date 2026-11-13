using ResourceManager.Core.Services;
using ResourceManager.TranslationApi.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ResourceManager.Views
{

    public partial class SettingsView : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly GeminiKeyStorage _geminiKeyStorage;
        private readonly MainWindow _mainWindow;
        public SettingsView(ProjectContext projectContext,GeminiKeyStorage geminiKeyStorage, MainWindow mainWindow)
        {
            InitializeComponent();
            _projectContext = projectContext;
            _geminiKeyStorage = geminiKeyStorage;
            _mainWindow = mainWindow;

            BaseFileComboBox.ItemsSource = _projectContext.ResxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();
            BaseFileComboBox.SelectedIndex = _projectContext.ResxFiles.ToList().FindIndex(path => path == _projectContext.BaseFilePath);

            TargetFileComboBox.ItemsSource = _projectContext.ResxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();
            TargetFileComboBox.SelectedIndex = _projectContext.ResxFiles.ToList().FindIndex(path => path == _projectContext.TargetFilePath);

            LoadLanguages();

            TargetLanguageComboBox.SelectedIndex = TargetLanguageComboBox.Items
                .Cast<ComboBoxItem>()
                .ToList()
                .FindIndex(item => item.Tag?.ToString() == _projectContext.TargetLanguage);

            ProjectFolderTextBox.Text = _projectContext.ProjectDirectory;
            _mainWindow = mainWindow;
        }
        private void LoadLanguages()
        {
            TargetLanguageComboBox.Items.Clear();

            var languages = LanguageService.GetAvailableLanguages();

            foreach (var language in languages)
            {
                TargetLanguageComboBox.Items.Add(new ComboBoxItem
                {
                    Content = $"{language.EnglishName} - {language.Name}",
                    Tag = language.Name
                });
            }

            TargetLanguageComboBox.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedValuePathProperty, "Tag");
        }

        public void Change_Click(object sender, EventArgs e)
        {
            _mainWindow.ShowFolderConfiguration();
        }

        private void RemoveKey_Click(object sender, RoutedEventArgs e)
        {
            _geminiKeyStorage.ClearApiKeyFromMemory();
            _geminiKeyStorage.DeleteSavedApiKey();
            _mainWindow.UpdateAddNewLanguageButtonStyle();

        }

        private async void Configure_Click(object sender, RoutedEventArgs e)
        {
            await _mainWindow.ShowGeminiKeyConfiguration();
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string? baseFileName = BaseFileComboBox.SelectedItem as string;
            string? targetFileName = TargetFileComboBox.SelectedItem as string;
            string? targetLanguage = (TargetLanguageComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            string[] _resxFiles = _projectContext.ResxFiles;
            string resourcesDirectory = _projectContext.ProjectDirectory;
            string baseFilePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_resxFiles[0])!, baseFileName);

            string targetFilePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_resxFiles[0])!, targetFileName);

            _projectContext.SetProject(resourcesDirectory, baseFilePath, targetFilePath, targetLanguage, _resxFiles);
            _mainWindow.UpdateCompleteTranslationsButtonStyle();
        }
    }
}
