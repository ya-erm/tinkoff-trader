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
                var position = await CalculatePositionAsync(pos.Figi);

                if (position == null)
                {
                    continue;
                }

                if (position.LotsCount != pos.Lots)
                {
                    Log.Error($"Рассчитанная позиция {position.Instrument.Ticker}: {position.LotsCount} шт., не совпадает с актуальной: {pos.Lots} шт.");
                }

                // TODO: Сохранить позицию в БД

                Positions.Add(position);
            }
        }

        #endregion


        public async Task<Position> CalculatePositionByTickerAsync(string ticker)
        {
            var instrument = await _instrumentsManager.GetInstrumentByTickerAsync(ticker);

            if (instrument == null)
            {
                Log.Error($"Не удалось найти инструмент по названию: {ticker}");

                return null;
            }

            return await CalculatePositionAsync(instrument?.Figi);
        }

        public async Task<Position> CalculatePositionAsync(string figi)
        {
            var instrument = await _instrumentsManager.GetInstrumentAsync(figi);

            if (instrument == null)
            {
                Log.Error($"Не удалось найти инструмент по идентификатору: {figi}");

                return null;
            }

            var position = new Position(instrument, 0);

            // TODO: не хардкодить дату
            var startDate = new DateTime(2019, 1, 1);
            var operations = await _context.OperationsAsync(startDate, DateTime.Now, position.Figi);
            operations.Reverse();

            ProcessOperations(position, operations);

            var currency = position.Instrument.Currency == Currency.Rub ? "₽" : "$";

            Log.Info($"Рассчитанная прибыль c {position.Instrument.Ticker}: {position.FixedPnL:F2} {currency}, позиция: {position.Quantity} шт.");

            return position;
        }

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
                var cost = -operation.Payment;
                var quantity = operation.Trades.Aggregate(0, (res, trade) => res + trade.Quantity);
                var commission = Math.Abs(operation.Commission?.Value ?? 0);
                var direction = operation.OperationType == ExtendedOperationType.Buy || 
                                operation.OperationType == ExtendedOperationType.BuyCard ? +1 : -1;

                var costCorrected = cost + commission;

                var sumUp = currentQuantity * (averagePrice ?? 0) + cost;
                var sumUpCorrected = currentQuantity * (averagePriceCorrected ?? 0) + costCorrected;

                var nextQuantity = currentQuantity + direction * quantity;

                decimal? fixedPnL = null;

                // Переход через 0
                if (nextQuantity < 0 && currentQuantity > 0 ||
                    nextQuantity > 0 && currentQuantity < 0)
                {
                    var proportion = Math.Abs(currentQuantity / (decimal)quantity);

                    var partialCostCorrected = costCorrected * proportion;

                    fixedPnL = Math.Sign(currentQuantity) * direction * (currentQuantity * (averagePriceCorrected ?? 0) + partialCostCorrected);

                    averagePrice = price;
                    averagePriceCorrected = price - commission * (1 - proportion);

                    currentQuantity = nextQuantity;
                }
                else
                {
                    if (direction * currentQuantity < 0)
                    {
                        fixedPnL = direction * quantity * (averagePriceCorrected ?? 0) - costCorrected;

                        currentQuantity = nextQuantity;
                    }
                    else
                    {
                        currentQuantity = nextQuantity;

                        if (currentQuantity != 0)
                        {
                            averagePrice = Math.Abs(sumUp / currentQuantity);
                            averagePriceCorrected = Math.Abs(sumUpCorrected / currentQuantity);
                        }
                    }

                    if(currentQuantity == 0)
                    {
                        averagePrice = null;
                        averagePriceCorrected = null;
                    }
                }

                totalFixedPnL += (fixedPnL ?? 0);

                var plus = direction > 0 ? "+" : "";
                var message = $"{position.Instrument.Ticker};\t{operation.Date:G};\t{price:F2};\t{plus}{direction*quantity};\t{plus}{cost:F2};\t{currentQuantity};\t{sumUp:F2};\t{averagePrice:F2};\t{sumUpCorrected:F2};\t{averagePriceCorrected:F2};\t{fixedPnL:f2}";
                
                Log.Debug(message);
            }

            position.Quantity = currentQuantity;
            position.AveragePrice = averagePrice;
            position.AveragePriceCorrected = averagePriceCorrected;
            position.FixedPnL = totalFixedPnL;
        }
    }
}
