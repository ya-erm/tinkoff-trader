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

        #region .ctor

        public Trader(string token, bool useSandbox)
        {
            _token = token;
            _useSandbox = useSandbox;
        }

        #endregion

        public async Task Main()
        {
            try
            {
                var context = _useSandbox
                    ? ConnectionFactory.GetSandboxConnection(_token).Context
                    : ConnectionFactory.GetConnection(_token).Context;

                var instrumentsManager = new InstrumentsManager(context);
                await instrumentsManager.InitializeAsync();

                var positionsManager = new PositionsManager(context, instrumentsManager);
                await positionsManager.LoadPositionsAsync();
            }
            catch (Exception exception)
            {
                
            }
        }
    }
}
