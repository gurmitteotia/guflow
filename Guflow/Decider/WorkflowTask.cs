// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the decision task in Amazon SWF.
    /// </summary>
    public class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly Func<WorkflowHost, CancellationToken, Task> _actionToExecute;
        private IErrorHandler _executionErrorHandler = ErrorHandler.NotHandled;
        private static readonly WorkflowDecision[] EmptyDecisions = new WorkflowDecision[0];
        private readonly DateTime _creationTime = DateTime.UtcNow;
        private readonly TimeSpan _timeToDownloadEvents;
        private WorkflowTask(DecisionTask decisionTask, TimeSpan timeToDownloadEvents)
        {
            _decisionTask = decisionTask;
            _timeToDownloadEvents = timeToDownloadEvents;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _timeToDownloadEvents = TimeSpan.Zero;
            _decisionTask = EmptyDecisionTask;
            _actionToExecute = async (t, c) => await Task.Yield();
        }

        /// <summary>
        /// Returns empty workflow task. Execution of empty <see cref="WorkflowTask"/> does not generate any decisions.
        /// </summary>
        public static readonly WorkflowTask Empty = new WorkflowTask();

        internal IEnumerable<HistoryEvent> AllEvents => _decisionTask.Events;
        internal IEnumerable<HistoryEvent> NewEvents {
            get { return AllEvents.TakeWhile(h => h.EventId > _decisionTask.PreviousStartedEventId).Reverse(); }
    }
        internal string WorkflowId => _decisionTask.WorkflowExecution.WorkflowId;
        internal string RunId => _decisionTask.WorkflowExecution.RunId;

        /// <summary>
        /// Create the instance from Amazon SWF DecisionTask.
        /// </summary>
        /// <param name="decisionTask"></param>
        /// <param name="downloadFactor">Indicate how much time it will take in milliseconds to download a history event. By default it assumes it will take 500 ms to download 1000 events.</param>
        /// <returns></returns>
        public static WorkflowTask Create(DecisionTask decisionTask, double downloadFactor=.5)
        {
            if(HasNewEvents(decisionTask))
                return ValidatedWorkflowTask(decisionTask, downloadFactor);
            return Empty;
        }

        private static WorkflowTask ValidatedWorkflowTask(DecisionTask decisionTask, double downloadFactor)
        {
            if (decisionTask.Events == null|| decisionTask.Events.Count==0)
                throw new ArgumentException("", "decisionTask.Events");
            return new WorkflowTask(decisionTask, TimeSpan.FromMilliseconds(downloadFactor * decisionTask.Events.Count));
        }

        /// <summary>
        /// Append events of <para>other</para> WorkflowTask.
        /// </summary>
        /// <param name="other"></param>
        public void Append(WorkflowTask other)
        {
            Ensure.NotNull(other, nameof(other));
            _decisionTask.Events.AddRange(other._decisionTask.Events);
        }

        private static DecisionTask EmptyDecisionTask =>
            new DecisionTask
            {
                Events = new List<HistoryEvent>(),
                PreviousStartedEventId = 0,
                StartedEventId = 0,
                WorkflowExecution = new WorkflowExecution {RunId = string.Empty, WorkflowId = string.Empty}
            };

        internal DateTime ServerTimeUtc
            => this==Empty
               ? DateTime.UtcNow
               : DecisionTaskStartTime + (DateTime.UtcNow - _creationTime);

        private DateTime DecisionTaskStartTime 
            =>  _decisionTask.Events[0].EventTimestamp + _timeToDownloadEvents;

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
            var historyEvents = new WorkflowHistoryEvents(this);

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

        internal void Validate()
        {
            if (this == Empty) return;
            if(_decisionTask.Events[0].EventType!=EventType.DecisionTaskStarted)
                throw new ArgumentException(Resources.DecisionTaskStarted_event_is_missing);
        }
    }
}