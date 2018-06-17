// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public abstract class LambdaEvent : WorkflowItemEvent
    {
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
            foreach (var historyEvent in eventGraph)
            {
                if (historyEvent.IsLambdaScheduledEvent(scheduledEventId))
                {
                    var attr = historyEvent.LambdaFunctionScheduledEventAttributes;
                    Input = attr.Input;
                    AwsIdentity = AwsIdentity.Raw(historyEvent.LambdaFunctionScheduledEventAttributes.Id);
                    if (!string.IsNullOrEmpty(attr.StartToCloseTimeout))
                        Timeout = TimeSpan.FromSeconds(Double.Parse(attr.StartToCloseTimeout));
                    break;
                }
            }
            if (AwsIdentity == null)
                throw new IncompleteEventGraphException($"Can not find lambda scheduled event for id {scheduledEventId}.");
        }
    }
}