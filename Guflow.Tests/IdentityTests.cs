using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class IdentityTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(Identity.New("transcode","1.0").Equals(Identity.New("transcode","1.0")));
            Assert.True(Identity.New("transcode", "1.0").Equals(Identity.New("Transcode", "1.0")));
            Assert.True(Identity.New("transcode", null).Equals(Identity.New("Transcode", null)));
            Assert.True(Identity.New("transcode", "1.0","first").Equals(Identity.New("Transcode", "1.0","first")));
            Assert.True(Identity.New("transcode", "1.0", "first").Equals(Identity.New("Transcode", "1.0", "First")));


            Assert.False(Identity.New("transcode", "1.0", "first").Equals(Identity.New("Transcode", "1.0", "second")));
            Assert.False(Identity.New("transcode", "1.0", "first").Equals(Identity.New("Transcode", "2.0", "first")));
            Assert.False(Identity.New("transcode", "1.0", "first").Equals(Identity.New("Download", "1.0", "first")));
            Assert.False(Identity.New("transcode", "1.0", "first").Equals(Identity.New("Transcode", "1.0")));
        }
        [Test]
        public void Json_tests()
        {
            var originalIdentity = Identity.New("transcode", "1.0", "first");
            string jsonIdentity = originalIdentity.ToJson();
            var recreatedFromJson = jsonIdentity.FromJson();

            Assert.That(recreatedFromJson,Is.EqualTo(originalIdentity));
        }
    }
}