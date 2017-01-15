using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ActivityTests
    {
        private ActivityArgs _activityArgs;

        [SetUp]
        public void Setup()
        {
            _activityArgs = new ActivityArgs("input","id" ,"wid", "rid", "token");
        }

        [Test]
        public void Throws_exception_when_activity_does_not_have_execution_method()
        {
            Assert.Throws<ActivityExecutionMethodException>(() => new NoExecutionMethodActivity());
        }

        [Test]
        public void Throws_exception_when_activity_has_more_than_one_execution_method()
        {
            Assert.Throws<ActivityExecutionMethodException>(() => new MoreThanOnExecutionMethod());
        }

        [Test]
        public async Task Execution_return_defferred_response_when_return_type_of_execution_method_is_void()
        {
            var activity = new ExecutionMethodWithVoidReturnTypeActivity();

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(ActivityResponse.Defferred));
        }

        [Test]
        public async Task Execution_return_defferred_response_when_return_type_of_execution_method_is_Task()
        {
            var activity = new ExecutionMethodWithTaskReturnTypeActivity();

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(ActivityResponse.Defferred));
        }

        [Test]
        public async Task Execution_method_can_return_custom_activity_response_asynchronously()
        {
            var response = new Mock<ActivityResponse>();
            var activity = new CustomAsynchronousResponseActivity(response.Object);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(response.Object));
        }

        [Test]
        public async Task Execution_method_can_return_custom_activity_response_synchronously()
        {
            var response = new Mock<ActivityResponse>();
            var activity = new CustomSynchronousResponseActivity(response.Object);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(response.Object));
        }

        [Test]
        public async Task Execution_method_can_return_primitive_data_type_in_activity_response_asynchronously()
        {
            var activity = new PrimitiveTypeAsynchronousResponseActivity(10);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(ActivityResponse.Complete("10")));
        }

        [Test]
        public async Task Execution_method_can_return_custom_data_type_in_activity_response_asynchronously()
        {
            var customData = new CustomData() {Id = 10, Name = "hello"};
            var activity = new CustomTypeAsynchronousResponseActivity(customData);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(ActivityResponse.Complete(customData.ToJson())));
        }

        [Test]
        public async Task Execution_method_can_return_primitive_data_type_in_activity_response_synchronously()
        {
            var activity = new PrimitiveTypeSynchronousResponseActivity(10);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(ActivityResponse.Complete("10")));
        }
        [Test]
        public async Task Execution_method_can_return_custom_data_type_in_activity_response_synchronously()
        {
            var customData = new CustomData { Id = 10, Name = "hello" };
            var activity = new CustomTypeSynchronousResponseActivity(customData);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(ActivityResponse.Complete(customData.ToJson())));
        }
        [Test]
        public async Task By_default_execution_method_convert_exception_to_failed_response()
        {
            var activity = new ActivityThrowingException(new IndexOutOfRangeException("blah"));

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(ActivityResponse.Fail("IndexOutOfRangeException", "blah")));
        }

        [Test]
        public void Activity_can_be_customized_to_not_convert_exception_to_failed_response()
        {
            var activity = new ActivityThrowingException(new IndexOutOfRangeException("blah"), failOnException:false);

            Assert.ThrowsAsync<IndexOutOfRangeException>(async ()=> await activity.ExecuteAsync(_activityArgs));
        }

        [Test]
        public async Task Activity_args_can_be_deserialized_into_method_parameters()
        {
            var activityArgs = new ActivityArgs(new Input {Id = 10, Details = "det"}.ToJson(), "id", "wid", "rid", "token");
            var activity = new ActivityMethodWithArgs();

            await activity.ExecuteAsync(activityArgs);

            Assert.That(activity.Input.Id, Is.EqualTo(10));
            Assert.That(activity.Input.Details, Is.EqualTo("det"));
            Assert.That(activity.TaskToken, Is.EqualTo("token"));
        }

        [Test]
        public void Heartbeat_started_when_it_it_enabled_on_activity()
        {
            
        }

        private class NoExecutionMethodActivity : Activity
        {
        }
        private class MoreThanOnExecutionMethod : Activity
        {
            [Execute]
            public void Execute1() { }
            [Execute]
            public void Execute2() { }
        }
        private class ExecutionMethodWithVoidReturnTypeActivity : Activity
        {
            [Execute]
            public void Execute()
            {
            }
        }
        private class ExecutionMethodWithTaskReturnTypeActivity : Activity
        {
            [Execute]
            public async Task Execute()
            {
                await Task.Delay(0);
            }
        }

        private class CustomAsynchronousResponseActivity : Activity
        {
            private readonly ActivityResponse _response;

            public CustomAsynchronousResponseActivity(ActivityResponse response)
            {
                _response = response;
            }

            [Execute]
            public async Task<ActivityResponse> Execute()
            {
                await Task.Delay(0);
                return _response;
            }
        }
        private class CustomSynchronousResponseActivity : Activity
        {
            private readonly ActivityResponse _response;

            public CustomSynchronousResponseActivity(ActivityResponse response)
            {
                _response = response;
            }

            [Execute]
            public ActivityResponse Execute()
            {
                return _response;
            }
        }
        private class PrimitiveTypeAsynchronousResponseActivity : Activity
        {
            private readonly int _response;

            public PrimitiveTypeAsynchronousResponseActivity(int response)
            {
                _response = response;
            }

            [Execute]
            public async Task<int> Execute()
            {
                await Task.Delay(0);
                return _response;
            }
        }

        private class CustomData
        {
            public int Id;
            public string Name;
        }
        private class CustomTypeAsynchronousResponseActivity : Activity
        {
            private readonly CustomData _response;

            public CustomTypeAsynchronousResponseActivity(CustomData response)
            {
                _response = response;
            }

            [Execute]
            public async Task<CustomData> Execute()
            {
                await Task.Delay(0);
                return _response;
            }
        }
        private class PrimitiveTypeSynchronousResponseActivity : Activity
        {
            private readonly int _response;

            public PrimitiveTypeSynchronousResponseActivity(int response)
            {
                _response = response;
            }

            [Execute]
            public int Execute()
            {
                return _response;
            }
        }
        private class CustomTypeSynchronousResponseActivity : Activity
        {
            private readonly CustomData _response;

            public CustomTypeSynchronousResponseActivity(CustomData response)
            {
                _response = response;
            }

            [Execute]
            public CustomData Execute()
            {
                return _response;
            }
        }

        private class ActivityThrowingException : Activity
        {
            private readonly Exception _exception;

            public ActivityThrowingException(Exception exception, bool failOnException= true)
            {
                _exception = exception;
                FailOnException = failOnException;
            }

            [Execute]
            public void ThrowError()
            {
                throw _exception;
            }
        }

        private class ActivityMethodWithArgs : Activity
        {
            [Execute]
            public void ActivityMethod(Input input, string taskToken)
            {
                Input = input;
                TaskToken = taskToken;
            }

            public Input Input { get; private set; }
            public string TaskToken { get; private set; }
        }

        [EnableHeartbeat]
        private class ActivityWithHeartbeat : Activity
        {
            private readonly TimeSpan _activityExecutionTime;

            public ActivityWithHeartbeat(string details, TimeSpan activityExecutionTime)
            {
                _activityExecutionTime = activityExecutionTime;
                Hearbeat.ProvideDetailsFrom(()=>details);
            }
            [Execute]
            public void TranscodeMe()
            {
                Thread.Sleep(_activityExecutionTime);
            }
        }

        private class Input
        {
            public int Id;
            public string Details;
        }
    }
}