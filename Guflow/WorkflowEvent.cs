namespace Guflow
{
    public abstract class WorkflowEvent
    {
        private readonly long _eventId;
        protected WorkflowEvent(long eventId)
        {
            _eventId = eventId;
        }

        internal abstract WorkflowAction Interpret(IWorkflow workflow);
        
        public bool IsNewerThan(WorkflowEvent otherWorkflowEvent)
        {
            return _eventId > otherWorkflowEvent._eventId;
        }

        public override string ToString()
        {
            return string.Format("{0} with event id {1}", GetType().Name, _eventId);
        }
        private bool Equals(WorkflowEvent other)
        {
            return _eventId == other._eventId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WorkflowEvent)obj);
        }

        public override int GetHashCode()
        {
            return _eventId.GetHashCode();
        }

        public static bool operator ==(WorkflowEvent left, WorkflowEvent right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WorkflowEvent left, WorkflowEvent right)
        {
            return !Equals(left, right);
        }
    }
}