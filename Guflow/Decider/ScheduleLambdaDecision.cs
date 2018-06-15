// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class ScheduleLambdaDecision : WorkflowDecision
    {
        private readonly Identity _identity;
        private readonly object _input;
        private readonly TimeSpan? _timout;

        internal ScheduleLambdaDecision(Identity identity, object input, TimeSpan? timout = null) : base(canCloseWorkflow:false, proposal: false)
        {
            _identity = identity;
            _input = input;
            _timout = timout;
        }

        internal override Decision SwfDecision()
        {
            return new Decision
            {
                DecisionType = DecisionType.ScheduleLambdaFunction,
                ScheduleLambdaFunctionDecisionAttributes = new ScheduleLambdaFunctionDecisionAttributes
                {
                    Id = _identity.Id,
                    Name = _identity.Name,
                    Input = _input.ToAwsString(),
                    StartToCloseTimeout = _timout.Seconds()
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