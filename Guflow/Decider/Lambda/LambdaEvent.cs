// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public abstract class LambdaEvent : WorkflowItemEvent
    {
        private long _scheduledEventId;
        private string _name;
        private string _positionalName;
        protected LambdaEvent(long eventId) : base(eventId)
        {
        }

        /// <summary>
        /// Returns the input for lamdba function.
        /// </summary>
        public string Input { get; private set; }
        /// <summary>
        /// Returns the timeout set for scheduling the lamdba function.
        /// </summary>
        public TimeSpan? Timeout { get; private set; }

        protected void PopulateProperties(long scheduledEventId, IEnumerable<HistoryEvent> eventGraph)
        {
            _scheduledEventId = scheduledEventId;
            foreach (var historyEvent in eventGraph)
            {
                if (historyEvent.IsLambdaScheduledEvent(scheduledEventId))
                {
                    var attr = historyEvent.LambdaFunctionScheduledEventAttributes;
                    Input = attr.Input;
                    _name = attr.Name;
                    _positionalName = attr.Control.FromJson<ScheduleData>().PN;
                    AwsIdentity = AwsIdentity.Raw(historyEvent.LambdaFunctionScheduledEventAttributes.Id);
                    if (!string.IsNullOrEmpty(attr.StartToCloseTimeout))
                        Timeout = TimeSpan.FromSeconds(Double.Parse(attr.StartToCloseTimeout));
                    break;
                }
            }
            if (AwsIdentity == null)
                throw new IncompleteEventGraphException($"Can not find lambda scheduled event for id {scheduledEventId}.");
        }
        internal override bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            var lambdaEvents = workflowItemEvents.OfType<LambdaEvent>();
            foreach (var lambdaEvent in lambdaEvents)
            {
                if (IsInChain(lambdaEvent))
                    return true;
            }
            return false;
        }

        private bool IsInChain(LambdaEvent other)
        {
            return _scheduledEventId == other._scheduledEventId;
        }

        public override string ToString()
        {
            return $"{GetType().Name} for lambda name {_name} and positional name {_positionalName}";
        }
    }
}