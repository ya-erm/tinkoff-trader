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
    }
}
