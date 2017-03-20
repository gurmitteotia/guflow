using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    public class HostedActivitiesTests
    {
        private Domain _domain;
        private Mock<IAmazonSimpleWorkflow> _simpleWorkflow;
        [SetUp]
        public void Setup()
        {
            _simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            _domain = new Domain("name", _simpleWorkflow.Object);
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
        public void Throws_exception_when_starting_execution_without_task_queue_for_multiple_hosted_activities()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1), typeof(TestActivity2) }, t => new TestActivity2());
            Assert.Throws<InvalidOperationException>(() => hostedActivities.StartExecution());
        }

        [Test]
        public void Throws_exception_when_starting_execution_without_task_queue_and_hosted_activity_does_not_have_default_task_queue()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) }, t => new TestActivity2());
            Assert.Throws<InvalidOperationException>(() => hostedActivities.StartExecution());
        }

        [Test]
        public void By_default_response_errors_are_unhandled()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });
            _simpleWorkflow.Setup(s=>s.RespondActivityTaskCompletedAsync(It.IsAny<RespondActivityTaskCompletedRequest>(), It.IsAny<CancellationToken>()))
                            .Throws(new UnknownResourceException(""));

            Assert.ThrowsAsync<UnknownResourceException>(() => hostedActivities.SendAsync(new ActivityCompleteResponse("token", "result")));
        }

        [Test]
        public async Task Response_error_can_be_handled_to_retry()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });
            _simpleWorkflow.SetupSequence(s => s.RespondActivityTaskCompletedAsync(It.IsAny<RespondActivityTaskCompletedRequest>(), It.IsAny<CancellationToken>()))
                            .Throws(new UnknownResourceException(""))
                            .Returns(Task.FromResult(new RespondActivityTaskCompletedResponse()));
            hostedActivities.OnResponseError(e => ErrorAction.Retry);

            await hostedActivities.SendAsync(new ActivityCompleteResponse("token", "result"));

            _simpleWorkflow.Verify(w=>w.RespondActivityTaskCompletedAsync(It.IsAny<RespondActivityTaskCompletedRequest>(), It.IsAny<CancellationToken>()),Times.Exactly(2));
        }

        [Test]
        public async Task Response_error_can_be_handled_to_retry_by_generic_error_handler()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });
            _simpleWorkflow.SetupSequence(s => s.RespondActivityTaskCompletedAsync(It.IsAny<RespondActivityTaskCompletedRequest>(), It.IsAny<CancellationToken>()))
                            .Throws(new UnknownResourceException(""))
                            .Returns(Task.FromResult(new RespondActivityTaskCompletedResponse()));
            hostedActivities.OnError(e => ErrorAction.Retry);

            await hostedActivities.SendAsync(new ActivityCompleteResponse("token", "result"));

            _simpleWorkflow.Verify(w => w.RespondActivityTaskCompletedAsync(It.IsAny<RespondActivityTaskCompletedRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public void Invalid_constructor_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(() => _domain.Host((IEnumerable<Type>)null));
            Assert.Throws<ArgumentException>(() => _domain.Host(Enumerable.Empty<Type>()));
            Assert.Throws<ArgumentException>(() => _domain.Host(new[] { (Type)null }));

            Assert.Throws<ArgumentNullException>(() => _domain.Host(new[] { typeof(TestActivity1) }, null));
        }

        [Test]
        public void Invalid_parameters_tests()
        {
            var hostedActivities = _domain.Host(new[] { typeof(TestActivity1) });

            Assert.Throws<ArgumentNullException>(()=> hostedActivities.OnError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnError((HandleError)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnPollingError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnPollingError((HandleError)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnResponseError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => hostedActivities.OnResponseError((HandleError)null));

            Assert.Throws<ArgumentNullException>(() => hostedActivities.Execution = null);
        }

        private void SetupAmazonSwfToThrowsExceptionOnResponse()
        {
                
        }

        [ActivityDescription("1.0")]
        private class TestActivity1 : Activity
        {
            [Execute]
            public void Execute()
            {
                
            }
        }
        [ActivityDescription("2.0")]
        private class TestActivity2 : Activity
        {
            [Execute]
            public void Execute()
            {

            }
        }
    }
}