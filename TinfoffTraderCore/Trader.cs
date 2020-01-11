using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffTraderCore.Modules.Instruments;
using TinkoffTraderCore.Modules.Positions;

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
            var context = _useSandbox
                ? ConnectionFactory.GetSandboxConnection(_token).Context
                : ConnectionFactory.GetConnection(_token).Context;

            var instrumentsManager = new InstrumentsManager(context);
            await instrumentsManager.InitializeAsync();

            var positionsManager = new PositionsManager(context, instrumentsManager);
            await positionsManager.LoadPositionsAsync();
        }
    }
}
