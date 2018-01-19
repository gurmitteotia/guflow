// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RecordMarkerWorkflowDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new RecordMarkerWorkflowDecision("name","detail").Equals(new RecordMarkerWorkflowDecision("name","detail")));
            Assert.True(new RecordMarkerWorkflowDecision("name", null).Equals(new RecordMarkerWorkflowDecision("name", null)));

            Assert.False(new RecordMarkerWorkflowDecision("name", "detail").Equals(new RecordMarkerWorkflowDecision("name1", "detail")));
            Assert.False(new RecordMarkerWorkflowDecision("name", "detail").Equals(new RecordMarkerWorkflowDecision("name", "detail1")));
        }

        [Test]
        public void Returns_swf_decision_to_record_a_marker()
        {
            var recordMarkerDecision = new RecordMarkerWorkflowDecision("name","detail");

            var decision = recordMarkerDecision.Decision();

            Assert.That(decision.DecisionType,Is.EqualTo(DecisionType.RecordMarker));
            Assert.That(decision.RecordMarkerDecisionAttributes.MarkerName, Is.EqualTo("name"));
            Assert.That(decision.RecordMarkerDecisionAttributes.Details, Is.EqualTo("detail"));
        }
    }
}