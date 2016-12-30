using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public class ActivityResponse
    {
        public static readonly ActivityResponse Defferred = new ActivityResponse();

        public async void SendAsync(IAmazonSimpleWorkflow simpleWorkflow)
        {
            
        }

        public static ActivityResponse Complete(string result)
        {
            return new CompleteResponse(result);
        }
        public static ActivityResponse Fail(string reason, string details)
        {
            return new FailResponse(reason, details);
        }

        private sealed class FailResponse : ActivityResponse
        {
            private bool Equals(FailResponse other)
            {
                return string.Equals(_reason, other._reason) && string.Equals(_details, other._details);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is FailResponse && Equals((FailResponse) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_reason != null ? _reason.GetHashCode() : 0) * 397) ^ (_details != null ? _details.GetHashCode() : 0);
                }
            }

            private readonly string _reason;
            private readonly string _details;

            public FailResponse(string reason, string details)
            {
                _reason = reason;
                _details = details;
            }
        }

        private sealed class CompleteResponse : ActivityResponse
        {
            private readonly string _result;

            public CompleteResponse(string result)
            {
                _result = result;
            }
            private bool Equals(CompleteResponse other)
            {
                return string.Equals(_result, other._result);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((CompleteResponse)obj);
            }

            public override int GetHashCode()
            {
                return (_result != null ? _result.GetHashCode() : 0);
            }
        }
    }
}