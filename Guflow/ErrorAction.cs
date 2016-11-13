namespace Guflow
{
    public class ErrorAction
    {
        private static readonly ErrorAction _retryAction = new ErrorAction();
        private static readonly ErrorAction _unhandledAction = new ErrorAction();
        private static readonly ErrorAction _continueAction = new ErrorAction();

        private ErrorAction()
        {
        }
        public static ErrorAction Retry { get { return _retryAction; } }
        public static ErrorAction Unhandled { get { return _unhandledAction; } }
        public static ErrorAction Continue { get{ return _continueAction;} }
    }
}