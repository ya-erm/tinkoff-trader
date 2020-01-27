namespace TinkoffTraderCore.Models
{
    public class PositionFill : Fill
    {
        /// <summary>
        /// Количество активов после совершения сделки
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// Суммарная стоимость активов
        /// </summary>
        public decimal SumUp { get; set; }

        /// <summary>
        /// Средняя цена актива
        /// </summary>
        public decimal? AveragePrice { get; set; }

        /// <summary>
        /// Суммарная стоимость активов с учётом комиссии
        /// </summary>
        public decimal SumUpCorrected { get; set; }

        /// <summary>
        /// Средняя цена актива с учётом комиссии
        /// </summary>
        public decimal? AveragePriceCorrected { get; set; }

        /// <summary>
        /// Зафиксированная прибыль или убыток
        /// </summary>
        public decimal? FixedPnL { get; set; }

    }
}
