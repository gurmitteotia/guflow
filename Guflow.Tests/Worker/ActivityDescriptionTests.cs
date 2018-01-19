// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Configuration;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ActivityDescriptionTests
    {
        [Test]
        public void Throws_exception_when_attribute_is_not_applied()
        {
            Assert.Throws<ActivityDescriptionMissingException>(() => ActivityDescriptionAttribute.FindOn<ActivityWithoutAttribute>());
        }

        [Test]
        public void Timeouts_are_null_when_not_set()
        {
            var activityDescription = ActivityDescriptionAttribute.FindOn<ActivityWithoutTimeoutSet>();

            Assert.IsNull(activityDescription.DefaultHeartbeatTimeout);
            Assert.IsNull(activityDescription.DefaultStartToCloseTimeout);
            Assert.IsNull(activityDescription.DefaultScheduleToCloseTimeout);
            Assert.IsNull(activityDescription.DefaultScheduleToStartTimeout);
        }

        [Test]
        public void Timeouts_are_non_empty_when_set()
        {
            var activityDescription = ActivityDescriptionAttribute.FindOn<ActivityWithTimeoutSet>();

            Assert.That(activityDescription.DefaultHeartbeatTimeout, Is.EqualTo("2"));
            Assert.That(activityDescription.DefaultScheduleToCloseTimeout, Is.EqualTo("3"));
            Assert.That(activityDescription.DefaultScheduleToStartTimeout, Is.EqualTo("4"));
            Assert.That(activityDescription.DefaultStartToCloseTimeout, Is.EqualTo("5"));
        }

        [Test]
        public void Throws_exception_when_version_is_empty()
        {
            Assert.Throws<ConfigurationErrorsException>(() => ActivityDescriptionAttribute.FindOn<ActivityWithEmptyVersion>());
        }

        [Test]
        public void Throws_exception_when_type_does_not_derive_from_activity_class()
        {
            Assert.Throws<NonActivityTypeException>(() => ActivityDescriptionAttribute.FindOn(typeof(NonActivity)));
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentNullException>(() => ActivityDescriptionAttribute.FindOn(null));
        }

        private class ActivityWithoutAttribute : Activity
        {
        }

        [ActivityDescription("1.0")]
        private class ActivityWithoutTimeoutSet : Activity
        {

        }

        [ActivityDescription("1.0", DefaultHeartbeatTimeoutInSeconds = 2, DefaultScheduleToCloseTimeoutInSeconds = 3, DefaultScheduleToStartTimeoutInSeconds = 4, DefaultStartToCloseTimeoutInSeconds = 5)]
        private class ActivityWithTimeoutSet : Activity
        {

        }

        [ActivityDescription("")]
        private class ActivityWithEmptyVersion : Activity
        {

        }
        private class NonActivity
        {

        }
    }
}