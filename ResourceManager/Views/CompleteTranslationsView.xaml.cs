using Microsoft.Win32;
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

namespace ResourceManager.Views
{
    /// <summary>
    /// Logika interakcji dla klasy CompleteTranslationsView.xaml
    /// </summary>
    public partial class CompleteTranslationsView : UserControl
    {
        private readonly ResourceReader _resourceReader = new();
        private List<ResourceEntry> missingTranslations = new();
        private Dictionary<string, string> approvedTranslations = new();
        private int actual_id = 0;



        public CompleteTranslationsView()
        {
            InitializeComponent();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            approvedTranslations.Clear();
            actual_id = 0;
            var dialog = new OpenFolderDialog
            {
                Title = "Wybierz folder z plikami Resources.resx"
            };

            if (dialog.ShowDialog() == true)
            {
                string resourcesDirectory = dialog.FolderName;

                string polishFile = System.IO.Path.Combine(
                    resourcesDirectory, "Resources.resx");

                string englishFile = System.IO.Path.Combine(
                    resourcesDirectory, "Resources.en-US.resx");

                missingTranslations = _resourceReader.FindMissingTranslations(
                polishFile,
                englishFile);

                IncompleteEntriesList.ItemsSource = missingTranslations
                    .Select(resource => resource.Name)
                    .ToList();

                if (missingTranslations.Count > 0)
                {
                    var firstResource = missingTranslations[actual_id];

                    NameTextBox.Text = firstResource.Name;
                    ValueTextBox.Text = firstResource.Value;
                    CommentTextBox.Text = firstResource.Comment;
                    EnglishValueTextBox.Text = firstResource.EnglishValue;
 
                }
            }
        }

        private void NextEntry_Click(object sender, RoutedEventArgs e)
        {
            if (actual_id < 0) return;
            actual_id++;
            IncompleteEntriesList.SelectedIndex = actual_id;

            DisplayCurrentEntry();

        }

        private void PreviousEntry_Click(object sender, RoutedEventArgs e)
        {
            if (actual_id > missingTranslations.Count - 1) return;
            actual_id--;
            IncompleteEntriesList.SelectedIndex = actual_id;

            DisplayCurrentEntry();
        }

        private void DisplayCurrentEntry()
        {
            var Resource = missingTranslations[actual_id];

            NameTextBox.Text = Resource.Name;
            ValueTextBox.Text = Resource.Value;
            CommentTextBox.Text = Resource.Comment;
            EnglishValueTextBox.Text = Resource.EnglishValue;

            EnglishValueTextBox.Text =
            approvedTranslations.TryGetValue(Resource.Name, out var translation)
            ? translation
            : Resource.EnglishValue;
        }

        private void IncompleteEntriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            actual_id = IncompleteEntriesList.SelectedIndex;
            DisplayCurrentEntry();
        }

        private void ApproveAndNext_Click(object sender, RoutedEventArgs e)
        {
            string translation = EnglishValueTextBox.Text.Trim();
            var resource = missingTranslations[actual_id];
            approvedTranslations[resource.Name] = translation;
            if (actual_id < missingTranslations.Count - 1)
            {
                actual_id++;
                IncompleteEntriesList.SelectedIndex = actual_id;
            }
        }
    }
}
