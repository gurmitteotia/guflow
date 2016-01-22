using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class ActivityFailedEventTests
    {
        private Mock<IWorkflow> _workflow;
        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflow>();
        }
            
        [Test]
        public void Return_activity_failed_decision()
        {
            var decisionOnFailedActivity = new TestWorkflowAction();
            var activityFailedEvent = new ActivityFailedEvent(new HistoryEvent());
            _workflow.Setup(w => w.ActivityFailed(activityFailedEvent)).Returns(decisionOnFailedActivity);

            var decision = activityFailedEvent.Interpret(_workflow.Object);

            Assert.That(decision,Is.EqualTo(decisionOnFailedActivity));
        }
    }
}