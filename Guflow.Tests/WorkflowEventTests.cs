using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowEventTests
    {

        [Test]
        public void Equality_test()
        {
            Assert.IsTrue(new TestWorkflowEvent(10).Equals(new TestWorkflowEvent(10)));
            Assert.IsTrue(new TestWorkflowEvent(10)==new TestWorkflowEvent(10));

            Assert.IsFalse(new TestWorkflowEvent(10).Equals(new TestWorkflowEvent(11)));
            Assert.IsFalse(new TestWorkflowEvent(10).Equals(null));
            Assert.IsFalse(new TestWorkflowEvent(10) == new TestWorkflowEvent(11));
            Assert.IsFalse(new TestWorkflowEvent(10) == null);
            Assert.IsFalse(null == new TestWorkflowEvent(11));
        }

        [Test]
        public void Inequality_test()
        {
            Assert.IsTrue(new TestWorkflowEvent(10) != new TestWorkflowEvent(11));
            Assert.IsTrue(new TestWorkflowEvent(10) != null);
            Assert.IsTrue(null != new TestWorkflowEvent(11));

            Assert.IsFalse(new TestWorkflowEvent(10) != new TestWorkflowEvent(10));
        }


        [Test]
        public void Greater_test()
        {
            Assert.IsTrue(new TestWorkflowEvent(10)>(new TestWorkflowEvent(9)));
            Assert.IsTrue(new TestWorkflowEvent(10) > null);
            Assert.IsTrue(new TestWorkflowEvent(10) >= (new TestWorkflowEvent(10)));
            Assert.IsTrue(new TestWorkflowEvent(10) >= (new TestWorkflowEvent(9)));
            Assert.IsTrue(new TestWorkflowEvent(10) >= null);


            Assert.IsFalse(new TestWorkflowEvent(10) > (new TestWorkflowEvent(12)));
            Assert.IsFalse(null > new TestWorkflowEvent(10));
            Assert.IsFalse(new TestWorkflowEvent(10) >= (new TestWorkflowEvent(12)));
            Assert.IsFalse(null >= (new TestWorkflowEvent(1)));
        }

        [Test]
        public void Lesser_test()
        {
            Assert.IsTrue(new TestWorkflowEvent(8) < (new TestWorkflowEvent(9)));
            Assert.IsTrue(null < new TestWorkflowEvent(10));
            Assert.IsTrue(new TestWorkflowEvent(10) <= (new TestWorkflowEvent(10)));
            Assert.IsTrue(new TestWorkflowEvent(8) <= (new TestWorkflowEvent(9)));
            Assert.IsTrue(null <= new TestWorkflowEvent(10));


            Assert.IsFalse(new TestWorkflowEvent(13) < (new TestWorkflowEvent(12)));
            Assert.IsFalse(new TestWorkflowEvent(10)< null );
            Assert.IsFalse(new TestWorkflowEvent(15) <= (new TestWorkflowEvent(12)));
            Assert.IsFalse(new TestWorkflowEvent(1)<=null);
        }

        private class TestWorkflowEvent : WorkflowEvent
        {
            public TestWorkflowEvent(long eventId) : base(eventId)
            {
            }
            internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}