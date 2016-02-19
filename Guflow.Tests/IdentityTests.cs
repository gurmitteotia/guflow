using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class IdentityTests
    {

        [Test]
        public void Equality_tests()
        {
            Assert.True(new Identity("transcode","1.0").Equals(new Identity("transcode","1.0")));
            Assert.True(new Identity("transcode", "1.0").Equals(new Identity("Transcode", "1.0")));
            Assert.True(new Identity("transcode", null).Equals(new Identity("Transcode", null)));
            Assert.True(new Identity("transcode", "1.0","first").Equals(new Identity("Transcode", "1.0","first")));
            Assert.True(new Identity("transcode", "1.0", "first").Equals(new Identity("Transcode", "1.0", "First")));


            Assert.False(new Identity("transcode", "1.0", "first").Equals(new Identity("Transcode", "1.0", "second")));
            Assert.False(new Identity("transcode", "1.0", "first").Equals(new Identity("Transcode", "2.0", "first")));
            Assert.False(new Identity("transcode", "1.0", "first").Equals(new Identity("Download", "1.0", "first")));
            Assert.False(new Identity("transcode", "1.0", "first").Equals(new Identity("Transcode", "1.0")));
        }
    }
}