using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffTraderCore.Models;
using TinkoffTraderCore.Storage;

namespace TinkoffTraderCore.Modules.Instruments
{
    /// <summary>
    /// Менеджер инструментов
    /// </summary>
    public class InstrumentsManager : IInstrumentsManager
    {
        #region Fields

        private readonly IContext _context;
        private readonly ApplicationDbContext _dbContext;

        private readonly ConcurrentDictionary<string, MarketInstrument> _instruments;

        #endregion

        #region .ctor

        public InstrumentsManager(IContext context, ApplicationDbContext dbContext)
        {
            _context = context;
            _dbContext = dbContext;

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
            Load();

            if (!_instruments.IsEmpty) return;

            try
            {
                var stocks = await _context.MarketStocksAsync();

                for (var i = 0; i < stocks?.Instruments.Count; i++)
                {
                    var instrument = stocks.Instruments[i];
                    _instruments[instrument.Figi] = instrument;
                }

                var funds = await _context.MarketEtfsAsync();

                for (var i = 0; i < funds?.Instruments.Count; i++)
                {
                    var instrument = funds.Instruments[i];
                    _instruments[instrument.Figi] = instrument;
                }

                var bonds = await _context.MarketBondsAsync();

                for (var i = 0; i < bonds?.Instruments.Count; i++)
                {
                    var instrument = bonds.Instruments[i];
                    _instruments[instrument.Figi] = instrument;
                }

                if (stocks != null && funds != null && bonds != null)
                {
                    Save();
                }
            }
            catch (Exception e)
            {

            }

        }

        private void Save()
        {
            var entities = _instruments.Values.Select(item => Instrument.From(item)).ToList();
            _dbContext.Instruments.AddRange(entities);
            _dbContext.SaveChanges();

            /*
            var lines = new List<string>();

            foreach (var instrument in _instruments.Values)
            {
                lines.Add($"{instrument.Figi};{instrument.Ticker};{instrument.Isin};{instrument.MinPriceIncrement};//{instrument.Lot};{instrument.Currency};{instrument.Name}");
            }

            File.WriteAllLines("instruments.csv", lines);
            */
        }

        private void Load()
        {
            foreach (var item in _dbContext.Instruments.Select(entity => entity.ToMarketInstrument()))
            {
                _instruments[item.Figi] = item;
            }

            /*
            if (!File.Exists("instruments.csv")) return;

            var lines = File.ReadAllLines("instruments.csv");

            for (var i = 0; i < lines.Length; i++)
            {
                var s = lines[i].Split(';');

                var instrument = new MarketInstrument(s[0], s[1], s[2], decimal.Parse(s[3]), int.Parse(s[4]), (Currency)Enum.Parse(typeof(Currency), s[5]), s[6]);

                _instruments[instrument.Figi] = instrument;
            }
            */
        }

    }
}
