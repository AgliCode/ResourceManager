using ResourceManager.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using ResourceManager.Core.Services;
using System.IO;
using ResourceManager.TranslationApi.Services;

namespace ResourceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ProjectContext ProjectContext { get; } = new();
        public GeminiKeyStorage GeminiKeyStorage { get; set; } = new();
        private readonly LanguageService _languageService = new();
        private string[] _resxFiles = Array.Empty<string>();
        private string resourcesDirectory {  get; set; } = string.Empty;
        private TaskCompletionSource<bool>? _geminiConfigurationTask;

        private ApiKeyStorageService _ApiKeyStorageService = new();
        private bool isInitialSetup = true;

        public MainWindow()
        {
            InitializeComponent();
            
            string? savedApiKey = _ApiKeyStorageService.Load();
            LoadLanguages();

            if (!string.IsNullOrWhiteSpace(savedApiKey))
            {
                OpenFolderSelection();
            }
            else
            {
                GeminiOverlay.Visibility = Visibility.Visible;
            }
        }

        public void UpdateCompleteTranslationsButtonStyle()
        {
            CompleteTranslationsButton.Style = (Style)CompleteTranslationsButton.FindResource(
                ProjectContext.HasSeparateTargetFile
                    ? "ModernMenuButtonStyle"
                    : "DisabledMenuButtonStyle"
            );
        }

        private async void CompleteTranslations_Click(object sender, RoutedEventArgs e)
        {
            var view = new CompleteTranslationsView(ProjectContext, GeminiKeyStorage);
            MainContent.Content = view;
            await view.LoadTranslationsAsync();
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            var view = new AddResourceView(ProjectContext, GeminiKeyStorage);
            MainContent.Content = view;
        }

        private async void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var view = new DashboardView(ProjectContext);
            MainContent.Content = view;
            await view.LoadStaticticAsync();
        }

        private async void AddNewLanguage_Click(object sender, RoutedEventArgs e)
        {
            var view = new AddNewLanguageView(ProjectContext, GeminiKeyStorage);
            MainContent.Content = view;
        }
        private async void BrowseResources_Click(object sender, RoutedEventArgs e)
        {
            var view = new BrowseResources(ProjectContext);

            MainContent.Content = view;
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var view = new SettingsView(ProjectContext, GeminiKeyStorage, this);
            MainContent.Content = view;
        }

        private void SelectProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder containing Resources.resx files."
            };

            if (dialog.ShowDialog() == true)
            {
                resourcesDirectory = dialog.FolderName;

                _resxFiles = Directory.GetFiles(
                    resourcesDirectory,
                    "*.resx",
                    SearchOption.TopDirectoryOnly);
  

                FolderOverlay.Visibility = Visibility.Collapsed;
                ResxSelectionOverlay.Visibility = Visibility.Visible;
                LoadResxFiles();
            }
        }

        private void LoadResxFiles()
        {
            BaseFileComboBox.ItemsSource = _resxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();

            TargetFileComboBox.ItemsSource = _resxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();

            BaseFileComboBox.SelectedIndex = 0;
            TargetFileComboBox.SelectedIndex = _resxFiles.Length > 1 ? 1 : -1;
        }


        private void LoadLanguages()
        {
            TargetLanguageComboBox.Items.Clear();

            var languages = _languageService.GetAvailableLanguages();

            foreach (var language in languages)
            {
                TargetLanguageComboBox.Items.Add(new ComboBoxItem
                {
                    Content = $"{language.EnglishName} - {language.Name}",
                    Tag = language.Name
                });
            }

            TargetLanguageComboBox.SelectedValuePath = "Tag";
        }

        private void AddApiKey_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = ApiKeyPasswordBox.Password;
            bool saveApiKeyLocally = RememberMeCheckBox.IsChecked == true;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show(
                    "Please enter your Gemini API key.",
                    "Missing API Key",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (saveApiKeyLocally)
            {
                _ApiKeyStorageService.Save(apiKey);
                GeminiKeyStorage.Set(string.Empty, true);
            }
            else
            {
                _ApiKeyStorageService.Delete();
                GeminiKeyStorage.Set(apiKey, false);
            }
            if (isInitialSetup)OpenFolderSelection();
            else GeminiOverlay.Visibility = Visibility.Collapsed;
            _geminiConfigurationTask?.TrySetResult(true);
            _geminiConfigurationTask = null;

        }
        private void CancelApiKey_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialSetup)
            {
                OpenFolderSelection();
            }
            else
            {
                GeminiOverlay.Visibility = Visibility.Collapsed;
            }

            _geminiConfigurationTask?.TrySetResult(false);
            _geminiConfigurationTask = null;
        }
        private void OpenFolderSelection()
        {
            GeminiOverlay.Visibility = Visibility.Collapsed;
            FolderOverlay.Visibility = Visibility.Visible;
        }

        private void GeminiApiKeyGuide_Click(object sender, RoutedEventArgs e)
        {
            var guideWindow = new GeminiApiKeyGuideWindow
            {
                Owner = this
            };

            guideWindow.ShowDialog();
        }




        private async void ContinueConfigurationFiles_Click(object sender, RoutedEventArgs e)
        {
            string? baseFileName = BaseFileComboBox.SelectedItem as string;
            string? targetFileName = TargetFileComboBox.SelectedItem as string;

            if (baseFileName == null || targetFileName == null)
            {
                MessageBox.Show("Please select both resource files.");
                return;
            }

            string baseFilePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_resxFiles[0])!,
                baseFileName);

            string targetFilePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_resxFiles[0])!,
                targetFileName);

            string? targetLanguage = (TargetLanguageComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            ProjectContext.SetProject(resourcesDirectory, baseFilePath, targetFilePath, targetLanguage, _resxFiles);
            ResxSelectionOverlay.Visibility = Visibility.Collapsed;
            UpdateCompleteTranslationsButtonStyle();
            var view = new DashboardView(ProjectContext);

            MainContent.Content = view;
            await view.LoadStaticticAsync();
        }

        public void ShowFolderConfiguration()
        {
            FolderOverlay.Visibility = Visibility.Visible;
            GeminiOverlay.Visibility = Visibility.Collapsed;
            SettingsNavButton.IsChecked = false;
            DashboardNavButton.IsChecked = true;

        }

        public Task ShowGeminiKeyConfiguration()
        {
            isInitialSetup = false;
            ApiKeyPasswordBox.Clear();
            RememberMeCheckBox.IsChecked = GeminiKeyStorage.SaveApiKeyLocally;
            _geminiConfigurationTask = new TaskCompletionSource<bool>();
            GeminiOverlay.Visibility = Visibility.Visible;
            return _geminiConfigurationTask.Task;
        }

    }
}