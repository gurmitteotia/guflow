using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ScheduleActivityDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new ScheduleActivityDecision(Identity.New("Download","1.0","First")).Equals(new ScheduleActivityDecision(Identity.New("Download","1.0","First"))));
            Assert.False(new ScheduleActivityDecision(Identity.New("Download", "1.0", "First")).Equals(new ScheduleActivityDecision(Identity.New("Download", "2.0", "First"))));
        }
    }
}