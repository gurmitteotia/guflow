using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent a event in workflow history.
    /// </summary>
    public abstract class WorkflowEvent : IComparable<WorkflowEvent>
    {
        private readonly long _eventId;
        protected WorkflowEvent(long eventId)
        {
            _eventId = eventId;
        }

        internal virtual WorkflowAction Interpret(IWorkflow workflow)
        {
            throw new NotSupportedException($"Can not interpret {this.GetType().Name}.");
        }

        internal virtual WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            throw new NotSupportedException($"DefaultAction is not supported {this.GetType().Name}.");
        }
        public static readonly IComparer<WorkflowEvent> IdComparer = new EventIdComparer();
    
        public int CompareTo(WorkflowEvent other)
        {
            if (other == null)
                return 1;
            return _eventId.CompareTo(other._eventId);
        }

        public override string ToString()
        {
            return string.Format("{0} with event id {1}", GetType().Name, _eventId);
        }
        private bool Equals(WorkflowEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            return _eventId == other._eventId;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return Equals((WorkflowEvent)other);
        }

        public override int GetHashCode()
        {
            return _eventId.GetHashCode();
        }

        public static bool operator ==(WorkflowEvent left, WorkflowEvent right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator >=(WorkflowEvent left, WorkflowEvent right)
        {
            return IdComparer.Compare(left,right) >= 0;
        }
        public static bool operator >(WorkflowEvent left, WorkflowEvent right)
        {
            return IdComparer.Compare(left, right) > 0;
        }
        public static bool operator <=(WorkflowEvent left, WorkflowEvent right)
        {
            return IdComparer.Compare(left, right) <= 0;
        }
        public static bool operator <(WorkflowEvent left, WorkflowEvent right)
        {
            return IdComparer.Compare(left, right) < 0;
        }
        public static bool operator !=(WorkflowEvent left, WorkflowEvent right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return false;
            if (ReferenceEquals(left, null))
                return true;
            return !left.Equals(right);
        }

        private class EventIdComparer : IComparer<WorkflowEvent> 
        {
            public int Compare(WorkflowEvent first, WorkflowEvent second)
            {
                if (first == null && second == null)
                    return 0;
                if (first == null)
                    return -1;
                return first.CompareTo(second);
            }
        }
    }
}