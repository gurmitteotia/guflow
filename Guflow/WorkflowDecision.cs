using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class WorkflowDecision
    {
       public abstract IEnumerable<Decision> Decisions();
    }
}
