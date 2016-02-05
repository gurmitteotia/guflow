using System.Collections.Generic;

namespace NetPlayground
{
    public abstract class WorkflowEvent
    {
        public abstract WorkflowAction Interpret(IWorkflow workflow);

        internal abstract SchedulableItem FindSchedulableItemIn(HashSet<SchedulableItem> allSchedulableItems);
    }
}