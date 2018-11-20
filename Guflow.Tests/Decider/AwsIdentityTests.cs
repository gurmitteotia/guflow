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
            Assert.That(ScheduleId.Create("name","ver","pos").Equals(ScheduleId.Create("name","ver","pos")));
            Assert.That(ScheduleId.Create("name", "ver", "").Equals(ScheduleId.Create("name", "ver", "")));
            Assert.That(ScheduleId.Raw("identity").Equals(ScheduleId.Raw("identity")));
            Assert.That(ScheduleId.Create("name", "ver", "pos")==ScheduleId.Create("name", "ver", "pos"));
            Assert.That(ScheduleId.Create("name", "ver", "")==ScheduleId.Create("name", "ver", ""));
            Assert.That(ScheduleId.Raw("identity")==ScheduleId.Raw("identity"));
            
            Assert.False(ScheduleId.Create("name", "ver", "pos").Equals(ScheduleId.Create("name", "ver", "pos1")));
            Assert.False(ScheduleId.Create("name", "ver", "pos").Equals(ScheduleId.Create("name", "ver1", "pos")));
            Assert.False(ScheduleId.Create("name", "ver", "pos").Equals(ScheduleId.Create("name1", "ver", "pos")));
            Assert.False(ScheduleId.Raw("identity").Equals(ScheduleId.Raw("identity1")));
            Assert.True(ScheduleId.Create("name", "ver", "pos")!=ScheduleId.Create("name", "ver", "pos1"));
            Assert.True(ScheduleId.Create("name", "ver", "pos")!=ScheduleId.Create("name", "ver1", "pos"));
            Assert.True(ScheduleId.Create("name", "ver", "pos")!=ScheduleId.Create("name1", "ver", "pos"));
        }
    }
}