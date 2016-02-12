using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class ScheduleActivityDecisionTests
    {

        [Test]
        public void Equality_tests()
        {
            Assert.True(new ScheduleActivityDecision("Download","1.0","First").Equals(new ScheduleActivityDecision("Download","1.0","First")));
        }

        [Test]
        public void Inequality_tests()
        {
            Assert.False(new ScheduleActivityDecision("Download", "1.0", "First").Equals(new ScheduleActivityDecision("Download", "2.0", "First")));
        }
    }
}