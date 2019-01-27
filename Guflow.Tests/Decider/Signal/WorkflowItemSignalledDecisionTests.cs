// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowItemSignalledDecisionTests
    {
        [Test]
        public void Equality()
        {
            var id = Identity.Lambda("a").ScheduleId();
            var id1 = Identity.Lambda("a1").ScheduleId();
            Assert.IsTrue(new WorkflowItemSignalledDecision(id, 1, "s", 2).Equals(new WorkflowItemSignalledDecision(id, 1, "s", 2)));
            Assert.IsTrue(new WorkflowItemSignalledDecision(id, 1, "s", 2).Equals(new WorkflowItemSignalledDecision(id, 1, "S",2)));

            Assert.IsFalse(new WorkflowItemSignalledDecision(id, 1, "s").Equals(new WorkflowItemSignalledDecision(id1, 1, "S")));
            Assert.IsFalse(new WorkflowItemSignalledDecision(id, 1, "s").Equals(new WorkflowItemSignalledDecision(id1, 2, "S")));
            Assert.IsFalse(new WorkflowItemSignalledDecision(id, 1, "s").Equals(new WorkflowItemSignalledDecision(id, 1, "S1")));
            Assert.IsFalse(new WorkflowItemSignalledDecision(id, 1, "s", 2).Equals(new WorkflowItemSignalledDecision(id, 1, "S", 3)));

        }

        [Test]
        public void Returns_swf_decision_to_record_the_marker()
        {
            var id = Identity.Lambda("a").ScheduleId();
            var decision = new WorkflowItemSignalledDecision(id, 10, "Signal", 3);

            var swfDecision = decision.SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.RecordMarker));
            var attr = swfDecision.RecordMarkerDecisionAttributes;
            Assert.That(attr.MarkerName, Is.EqualTo("WorkflowItemSignalled_Guflow_Internal_Marker"));
            var data = attr.Details.AsDynamic();
            Assert.That((string)data.ScheduleId, Is.EqualTo(id.ToString()));
            Assert.That((long)data.TriggerEventId, Is.EqualTo(10));
            Assert.That((string)data.SignalName, Is.EqualTo("Signal"));
            Assert.That((long)data.SignalEventId, Is.EqualTo(3));

        }
    }
}