using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    public class HostedActivitiesTests
    {
        private Domain _domain;
        [SetUp]
        public void Setup()
        {
            _domain = new Domain("name", new Mock<IAmazonSimpleWorkflow>().Object);
        }

        [Test]
        public void Returns_matching_hosted_activity_by_name_and_version()
        {
            var hostedActivity1 = new TestActivity1();
            var hostedActivity2 = new TestActivity2();
            var hostedActivities = _domain.Host(new Activity[] {hostedActivity1, hostedActivity2});

            Assert.That(hostedActivities.FindBy("TestActivity1", "1.0"), Is.EqualTo(hostedActivity1));
            Assert.That(hostedActivities.FindBy("TestActivity2", "2.0"), Is.EqualTo(hostedActivity2));
        }

        [Test]
        public void Throws_exception_when_hosted_activity_is_not_found()
        {
            var hostedActivity = new TestActivity1();
            var hostedActivities = _domain.Host(new[] {hostedActivity});

            Assert.Throws<ActivityNotHostedException>(() => hostedActivities.FindBy("TestWorkflow2", "2.0"));
        }

        [Test]
        public void Throws_exception_when_same_activity_is_hosted_twice()
        {
            var hostedActivity1 = new TestActivity1();
            var hostedActivity2 = new TestActivity1();
            Assert.Throws<ActivityAlreadyHostedException>(() => _domain.Host(new []{hostedActivity1, hostedActivity2}));
        }

        [Test]
        public void Invalid_constructor_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(() => _domain.Host((IEnumerable<Activity>)null));
            Assert.Throws<ArgumentException>(() => _domain.Host(Enumerable.Empty<Activity>()));
            Assert.Throws<ArgumentException>(() => _domain.Host(new[] { (Activity)null }));

            Assert.Throws<ArgumentNullException>(() => _domain.Host((IEnumerable<Type>)null));
            Assert.Throws<ArgumentException>(() => _domain.Host(Enumerable.Empty<Type>()));
            Assert.Throws<ArgumentException>(() => _domain.Host(new[] { (Type)null }));

            Assert.Throws<ArgumentNullException>(() => _domain.Host(new[] { typeof(TestActivity1) }, null));
        }

        [Test]
        public void Return_the_new_instance_of_activity_type_by_name_and_version()
        {
            var hostedActivities = _domain.Host(new[] {typeof(TestActivity1), typeof(TestActivity2)});

            var hostedActivity = hostedActivities.FindBy("TestActivity1", "1.0");

            Assert.That(hostedActivity.GetType(), Is.EqualTo(typeof(TestActivity1)));
        }
        [Test]
        public void Throws_exception_when_hosted_activity_type_is_not_found()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });

            Assert.Throws<ActivityNotHostedException>(()=> hostedActivities.FindBy("TestActivity1", "5.0"));
        }

        [Test]
        public void Throws_exception_when_same_activity_type_is_hosted_twice()
        {
            Assert.Throws<ActivityAlreadyHostedException>(() => _domain.Host(new[] { typeof(TestActivity1), typeof(TestActivity1)}));
        }

        [Test]
        public void Return_the_instance_of_activity_type_from_activity_creator()
        {
            var expectedInstance = new TestActivity1();
            Func<Type, Activity> instanceCreator = t =>
            {
                Assert.That(t, Is.EqualTo(typeof(TestActivity1)));
                return expectedInstance;
            };
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1)}, instanceCreator);

            var actualInstance = hostedActivities.FindBy("TestActivity1", "1.0");

            Assert.That(actualInstance, Is.EqualTo(expectedInstance));
        }

        [Test]
        public void Throws_exception_when_instance_creator_returns_null_activity_instance()
        {
            var hostedActivities = _domain.Host(new[] {typeof(TestActivity1)}, t => null);
            Assert.Throws<ActivityInstanceCreationException>(() => hostedActivities.FindBy("TestActivity1", "1.0"));
        }

        [Test]
        public void Throws_exception_when_instance_creator_returns_instance_for_different_activity()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) }, t=>new TestActivity2());
            Assert.Throws<ActivityInstanceMismatchedException>(() => hostedActivities.FindBy("TestActivity1", "1.0"));
        }

        [Test]
        public void Start_execution_throws_exception_when_task_queue_is_null()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });

            Assert.Throws<ArgumentNullException>(() => hostedActivities.StartExecution(null));
        }

        [Test]
        public void Invalid_parameters_tests()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });

            Assert.Throws<ArgumentNullException>(()=> hostedActivities.OnError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnError((HandleError)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnPollingError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnPollingError((HandleError)null));

            Assert.Throws<ArgumentNullException>(() => hostedActivities.Execution = null);
        }
        [ActivityDescription("1.0")]
        private class TestActivity1 : Activity
        {
        }
        [ActivityDescription("2.0")]
        private class TestActivity2 : Activity
        {
        }
    }
}