using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffTraderCore.Models;
using TinkoffTraderCore.Modules.Instruments;
using TinkoffTraderCore.Utils;

[assembly: InternalsVisibleTo("TinkoffTraderCore.Tests")]
namespace TinkoffTraderCore.Modules.Positions
{
    /// <summary>
    /// Менеджер позиций
    /// </summary>
    public class PositionsManager : IPositionsManager
    {
        private readonly IContext _context;
        private readonly IInstrumentsManager _instrumentsManager;

        public static ILogger Log { get; set; } = ConsoleLogger.Default;

        #region .ctor

        public PositionsManager(IContext context, IInstrumentsManager instrumentsManager)
        {
            _context = context;
            _instrumentsManager = instrumentsManager;
        }

        #endregion

        #region Implementation of IPositionsManager

        public ObservableCollection<Position> Positions { get; } = new ObservableCollection<Position>();

        public async Task LoadPositionsAsync()
        {
            var portfolio = await _context.PortfolioAsync();

            foreach (var pos in portfolio.Positions)
            {
                var instrument = await _instrumentsManager.GetInstrumentAsync(pos.Figi);

                if (instrument == null)
                {
                    Log.Error($"Не удалось найти инструмент по идентификатору: {pos.Figi}");

                    continue;
                }

                var position = new Position(instrument, 0);

                // TODO: не хардкодить дату
                var startDate = new DateTime(2019, 1, 1);
                var operations = await _context.OperationsAsync(startDate, DateTime.Now, position.Figi);

                ProcessOperations(position, operations);

                if (position.LotsCount != pos.Lots)
                {
                    Log.Error($"Рассчитанная позиция {instrument.Ticker}: {position.LotsCount} шт., не совпадает с актуальной: {pos.Lots} шт.");
                }

                // TODO: Сохранить позицию в БД

                Positions.Add(position);
            }
        }

        #endregion

        /// <summary>
        /// Рассчитать позицию на основе новых сделок
        /// </summary>
        /// <param name="position">Позиция с её текущим состоянием</param>
        /// <param name="operations">Список новых операций</param>
        internal static void ProcessOperations(Position position, IEnumerable<Operation> operations)
        {
            var currentQuantity = position.Quantity;

            var averagePrice = position.AveragePrice;
            var averagePriceCorrected = position.AveragePriceCorrected;

            foreach (var operation in operations)
            {
                if (operation.OperationType != ExtendedOperationType.Buy &&
                    operation.OperationType != ExtendedOperationType.Sell)
                    continue;

                var price = operation.Price;
                var payment = operation.Payment;
                var quantity = operation.Quantity;
                var commission = operation.Commission?.Value ?? 0;
                var direction = operation.OperationType == ExtendedOperationType.Buy ? +1 : -1;

                var paymentCorrected = payment + commission;

                var sumUp = (currentQuantity * (averagePrice ?? 0)) + payment;
                var sumUpCorrected = (currentQuantity * (averagePriceCorrected ?? 0)) + paymentCorrected;

                var nextQuantity = currentQuantity + direction * quantity;
                
                // Переход через 0
                if (nextQuantity * currentQuantity < 0)
                {
                    var partialPayment = currentQuantity * payment / quantity;
                    var partialPaymentCorrected = currentQuantity * paymentCorrected / quantity;

                    averagePrice = (payment - partialPayment) / (quantity - currentQuantity);
                    averagePriceCorrected = (paymentCorrected - partialPaymentCorrected) / (quantity - currentQuantity);

                    currentQuantity = nextQuantity;
                }
                else
                {
                    currentQuantity += direction * quantity;

                    if (currentQuantity != 0)
                    {
                        averagePrice = sumUp / currentQuantity;
                        averagePriceCorrected = sumUpCorrected / currentQuantity;
                    }
                    else
                    {
                        averagePrice = null;
                        averagePriceCorrected = null;
                    }
                }

                Log.Debug($"{position.Instrument.Ticker}: {direction*quantity}x{price:F2}={payment:F2} --- C:{currentQuantity} --- S:{sumUp:F2} A:{averagePrice:F2} --- S:{sumUpCorrected:F2} A:{averagePriceCorrected:F2}");

            }

            position.Quantity = currentQuantity;
            position.AveragePrice = averagePrice;
            position.AveragePriceCorrected = averagePriceCorrected;
        }
    }
}
