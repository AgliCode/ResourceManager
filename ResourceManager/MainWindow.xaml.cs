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
using ResourceManager.Services;
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
        private string[] _resxFiles = Array.Empty<string>();
        private string resourcesDirectory {  get; set; } = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CompleteTranslations_Click(object sender, RoutedEventArgs e)
        {
            var view = new CompleteTranslationsView(ProjectContext, GeminiKeyStorage);

            MainContent.Content = view;

            await view.LoadTranslationsAsync();
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {

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

        private void AddApiKey_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = ApiKeyPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show(
                    "Please enter your Gemini API key.",
                    "Missing API Key",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            GeminiKeyStorage.Set(apiKey);
            OpenFolderSelection();
        }
        private void CancelApiKey_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderSelection();
        }
        private void OpenFolderSelection()
        {
            GeminiOverlay.Visibility = Visibility.Collapsed;
            FolderOverlay.Visibility = Visibility.Visible;
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

            ProjectContext.SetProject(resourcesDirectory, baseFilePath, targetFilePath, targetLanguage);
            ResxSelectionOverlay.Visibility = Visibility.Collapsed;
            var view = new DashboardView(ProjectContext);

            MainContent.Content = view;
            await view.LoadStaticticAsync();
        }

    }
}