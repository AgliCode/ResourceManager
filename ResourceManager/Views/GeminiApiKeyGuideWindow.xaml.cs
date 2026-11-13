using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows;


namespace ResourceManager.Views
{

    public partial class GeminiApiKeyGuideWindow : Window
    {
        public GeminiApiKeyGuideWindow()
        {
            InitializeComponent();

            Loaded += GeminiApiKeyGuideWindow_Loaded;

        }

        private async void GeminiApiKeyGuideWindow_Loaded(
            object sender, RoutedEventArgs e)
        {
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ResourceManager",
                "WebView2"
            );
            try
            {
                Directory.CreateDirectory(userDataFolder);

 
            CoreWebView2Environment environment;

           
                environment = await CoreWebView2Environment.CreateAsync(
                    null,
                    userDataFolder
                );


                await GuideBrowser.EnsureCoreWebView2Async(environment);

                string guidePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "Instruction",
                    "GeminiApiKeyGuide.html"
                );

                GuideBrowser.SetCurrentValue(Microsoft.Web.WebView2.Wpf.WebView2.SourceProperty, new Uri(guidePath));

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                   $"Could not load the Gemini API key guide:\n\n{ex.Message}",
                   "WebView2 Error",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error
               );
            }
            
            
        }

    }
}
