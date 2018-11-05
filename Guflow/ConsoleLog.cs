// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow
{
    internal class ConsoleLog : ILog
    {
        private readonly string _typeName;
        private const string INFO = "INFO";
        private const string DEBUG = "DEBUG";
        private const string WARN = "WARN";
        private const string ERROR = "ERROR";
        private const string FATAL = "FATAL";
        
        public ConsoleLog(string typeName)
        {
            _typeName = typeName;
        }

        public void Info(string message)
        {
            Console.WriteLine(FormatMessage(INFO, message));
        }

        public void Info(string message, Exception exception)
        {
            Console.WriteLine(FormatMessage(INFO, message, exception));
        }

        public void Debug(string message)
        {
            Console.WriteLine(FormatMessage(DEBUG, message));
        }

        public void Debug(string message, Exception exception)
        {
            Console.WriteLine(FormatMessage(DEBUG, message, exception));
        }

        public void Warn(string message)
        {
            Console.WriteLine(FormatMessage(WARN, message));
        }

        public void Warn(string message, Exception exception)
        {
            Console.WriteLine(FormatMessage(WARN, message, exception));
        }

        public void Error(string message)
        {
            Console.WriteLine(FormatMessage(ERROR, message));
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine(FormatMessage(ERROR, message, exception));
        }

        public void Fatal(string message)
        {
            Console.WriteLine(FormatMessage(FATAL, message));
        }

        public void Fatal(string message, Exception exception)
        {
            Console.WriteLine(FormatMessage(FATAL, message, exception));
        }

        private string FormatMessage(string level, string message)
        {
            return string.Format("{0} {1} {2}- {3} \r\n", DateTime.UtcNow, level, _typeName, message);
        }
        private string FormatMessage(string level, string message, Exception exception)
        {
            return string.Format("{0} {1} {2}- {3} {4}\r\n", DateTime.UtcNow, level, _typeName, message, exception);
        }
    }
}