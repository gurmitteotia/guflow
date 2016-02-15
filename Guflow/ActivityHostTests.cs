using Moq;
using NUnit.Framework;

namespace Guflow
{
    [TestFixture]
    public class ActivityHostTests
    {
        private Mock<IActivityConnection> _activityConnection;

        [SetUp]
        public void Setup()
        {
            _activityConnection = new Mock<IActivityConnection>();
        }

        [Test]
        public void Should_poll_for_new_work_when_opened()
        {
            var activityHost = new ActivityHost(_activityConnection.Object);

            activityHost.Open();

            _activityConnection.Verify(a=>a.PollForNewTask(),Times.Once);
        }
    }
}