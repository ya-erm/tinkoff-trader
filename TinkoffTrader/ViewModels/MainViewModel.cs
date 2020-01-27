using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TinkoffTrader.Utils;
using TinkoffTraderCore;
using TinkoffTraderCore.Models;
using TinkoffTraderCore.Modules.Positions;

namespace TinkoffTrader.ViewModels
{
    internal class MainViewModel : ABaseViewModel
    {
        #region Properties
        
        public LoginViewModel LoginViewModel { get; } = new LoginViewModel();

        /// <summary>
        /// True, если включен режим песочницы
        /// </summary>
        public bool UseSandboxMode { get => Get<bool>(true); set => Set(value); }

        /// <summary>
        /// Текст вывода
        /// </summary>
        public string Output { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Идентификатор инструмента
        /// </summary>
        public string Instrument { get => Get<string>(); set => Set(value); }

        public ObservableCollection<Position> Positions { get; } = new ObservableCollection<Position>();

        public ObservableCollection<PositionFill> PositionFills { get; } = new ObservableCollection<PositionFill>();

        public ICommand Start => new BaseCommand(StartAction);

        public ICommand CalculatePosition => new BaseCommand(CalculatePositionAction);

        #endregion

        private readonly Trader _trader = new Trader();

        #region .ctor

        public MainViewModel()
        {
            LoginViewModel.OnTokenLoaded += async(_, token) =>
            {
                PositionsManager.Log = new PrintLogger(WriteLine);

                _trader.Initialize(token, UseSandboxMode);

                await _trader.InstrumentsManager.InitializeAsync();
            };
        }

        #endregion

        public void WriteLine(string text = null)
        {
            if (text != null)
            {
                Output += text;
            }

            Output += "\n";
        }
        
        private async void StartAction()
        {
            await _trader.PositionsManager.LoadPositionsAsync();

            Positions.Clear();

            foreach (var position in _trader.PositionsManager.Positions)
            {
                Positions.Add(position);
            }
        }

        private async void CalculatePositionAction()
        {
            var position = await _trader.PositionsManager.CalculatePositionByTickerAsync(Instrument);

            PositionFills.Clear();

            foreach (var item in position.Fills)
            {
                PositionFills.Add(item);
            }

            PositionFills.Add(new PositionFill() { FixedPnL = position.FixedPnL });
        }

    }
}
