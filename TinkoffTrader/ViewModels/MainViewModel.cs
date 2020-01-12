using System.Windows.Input;
using TinkoffTrader.Utils;
using TinkoffTraderCore;
using TinkoffTraderCore.Modules.Positions;

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

        /// <summary>
        /// Идентификатор инструмента
        /// </summary>
        public string Instrument { get => Get<string>(); set => Set(value); }

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

            PositionsManager.Log = new PrintLogger(WriteLine);

            await trader.Main();
        }

        public  ICommand CalculatePosition => new BaseCommand(CalculatePositionAction);

        private async void CalculatePositionAction()
        {
            var trader = new Trader(LoginViewModel.Token, IsSandboxMode);

            PositionsManager.Log = new PrintLogger(WriteLine);

            await trader.PositionsManager.CalculatePositionByTickerAsync(Instrument);
        }
    }
}
