using System.Threading.Tasks;
using System.Windows.Input;
using TinkoffTraderCore;

namespace TinkoffTrader.ViewModels
{
    internal class MainViewModel : ABaseViewModel
    {
        public LoginViewModel LoginViewModel { get; } = new LoginViewModel();

        /// <summary>
        /// True, если включен режим песочницы
        /// </summary>
        public bool IsSandboxMode { get => Get<bool>(true); set => Set(value); }

        /// <summary>
        /// Текст вывода
        /// </summary>
        public string Output { get => Get<string>(); set => Set(value); }

        public void WriteLine(string text = null)
        {
            if (text != null)
            {
                Output += text;
            }

            Output += "\n";
        }

        public ICommand Start => new BaseCommand(StartAction);

        private async void StartAction()
        {
            var trader = new Trader(LoginViewModel.Token, IsSandboxMode);

            await trader.Main();
        }
    }
}
