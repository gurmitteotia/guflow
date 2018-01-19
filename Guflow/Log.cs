// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow
{
    /// <summary>
    /// Allows you to configure logging.
    /// </summary>
    public class Log
    {
        private static readonly ILog NullLog = new NullLog();
        public static readonly Func<Type, ILog> NullLogger = t => NullLog;
        public static readonly Func<Type, ILog> ConsoleLogger = t => new ConsoleLog(t.Name);
        private static Func<Type, ILog> _logFactory = NullLogger;
      
        internal static ILog GetLogger<T>()
        {
            var log = _logFactory(typeof(T));
            log = log ?? NullLog;

            return log;
        }
        /// <summary>
        /// Register your custom logger to be used by Guflow.
        /// </summary>
        /// <param name="logFactoryFunc"></param>
        public static void Register(Func<Type, ILog> logFactoryFunc)
        {
            Ensure.NotNull(logFactoryFunc, "logFactoryFunc");
            _logFactory = logFactoryFunc;
        }
    }
}