using System.Windows;
using UrlVideoConverter.ViewsModels;

namespace UrlVideoConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindow_VM();
        }
    }
}