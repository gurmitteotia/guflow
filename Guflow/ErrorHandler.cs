// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow
{
    internal class ErrorHandler : IErrorHandler
    {
        private readonly IErrorHandler _nextErrorHandler;
        private readonly IErrorHandler _defaultErrorHandler;
       
        public static readonly ErrorHandler NotHandled= new ErrorHandler((e)=>ErrorAction.Unhandled);
        public static readonly ErrorHandler Continue= new ErrorHandler((e)=>ErrorAction.Continue);

        public static ErrorHandler Default(HandleError errorHandler)
        {
            return new ErrorHandler(errorHandler);
        }

        private ErrorHandler(IErrorHandler defaultErrorHandler, IErrorHandler nextErrorHandler)
        {
            _defaultErrorHandler = defaultErrorHandler;
            _nextErrorHandler = nextErrorHandler;
        }
        private ErrorHandler(HandleError defaultErrorHandler): this(new DelgateErrorHandler(defaultErrorHandler), null)
        {
        }

        public ErrorAction OnError(Error error)
        {
            var errorAction = _defaultErrorHandler.OnError(error);

            if (errorAction == ErrorAction.Unhandled && _nextErrorHandler != null)
                return _nextErrorHandler.OnError(error);

            return errorAction;
        }
        public ErrorHandler WithFallback(IErrorHandler errorHandler)
        {
            return new ErrorHandler(_defaultErrorHandler, errorHandler);
        }

        private class DelgateErrorHandler : IErrorHandler
        {
            private readonly HandleError _handleError;

            public DelgateErrorHandler(HandleError handleError)
            {
                _handleError = handleError;
            }

            public ErrorAction OnError(Error error)
            {
                return _handleError(error);
            }
        }
    }
}