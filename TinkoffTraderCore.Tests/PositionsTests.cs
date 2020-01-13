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
        /// <param name="quantity">Количество (для покупки - положительное число, для продажи - отрицательное)</param>
        /// <returns></returns>
        private Operation CreateOperation(MarketInstrument instrument, decimal price, int quantity, decimal? commission = null)
        {
            return new Operation
            ("1",
                OperationStatus.Done,
                new List<Trade>{new Trade("id", DateTime.Now, price, Math.Abs(quantity))}, 
                new MoneyAmount(instrument.Currency, Math.Round(-Math.Abs(quantity) * price * (commission ?? 0.0005M), 2)),
                instrument.Currency,
                -quantity * price,
                price,
                Math.Abs(quantity),
                instrument.Figi,
                InstrumentType.Stock,
                false,
                DateTime.Now,
                quantity > 0 ? ExtendedOperationType.Buy : ExtendedOperationType.Sell
            );
        }

        [TestMethod]
        public void Test1_PositionsProcess()
        {
            var instrument = new MarketInstrument(@"TSLA_FIGI", @"TSLA", "", 0.01M, 1, Currency.Usd, "Tesla");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 478.82M, +1),
                CreateOperation(instrument, 476.70M, +1),
                CreateOperation(instrument, 475.58M, +1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(3, position.Quantity);
            Assert.AreEqual(477.03, Convert.ToDouble(position.AveragePrice), 0.01);

            operations = new List<Operation>
            {
                CreateOperation(instrument, 477.50M, -2),
                CreateOperation(instrument, 478.65M, -1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(0, position.Quantity);
        }


        [TestMethod]
        public void Test2_PositionsProcess_OverZero()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 100, +1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(1, position.Quantity);

            operations = new List<Operation>
            {
                CreateOperation(instrument, 120, -2),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(-1, position.Quantity);

            Assert.AreEqual(120.0, Convert.ToDouble(position.AveragePrice), 0.01);

            Assert.AreEqual(19.89, Convert.ToDouble(position.FixedPnL), 0.01);
        }


        [TestMethod]
        public void Test3_PositionsProcess_OverZero_Short()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 120, -1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(-1, position.Quantity);

            operations = new List<Operation>
            {
                CreateOperation(instrument, 100, 2),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(1, position.Quantity);

            Assert.AreEqual(100.0, Convert.ToDouble(position.AveragePrice), 0.01);

            Assert.AreEqual(19.89, Convert.ToDouble(position.FixedPnL), 0.01);
        }

        [TestMethod]
        public void Test4_PositionsProcess_FixedPnL()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 100, +2),
                CreateOperation(instrument, 120, -1),
            };

            PositionsManager.ProcessOperations(position, operations);
            
            Assert.AreEqual(19.89, Convert.ToDouble(position.FixedPnL), 0.01);
        }

        [TestMethod]
        public void Test4_PositionsProcess_FixedPnL_Short()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 120, -1),
                CreateOperation(instrument, 100, +1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(19.89, Convert.ToDouble(position.FixedPnL), 0.01);
        }


        //[TestMethod]
        //public void Test5_PositionsProcess()
        //{
        //    var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

        //    var position = new Position(instrument, 0);

        //    var operations = new List<Operation>
        //    {
        //        CreateOperation(instrument, 220.49M, +10),
        //        CreateOperation(instrument, 260.37M, -20),
        //    };

        //    PositionsManager.ProcessOperations(position, operations);

        //    Assert.AreEqual(260.37, Convert.ToDouble(position.AveragePrice), 0.01);

        //    operations = new List<Operation>
        //    {
        //        CreateOperation(instrument, 260.95M, -20),
        //    };

        //     PositionsManager.ProcessOperations(position, operations);

        //     Assert.AreEqual(260.76, Convert.ToDouble(position.AveragePrice), 0.01);

        //     operations = new List<Operation>
        //     {
        //         CreateOperation(instrument, 260.5M, +20),
        //     };

        //     PositionsManager.ProcessOperations(position, operations);

        //     Assert.AreEqual(261.27, Convert.ToDouble(position.AveragePrice), 0.01);
        //}

        [TestMethod]
        public void Test6_PositionsProcess_FixedLoss()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 120, +1),
                CreateOperation(instrument, 100, -1),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(-20.11, Convert.ToDouble(position.FixedPnL), 0.01);
        }

        [TestMethod]
        public void Test7_PositionsProcess_FixedLoss_Short()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 100, -1),
                CreateOperation(instrument, 120, +2),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(-20.11, Convert.ToDouble(position.FixedPnL), 0.01);
        }

        [TestMethod]
        public void Test8_PositionsProcess_FixedPnlWithFee()
        {
            var instrument = new MarketInstrument(@"", @"GAZP", "", 0.01M, 1, Currency.Rub, "");

            var position = new Position(instrument, 0);

            var operations = new List<Operation>
            {
                CreateOperation(instrument, 100, +1, 0.1M),
                CreateOperation(instrument, 130, -2, 0.1M),
            };

            PositionsManager.ProcessOperations(position, operations);

            Assert.AreEqual(7, Convert.ToDouble(position.FixedPnL), 0.01);

            Assert.AreEqual(117, Convert.ToDouble(position.AveragePriceCorrected), 0.01);
        }

    }
}
