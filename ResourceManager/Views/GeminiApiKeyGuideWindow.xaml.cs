using System.Windows;


namespace ResourceManager.Views
{

    public partial class GeminiApiKeyGuideWindow : Window
    {
        public GeminiApiKeyGuideWindow()
        {
            InitializeComponent();

            string guidePath = System.IO.Path.Combine(
           AppContext.BaseDirectory,
           "Instruction","GeminiApiKeyGuide.html");
            Console.WriteLine(guidePath);
            GuideBrowser.Source = new Uri(guidePath);
        }
    }
}
