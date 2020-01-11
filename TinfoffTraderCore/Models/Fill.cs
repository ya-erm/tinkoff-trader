using System;

namespace TinkoffTraderCore.Models
{
    /// <summary>
    /// Сделка
    /// </summary>
    public class Fill
    {
        /// <summary>
        /// Дата и время совершения сделки
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Актив
        /// </summary>
        public string Figi { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Объём сделки
        /// </summary>
        public int Count { get; set; }
    }
}
