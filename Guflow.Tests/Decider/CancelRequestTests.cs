using System;
using Guflow.Decider;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelRequestTests
    {
        private CancelRequest _cancelRequest;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            _cancelRequest = new CancelRequest(null);
        }
        [Test]
        public void Invalid_arugments_test()
        {
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForActivity(null, "1.0"));
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForActivity("activity", null));
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForTimer( null));
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForWorkflow(null));
            Assert.Throws<ArgumentNullException>(() => _cancelRequest.For(null));
        }

        [Test]
        public void Can_return_cancel_request_for_activity()
        {
            var activityItem = CreateActivity("name1", "1.0", "pname");
            var workflowItems = new WorkflowItems();
            workflowItems.Add(activityItem);
            var cancelRequest = new CancelRequest(workflowItems);

            var decisions = cancelRequest.ForActivity<TestActivity>("pname").GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new CancelActivityDecision(Identity.New("name1", "1.0", "pname"))}));
        }

        private ActivityItem CreateActivity(string name, string version, string positionalName)
        {
            var identity = Identity.New(name, version, positionalName);
            var historyEvent = new WorkflowHistoryEvents(_builder.ActivityScheduledGraph(identity));
            var workflow = new Mock<IWorkflow>();
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(historyEvent);
            return new ActivityItem(identity, workflow.Object);
        }

        [ActivityDescription("1.0", Name="name1")]
        private class TestActivity : Activity
        {
            
        }
    }
}