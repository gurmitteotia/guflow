// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;

namespace Guflow.Tests
{
    internal static class WorkflowTestExtension
    {

        public static IEnumerable<WorkflowDecision> Interpret(this Workflow workflow, WorkflowHistoryEvents historyEvents)
        {
            using (var execution = workflow.NewExecutionFor(historyEvents))
                return execution.Execute().ToArray();
        }
    }
}