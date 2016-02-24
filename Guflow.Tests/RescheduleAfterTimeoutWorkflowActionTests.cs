using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class RescheduleAfterTimeoutWorkflowActionTests
    {
        private readonly Mock<IWorkflowItems> _workflowItems = new Mock<IWorkflowItems>();

        [Test]
        public void Equality_tests()
        {

            Assert.True(new RescheduleAfterTimeoutWorkflowAction(new TimerItem("Somename", _workflowItems.Object),
                TimeSpan.FromSeconds(2))
                .Equals(new RescheduleAfterTimeoutWorkflowAction(new TimerItem("Somename", _workflowItems.Object),
                    TimeSpan.FromSeconds(2))));

            Assert.False(new RescheduleAfterTimeoutWorkflowAction(new TimerItem("Somename", _workflowItems.Object),
                new TimeSpan())
                .Equals(new RescheduleAfterTimeoutWorkflowAction(new TimerItem("Somename1", _workflowItems.Object),
                    new TimeSpan())));

            Assert.False(new RescheduleAfterTimeoutWorkflowAction(new TimerItem("Somename", _workflowItems.Object),
                TimeSpan.FromSeconds(1))
                .Equals(new RescheduleAfterTimeoutWorkflowAction(new TimerItem("Somename", _workflowItems.Object),
                    TimeSpan.FromSeconds(2))));
        }

        [Test]
        public void Should_schedule_the_timer_for_workflow_item()
        {
            var workflowAction = new RescheduleAfterTimeoutWorkflowAction(new ActivityItem("activityname","version","position",_workflowItems.Object), TimeSpan.FromSeconds(2));

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new []{new ScheduleTimerDecision(Identity.New("activityname", "version", "position"),TimeSpan.FromSeconds(2))}));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            TimeSpan resheduleAfter = TimeSpan.FromSeconds(2);
            var workflow = new SingleActivityWorkflow(resheduleAfter);
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(new RescheduleAfterTimeoutWorkflowAction(workflow.CompletedWorkflowItem, resheduleAfter)));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow(TimeSpan rescheduleAfter)
            {
                CompletedWorkflowItem =AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c=>RescheduleAfter(c,rescheduleAfter));
            }

            public WorkflowItem CompletedWorkflowItem { get; private set; }
        }
    }
}