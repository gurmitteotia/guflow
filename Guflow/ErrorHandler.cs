namespace Guflow
{
    internal class ErrorHandler : IErrorHandler
    {
        private readonly IErrorHandler _nextErrorHandler;
        private readonly HandleError _defaultErrorHandler;
       
        public static readonly ErrorHandler NotHandled= new ErrorHandler((e)=>ErrorAction.Unhandled, null);

        public static ErrorHandler Default(HandleError errorHandler)
        {
            return new ErrorHandler(errorHandler, null);
        }

        private ErrorHandler(HandleError defaultErrorHandler, IErrorHandler nextErrorHandler)
        {
            _defaultErrorHandler = defaultErrorHandler;
            _nextErrorHandler = nextErrorHandler;
        }

        public ErrorAction OnError(Error error)
        {
            var errorAction = _defaultErrorHandler(error);

            if (errorAction == ErrorAction.Unhandled && _nextErrorHandler != null)
                return _nextErrorHandler.OnError(error);

            return errorAction;
        }
        public ErrorHandler WithFallback(IErrorHandler errorHandler)
        {
            return new ErrorHandler(_defaultErrorHandler, errorHandler);
        }
    }
}