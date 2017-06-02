﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class RestartWorkflowDecision : WorkflowClosingDecision
    {
        private readonly RestartWorkflowAction _restartWorkflowAction;

        public RestartWorkflowDecision(RestartWorkflowAction restartWorkflowAction)
        {
            _restartWorkflowAction = restartWorkflowAction;
        }
     
        internal override Decision Decision()
        {
            return new Decision
            {
                DecisionType = DecisionType.ContinueAsNewWorkflowExecution,
                ContinueAsNewWorkflowExecutionDecisionAttributes =
                    new ContinueAsNewWorkflowExecutionDecisionAttributes()
                    {
                        Input = _restartWorkflowAction.Input,
                        TaskList = string.IsNullOrEmpty(_restartWorkflowAction.TaskList) ? null : new TaskList() { Name = _restartWorkflowAction.TaskList },
                        ChildPolicy = _restartWorkflowAction.ChildPolicy,
                        ExecutionStartToCloseTimeout =
                            _restartWorkflowAction.ExecutionStartToCloseTimeout.HasValue
                                ? _restartWorkflowAction.ExecutionStartToCloseTimeout.Value.TotalSeconds.ToString()
                                : null,
                       TagList =  _restartWorkflowAction.TagList.ToList(),
                        TaskPriority = _restartWorkflowAction.TaskPriority.HasValue ? _restartWorkflowAction.TaskPriority.Value.ToString() : null,
                        TaskStartToCloseTimeout = _restartWorkflowAction.TaskStartToCloseTimeout.HasValue ? _restartWorkflowAction.TaskStartToCloseTimeout.Value.TotalSeconds.ToString() : null,
                        WorkflowTypeVersion = _restartWorkflowAction.WorkflowTypeVersion
                    }
            };
        }

        internal override WorkflowAction ProvideFinalActionFrom(IWorkflowClosingActions workflowClosingActions)
        {
            return _restartWorkflowAction;
        }

        internal override void Raise(PostExecutionEvents postExecutionEvents)
        {
            postExecutionEvents.Restarted();
        }
    }
}