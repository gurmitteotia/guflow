using System.Linq;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class ActivityFailedEventTests
    {
        private ActivityFailedEvent _activityFailedEvent;
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private const string _reason = "reason";
        private const string _detail = "detail";
        [SetUp]
        public void Setup()
        {
            var failedActivityEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(_activityName,_activityVersion,_positionalName,_identity,_reason,_detail);
            _activityFailedEvent = new ActivityFailedEvent(failedActivityEventGraph.First(),failedActivityEventGraph);
        }
            
        [Test]
        public void Populate_event_attributes()
        {
            Assert.That(_activityFailedEvent.Name,Is.EqualTo(_activityName));
            Assert.That(_activityFailedEvent.Version,Is.EqualTo(_activityVersion));
            Assert.That(_activityFailedEvent.PositionalName,Is.EqualTo(_positionalName));
            Assert.That(_activityFailedEvent.Identity,Is.EqualTo(_identity));
            Assert.That(_activityFailedEvent.Reason,Is.EqualTo(_reason));
            Assert.That(_activityFailedEvent.Detail,Is.EqualTo(_detail));
        }

        [Test]
        public void Throws_exception_when_failed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityFailedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Return_workflow_failed_decision()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activityFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{new FailWorkflowDecision(_reason,_detail)}));
        }


        private class EmptyWorkflow : Workflow
        {
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                AddActivity(_activityName, _activityVersion, _positionalName);
            }
        }

    }
}