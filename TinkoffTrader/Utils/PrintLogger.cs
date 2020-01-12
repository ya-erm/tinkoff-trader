using System;
using TinkoffTraderCore.Utils;

namespace TinkoffTrader.Utils
{
    internal class PrintLogger : ILogger
    {
        private readonly Action<string> _action;

        #region .ctor

        public PrintLogger(Action<string> action)
        {
            _action = action;
        }

        #endregion

        #region Implementation of ILogger

        public void Debug(string message)
        {
            _action.Invoke(message);
        }

        public void Info(string message)
        {
            _action.Invoke(message);
        }

        public void Warn(string message)
        {
            _action.Invoke(message);
        }

        public void Error(string message)
        {
            _action.Invoke(message);
        }

        #endregion
    }
}
