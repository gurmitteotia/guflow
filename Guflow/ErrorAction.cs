namespace Guflow
{
    public class ErrorAction
    {
        private static readonly ErrorAction _retryAction = new ErrorAction(rethrow:false, retry:true);
        private static readonly ErrorAction _unhandledAction = new ErrorAction(rethrow:true, retry:false);
        private static readonly ErrorAction _continueAction = new ErrorAction(rethrow:false, retry:false);

        private ErrorAction(bool rethrow, bool retry)
        {
            IsRethrow = rethrow;
            IsRetry = retry;
        }

        public static ErrorAction Retry { get { return _retryAction; } }
        public static ErrorAction Unhandled { get { return _unhandledAction; } }
        public static ErrorAction Continue { get { return _continueAction; } }
        
        internal bool IsRethrow { get; private set; }
        internal bool IsRetry { get; private set; }
    }

   
}