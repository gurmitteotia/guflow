// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowActionItemTests
    {
        private const string ChildWorkflowName = "Name";
        private const string ChildWorkflowVersion = "1.0";
        private const string ChildWorkflowPosName = "pos";
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }
        [Test]
        public void Can_be_scheduled_after_child_workflow()
        {
            var events = ChildWorkflowCompletedEventGraph();

            var decisions = new WorkflowActionAfterChildWorkflow("result").Decisions(events);

            Assert.That(decisions, Is.EqualTo(new[]{new CompleteWorkflowDecision("result")}));

        }

        [Test]
        public void Can_be_scheduled_after_child_workflow_using_generic_api()
        {
            var events = ChildWorkflowCompletedEventGraph();

            var decisions = new WorkflowActionAfterChildWorkflowGenericType("result").Decisions(events);

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));

        }

        [Test]
        public void Invalid_arguments()
        {
            var workflowActionItem = new WorkflowActionItem(_=>WorkflowAction.Empty, Mock.Of<IWorkflow>());

            Assert.Throws<ArgumentException>(() => workflowActionItem.AfterChildWorkflow(null, "1.0"));
            Assert.Throws<ArgumentException>(() => workflowActionItem.AfterChildWorkflow("name", null));
            Assert.Throws<ArgumentException>(() => workflowActionItem.AfterActivity("name", null));
            Assert.Throws<ArgumentException>(() => workflowActionItem.AfterActivity(null, "1.0"));
            Assert.Throws<ArgumentException>(() => workflowActionItem.AfterLambda(null));
            Assert.Throws<ArgumentException>(() => workflowActionItem.AfterTimer(null));
        }

        private WorkflowHistoryEvents ChildWorkflowCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            var scheduleId = Identity.New(ChildWorkflowName, ChildWorkflowVersion, ChildWorkflowPosName).ScheduleId();
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ChildWorkflowCompletedGraph(scheduleId, "rid", "input",
                    "result")
                .ToArray());
            return _eventsBuilder.Result();

        }

        private class WorkflowActionAfterChildWorkflow : Workflow
        {
            public WorkflowActionAfterChildWorkflow(string result)
            {
                ScheduleChildWorkflow(ChildWorkflowName, ChildWorkflowVersion, ChildWorkflowPosName);
                ScheduleAction(_=>CompleteWorkflow(result))
                    .AfterChildWorkflow(ChildWorkflowName, ChildWorkflowVersion, ChildWorkflowPosName);
            }
        }

        private class WorkflowActionAfterChildWorkflowGenericType : Workflow
        {
            public WorkflowActionAfterChildWorkflowGenericType(string result)
            {
                ScheduleChildWorkflow<ChildWorkflow>(ChildWorkflowPosName);
                ScheduleAction(_=>CompleteWorkflow(result))
                    .AfterChildWorkflow<ChildWorkflow>(ChildWorkflowPosName);
            }
        }
        [WorkflowDescription(ChildWorkflowVersion, Name = ChildWorkflowName)]
        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
            }
        }
    }
}