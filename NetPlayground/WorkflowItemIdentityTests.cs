using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class WorkflowItemIdentityTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new WorkflowItemIdentity("Download","1.0","First").Equals(new WorkflowItemIdentity("Download","1.0","First")));
            Assert.True(new WorkflowItemIdentity("Download", "1.0", "").Equals(new WorkflowItemIdentity("Download", "1.0", "")));
            Assert.True(new WorkflowItemIdentity("Download", "", "First").Equals(new WorkflowItemIdentity("Download", "", "First")));
        }

        [Test]
        public void Inequality_tests()
        {
            Assert.False(new WorkflowItemIdentity("Download", "1.0", "First").Equals(new WorkflowItemIdentity("Download", "1.0", "Second")));
            Assert.False(new WorkflowItemIdentity("Download", "1.0", "").Equals(new WorkflowItemIdentity("Transcode", "1.0", "")));
            Assert.False(new WorkflowItemIdentity("Download", "", "").Equals(new WorkflowItemIdentity("Transcode", "", "")));
        }
    }
}