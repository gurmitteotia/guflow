﻿using System.Collections.Generic;

namespace Guflow
{
    public interface IWorkflowItems
    {
        IEnumerable<WorkflowItem> GetStartupWorkflowItems();

        IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item);

        WorkflowItem Find(Identity identity);
    }
}