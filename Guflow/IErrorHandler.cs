// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow
{
    internal interface IErrorHandler
    {
        ErrorAction OnError(Error error);
    }
}