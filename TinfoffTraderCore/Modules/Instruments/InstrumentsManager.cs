using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffTraderCore.Modules.Instruments
{
    /// <summary>
    /// Менеджер инструментов
    /// </summary>
    public class InstrumentsManager : IInstrumentsManager
    {
        private readonly IContext _context;

        private readonly ConcurrentDictionary<string, MarketInstrument> _instruments;

        #region .ctor

        public InstrumentsManager(IContext context)
        {
            _context = context;
            _instruments = new ConcurrentDictionary<string, MarketInstrument>();
        }

        #endregion

        #region Implementation of IInstrumentsManager

        /// <summary>
        /// Получить инструмент по его идентификатору
        /// </summary>
        /// <param name="figi">Идентификатор инструмента</param>
        /// <returns></returns>
        public async Task<MarketInstrument> GetInstrumentAsync(string figi)
        {
            if (_instruments.TryGetValue(figi, out var instrument))
            {
                return instrument;
            }

            var response = await _context.MarketSearchByFigiAsync(figi);

            instrument = response.Instruments.FirstOrDefault();

            _instruments[figi] = instrument;

            return instrument;
        }

        #endregion

        /// <summary>
        /// Инициализация менеджера инструментов
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            var response = await _context.MarketStocksAsync();

            foreach(var instrument in response.Instruments)
            {
                _instruments[instrument.Figi] = instrument;
            }
        }

    }
}
