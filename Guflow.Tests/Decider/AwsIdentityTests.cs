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
            Assert.That(AwsIdentity.Create("name","ver","pos").Equals(AwsIdentity.Create("name","ver","pos")));
            Assert.That(AwsIdentity.Create("name", "ver", "").Equals(AwsIdentity.Create("name", "ver", "")));
            Assert.That(AwsIdentity.Raw("identity").Equals(AwsIdentity.Raw("identity")));
            Assert.That(AwsIdentity.Create("name", "ver", "pos")==AwsIdentity.Create("name", "ver", "pos"));
            Assert.That(AwsIdentity.Create("name", "ver", "")==AwsIdentity.Create("name", "ver", ""));
            Assert.That(AwsIdentity.Raw("identity")==AwsIdentity.Raw("identity"));
            
            Assert.False(AwsIdentity.Create("name", "ver", "pos").Equals(AwsIdentity.Create("name", "ver", "pos1")));
            Assert.False(AwsIdentity.Create("name", "ver", "pos").Equals(AwsIdentity.Create("name", "ver1", "pos")));
            Assert.False(AwsIdentity.Create("name", "ver", "pos").Equals(AwsIdentity.Create("name1", "ver", "pos")));
            Assert.False(AwsIdentity.Raw("identity").Equals(AwsIdentity.Raw("identity1")));
            Assert.True(AwsIdentity.Create("name", "ver", "pos")!=AwsIdentity.Create("name", "ver", "pos1"));
            Assert.True(AwsIdentity.Create("name", "ver", "pos")!=AwsIdentity.Create("name", "ver1", "pos"));
            Assert.True(AwsIdentity.Create("name", "ver", "pos")!=AwsIdentity.Create("name1", "ver", "pos"));
        }
    }
}