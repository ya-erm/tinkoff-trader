using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using static Tinkoff.Trading.OpenApi.Models.StreamingRequest;

namespace TinkoffTraderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            new Task(StartProccess).Start();

            while (true)
            {

            }
        }

        static async void StartProccess()
        {

            // токен аутентификации в песочнице
            var token = "";

            try
            {
                // для работы в песочнице используйте GetSandboxConnection
                var connection = ConnectionFactory.GetSandboxConnection(token);
                //var connection = ConnectionFactory.GetConnection(token);
                var context = connection.Context;

                // вся работа происходит асинхронно через объект контекста
                var portfolio = await context.PortfolioAsync();

                Console.WriteLine($"Portfolio positions");
                foreach (var position in portfolio.Positions)
                {
                    Console.WriteLine($"{position.Figi} - Balance: {position.Balance} ({position.Blocked} blocked), Lots: {position.Lots}");
                }

                var stoks = await context.MarketStocksAsync();

                using (var file = File.Open("instruments.csv", FileMode.OpenOrCreate))
                using (var writer = new StreamWriter(file))
                {
                    writer.WriteLine("Figi; Currency; Lot; MinPriceIncrement; Ticker; Isin;");
                    for (var i = 0; i < stoks.Instruments.Count; i++)
                    {
                        var item = stoks.Instruments[i];
                        writer.WriteLine($"{item.Figi}; {item.Currency}; {item.Lot}; {item.MinPriceIncrement}; {item.Ticker}; {item.Isin};");
                    }
                }

                var russianInstruments = stoks.Instruments.Where(instrument => instrument.Currency == Currency.Rub);
                //var asset = russianInstruments.FirstOrDefault(x => x.Ticker == "SNGS");

                var asset = stoks.Instruments.FirstOrDefault(x => x.Ticker == "TWTR");

                await context.SendStreamingRequestAsync<CandleSubscribeRequest>(new CandleSubscribeRequest(asset.Figi, CandleInterval.Minute));

                context.StreamingEventReceived += Context_StreamingEventReceived;

                // задаём свой баланс в песочнице
                await context.SetCurrencyBalanceAsync(Currency.Rub, 100000);

                // регистрируемся
                await context.RegisterAsync();
                
                //выставляем заявку
                //var placedOrder = await context.PlaceLimitOrderAsync(new LimitOrder(asset.Figi, 1, OperationType.Buy, 50));
                //Console.WriteLine($"{placedOrder.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void Context_StreamingEventReceived(object sender, StreamingEventReceivedEventArgs e)
        {
            Console.Write($"{e.Response.Event} ");

            if (e.Response is CandleResponse response)
            {
                var candle = response.Payload;

                Console.WriteLine($"{DateTime.Now:HH:MM:ss.fff} - L:{candle.Low:0.00}; H:{candle.High:0.00}; O:{candle.Open:0.00}; C:{candle.Close:0.00}; T:{candle.Time.ToLocalTime():HH:mm}");
            }

        }

        private async Task Method1(IContext context)
        {

            var portfolio = await context.PortfolioAsync();

            //var order = new Tinkoff.Trading.OpenApi.Models.LimitOrder("BBG004730N88", 1, Tinkoff.Trading.OpenApi.Models.OperationType.Buy, 1000);
            //var placedOrder = await context.PlaceLimitOrderAsync(order);

            var instrument = portfolio.Positions[2].Figi;

            var operations = await context.OperationsAsync(new DateTime(2019, 1, 1, 1, 1, 1), DateTime.Now, instrument);

        }
    }
}