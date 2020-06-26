// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using NUnit.Framework;
using System.Linq;
using Moq;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class IgnoreWorkflowActionTests
    {
        private EventGraphBuilder _graphBuilder;
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private HistoryEventsBuilder _builder;
        
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
        }
      
        [Test]
        public void Return_empty_decisions()
        {
            var workflowAction = WorkflowAction.Ignore(null);
            Assert.That(workflowAction.Decisions(Mock.Of<IWorkflow>()),Is.Empty);
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var id = Identity.New(ActivityName, ActivityVersion, string.Empty).ScheduleId();
            _builder.AddNewEvents(_graphBuilder.ActivityCompletedGraph(id, "id", "res"));
            var workflow = new WorkflowReturningStartWorkflowAction();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }
        private class WorkflowReturningStartWorkflowAction : Workflow
        {
            public WorkflowReturningStartWorkflowAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion).OnCompletion(e => Ignore);
            }
        }
    }
}