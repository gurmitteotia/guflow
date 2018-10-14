// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public static class ChildWorkflowItemExtensions
    {
        internal static IChildWorkflowItem First(this IEnumerable<IChildWorkflowItem> items, string name, string version,string positionalName = "")
        {
            return items.OfType<ChildWorkflowItem>().First(t => t.Has(Identity.New(name,version ,positionalName)));
        }
        internal static IChildWorkflowItem First<TWorkflow>(this IEnumerable<IChildWorkflowItem> items, string positionalName = "") where TWorkflow :Workflow
        {
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return items.First(desc.Name, desc.Version, positionalName);
        }
    }
}