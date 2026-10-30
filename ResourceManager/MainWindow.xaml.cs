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

namespace ResourceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CompleteTranslations_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CompleteTranslationsView();
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}