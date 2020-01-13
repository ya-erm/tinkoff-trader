using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffTraderCore.Models
{
    /// <summary>
    /// Позиция
    /// </summary>
    [DebuggerDisplay("{Instrument.Ticker} - {LotsCount}x{Instrument.Lot}={Quantity} - {AveragePrice}")]
    public class Position
    {
        #region .ctor

        public Position(MarketInstrument instrument, int lotsCount)
        {
            Instrument = instrument;
            LotsCount = lotsCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Инструмент торговли (например, акция)
        /// </summary>
        public MarketInstrument Instrument { get; }

        /// <summary>
        /// Идентификатор инструмента
        /// </summary>
        public string Figi => Instrument.Figi;

        /// <summary>
        /// Количество лотов
        /// </summary>
        public int LotsCount { get; set; }

        /// <summary>
        /// Количество акций (кратно размеру лота)
        /// </summary>
        public int Quantity
        {
            get => LotsCount * Instrument.Lot;
            set => LotsCount = value / Instrument.Lot;
        }

        /// <summary>
        /// Средняя цена акции
        /// </summary>
        public decimal? AveragePrice { get; set; }

        /// <summary>
        /// Средняя цена акции с учётом комиссии
        /// </summary>
        public decimal? AveragePriceCorrected { get; set; }

        /// <summary>
        /// Время последнего изменения
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// Зафиксированная прибыль или убыток
        /// </summary>
        public decimal FixedPnL { get; set; }

        /// <summary>
        /// Список сделок по данной позиции
        /// </summary>
        public List<PositionFill> Fills { get; set; } = new List<PositionFill>();

        #endregion
    }
}
