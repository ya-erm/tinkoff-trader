using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                operations.Reverse();

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
            var totalFixedPnL = position.FixedPnL;

            var availableOperations = new[]
            {
                ExtendedOperationType.Buy, ExtendedOperationType.BuyCard, ExtendedOperationType.Sell
            };

            foreach (var operation in operations)
            {
                if (!availableOperations.Contains(operation.OperationType)) continue;
                if (operation.Status != OperationStatus.Done) continue;

                var price = operation.Price;
                var payment = operation.Payment;
                var quantity = operation.Trades.Aggregate(0, (res, trade) => res + trade.Quantity);
                var commission = operation.Commission?.Value ?? 0;
                var direction = operation.OperationType == ExtendedOperationType.Buy || 
                                operation.OperationType == ExtendedOperationType.BuyCard ? +1 : -1;

                var paymentCorrected = payment + commission;

                var sumUp = (currentQuantity * (averagePrice ?? 0)) + payment;
                var sumUpCorrected = (currentQuantity * (averagePriceCorrected ?? 0)) + paymentCorrected;

                var nextQuantity = currentQuantity + direction * quantity;

                decimal? fixedPnL = null;

                // Переход через 0
                if (nextQuantity < 0 && currentQuantity > 0 ||
                    nextQuantity > 0 && currentQuantity < 0)
                {
                    var partialPayment = currentQuantity * payment / quantity;
                    var partialPaymentCorrected = currentQuantity * paymentCorrected / quantity;

                    fixedPnL = Math.Sign(currentQuantity) * ( -direction * currentQuantity * (averagePriceCorrected ?? 0) + partialPaymentCorrected);

                    averagePrice = (payment - partialPayment) / (quantity - currentQuantity);
                    averagePriceCorrected = (paymentCorrected - partialPaymentCorrected) / (quantity - currentQuantity);

                    currentQuantity = nextQuantity;
                }
                else
                {
                    if (direction * currentQuantity < 0)
                    {
                        fixedPnL = -direction * quantity * (averagePriceCorrected ?? 0) + paymentCorrected;
                    }

                    currentQuantity = nextQuantity;

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

                totalFixedPnL += (fixedPnL ?? 0);

                var message = $"{position.Instrument.Ticker};\t{direction};\t{quantity};\t{price:F2};\t{currentQuantity};\t{sumUp:F2};\t{averagePrice:F2};\t{sumUpCorrected:F2};\t{averagePriceCorrected:F2};\t{fixedPnL:f2}";
                
                Log.Debug(message);
            }

            position.Quantity = currentQuantity;
            position.AveragePrice = averagePrice;
            position.AveragePriceCorrected = averagePriceCorrected;
            position.FixedPnL = totalFixedPnL;
        }
    }
}
