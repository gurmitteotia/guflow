// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class AwsIdentityTests
    {
        [Test]
        public void Equality_test()
        {
            Assert.That(SwfIdentity.Create("name","ver","pos").Equals(SwfIdentity.Create("name","ver","pos")));
            Assert.That(SwfIdentity.Create("name", "ver", "").Equals(SwfIdentity.Create("name", "ver", "")));
            Assert.That(SwfIdentity.Raw("identity").Equals(SwfIdentity.Raw("identity")));
            Assert.That(SwfIdentity.Create("name", "ver", "pos")==SwfIdentity.Create("name", "ver", "pos"));
            Assert.That(SwfIdentity.Create("name", "ver", "")==SwfIdentity.Create("name", "ver", ""));
            Assert.That(SwfIdentity.Raw("identity")==SwfIdentity.Raw("identity"));
            
            Assert.False(SwfIdentity.Create("name", "ver", "pos").Equals(SwfIdentity.Create("name", "ver", "pos1")));
            Assert.False(SwfIdentity.Create("name", "ver", "pos").Equals(SwfIdentity.Create("name", "ver1", "pos")));
            Assert.False(SwfIdentity.Create("name", "ver", "pos").Equals(SwfIdentity.Create("name1", "ver", "pos")));
            Assert.False(SwfIdentity.Raw("identity").Equals(SwfIdentity.Raw("identity1")));
            Assert.True(SwfIdentity.Create("name", "ver", "pos")!=SwfIdentity.Create("name", "ver", "pos1"));
            Assert.True(SwfIdentity.Create("name", "ver", "pos")!=SwfIdentity.Create("name", "ver1", "pos"));
            Assert.True(SwfIdentity.Create("name", "ver", "pos")!=SwfIdentity.Create("name1", "ver", "pos"));
        }
    }
}