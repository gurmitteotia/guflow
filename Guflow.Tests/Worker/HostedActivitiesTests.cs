﻿using System;
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
        public void Returns_matching_hosted_workflow_by_name_and_version()
        {
            var hostedActivity1 = new TestActivity1();
            var hostedActivity2 = new TestActivity2();
            var hostedActivities = _domain.Host(new Activity[] {hostedActivity1, hostedActivity2});

            Assert.That(hostedActivities.FindBy("TestActivity1", "1.0"), Is.EqualTo(hostedActivity1));
            Assert.That(hostedActivities.FindBy("TestActivity2", "2.0"), Is.EqualTo(hostedActivity2));
        }

        [Test]
        public void Throws_exception_when_hosted_workflow_is_not_found()
        {
            var hostedActivity = new TestActivity1();
            var hostedActivities = _domain.Host(new[] {hostedActivity});

            Assert.Throws<ActivityNotHostedException>(() => hostedActivities.FindBy("TestWorkflow2", "2.0"));
        }

        [Test]
        public void Throws_exception_when_same_workflow_is_hosted_twice()
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