using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class SignalsTimedoutDecisionTests
    {

        [Test]
        public void Equality()
        {
            var id1 = Identity.Lambda("l1").ScheduleId();
            var id2 = Identity.Lambda("l2").ScheduleId();
            Assert.IsTrue(new WorkflowItemSignalsTimedoutDecision(id1, 10, new []{"signal1"}, 1)
                .Equals(new WorkflowItemSignalsTimedoutDecision(id1, 10, new[] { "signal2" }, 2)));

            Assert.IsTrue(new WorkflowItemSignalsTimedoutDecision(id1, 10, new[] { "signal1" }, 1)
                .Equals(new WorkflowItemSignalsTimedoutDecision(id1, 10, new[] { "signal1" }, 1)));

            Assert.IsFalse(new WorkflowItemSignalsTimedoutDecision(id1, 10, new[] { "signal1" }, 1)
                .Equals(new WorkflowItemSignalsTimedoutDecision(id1, 11, new[] { "signal1" }, 1)));

            Assert.IsFalse(new WorkflowItemSignalsTimedoutDecision(id1, 10, new[] { "signal1" }, 1)
                .Equals(new WorkflowItemSignalsTimedoutDecision(id2, 10, new[] { "signal1" }, 1)));
        }

        [Test]
        public void SWF_decision_is_a_record_marker_decision()
        {
            var id = Identity.Lambda("l1").ScheduleId();
            var decision = new WorkflowItemSignalsTimedoutDecision(id, 10, new[] {"signal1"}, 2);

            var swfDecision = decision.SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.RecordMarker));
            var attr = swfDecision.RecordMarkerDecisionAttributes;
            Assert.That(attr.MarkerName, Is.EqualTo("WorkflowItemSignalsTimedout_Guflow_Internal_Marker"));
            var data = attr.Details.AsDynamic();
            Assert.That((string)data.ScheduleId, Is.EqualTo(id.ToString()));
            Assert.That((long)data.TriggerEventId, Is.EqualTo(10));
            Assert.That(data.TimedoutSignalNames.ToObject<string[]>(), Is.EqualTo(new[] { "signal1" }));
            Assert.That((long)data.TimeoutTriggerEventId, Is.EqualTo(2));
        }
    }
}