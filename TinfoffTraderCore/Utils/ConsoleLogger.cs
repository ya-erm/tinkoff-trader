using System;

namespace TinkoffTraderCore.Utils
{
    public class ConsoleLogger : ILogger
    {
        public static ConsoleLogger Default { get; } = new ConsoleLogger();

        #region Implementation of ILogger

        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Warn(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(string message)
        {
            Console.WriteLine(message);
        }

        #endregion
    }
}
