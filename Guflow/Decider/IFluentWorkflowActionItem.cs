// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;

namespace Guflow.Decider
{
    public interface IFluentWorkflowActionItem : IFluentWorkflowItem<IFluentWorkflowActionItem>
    {
        /// <summary>
        /// Schedule the workflow action only when given function is evaluated to true.
        /// </summary>
        /// <param name="true"></param>
        /// <returns></returns>
        IFluentWorkflowActionItem When(Func<IWorkflowItem, bool> @true);

    }
}