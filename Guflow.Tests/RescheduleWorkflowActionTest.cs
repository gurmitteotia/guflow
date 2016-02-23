using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class RescheduleWorkflowActionTest
    {
        private readonly Mock<IWorkflowItems> _workflowItems = new Mock<IWorkflowItems>();

        [Test]
        public void Equality_tests()
        {
            Assert.True(new RescheduleWorkflowAction(new TimerItem("Somename",_workflowItems.Object)).Equals(new RescheduleWorkflowAction(new TimerItem("Somename",_workflowItems.Object))));
            Assert.False(new RescheduleWorkflowAction(new TimerItem("Somename", _workflowItems.Object)).Equals(new RescheduleWorkflowAction(new TimerItem("Somename1", _workflowItems.Object))));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = new TimerItem("Somename",_workflowItems.Object);
            var workflowAction = new RescheduleWorkflowAction(workflowItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{workflowItem.GetDecision()}));
        }
        
    }
}