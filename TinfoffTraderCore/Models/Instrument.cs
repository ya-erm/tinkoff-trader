using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffTraderCore.Models
{
    /// <summary>
    /// Инструмент торговли
    /// </summary>
    public class Instrument
    {
        public string Figi { get; set; }

        [Key]
        public string Tiker { get; set; }

        public decimal PriceStep { get; set; }

        public int LotSize { get; set; }

        public string Currency { get; set; }

        public string Name { get; set; }

        
        #region Converters

        public static Instrument From(MarketInstrument instrument)
        {
            return new Instrument
            {
                Figi = instrument.Figi,
                Tiker = instrument.Ticker,
                PriceStep = instrument.MinPriceIncrement,
                LotSize = instrument.Lot,
                Currency = instrument.Currency.ToString(),
                Name = instrument.Name
            };
        }

        public MarketInstrument ToMarketInstrument()
        {
            return new MarketInstrument
            (
                Figi,
                Tiker,
                string.Empty,
                PriceStep,
                LotSize,
                (Currency)Enum.Parse(typeof(Currency), Currency),
                Name
            );
        }

        #endregion
    }
}
