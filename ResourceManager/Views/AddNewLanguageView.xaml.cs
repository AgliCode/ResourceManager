using ResourceManager.Core.Models;
using ResourceManager.Core.Services;
using ResourceManager.TranslationApi.Services;
using System.Windows;
using System.Windows.Controls;



namespace ResourceManager.Views
{
    public partial class AddNewLanguageView : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly GeminiKeyStorage _geminiKeyStorage;
        private readonly ResourceReader _resourceReader = new();
        private List<NewLanguageEntry> entries = new();
        private readonly TranslationApiClient _translationApiClient = new();
        private string? _outputFilePath;
        const int batchSize = 300;
        private readonly MainWindow _mainWindow;

        private bool isUpdatingSelectAll = false;
        public AddNewLanguageView(
            ProjectContext projectContext,
            GeminiKeyStorage geminiKeyStorage,
            MainWindow mainwindow)
        {
            InitializeComponent();
            _mainWindow = mainwindow;
            _projectContext = projectContext;
            _geminiKeyStorage = geminiKeyStorage;
            TargetLanguageComboBox.SelectionChanged += TargetLanguageComboBox_SelectionChanged;
            LoadLanguages();

            BaseFileComboBox.ItemsSource = _projectContext.ResxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();
            BaseFileComboBox.SelectedIndex = _projectContext.ResxFiles.ToList().FindIndex(path => path == _projectContext.BaseFilePath);

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
        private void TargetLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TargetLanguageComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            string selectedLanguage = selectedItem.Content?.ToString() ?? string.Empty;
            int separatorIndex = selectedLanguage.LastIndexOf(" - ");

            if (separatorIndex < 0)
                return;

            string languageCode = selectedLanguage.Substring(separatorIndex + 3);
            OutputFileNameTextBox.SetCurrentValue(TextBox.TextProperty, $"Resources.{languageCode}");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_projectContext.IsProjectLoaded)
            {
                MessageBox.Show("No project folder has been selected.");
                return;
            }

            if (TargetLanguageComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a target language.");
                return;
            }

            string fileName = OutputFileNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show("Please enter an output file name.");
                return;
            }

            if (fileName.EndsWith(".resx", System.StringComparison.OrdinalIgnoreCase))
                fileName = fileName[..^5];

            if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("The file name contains invalid characters.");
                return;
            }

            string outputPath = System.IO.Path.Combine(_projectContext.ProjectDirectory!, $"{fileName}.resx");
            _outputFilePath = outputPath;

            if (System.IO.File.Exists(outputPath))
            {
                MessageBox.Show(
                    "A file with this name already exists. Please choose another name.",
                    "File Already Exists",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                $"Configuration is valid.\n\nOutput file:\n{outputPath}",
                "Configuration",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            try
            {
                var resources = _resourceReader.ReadResources(_projectContext.BaseFilePath!);

                entries = resources.Select(resource => new NewLanguageEntry
                {
                    Name = resource.Name,
                    Value = resource.Value,
                    Comment = resource.Comment,
                    IsSelected = true
                })
                .ToList();

                EntriesItemsControl.SetCurrentValue(ItemsControl.ItemsSourceProperty, entries);

                SelectAllCheckBox.SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, true);

                ConfigurationPanel.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                EntriesPanel.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"Failed to read the base RESX file.\n\nDetails: {ex.Message}",
                    "File Read Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EntryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            isUpdatingSelectAll = true;

            try
            {
                bool allSelected = entries.Count > 0 && entries.All(entry => entry.IsSelected);
                SelectAllCheckBox.SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, allSelected);
            }
            finally
            {
                isUpdatingSelectAll = false;
            }
        }
        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (isUpdatingSelectAll)
                return;
            foreach (var entry in entries)
                entry.IsSelected = true;

            EntriesItemsControl?.Items.Refresh();
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isUpdatingSelectAll)
                return;
            foreach (var entry in entries)
                entry.IsSelected = false;

            EntriesItemsControl?.Items.Refresh();
        }

        private async void TranslateSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntries = entries
            .Where(entry => entry.IsSelected)
            .ToList();

            if (selectedEntries.Count == 0)
            {
                MessageBox.Show("Please select at least one entry to translate.");
                return;
            }
            if (TargetLanguageComboBox.SelectedItem is not ComboBoxItem selectedItem)
            {
                MessageBox.Show("Please select a target language.");
                return;
            }

            string selectedLanguage = selectedItem.Content?.ToString() ?? string.Empty;
            int separatorIndex = selectedLanguage.LastIndexOf(" - ");
            string languageCode = selectedLanguage.Substring(separatorIndex + 3);

            try
            {
                _mainWindow.ChangeLoadingVisible();
                var items = selectedEntries.Select(entry =>
                    new TranslationApiClient.TranslationApiItem
                    {
                        Key = entry.Name,
                        BaseText = entry.Value ?? string.Empty,
                        Comment = entry.Comment
                    }).ToList();

                var translations = new Dictionary<string, string>();

                for (int i = 0; i < items.Count; i += batchSize)
                {
                    var batch = items
                        .Skip(i)
                        .Take(batchSize)
                        .ToList();

                    var batchTranslations = await _translationApiClient.TranslateAsync(
                        batch,
                        _geminiKeyStorage.ReadApiKey()!,
                        languageCode);

                    foreach (var translation in batchTranslations)
                    {
                        translations[translation.Key] = translation.Value;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                foreach (var entry in selectedEntries)
                {
                    if (translations.TryGetValue(entry.Name, out string? translation))
                    {
                        entry.TranslatedValue = translation;
                    }
                }

                EntriesItemsControl.Items.Refresh();

                MessageBox.Show(
                    $"Successfully translated {translations.Count} entries.",
                    "Translation Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ResxWriter.CreateTranslatedResx(
                _projectContext.BaseFilePath!,
                _outputFilePath!,
                selectedEntries);

                MessageBox.Show(
                    $"Successfully translated and saved {selectedEntries.Count} entries to " +
                    $"{System.IO.Path.GetFileName(_outputFilePath)}.",
                    "Translation Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Translation failed.\n\nDetails: {ex.Message}",
                    "Translation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _mainWindow.ChangeLoadingVisible();
            }
            

        }
    }
    
}
