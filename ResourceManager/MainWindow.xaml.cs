using Microsoft.Win32;
using ResourceManager.Core.Services;
using ResourceManager.TranslationApi.Services;
using ResourceManager.Views;
using System.Diagnostics;
using System.IO;
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
using System.Net.Http;

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
        private readonly ApiProcessManager _apiProcessManager = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

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

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string apiDirectory = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(AppContext.BaseDirectory, "..", "API"));

                await _apiProcessManager.StartAsync(apiDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not start the API:\n{ex.Message}",
                    "API Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _apiProcessManager.Dispose();
        }

        public void UpdateCompleteTranslationsButtonStyle()
        {
            bool hasSeparateTargetFile = ProjectContext.HasSeparateTargetFile;
            CompleteTranslationsButton.SetCurrentValue(StyleProperty, (Style)CompleteTranslationsButton.FindResource(
                hasSeparateTargetFile
                    ? "ModernMenuButtonStyle"
                    : "DisabledMenuButtonStyle"
            ));

            CompleteTranslationsButton.SetCurrentValue(ToolTipProperty, hasSeparateTargetFile
            ? null
            : "To use this feature, the Base file and Target file must be different.");
        }

        public void UpdateAddNewLanguageButtonStyle()
        {
            bool hasApiKey = !string.IsNullOrWhiteSpace(GeminiKeyStorage.ReadApiKey());

            AddNewLanguageButton.SetCurrentValue(
            StyleProperty,
            (Style)AddNewLanguageButton.FindResource(
                hasApiKey
                    ? "ModernMenuButtonStyle"
                    : "DisabledMenuButtonStyle"
            ));

            AddNewLanguageButton.SetCurrentValue(ToolTipProperty, hasApiKey
                ? null
                : "Gemini API key must be configured.");
        }

        private async void CompleteTranslations_Click(object sender, RoutedEventArgs e)
        {
            var view = new CompleteTranslationsView(ProjectContext, GeminiKeyStorage, this);
            MainContent.Content = view;
            await view.LoadTranslationsAsync();
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            var view = new AddResourceView(ProjectContext, GeminiKeyStorage, this);
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
            var view = new AddNewLanguageView(ProjectContext, GeminiKeyStorage, this);
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
                try
                {
                    resourcesDirectory = dialog.FolderName;

                    _resxFiles = Directory.GetFiles(
                        resourcesDirectory,
                        "*.resx",
                        SearchOption.TopDirectoryOnly);
                
                    if (_resxFiles.Length == 0)
                    {
                        MessageBox.Show(
                        "No RESX files were found in the selected folder. Please select another folder.",
                        "No RESX Files Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                        return;
                    }

                    FolderOverlay.Visibility = Visibility.Collapsed;
                    ResxSelectionOverlay.Visibility = Visibility.Visible;
                    LoadResxFiles();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                $"Failed to access the selected folder.\n\nDetails: {ex.Message}",
                "Folder Access Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                }
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

            UpdateAddNewLanguageButtonStyle();

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

            UpdateAddNewLanguageButtonStyle();
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
            ResxSelectionOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            UpdateCompleteTranslationsButtonStyle();
            var view = new DashboardView(ProjectContext);

            MainContent.SetCurrentValue(ContentProperty, view);
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

        public void ChangeLoadingVisible()
        {
            if(LoadingOverlay.Visibility == Visibility.Visible) LoadingOverlay.Visibility = Visibility.Collapsed;
            else LoadingOverlay.Visibility = Visibility.Visible;
        }
    
    }
}