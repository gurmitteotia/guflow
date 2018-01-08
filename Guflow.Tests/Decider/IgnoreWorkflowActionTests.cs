using Guflow.Decider;
using NUnit.Framework;
using System.Linq;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class IgnoreWorkflowActionTests
    {
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }
        //[Test]
        //public void Equality_tests()
        //{
        //    Assert.That(WorkflowAction.Ignore.Equals(WorkflowAction.Ignore));

        //    Assert.That(WorkflowAction.Ignore.Equals(WorkflowAction.Ignore(false)));

        //    Assert.IsFalse(WorkflowAction.Ignore(true).Equals(WorkflowAction.Ignore(false)));
        //}
        [Test]
        public void Return_empty_decisions()
        {
            var workflowAction = WorkflowAction.Ignore(null);
            Assert.That(workflowAction.GetDecisions(),Is.Empty);
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowReturningStartWorkflowAction();
            var activityCompletedEvent = CreateCompletedActivityEvent(WorkflowReturningStartWorkflowAction.ActivityName, WorkflowReturningStartWorkflowAction.ActivityVersion);

            var workflowAction = activityCompletedEvent.Interpret(workflow);

            Assert.That(workflowAction.GetDecisions(), Is.Empty);
        }
        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion)
        {
            var allHistoryEvents = _builder.ActivityCompletedGraph(Identity.New(activityName, activityVersion, string.Empty), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private class WorkflowReturningStartWorkflowAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public WorkflowReturningStartWorkflowAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion).OnCompletion(e => Ignore);
            }
        }
    }
}