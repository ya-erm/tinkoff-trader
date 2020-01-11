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
    }
}
