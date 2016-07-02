using System;
using System.Collections.Generic;

namespace Guflow
{
    public abstract class WorkflowEvent : IComparable<WorkflowEvent>
    {
        private readonly long _eventId;
        protected WorkflowEvent(long eventId)
        {
            _eventId = eventId;
        }

        internal abstract WorkflowAction Interpret(IWorkflow workflow);
        public static readonly IComparer<WorkflowEvent> IdComparer = new EventIdComparer();
    
        public int CompareTo(WorkflowEvent other)
        {
            return _eventId.CompareTo(other._eventId);
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

        public static bool operator >=(WorkflowEvent left, WorkflowEvent right)
        {
            return left.CompareTo(right)>=0;
        }
        public static bool operator >(WorkflowEvent left, WorkflowEvent right)
        {
            return left.CompareTo(right) >0;
        }
        public static bool operator <=(WorkflowEvent left, WorkflowEvent right)
        {
            return left.CompareTo(right)<=0;
        }
        public static bool operator <(WorkflowEvent left, WorkflowEvent right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator !=(WorkflowEvent left, WorkflowEvent right)
        {
            return !Equals(left, right);
        }

        private class EventIdComparer : IComparer<WorkflowEvent> 
        {
            public int Compare(WorkflowEvent first, WorkflowEvent second)
            {
                return first.CompareTo(second);
            }
        }
    }
}