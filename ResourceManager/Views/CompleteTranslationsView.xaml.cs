using Microsoft.Win32;
using ResourceManager.Enums;
using ResourceManager.Models;
using ResourceManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static ResourceManager.Services.TranslationApiClient;


namespace ResourceManager.Views
{

    public partial class CompleteTranslationsView : UserControl
    {
        private readonly ResourceReader _resourceReader = new();
        private List<ResourceEntry> missingTranslations = new();
        private Dictionary<string, string> approvedTranslations = new();
        private string? englishFilePath;
        private int actual_id = 0;
        private readonly TranslationApiClient _translationApiClient = new();
        private Dictionary<string, string> aiTranslations = new();
        private readonly ResxWriter _resxWriter = new();
        private readonly ProjectContext _projectContext;



        public CompleteTranslationsView(ProjectContext projectContext)
        {
            InitializeComponent();
            _projectContext = projectContext;
        }

        public async Task LoadTranslationsAsync()
        {
            if (!_projectContext.IsProjectLoaded)
            {
                MessageBox.Show("No project folder has been selected.");
                return;
            }

            approvedTranslations.Clear();
            aiTranslations.Clear();
            actual_id = 0;

            NameTextBox.Clear();
            ValueTextBox.Clear();
            CommentTextBox.Clear();
            EnglishValueTextBox.Clear();

            PreviousButton.IsEnabled = false;
            NextButton.IsEnabled = false;

            IncompleteEntriesList.ItemsSource = null;
            missingTranslations.Clear();

                try
                {
                    missingTranslations = _resourceReader.FindMissingTranslations(
                    _projectContext.PolishFilePath!,
                    _projectContext.EnglishFilePath!);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                    $"Failed to read the RESX files.\n\nDetails: {ex.Message}",
                    "File Read Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                    return;
                }

                IncompleteEntriesList.ItemsSource = missingTranslations;
                UpdateNavigationButtons();

                if (missingTranslations.Count > 0)
                {
                    
                    try
                    {
                        var itemsToTranslate = missingTranslations
                            .Select(resource => new TranslationApiItem
                            {
                                Key = resource.Name,
                                PolishText = resource.Value,
                                Comment = resource.Comment
                            })
                            .ToList();

                        aiTranslations = await _translationApiClient.TranslateAsync(
                            itemsToTranslate);

                        MessageBox.Show(
                            $"Otrzymano {aiTranslations.Count} propozycji tłumaczeń.",
                            "Tłumaczenie AI",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Nie udało się pobrać tłumaczeń AI:\n{ex.Message}",
                            "Błąd AI",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    actual_id = 0;
                    IncompleteEntriesList.SelectedIndex = 0;
                    DisplayCurrentEntry();
                }
                else
                {
                    IncompleteEntriesList.ItemsSource = null;

                    MessageBox.Show(
                        "No missing translations were found.",
                        "Translation Check",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

        }
            
        

        private void NextEntry_Click(object sender, RoutedEventArgs e)
        {
            
            actual_id = (actual_id + 1) % missingTranslations.Count;

            DisplayCurrentEntry();

        }

        private void PreviousEntry_Click(object sender, RoutedEventArgs e)
        {
            actual_id = (actual_id - 1) % missingTranslations.Count;

            DisplayCurrentEntry();
        }

        private void DisplayCurrentEntry()
        {
            if (actual_id < 0 || actual_id >= missingTranslations.Count)
                return;

            IncompleteEntriesList.SelectedIndex = actual_id;
            var resource = missingTranslations[actual_id];

            NameTextBox.Text = resource.Name;
            ValueTextBox.Text = resource.Value;
            CommentTextBox.Text = resource.Comment;

            if (approvedTranslations.TryGetValue(resource.Name, out var approved))
            {
                EnglishValueTextBox.Text = approved;
            }
            else if (aiTranslations.TryGetValue(resource.Name, out var suggestion))
            {
                EnglishValueTextBox.Text = suggestion;
            }
            else
            {
                EnglishValueTextBox.Text = resource.EnglishValue;
            }
        }

        private void IncompleteEntriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            actual_id = IncompleteEntriesList.SelectedIndex;

            if (actual_id < 0 || actual_id >= missingTranslations.Count)
                return;

            missingTranslations[actual_id].Status = TranslationStatus.Current;

            DisplayCurrentEntry();

            IncompleteEntriesList.Items.Refresh();
        }

        private void ApproveAndNext_Click(object sender, RoutedEventArgs e)
        {
            string translation = EnglishValueTextBox.Text.Trim();
            var resource = missingTranslations[actual_id];
            approvedTranslations[resource.Name] = translation;
            resource.Status = TranslationStatus.Approved;
            if (actual_id < missingTranslations.Count - 1)
            {
                actual_id++;
                IncompleteEntriesList.SelectedIndex = actual_id;
            }
            IncompleteEntriesList.Items.Refresh();
        }

        

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
          _resxWriter.SaveTranslationToResx(approvedTranslations, _projectContext.EnglishFilePath); 
        }

        private void UpdateNavigationButtons()
        {
            bool hasEntries = missingTranslations.Count > 0;

            PreviousButton.IsEnabled = hasEntries;
            NextButton.IsEnabled = hasEntries;
        }
    }
}
