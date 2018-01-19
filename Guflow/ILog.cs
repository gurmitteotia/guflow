// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow
{
    public interface ILog
    {
        void Info(string message);
        void Info(string message, Exception exception);

        void Debug(string message);
        void Debug(string message, Exception exception);

        void Warn(string message);
        void Warn(string message, Exception exception);

        void Error(string message);
        void Error(string message, Exception exception);

        void Fatal(string message);
        void Fatal(string message, Exception exception);
    }
}