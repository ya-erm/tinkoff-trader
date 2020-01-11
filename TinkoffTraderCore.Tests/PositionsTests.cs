using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffTraderCore.Models;
using TinkoffTraderCore.Modules.Instruments;
using TinkoffTraderCore.Modules.Positions;

namespace TinkoffTraderCore.Tests
{
    [TestClass]
    public class PositionsTests
    {
        /// <summary>
        /// Создать операцию
        /// </summary>
        /// <param name="instrument">Инструмент</param>
        /// <param name="price">Цена одного инструмента</param>
        /// <param name="quantity">Количество (для покупки - отрицательное число, для продажи - положительное)</param>
        /// <returns></returns>
        private Operation CreateOperation(MarketInstrument instrument, decimal price, int quantity)
        {
            return new Operation
            ("1",
                OperationStatus.Done,
                null,
                new MoneyAmount(instrument.Currency,  Math.Round(price * Math.Abs(quantity) * 0.00025M, 2)),
                instrument.Currency,
                price * quantity,
                price,
                Math.Abs(quantity),
                instrument.Figi,
                InstrumentType.Stock,
                false,
                DateTime.Now,
                quantity < 0 ? ExtendedOperationType.Buy : ExtendedOperationType.Sell
            );
        }

        [TestMethod]
        public void Test1_PositionsProcess()
        {
            var instrument = new MarketInstrument(@"TSLA_FIGI", @"TSLA", "", 0.01M, 1, Currency.Usd, "Tesla");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 478.82M, -1),
                CreateOperation(instrument, 476.70M, -1),
                CreateOperation(instrument, 475.58M, -1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(3, position.Quantity);
            Assert.AreEqual(-477.03, Convert.ToDouble(position.AveragePrice), 0.01);

            operations = new List<Operation>
            {
                CreateOperation(instrument, 477.50M, +2),
                CreateOperation(instrument, 478.65M, +1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(0, position.Quantity);

        }
    }
}
