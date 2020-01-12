using System;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffTraderCore.Modules.Instruments;
using TinkoffTraderCore.Modules.Positions;
using TinkoffTraderCore.Utils;

namespace TinkoffTraderCore
{
    public class Trader
    {
        private readonly string _token;
        private readonly bool _useSandbox;

        public InstrumentsManager InstrumentsManager { get; }

        public PositionsManager PositionsManager { get; }

        #region .ctor

        public Trader(string token, bool useSandbox)
        {
            _token = token;
            _useSandbox = useSandbox;

            var context = _useSandbox
                ? ConnectionFactory.GetSandboxConnection(_token).Context
                : ConnectionFactory.GetConnection(_token).Context;
            
            InstrumentsManager = new InstrumentsManager(context);

            PositionsManager = new PositionsManager(context, InstrumentsManager);
        }

        #endregion

        public async Task Main()
        {
            try
            {
                await InstrumentsManager.InitializeAsync();
                await PositionsManager.LoadPositionsAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{exception.Message}\n{exception.StackTrace}");
            }
        }

    }
}
