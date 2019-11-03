// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent a event in workflow history.
    /// </summary>
    public abstract class WorkflowEvent : IComparable<WorkflowEvent>
    {
        private readonly HistoryEvent _historyEvent;

        protected WorkflowEvent(HistoryEvent historyEvent)
        {
            _historyEvent = historyEvent;
        }

        internal virtual WorkflowAction Interpret(IWorkflow workflow)
        {
            throw new NotSupportedException($"Can not interpret {this.GetType().Name}.");
        }

        internal virtual WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            throw new NotSupportedException($"DefaultAction is not supported {this.GetType().Name}.");
        }

        internal long EventId => _historyEvent.EventId;
        internal DateTime Timestamp => _historyEvent.EventTimestamp;
        public static readonly IComparer<WorkflowEvent> IdComparer = new EventIdComparer();
    
        public int CompareTo(WorkflowEvent other)
        {
            if (other == null)
                return 1;
            return EventId.CompareTo(other.EventId);
        }

        public override string ToString()
        {
            return $"{GetType().Name} with event id {EventId}";
        }
        private bool Equals(WorkflowEvent other)
        {
            return EventId == other?.EventId;
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
            return EventId.GetHashCode();
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