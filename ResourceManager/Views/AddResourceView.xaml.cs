using ResourceManager.Core.Services;
using ResourceManager.TranslationApi.Services;
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

namespace ResourceManager.Views
{
    /// <summary>
    /// Logika interakcji dla klasy AddResourceView.xaml
    /// </summary>
    public partial class AddResourceView : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly GeminiKeyStorage _geminiKeyStorage;
        private readonly TranslationApiClient _translationApiClient = new();
        private readonly ResxWriter _resxWriter = new();

        public AddResourceView(ProjectContext projectContext, GeminiKeyStorage geminiKeyStorage)
        {
            InitializeComponent();
            _projectContext = projectContext;
            _geminiKeyStorage = geminiKeyStorage;
            UpdateTargetPanelVisible();
            TargetValueLabel.Text =
                $"Target value ({_projectContext.TargetLanguage})";
        }
        public void UpdateTargetPanelVisible()
        {
            TargetValuePanel.Visibility = _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            TargetValueTextBox.Visibility = _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            SuggestTranslationButton.Visibility = _projectContext.HasSeparateTargetFile
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private async void SuggestTranslationButton_Click(object sender, RoutedEventArgs e)
        {
            string neutralValue = NeutralValueTextBox.Text.Trim();
            string keyName = KeyNameTextBox.Text.Trim();
            string? comment = NeutralCommentTextBox.Text.Trim();


            try
            {
                var items = new List<TranslationApiClient.TranslationApiItem>
                {
                    new()
                    {
                        Key = keyName,
                        PolishText = neutralValue,
                        Comment = comment
                    }
                };

                var translations = await _translationApiClient.TranslateAsync(
                    items,
                    _geminiKeyStorage.ReadApiKey()!,
                    _projectContext.TargetLanguage!);

                if (translations.TryGetValue(keyName, out string? translatedText))
                {
                    TargetValueTextBox.Text = translatedText;
                }
                else
                {
                    MessageBox.Show(
                        "No translation was returned.",
                        "Translation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to generate translation:\n{ex.Message}",
                    "Translation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SuggestTranslationButton.IsEnabled = true;
            }
        }

        private void AddEntryButton_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyNameTextBox.Text.Trim();
            string neutralValue = NeutralValueTextBox.Text.Trim();
            string? comment = NeutralCommentTextBox.Text.Trim();
            string translatedValue = TargetValueTextBox.Text.Trim();

            bool hasSeparateTargetFile = _projectContext.HasSeparateTargetFile;

            // Validate required fields.
            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(neutralValue) ||
                (hasSeparateTargetFile &&  string.IsNullOrWhiteSpace(translatedValue)))
            {
                MessageBox.Show(
                    "Please fill in the key name, neutral value and target value.",
                    "Missing Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                _resxWriter.AddEntryToResx(
                    key,
                    neutralValue,
                    comment,
                    _projectContext.HasSeparateTargetFile ? translatedValue : null,
                    _projectContext.BaseFilePath,
                    _projectContext.HasSeparateTargetFile
                        ? _projectContext.TargetFilePath
                        : null);

                MessageBox.Show(
                    "The resource entry has been successfully added to both files.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Clear the form.
                KeyNameTextBox.Clear();
                NeutralValueTextBox.Clear();
                NeutralCommentTextBox.Clear();
                TargetValueTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to add the resource entry:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
