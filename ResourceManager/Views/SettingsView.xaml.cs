using ResourceManager.Services;
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
    /// Logika interakcji dla klasy SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly ProjectContext _projectContext;
        private readonly GeminiKeyStorage _geminiKeyStorage;
        private readonly MainWindow _mainWindow;
        public SettingsView(ProjectContext projectContext,GeminiKeyStorage geminiKeyStorage, MainWindow mainWindow)
        {
            InitializeComponent();
            _projectContext = projectContext;
            _geminiKeyStorage = geminiKeyStorage;
            _mainWindow = mainWindow;

            BaseFileComboBox.ItemsSource = _projectContext.ResxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();
            BaseFileComboBox.SelectedIndex = _projectContext.ResxFiles.ToList().FindIndex(path => path == _projectContext.BaseFilePath);

            TargetFileComboBox.ItemsSource = _projectContext.ResxFiles
                .Select(System.IO.Path.GetFileName)
                .ToList();
            TargetFileComboBox.SelectedIndex = _projectContext.ResxFiles.ToList().FindIndex(path => path == _projectContext.TargetFilePath);

            TargetLanguageComboBox.SelectedIndex = TargetLanguageComboBox.Items
                .Cast<ComboBoxItem>()
                .ToList()
                .FindIndex(item => item.Tag?.ToString() == _projectContext.TargetLanguage);

            if (string.IsNullOrWhiteSpace(_geminiKeyStorage.ApiKey))
            {
                ApiKeyStatusBadge.Text = "Not configured";
                ApiKeyStatusBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fc3838"));
            }
            else
            {
                ApiKeyStatusBadge.Text = "Configured";
                ApiKeyStatusBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3bfc38"));

            }
            ProjectFolderTextBox.Text = _projectContext.ProjectDirectory;
            _mainWindow = mainWindow;
        }

        public void Change_Click(object sender, EventArgs e)
        {
            _mainWindow.ShowFolderConfiguration();
        }
    }
}
