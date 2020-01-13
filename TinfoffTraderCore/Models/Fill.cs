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
        public DateTime Date { get; set; }

        /// <summary>
        /// Идентификатор инструмента
        /// </summary>
        public string Figi { get; set; }

        /// <summary>
        /// Цена одной единицы
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Объём сделки, количество активов.
        /// Положительное число - покупка,
        /// Отрицательное - продажа
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Направление сделки
        /// </summary>
        public FillDirection Direction => Count > 0
            ? FillDirection.Buy
            : FillDirection.Sell;
        
        /// <summary>
        /// Стоимость сделки
        /// </summary>
        public decimal Cost => Price * Count;
        
        /// <summary>
        /// Комиссия
        /// </summary>
        public decimal Commission { get; set; }
    }

    public enum FillDirection
    {
        Buy,
        Sell,
    }
}
