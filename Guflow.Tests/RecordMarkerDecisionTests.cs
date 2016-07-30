using Amazon.SimpleWorkflow;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class RecordMarkerDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new RecordMarkerDecision("name","detail").Equals(new RecordMarkerDecision("name","detail")));
            Assert.True(new RecordMarkerDecision("name", null).Equals(new RecordMarkerDecision("name", null)));

            Assert.False(new RecordMarkerDecision("name", "detail").Equals(new RecordMarkerDecision("name1", "detail")));
            Assert.False(new RecordMarkerDecision("name", "detail").Equals(new RecordMarkerDecision("name", "detail1")));
        }

        [Test]
        public void Returns_swf_decision_to_record_a_marker()
        {
            var recordMarkerDecision = new RecordMarkerDecision("name","detail");

            var decision = recordMarkerDecision.Decision();

            Assert.That(decision.DecisionType,Is.EqualTo(DecisionType.RecordMarker));
            Assert.That(decision.RecordMarkerDecisionAttributes.MarkerName, Is.EqualTo("name"));
            Assert.That(decision.RecordMarkerDecisionAttributes.Details, Is.EqualTo("detail"));
        }
    }
}