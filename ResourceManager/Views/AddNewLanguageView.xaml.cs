using ResourceManager.Models;
using ResourceManager.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ResourceManager.Services.TranslationApiClient;


namespace ResourceManager.Views
{
    public partial class AddNewLanguageView : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly ResourceReader _resourceReader = new();
        private List<NewLanguageEntry> entries = new();
        private readonly TranslationApiClient _translationApiClient = new();
        private string? _outputFilePath;
        private readonly ResxWriter _resxWriter = new();
        const int batchSize = 300;
        public AddNewLanguageView(ProjectContext projectContext)
        {
            InitializeComponent();
            _projectContext = projectContext;
            TargetLanguageComboBox.SelectionChanged += TargetLanguageComboBox_SelectionChanged;

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
            OutputFileNameTextBox.Text = $"Resources.{languageCode}";
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
                var resources = _resourceReader.ReadResources(
                    _projectContext.PolishFilePath!);

                entries = resources.Select(resource => new NewLanguageEntry
                {
                    Name = resource.Name,
                    Value = resource.Value,
                    Comment = resource.Comment,
                    IsSelected = true
                }).ToList();

                EntriesItemsControl.ItemsSource = entries;

                SelectAllCheckBox.IsChecked = true;

                ConfigurationPanel.Visibility = Visibility.Collapsed;
                EntriesPanel.Visibility = Visibility.Visible;
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
            if (entries.All(entry => entry.IsSelected))
            {
            SelectAllCheckBox.IsChecked = true;
            }
            else
            {
            SelectAllCheckBox.IsChecked = false;
            }
        }
        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var entry in entries)
                entry.IsSelected = true;

            EntriesItemsControl?.Items.Refresh();
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
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
                var items = selectedEntries.Select(entry =>
                    new TranslationApiClient.TranslationApiItem
                    {
                        Key = entry.Name,
                        PolishText = entry.Value ?? string.Empty,
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

                _resxWriter.CreateTranslatedResx(
                _projectContext.PolishFilePath!,
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
            

        }
    }
    
}
