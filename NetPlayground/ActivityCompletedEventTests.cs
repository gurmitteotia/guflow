using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class ActivityCompletedEventTests
    {
        private Mock<IWorkflow> _workflow;

        private const string _result = "result";

        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflow>();
        }

        [Test]
        public void Return_activity_completed_decision()
        {
            var decisionOnActivityCompletion = new TestWorkflowAction();
            var activityCompletedEvent = new ActivityCompletedEvent(CreateActivityCompletionEvent(_result));
            _workflow.Setup(w => w.ActivityCompleted(activityCompletedEvent)).Returns(decisionOnActivityCompletion);
            
            var interpretedDecision = activityCompletedEvent.Interpret(_workflow.Object);

            Assert.That(interpretedDecision,Is.EqualTo(decisionOnActivityCompletion));
        }

        [Test]
        public void Populate_resul_from_event_attribute()
        {
            var activityCompletedEvent = new ActivityCompletedEvent(CreateActivityCompletionEvent(_result));

            Assert.That(activityCompletedEvent.Result,Is.EqualTo(_result));
        }

        private HistoryEvent CreateActivityCompletionEvent(string result)
        {
            return new HistoryEvent() {ActivityTaskCompletedEventAttributes = new ActivityTaskCompletedEventAttributes() {Result = result}};
        }
    }
}