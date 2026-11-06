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
using System.Windows.Shapes;

namespace ResourceManager.Views
{
    /// <summary>
    /// Logika interakcji dla klasy GeminiApiKeyGuideWindow.xaml
    /// </summary>
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
