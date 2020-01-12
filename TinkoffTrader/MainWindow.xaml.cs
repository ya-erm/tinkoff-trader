using System.Windows;
using TinkoffTrader.ViewModels;
using TinkoffTraderCore;

namespace TinkoffTrader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private  MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
