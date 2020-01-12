using System.Collections.Concurrent;
using System.IO.IsolatedStorage;
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

            instrument = await _context.MarketSearchByFigiAsync(figi);

            if (instrument != null)
            {
                _instruments[figi] = instrument;
            }

            return instrument;
        }

        /// <summary>
        /// Получить инструмент по его аббревиатуре
        /// </summary>
        /// <param name="ticker">Аббревиатура инструмента</param>
        /// <returns></returns>
        public async Task<MarketInstrument> GetInstrumentByTickerAsync(string ticker)
        {
            var instrument = _instruments.Values.FirstOrDefault(_ => _.Ticker == ticker);
            
            if (instrument != null)
            {
                return instrument;
            }

            var response = await _context.MarketSearchByTickerAsync(ticker);

            instrument = response.Instruments?.FirstOrDefault();

            if (instrument != null)
            {
                _instruments[instrument.Figi] = instrument;
            }

            return instrument;
        }

        #endregion

        /// <summary>
        /// Инициализация менеджера инструментов
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            var stocks = await _context.MarketStocksAsync();

            foreach(var instrument in stocks.Instruments)
            {
                _instruments[instrument.Figi] = instrument;
            }

            var funds = await _context.MarketEtfsAsync();

            foreach (var instrument in funds.Instruments)
            {
                _instruments[instrument.Figi] = instrument;
            }

            var bonds = await _context.MarketBondsAsync();

            foreach (var instrument in bonds.Instruments)
            {
                _instruments[instrument.Figi] = instrument;
            }

        }

    }
}
