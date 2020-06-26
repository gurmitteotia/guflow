// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class ScheduleLambdaDecision : WorkflowDecision
    {
        private readonly ScheduleId _id;
        private readonly object _input;
        private readonly TimeSpan? _timeout;

        internal ScheduleLambdaDecision(ScheduleId id, object input, TimeSpan? timeout = null) : base(canCloseWorkflow:false, proposal: false)
        {
            _id = id;
            _input = input;
            _timeout = timeout;
        }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_id);
        }

        internal override Decision SwfDecision()
        {
            return new Decision
            {
                DecisionType = DecisionType.ScheduleLambdaFunction,
                ScheduleLambdaFunctionDecisionAttributes = new ScheduleLambdaFunctionDecisionAttributes
                {
                    Id = _id,
                    Name = _id.Name,
                    Input = _input.ToLambdaInput(),
                    StartToCloseTimeout = _timeout.Seconds(),
                    Control = new ScheduleData() { PN = _id.PositionalName}.ToJson()
                }
            };
        }
        public override bool Equals(object obj)
        {
            return obj is ScheduleLambdaDecision decision && _id.Equals(decision._id);
        }
        public override int GetHashCode()
        {
            return -1493283476 + _id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} with schedule id{_id}";
        }
    }
}