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

namespace ResourceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ProjectContext ProjectContext { get; } = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CompleteTranslations_Click(object sender, RoutedEventArgs e)
        {
            var view = new CompleteTranslationsView(ProjectContext);

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
            var view = new AddNewLanguageView(ProjectContext);

            MainContent.Content = view;
        }
        private async void BrowseResources_Click(object sender, RoutedEventArgs e)
        {
            var view = new BrowseResources(ProjectContext);

            MainContent.Content = view;
        }

        private async void SelectProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder containing Resources.resx files."
            };

            if (dialog.ShowDialog() == true)
            {
                string resourcesDirectory = dialog.FolderName;

                string polishFile = System.IO.Path.Combine(
                    resourcesDirectory, "Resources.resx");

                string englishFile = System.IO.Path.Combine(
                    resourcesDirectory, "Resources.en-US.resx");


                //Security check
                if (!System.IO.File.Exists(polishFile))
                {
                    MessageBox.Show(
                        "The Resources.resx file was not found in the selected folder.",
                        "File Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
                if (!System.IO.File.Exists(englishFile))
                {
                    MessageBox.Show(
                        "The Resources.en-US.resx file was not found in the selected folder.",
                        "File Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
                ProjectContext.SetProject(resourcesDirectory);

                FolderOverlay.Visibility = Visibility.Collapsed;

                var view = new DashboardView(ProjectContext);

                MainContent.Content = view;
                await view.LoadStaticticAsync();
            }
        }

        
    }
}