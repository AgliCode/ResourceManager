using ResourceManager.Core.Services;
using System.Windows;
using System.Windows.Controls;


namespace ResourceManager.Views
{

    public partial class BrowseResources : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly ResourceReader _resourceReader = new();
        public BrowseResources(ProjectContext projectContext)
        {
            InitializeComponent();
            _projectContext = projectContext;

            ResourceFileComboBox.ItemsSource = _projectContext.ResxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();
            ResourceFileComboBox.SelectedIndex = 0;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? selectedFileName = ResourceFileComboBox.SelectedItem as string;
                string? selectedFilePath = _projectContext.ResxFiles
                    .FirstOrDefault(path => System.IO.Path.GetFileName(path) == selectedFileName);
                var resources = _resourceReader.ReadResources(selectedFilePath);
                ResourcesDataGrid.ItemsSource = resources;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                   $"Could not load resources: {ex.Message}",
                   "Error",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error);
            }
        }
    }
}
