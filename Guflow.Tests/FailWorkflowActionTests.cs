using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class FailWorkflowActionTests
    {

        [Test]
        public void Equality_tests()
        {
            Assert.True(new FailWorkflowAction("reason","detail").Equals(new FailWorkflowAction("reason","detail")));
            Assert.True(new FailWorkflowAction("", "").Equals(new FailWorkflowAction("", "")));

            Assert.False(new FailWorkflowAction("reason", "detail").Equals(new FailWorkflowAction("reason1", "detail")));
            Assert.False(new FailWorkflowAction("reason", "detail").Equals(new FailWorkflowAction("reason", "detail1")));
            Assert.False(new FailWorkflowAction("reason", "detail").Equals(new FailWorkflowAction(null, "detail")));
        }
    }
}