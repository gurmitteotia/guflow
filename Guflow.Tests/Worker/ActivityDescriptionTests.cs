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
        public void Throws_exception_when_activity_description_not_supplied()
        {
            Assert.Throws<ActivityDescriptionMissingException>(() => ActivityDescriptionAttribute.FindOn<ActivityWithoutDescription>());
        }

        [Test]
        public void Activity_description_attribute_has_only_version()
        {
            var d = ActivityDescription.FindOn<ActivityWithVersionOnly>();

            Assert.That(d.Name, Is.EqualTo(nameof(ActivityWithVersionOnly)));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Description, Is.Null);
            Assert.That(d.DefaultTaskListName, Is.Null);
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(0));
            Assert.IsNull(d.DefaultHeartbeatTimeout);
            Assert.IsNull(d.DefaultStartToCloseTimeout);
            Assert.IsNull(d.DefaultScheduleToCloseTimeout);
            Assert.IsNull(d.DefaultScheduleToStartTimeout);
        }

        [Test]
        public void Activity_description_attribute_has_all_properties_set()
        {
            var d = ActivityDescription.FindOn<ActivityWithAllDescriptionProperties>();

            Assert.That(d.Name, Is.EqualTo("namea"));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Description, Is.EqualTo("desc"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tasklist"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(10));
            Assert.That(d.DefaultHeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(d.DefaultScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(d.DefaultScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(d.DefaultStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Throws_exception_when_version_is_empty()
        {
            Assert.Throws<ArgumentException>(() => ActivityDescription.FindOn<ActivityWithEmptyVersion>());
        }

        [Test]
        public void Throws_exception_when_type_does_not_derive_from_activity_class()
        {
            Assert.Throws<NonActivityTypeException>(() => ActivityDescription.FindOn(typeof(NonActivity)));
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentNullException>(() => ActivityDescription.FindOn(null));
        }

        [Test]
        public void Read_activity_description_from_factory_method_when_provided()
        {
            var d = ActivityDescription.FindOn<ActivityWithFactoryDescriptionMethod>();

            Assert.That(d.Name, Is.EqualTo("test"));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Description, Is.EqualTo("desc"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tasklist"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(10));
            Assert.That(d.DefaultHeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(d.DefaultScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(d.DefaultScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(d.DefaultStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Factory_method_has_priority_over_description_attribute()
        {
            var d = ActivityDescription.FindOn<ActivityWithFactoryDescriptionMethodAndAttribute>();

            Assert.That(d.Name, Is.EqualTo("test-diff"));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            
            Assert.That(d.Description, Is.EqualTo("desc-diff"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tasklist-diff"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(11));
            Assert.That(d.DefaultHeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(d.DefaultScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(d.DefaultScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(d.DefaultStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(6)));
        }

        [Test]
        public void Read_activity_description_from_factory_property_when_provided()
        {
            var d = ActivityDescription.FindOn<ActivityWithFactoryDescriptionMethodProperty>();

            Assert.That(d.Name, Is.EqualTo("test"));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Description, Is.EqualTo("desc"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tasklist"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(10));
            Assert.That(d.DefaultHeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(d.DefaultScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(d.DefaultScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(d.DefaultStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        private class ActivityWithoutDescription : Activity
        {
        }

        [ActivityDescription("1.0")]
        private class ActivityWithVersionOnly : Activity
        {

        }

        [ActivityDescription("1.0", DefaultHeartbeatTimeoutInSeconds = 2, DefaultScheduleToCloseTimeoutInSeconds = 3,
            DefaultScheduleToStartTimeoutInSeconds = 4, DefaultStartToCloseTimeoutInSeconds = 5, DefaultTaskListName = "tasklist",
            Description = "desc", DefaultTaskPriority = 10, Name = "namea")]
        private class ActivityWithAllDescriptionProperties : Activity
        {

        }

        [ActivityDescription("")]
        private class ActivityWithEmptyVersion : Activity
        {

        }
        private class NonActivity
        {

        }

        private class ActivityWithFactoryDescriptionMethod : Activity
        {
            private static ActivityDescription ActivityDescription()
            {
                return new ActivityDescription("1.0")
                {
                    Name = "test",
                    DefaultTaskListName = "tasklist",
                    Description = "desc",
                    DefaultTaskPriority = 10,
                    DefaultHeartbeatTimeout = TimeSpan.FromSeconds(2),
                    DefaultScheduleToCloseTimeout = TimeSpan.FromSeconds(3),
                    DefaultScheduleToStartTimeout = TimeSpan.FromSeconds(4),
                    DefaultStartToCloseTimeout = TimeSpan.FromSeconds(5)
                };
            }
        }

        [ActivityDescription("1.0", DefaultHeartbeatTimeoutInSeconds = 2, DefaultScheduleToCloseTimeoutInSeconds = 3,
            DefaultScheduleToStartTimeoutInSeconds = 4, DefaultStartToCloseTimeoutInSeconds = 5, DefaultTaskListName = "tasklist",
            Description = "desc", DefaultTaskPriority = 10)]
        private class ActivityWithFactoryDescriptionMethodAndAttribute : Activity
        {
            private static ActivityDescription FactoryMethod()
            {
                return new ActivityDescription("1.0")
                {
                    Name = "test-diff",
                    DefaultTaskListName = "tasklist-diff",
                    Description = "desc-diff",
                    DefaultTaskPriority = 11,
                    DefaultHeartbeatTimeout = TimeSpan.FromSeconds(3),
                    DefaultScheduleToCloseTimeout = TimeSpan.FromSeconds(4),
                    DefaultScheduleToStartTimeout = TimeSpan.FromSeconds(5),
                    DefaultStartToCloseTimeout = TimeSpan.FromSeconds(6)
                };
            }
        }

        private class ActivityWithFactoryDescriptionMethodProperty : Activity
        {
            private static ActivityDescription ActivityDescription
            =>
                new ActivityDescription("1.0")
                {
                    Name = "test",
                    DefaultTaskListName = "tasklist",
                    Description = "desc",
                    DefaultTaskPriority = 10,
                    DefaultHeartbeatTimeout = TimeSpan.FromSeconds(2),
                    DefaultScheduleToCloseTimeout = TimeSpan.FromSeconds(3),
                    DefaultScheduleToStartTimeout = TimeSpan.FromSeconds(4),
                    DefaultStartToCloseTimeout = TimeSpan.FromSeconds(5)
                };
            
        }
    }
}