using System.Windows.Controls;
using System.Windows.Input;
using TinkoffTrader.ViewModels;

namespace TinkoffTrader.Views
{
    public partial class LoginComponent : UserControl
    {
        public LoginComponent()
        {
            InitializeComponent();
        }

        private void LoginComponent_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (DataContext as LoginViewModel)?.Command.Execute(PasswordBox);
            }
        }
    }
}
