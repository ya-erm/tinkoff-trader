using System;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffTraderCore.Modules.Instruments;
using TinkoffTraderCore.Modules.Positions;
using TinkoffTraderCore.Storage;
using TinkoffTraderCore.Utils;

namespace TinkoffTraderCore
{
    public class Trader
    {
        private string _token;
        private bool _useSandbox;

        public InstrumentsManager InstrumentsManager { get; private set; }

        public PositionsManager PositionsManager { get; private set; }

        public ApplicationDbContext DbContext { get; private set; }

        #region .ctor

        public Trader()
        {
            DbContext  = new ApplicationDbContext();
        }

        public void Initialize(string token, bool useSandbox)
        {
            _token = token;
            _useSandbox = useSandbox;

            var context = _useSandbox
                ? ConnectionFactory.GetSandboxConnection(_token).Context
                : ConnectionFactory.GetConnection(_token).Context;

            PositionsManager = new PositionsManager(context, InstrumentsManager);
            InstrumentsManager = new InstrumentsManager(context, DbContext);

            PositionsManager = new PositionsManager(context, DbContext, InstrumentsManager);
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
