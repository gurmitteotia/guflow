// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        private const string _taskToken = "token";
        private const int HeartbeatInterval = 10;
        private const int WaitPeriod = HeartbeatInterval * 5000;

        [SetUp]
        public void Setup()
        {
            _activityArgs = new ActivityArgs("input","id" ,"wid", "rid", _taskToken);
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

            Assert.That(response, Is.EqualTo(ActivityResponse.Defer));
        }

        [Test]
        public async Task Execution_return_defferred_response_when_return_type_of_execution_method_is_Task()
        {
            var activity = new ExecutionMethodWithTaskReturnTypeActivity();

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(ActivityResponse.Defer));
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

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompletedResponse("10")));
        }

        [Test]
        public async Task Execution_method_can_return_custom_data_type_in_activity_response_asynchronously()
        {
            var customData = new CustomData() {Id = 10, Name = "hello"};
            var activity = new CustomTypeAsynchronousResponseActivity(customData);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompletedResponse(customData.ToJson())));
        }

        [Test]
        public async Task Execution_method_can_return_primitive_data_type_in_activity_response_synchronously()
        {
            var activity = new PrimitiveTypeSynchronousResponseActivity(10);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompletedResponse("10")));
        }
        [Test]
        public async Task Execution_method_can_return_custom_data_type_in_activity_response_synchronously()
        {
            var customData = new CustomData { Id = 10, Name = "hello" };
            var activity = new CustomTypeSynchronousResponseActivity(customData);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompletedResponse(customData.ToJson())));
        }
        [Test]
        public async Task By_default_execution_method_convert_exception_to_failed_response()
        {
            var activity = new ActivityThrowingException(new IndexOutOfRangeException("blah"));

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityFailedResponse("IndexOutOfRangeException", "blah")));
        }

        [Test]
        public void Activity_can_be_customized_to_not_convert_exception_to_failed_response()
        {
            var activity = new ActivityThrowingException(new IndexOutOfRangeException("blah"), failOnException:false);

            Assert.ThrowsAsync<IndexOutOfRangeException>(async ()=> await activity.ExecuteAsync(_activityArgs));
        }

        [Test]
        public async Task Activity_convert_the_exception_to_fail_reponse_without_using_error_handler()
        {
            var activity = new ActivityThrowingException(new IndexOutOfRangeException("blah"));
            activity.SetErrorHandler(ErrorHandler.Continue);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityFailedResponse("IndexOutOfRangeException", "blah")));
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
        public async Task Heartbeat_started_when_it_it_enabled_on_activity_by_attribute()
        {
            var hearbeatApi = new TestHeartbeatSwfApi(()=>false);

            var activity = new ActivityWithHeartbeatEnabledByAttribute("details", TimeSpan.FromSeconds(5));
            activity.SetSwfApi(hearbeatApi);
            await activity.ExecuteAsync(_activityArgs);
            Assert.IsTrue(hearbeatApi.Wait(WaitPeriod));

            Assert.That(hearbeatApi.HearbeatRecorded);
            Assert.That(hearbeatApi.Details, Is.EqualTo("details"));
        }

        [Test]
        public void Activity_execution_throws_exception_when_heartbeat_is_enabled_but_interval_is_not_configured()
        {
            Assert.Throws<ActivityConfigurationException>(()=> new ActivityWithHearbeatIntervalMissing());
        }

        [Test]
        public async Task Does_not_send_hearbeat_to_amazon_swf_when_not_enabled()
        {
            var hearbeatApi = new TestHeartbeatSwfApi(() => false);
            var activity = new ActivityWithoutHearbeat("details", TimeSpan.FromSeconds(5));
            activity.SetSwfApi(hearbeatApi);
            await activity.ExecuteAsync(_activityArgs);

            Assert.IsFalse(hearbeatApi.Wait(HeartbeatInterval*100));
            Assert.IsFalse(hearbeatApi.HearbeatRecorded);
        }

        [Test]
        public async Task Heartbeat_started_when_it_it_enabled_on_activity_programmatically()
        {
            var hearbeatApi = new TestHeartbeatSwfApi(() => false);
            var activity = new ActivityWithHeartbeatEnabledProgrammatically("details", TimeSpan.FromSeconds(5));
            activity.SetSwfApi(hearbeatApi);
            await activity.ExecuteAsync(_activityArgs);

            Assert.IsTrue(hearbeatApi.Wait(WaitPeriod));
            Assert.That(hearbeatApi.Details, Is.EqualTo("details"));
        }

        [Test]
        public async Task Activity_can_return_complete_response()
        {
            var activity = new ActivityReturningCompleteResponse(new{id=1});

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityCompletedResponse(new{id=1})));
        }

        [Test]
        public async Task Activity_can_return_cancel_response()
        {
            var activity = new ActivityReturningCancelResponse(new{detail ="details"});

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityCancelledResponse(new { detail = "details" })));
        }

        [Test]
        public async Task Activity_can_return_fail_response()
        {
            var activity = new ActivityReturningFailResponse(new{detail ="detail"}, "reason");

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityFailedResponse("reason", new { detail = "detail" })));
        }

        [Test]
        public async Task By_default_execution_method_convert_task_cancellation_exception_to_activity_cancel_response()
        {
            var activity = new CancelledActivity();

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityCancelledResponse("Activity name: CancelledActivity and version: 1.0 is cancelled.")));
        }

        [Test]
        public async Task Cancellation_token_is_set_when_heartbeat_returns_cancellation_request()
        {
            var hearbeatApi = new TestHeartbeatSwfApi(()=>true, setEventOnCalledTimes:2);
            var activity = new ActivityWithCancellationToken(executionTime: TimeSpan.FromSeconds(1));
            activity.SetSwfApi(hearbeatApi);
           
            await activity.ExecuteAsync(_activityArgs);
            
            Assert.IsTrue(hearbeatApi.Wait(WaitPeriod));
            Assert.That(activity.CancellationRequested, Is.True);
        }

        private class NoExecutionMethodActivity : Activity
        {
        }
        private class MoreThanOnExecutionMethod : Activity
        {
            [ActivityMethod]
            public void Execute1() { }
            [ActivityMethod]
            public void Execute2() { }
        }
        private class ExecutionMethodWithVoidReturnTypeActivity : Activity
        {
            [ActivityMethod]
            public void Execute()
            {
            }
        }
        private class ExecutionMethodWithTaskReturnTypeActivity : Activity
        {
            [ActivityMethod]
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

            [ActivityMethod]
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

            [ActivityMethod]
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

            [ActivityMethod]
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

            [ActivityMethod]
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

            [ActivityMethod]
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

            [ActivityMethod]
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

            [ActivityMethod]
            public void ThrowError()
            {
                throw _exception;
            }
        }

        private class ActivityMethodWithArgs : Activity
        {
            [ActivityMethod]
            public void ActivityMethod(Input input, string taskToken)
            {
                Input = input;
                TaskToken = taskToken;
            }

            public Input Input { get; private set; }
            public string TaskToken { get; private set; }
        }

        [EnableHeartbeat(IntervalInMilliSeconds = HeartbeatInterval)]
        private class ActivityWithHeartbeatEnabledByAttribute : Activity
        {
            private readonly TimeSpan _activityExecutionTime;

            public ActivityWithHeartbeatEnabledByAttribute(string details, TimeSpan activityExecutionTime)
            {
                _activityExecutionTime = activityExecutionTime;
                Hearbeat.ProvideDetails(()=>details);
            }
            [ActivityMethod]
            public void TranscodeMe()
            {
                Thread.Sleep(_activityExecutionTime);
            }
        }

        [EnableHeartbeat]
        [ActivityDescription("1.0")]
        private class ActivityWithHearbeatIntervalMissing : Activity
        {
            public ActivityWithHearbeatIntervalMissing()
            {
            }
            [ActivityMethod]
            public void TranscodeMe()
            {

            }
        }

        private class ActivityWithoutHearbeat : Activity
        {
            private readonly TimeSpan _activityExecutionTime;

            public ActivityWithoutHearbeat(string details, TimeSpan activityExecutionTime)
            {
                _activityExecutionTime = activityExecutionTime;
                Hearbeat.ProvideDetails(() => details);
            }
            [ActivityMethod]
            public void TranscodeMe()
            {
                Thread.Sleep(_activityExecutionTime);
            }
        }

        private class ActivityWithHeartbeatEnabledProgrammatically : Activity
        {
            private readonly TimeSpan _activityExecutionTime;

            public ActivityWithHeartbeatEnabledProgrammatically(string details, TimeSpan activityExecutionTime)
            {
                _activityExecutionTime = activityExecutionTime;
                Hearbeat.Enable(TimeSpan.FromMilliseconds(HeartbeatInterval));
                Hearbeat.ProvideDetails(() => details);
            }
            [ActivityMethod]
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

        private class ActivityReturningCompleteResponse : Activity
        {
            private readonly object _result;

            public ActivityReturningCompleteResponse(object result)
            {
                _result = result;
            }

            [ActivityMethod]
            public ActivityResponse Execute()
            {
                return Complete(_result);
            }
        }

        private class ActivityReturningCancelResponse : Activity
        {
            private readonly object _details;

            public ActivityReturningCancelResponse(object details)
            {
                _details = details;
            }

            [ActivityMethod]
            public ActivityResponse Execute()
            {
                return Cancel(_details);
            }
        }

        private class ActivityReturningFailResponse : Activity
        {
            private readonly string _reason;
            private readonly object _details;

            public ActivityReturningFailResponse(object details, string reason)
            {
                _details = details;
                _reason = reason;
            }

            [ActivityMethod]
            public ActivityResponse Execute()
            {
                return Fail(_reason, _details);
            }
        }
       
        [EnableHeartbeat(IntervalInMilliSeconds = 1)]
        private class ActivityWithCancellationToken : Activity
        {
            private CancellationToken _cancellationToken;
            private readonly TimeSpan _executionTime;

            public ActivityWithCancellationToken(TimeSpan executionTime)
            {
                _executionTime = executionTime;
            }

            [ActivityMethod]
            public void Execute(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
                Thread.Sleep(_executionTime);
            }

            public bool CancellationRequested => _cancellationToken.IsCancellationRequested;
        }

        [ActivityDescription("1.0")]
        private class CancelledActivity : Activity
        {
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public CancelledActivity()
            {
                _cancellationTokenSource.Cancel();
            }
            [ActivityMethod]
            public void Execute()
            {
                if(_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }
    }
}