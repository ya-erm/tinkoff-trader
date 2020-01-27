using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffTraderCore.Modules.Instruments
{
    /// <summary>
    /// Интерфейс менеджера инструментов
    /// </summary>
    public interface IInstrumentsManager
    {
        /// <summary>
        /// Получить инструмент по его идентификатору
        /// </summary>
        /// <param name="figi">Идентификатор инструмента</param>
        /// <returns></returns>
        Task<MarketInstrument> GetInstrumentAsync(string figi);

        /// <summary>
        /// Получить инструмент по его аббревиатуре
        /// </summary>
        /// <param name="ticker">Аббревиатура инструмента</param>
        /// <returns></returns>
        Task<MarketInstrument> GetInstrumentByTickerAsync(string ticker);
    }
}
