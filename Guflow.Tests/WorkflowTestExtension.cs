// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;

namespace Guflow.Tests
{
    internal static class WorkflowTestExtension
    {

        public static IEnumerable<WorkflowDecision> Interpret(this IWorkflow workflow, IEnumerable<HistoryEvent> historyEvents)
        {
            var @events = new WorkflowHistoryEvents(historyEvents);
            return @events.InterpretNewEvents(workflow);
        }
    }
}