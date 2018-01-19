// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
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
            string jsonIdentity = originalIdentity.To(IdentityFormat.Json);
            var recreatedFromJson = Identity.From(jsonIdentity, IdentityFormat.Json);

            Assert.That(recreatedFromJson,Is.EqualTo(originalIdentity));
        }
        [Test]
        public void Allowed_length_test()
        {
            Assert.DoesNotThrow(() => Identity.New(GetStringWithLength(200), GetStringWithLength(50), GetStringWithLength(4)));
        }

        private string GetStringWithLength(int length)
        {
            var data = new char[length];
            for (int i = 0; i < data.Length; i++)
                data[i] = 'a';
            return new string(data);
        }
    }
}