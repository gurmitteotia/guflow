using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowHistoryEventsTests
    {
        private Mock<IWorkflow> _workflow1;
        private Mock<WorkflowAction> _expectedWorkflowAction;
        [SetUp]
        public void Setup()
        {
            _workflow1 = new Mock<IWorkflow>();
            _expectedWorkflowAction = new Mock<WorkflowAction>();
        }
        [Test]
        public void Can_interpret_the_activity_completed_event()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("activity", "1.0"), "id", "result");
            var expectedActivityCompletedEvent = new ActivityCompletedEvent(activityCompletedEventGraph.First(),activityCompletedEventGraph);
            _workflow1.Setup(w => w.ActivityCompleted(expectedActivityCompletedEvent)).Returns(_expectedWorkflowAction.Object);
            var historyEvents = new WorkflowHistoryEvents(activityCompletedEventGraph);

            var workflowAction =historyEvents.InterpretNewEventsFor(_workflow1.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] {_expectedWorkflowAction.Object}));
        }
    }
}