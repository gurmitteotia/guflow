// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForSignalDecisionTests
    {

        [Test]
        public void Equality()
        {
            var id = Identity.Lambda("a").ScheduleId();
            var id1 = Identity.Lambda("a1").ScheduleId();
            Assert.IsTrue(new WaitForSignalDecision(id, 1, "s").Equals(new WaitForSignalDecision(id, 1, "s")));
            Assert.IsTrue(new WaitForSignalDecision(id, 1, "s").Equals(new WaitForSignalDecision(id, 1, "S")));

            Assert.IsFalse(new WaitForSignalDecision(id, 1, "s").Equals(new WaitForSignalDecision(id1, 1, "S")));
            Assert.IsFalse(new WaitForSignalDecision(id, 1, "s").Equals(new WaitForSignalDecision(id1, 2, "S")));
            Assert.IsFalse(new WaitForSignalDecision(id, 1, "s").Equals(new WaitForSignalDecision(id, 1, "S1")));
        }

        [Test]
        public void Returns_swf_decision_to_record_the_marker()
        {
            var id = Identity.Lambda("a").ScheduleId();
            var decision = new WaitForSignalDecision(id, 10, "Signal");

            var swfDecision = decision.SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.RecordMarker));
            var attr = swfDecision.RecordMarkerDecisionAttributes;
            Assert.That(attr.MarkerName, Is.EqualTo("WaitForSignals_Guflow_Internal_Marker"));
            var data = attr.Details.AsDynamic();
            Assert.That((string)data.ScheduleId, Is.EqualTo(id.ToString()));
            Assert.That((long)data.EventId, Is.EqualTo(10));
            Assert.That(data.EventNames.ToObject<string[]>(), Is.EqualTo(new[]{"Signal"}));
            Assert.That((SignalWaitType)data.WaitType, Is.EqualTo(SignalWaitType.Any));
            Assert.That((SignalNextAction)data.NextAction, Is.EqualTo(SignalNextAction.Continue));
        }
    }
}