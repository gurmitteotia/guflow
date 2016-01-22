using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public abstract class WorkflowDecision
    {
       public abstract IEnumerable<Decision> Decisions();
    }
}
