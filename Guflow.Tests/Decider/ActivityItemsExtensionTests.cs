// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Guflow.Decider;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityItemsExtensionTests
    {
        [Test]
        public void First_test()
        {
            var activities = new[]
                {CreateActivity("name1", "ver1", "pname1"), CreateActivity("name2", "ver2", "pname2")};

            Assert.That(activities.First("name1","ver1", "pname1"), Is.EqualTo(activities[0]));
            Assert.That(activities.First<ActivityTest>("pname2"), Is.EqualTo(activities[1]));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            IEnumerable<IActivityItem> activityItems = null;

            Assert.Throws<ArgumentNullException>(() => activityItems.First("", ""));
            Assert.Throws<ArgumentNullException>(() => activityItems.First<ActivityTest>());
        }
        private static IActivityItem CreateActivity(string name, string version, string positionName = "")
        {
            return new ActivityItem(Identity.New(name, version, positionName), Mock.Of<IWorkflow>());
        }

        [ActivityDescription("ver2", Name = "name2")]
        private class ActivityTest : Activity
        {
            
        }
    }
}