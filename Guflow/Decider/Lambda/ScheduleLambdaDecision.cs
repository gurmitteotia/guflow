// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class ScheduleLambdaDecision : WorkflowDecision
    {
        private readonly ScheduleId _identity;
        private readonly object _input;
        private readonly TimeSpan? _timout;

        internal ScheduleLambdaDecision(ScheduleId identity, object input, TimeSpan? timout = null) : base(canCloseWorkflow:false, proposal: false)
        {
            _identity = identity;
            _input = input;
            _timout = timout;
        }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_identity);
        }

        internal override Decision SwfDecision()
        {
            return new Decision
            {
                DecisionType = DecisionType.ScheduleLambdaFunction,
                ScheduleLambdaFunctionDecisionAttributes = new ScheduleLambdaFunctionDecisionAttributes
                {
                    Id = _identity,
                    Name = _identity.Name,
                    Input = _input.ToLambdaInput(),
                    StartToCloseTimeout = _timout.Seconds(),
                    Control = new ScheduleData() { PN = _identity.PositionalName}.ToJson()
                }
            };
        }
        public override bool Equals(object obj)
        {
            var decision = obj as ScheduleLambdaDecision;
            return decision != null && _identity.Equals(decision._identity);

        }
        public override int GetHashCode()
        {
            return -1493283476 + _identity.GetHashCode();
        }

    }
}