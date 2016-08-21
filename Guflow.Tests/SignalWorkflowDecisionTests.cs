using Amazon.SimpleWorkflow;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class SignalWorkflowDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new SignalWorkflowDecision("name", "input", "wid", "rid").Equals(new SignalWorkflowDecision("name", "input", "wid", "rid")));
            Assert.True(new SignalWorkflowDecision("name", "input", "wid", null).Equals(new SignalWorkflowDecision("name", "input", "wid", null)));

            Assert.False(new SignalWorkflowDecision("name", "input", "wid", null).Equals(new SignalWorkflowDecision("name1", "input", "wid", null)));
            Assert.False(new SignalWorkflowDecision("name", "input", "wid", null).Equals(new SignalWorkflowDecision("name", "input1", "wid", null)));
            Assert.False(new SignalWorkflowDecision("name", "input", "wid", null).Equals(new SignalWorkflowDecision("name", "input", "wid1", null)));
            Assert.False(new SignalWorkflowDecision("name", "input", "wid", null).Equals(new SignalWorkflowDecision("name", "input", "wid", "diff")));
        }

        [Test]
        public void Return_aws_decision_to_signal_external_workflow()
        {
            var signalDecision = new SignalWorkflowDecision("name","input","wid","rid");

            var awsDecision = signalDecision.Decision();

            Assert.That(awsDecision.DecisionType,Is.EqualTo(DecisionType.SignalExternalWorkflowExecution));
            Assert.That(awsDecision.SignalExternalWorkflowExecutionDecisionAttributes.SignalName, Is.EqualTo("name"));
            Assert.That(awsDecision.SignalExternalWorkflowExecutionDecisionAttributes.Input, Is.EqualTo("input"));
            Assert.That(awsDecision.SignalExternalWorkflowExecutionDecisionAttributes.WorkflowId, Is.EqualTo("wid"));
            Assert.That(awsDecision.SignalExternalWorkflowExecutionDecisionAttributes.RunId, Is.EqualTo("rid"));
        }
    }
}