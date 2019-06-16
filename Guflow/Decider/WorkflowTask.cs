// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the new decision task in Amazon SWF.
    /// </summary>
    public class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly Func<WorkflowHost, CancellationToken, Task> _actionToExecute;
        private IErrorHandler _executionErrorHandler = ErrorHandler.NotHandled;
        private static readonly WorkflowDecision[] EmptyDecisions = new WorkflowDecision[0];

        private WorkflowTask(DecisionTask decisionTask)
        {
            _decisionTask = decisionTask;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _actionToExecute = async (w, c) => { await Task.Yield(); };
        }

        /// <summary>
        /// Represent Decision task with no new events. Execution of empty <see cref="WorkflowTask"/> does not generate any decisions.
        /// </summary>
        public static readonly WorkflowTask Empty = new WorkflowTask();

        /// <summary>
        /// Create the instance from Amazon SWF DecisionTask.
        /// </summary>
        /// <param name="decisionTask"></param>
        /// <returns></returns>
        public static WorkflowTask Create(DecisionTask decisionTask)
        {
            if(HasNewEvents(decisionTask))
                return new WorkflowTask(decisionTask);
            return Empty;
        }

        /// <summary>
        /// Append events of <para>other</para> WorkflowTask.
        /// </summary>
        /// <param name="other"></param>
        public void Append(WorkflowTask other)
        {
            _decisionTask.Events.AddRange(other._decisionTask.Events);
        }


        private static bool HasNewEvents(DecisionTask decision)
        {
            return !string.IsNullOrEmpty(decision?.TaskToken);
        }

        internal async Task ExecuteAsync(WorkflowHost workflowHost, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _actionToExecute(workflowHost, cancellationToken);
        }
        internal void OnExecutionError(IErrorHandler errorHandler)
        {
            _executionErrorHandler = errorHandler;
        }
        private async Task ExecuteTasks(WorkflowHost workflowHost, CancellationToken cancellationToken)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = workflowHost.Workflow(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask);

            var decisions = Perform(() => workflow.Decisions(historyEvents));
            await workflowHost.SendDecisionsAsync(_decisionTask.TaskToken, decisions);
            RaisePostExecutionEvents(decisions, workflow);
        }
        private WorkflowDecision[] Perform(Func<IEnumerable<WorkflowDecision>> decisions)
        {
            var retryableAction = new RetryableFunc(_executionErrorHandler);
            return retryableAction.Execute(() => decisions().ToArray(), EmptyDecisions);
        }
        private void RaisePostExecutionEvents(IEnumerable<WorkflowDecision> workflowDecisions, Workflow workflow)
        {
            var postExecutionEvents = new PostExecutionEvents(workflow, _decisionTask.WorkflowExecution.WorkflowId, _decisionTask.WorkflowExecution.RunId);
            var workflowClosingDecisions = workflowDecisions.OfType<WorkflowClosingDecision>().ToList();
            workflowClosingDecisions.ForEach(d => d.Raise(postExecutionEvents));
        }
    }
}