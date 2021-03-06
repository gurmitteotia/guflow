﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CancelActivityDecision : WorkflowDecision
    {
        private readonly ScheduleId _id;

        public CancelActivityDecision(ScheduleId id):base(false)
        {
            _id = id;
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.RequestCancelActivityTask,
                RequestCancelActivityTaskDecisionAttributes = new RequestCancelActivityTaskDecisionAttributes()
                {
                    ActivityId = _id.ToString(),
                }
            };
        }
        private bool Equals(CancelActivityDecision other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CancelActivityDecision && Equals((CancelActivityDecision)obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} for {1}", GetType().Name, _id);
        }
    }
}