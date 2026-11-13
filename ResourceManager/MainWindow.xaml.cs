using Microsoft.Win32;
using ResourceManager.Core.Services;
using ResourceManager.TranslationApi.Services;
using ResourceManager.Views;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using ResourceManager.Helpers;

namespace ResourceManager
{

    public partial class MainWindow : Window
    {
        public ProjectContext ProjectContext { get; } = new();
        public GeminiKeyStorage GeminiKeyStorage { get; set; } = new();
        private string[] _resxFiles = Array.Empty<string>();
        private string resourcesDirectory {  get; set; } = string.Empty;
        private TaskCompletionSource<bool>? _geminiConfigurationTask;

        private ApiKeyStorageService _ApiKeyStorageService = new();
        private bool isInitialSetup = true;
        private readonly ApiProcessManager _apiProcessManager = new();
        private readonly ResourceReader reader = new();

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
            StyleHelper.UpdateNavButtonStyle(
                CompleteTranslationsButton,
                ProjectContext.HasSeparateTargetFile,
                "To use this feature, the Base file and Target file must be different.");
        }

        public void UpdateAddNewLanguageButtonStyle()
        {
            StyleHelper.UpdateNavButtonStyle(
                AddNewLanguageButton,
                !string.IsNullOrWhiteSpace(GeminiKeyStorage.ReadApiKey()),
                "Gemini API key must be configured.");
        }

        private async void CompleteTranslations_Click(object sender, RoutedEventArgs e)
        {
            var view = new CompleteTranslationsView(ProjectContext, GeminiKeyStorage, this);
            MainContent.SetCurrentValue(ContentProperty, view);
            await view.LoadTranslationsAsync();
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            var view = new AddResourceView(ProjectContext, GeminiKeyStorage, this);
            MainContent.SetCurrentValue(ContentProperty, view);
        }

        private async void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var view = new DashboardView(ProjectContext);
            MainContent.SetCurrentValue(ContentProperty, view);
            await view.LoadStaticticAsync();
        }

        private async void AddNewLanguage_Click(object sender, RoutedEventArgs e)
        {
            var view = new AddNewLanguageView(ProjectContext, GeminiKeyStorage, this);
            MainContent.SetCurrentValue(ContentProperty, view);
        }
        private async void BrowseResources_Click(object sender, RoutedEventArgs e)
        {
            var view = new BrowseResources(ProjectContext);
            MainContent.SetCurrentValue(ContentProperty, view);
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var view = new SettingsView(ProjectContext, GeminiKeyStorage, this);
            MainContent.SetCurrentValue(ContentProperty, view);
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

                    _resxFiles = reader.GetResxFiles(resourcesDirectory);


                    if (_resxFiles.Length == 0)
                    {
                        MessageBox.Show(
                        "No RESX files were found in the selected folder. Please select another folder.",
                        "No RESX Files Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                        return;
                    }

                    FolderOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                    ResxSelectionOverlay.SetCurrentValue(VisibilityProperty, Visibility.Visible);
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
            BaseFileComboBox.SetCurrentValue(ItemsControl.ItemsSourceProperty, _resxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList());

            TargetFileComboBox.SetCurrentValue(ItemsControl.ItemsSourceProperty, _resxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList());

            BaseFileComboBox.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedIndexProperty, 0);
            TargetFileComboBox.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedIndexProperty, _resxFiles.Length > 1 ? 1 : -1);
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
            else GeminiOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
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
                GeminiOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }

            _geminiConfigurationTask?.TrySetResult(false);
            _geminiConfigurationTask = null;

            UpdateAddNewLanguageButtonStyle();
        }
        private void OpenFolderSelection()
        {
            GeminiOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            FolderOverlay.SetCurrentValue(VisibilityProperty, Visibility.Visible);
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
            FolderOverlay.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            GeminiOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            SettingsNavButton.SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, false);
            DashboardNavButton.SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, true);
        }

        public Task ShowGeminiKeyConfiguration()
        {
            isInitialSetup = false;
            ApiKeyPasswordBox.Clear();
            RememberMeCheckBox.SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, GeminiKeyStorage.SaveApiKeyLocally);
            _geminiConfigurationTask = new TaskCompletionSource<bool>();
            GeminiOverlay.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            return _geminiConfigurationTask.Task;
        }

        public void ChangeLoadingVisible()
        {
            if(LoadingOverlay.Visibility == Visibility.Visible) LoadingOverlay.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            else LoadingOverlay.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }
    
    }
}