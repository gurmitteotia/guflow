using System;
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
        [Test]
        public void Activity_id_test()
        {
            var originalIdentity = Identity.New("transcode", "1.0", "first");
            string id = originalIdentity.Id;
            var recreatedFromId = Identity.FromId(id);

            Assert.That(recreatedFromId,Is.EqualTo(originalIdentity));
        }
        [Test]
        public void Timer_id_test()
        {
            var originalIdentity = Identity.Timer("transcode");
            string id = originalIdentity.Id;
            var recreatedFromId = Identity.FromId(id);

            Assert.That(recreatedFromId, Is.EqualTo(originalIdentity));
        }

        [Test]
        public void Throws_exception_when_id_is_invalid()
        {
            Assert.Throws<ArgumentException>(() => Identity.FromId("somename"));
        }
        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentException>(()=>Identity.New("shouldnothave;", "version", "first"));
            Assert.Throws<ArgumentException>(() => Identity.New("download", "shouldnothave;", "first"));
            Assert.Throws<ArgumentException>(() => Identity.New("download", "version", "shouldnothave;"));
        }
    }
}