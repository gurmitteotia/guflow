using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
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
        private Mock<IAmazonSimpleWorkflow> _amazonSimpleWorkflow;
        private const int _heartbeatInterval = 10;

        [SetUp]
        public void Setup()
        {
            _amazonSimpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
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

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompleteResponse(_taskToken, "10")));
        }

        [Test]
        public async Task Execution_method_can_return_custom_data_type_in_activity_response_asynchronously()
        {
            var customData = new CustomData() {Id = 10, Name = "hello"};
            var activity = new CustomTypeAsynchronousResponseActivity(customData);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompleteResponse(_taskToken, customData.ToJson())));
        }

        [Test]
        public async Task Execution_method_can_return_primitive_data_type_in_activity_response_synchronously()
        {
            var activity = new PrimitiveTypeSynchronousResponseActivity(10);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompleteResponse(_taskToken, "10")));
        }
        [Test]
        public async Task Execution_method_can_return_custom_data_type_in_activity_response_synchronously()
        {
            var customData = new CustomData { Id = 10, Name = "hello" };
            var activity = new CustomTypeSynchronousResponseActivity(customData);

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityCompleteResponse(_taskToken, customData.ToJson())));
        }
        [Test]
        public async Task By_default_execution_method_convert_exception_to_failed_response()
        {
            var activity = new ActivityThrowingException(new IndexOutOfRangeException("blah"));

            var actualResponse = await activity.ExecuteAsync(_activityArgs);

            Assert.That(actualResponse, Is.EqualTo(new ActivityFailResponse(_taskToken, "IndexOutOfRangeException", "blah")));
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

            Assert.That(actualResponse, Is.EqualTo(new ActivityFailResponse(_taskToken, "IndexOutOfRangeException", "blah")));
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
            var heartbeatEvent = SetupUpAmazonSWFCallbackToRaiseEvent();

            var activity = new ActivityWithHeartbeatEnabledByAttribute("details", TimeSpan.FromSeconds(1));
            activity.SetAmazonSwfClient(_amazonSimpleWorkflow.Object);
            await activity.ExecuteAsync(_activityArgs);
            Assert.IsTrue(heartbeatEvent.WaitOne(_heartbeatInterval*500));

            AssertThatHearbeatIsSendToAmazonSwf("details");
        }

        [Test]
        public void Activity_execution_throws_exception_when_heartbeat_is_enabled_but_interval_is_not_configured()
        {
            Assert.Throws<ActivityConfigurationException>(()=> new ActivityWithHearbeatIntervalMissing());
        }

        [Test]
        public async Task Does_not_send_hearbeat_to_amazon_swf_when_not_enabled()
        {
            var activity = new ActivityWithoutHearbeat("details", TimeSpan.FromSeconds(1));
            activity.SetAmazonSwfClient(_amazonSimpleWorkflow.Object);
            await activity.ExecuteAsync(_activityArgs);

            AssertThatNoHearbeatIsSendToAmazonSwf();
        }

        [Test]
        public async Task Heartbeat_started_when_it_it_enabled_on_activity_programmatically()
        {
            var heartbeatEvent = SetupUpAmazonSWFCallbackToRaiseEvent();
            var activity = new ActivityWithHeartbeatEnabledProgrammatically("details", TimeSpan.FromSeconds(1));
            activity.SetAmazonSwfClient(_amazonSimpleWorkflow.Object);
            await activity.ExecuteAsync(_activityArgs);

            Assert.IsTrue(heartbeatEvent.WaitOne(_heartbeatInterval*500));
            AssertThatHearbeatIsSendToAmazonSwf("details");
        }

        [Test]
        public async Task Activity_can_return_complete_response()
        {
            var activity = new ActivityReturningCompleteResponse("result");

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityCompleteResponse(_taskToken, "result")));
        }

        [Test]
        public async Task Activity_can_return_cancel_response()
        {
            var activity = new ActivityReturningCancelResponse("details");

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityCancelResponse(_taskToken, "details")));
        }

        [Test]
        public async Task Activity_can_return_fail_response()
        {
            var activity = new ActivityReturningFailResponse("details", "reason");

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityFailResponse(_taskToken, "reason", "details")));
        }

        [Test]
        public async Task By_default_execution_method_convert_task_cancellation_exception_to_activity_cancel_response()
        {
            var activity = new CancelledActivity();

            var response = await activity.ExecuteAsync(_activityArgs);

            Assert.That(response, Is.EqualTo(new ActivityCancelResponse(_taskToken, "The operation was canceled.")));
        }

        [Test]
        public async Task Cancellation_token_is_set_when_heartbeat_returns_cancellation_request()
        {
            var heartbeatEvent = AmazonSwfReturnActivityCancellationRequested();
            var activity = new ActivityWithCancellationToken(executionTime: TimeSpan.FromSeconds(1));
            activity.SetAmazonSwfClient(_amazonSimpleWorkflow.Object);
           
            await activity.ExecuteAsync(_activityArgs);
            
            Assert.IsTrue(heartbeatEvent.WaitOne(_heartbeatInterval *100));
            Assert.That(activity.CancellationRequested, Is.True);
        }

        private void AssertThatHearbeatIsSendToAmazonSwf(string details)
        {
            Func<RecordActivityTaskHeartbeatRequest, bool> req = r =>
            {
                Assert.That(r.Details, Is.EqualTo(details));
                Assert.That(r.TaskToken, Is.EqualTo(_activityArgs.TaskToken));
                return true;
            };
            _amazonSimpleWorkflow.Verify(s=>s.RecordActivityTaskHeartbeatAsync(It.Is<RecordActivityTaskHeartbeatRequest>(r=>req(r)), It.IsAny<CancellationToken>()));
        }

        private ManualResetEvent SetupUpAmazonSWFCallbackToRaiseEvent()
        {
            var heartbeatEvent = new ManualResetEvent(false);
            _amazonSimpleWorkflow
                .Setup(s => s.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RecordActivityTaskHeartbeatResponse(){ActivityTaskStatus = new ActivityTaskStatus()})
                .Callback(() =>{heartbeatEvent.Set();});
            return heartbeatEvent;
        }

        private void AssertThatNoHearbeatIsSendToAmazonSwf()
        {
            _amazonSimpleWorkflow.Verify(s => 
                            s.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                            It.IsAny<CancellationToken>()), Times.Never);
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

        [EnableHeartbeat(IntervalInMilliSeconds = _heartbeatInterval)]
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
                Hearbeat.Enable(TimeSpan.FromMilliseconds(_heartbeatInterval));
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
            private readonly string _result;

            public ActivityReturningCompleteResponse(string result)
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
            private readonly string _details;

            public ActivityReturningCancelResponse(string details)
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
            private readonly string _details;

            public ActivityReturningFailResponse(string details, string reason)
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

        private class ActivityReturningDeferResponse : Activity
        {
            [ActivityMethod]
            public ActivityResponse Execute()
            {
                return Defer;
            }
        }

        [EnableHeartbeat(IntervalInMilliSeconds = 10)]
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

        private ManualResetEvent AmazonSwfReturnActivityCancellationRequested()
        {
            var response = new RecordActivityTaskHeartbeatResponse()
            {
                ActivityTaskStatus = new ActivityTaskStatus() { CancelRequested = true }
            };
            var @event = new ManualResetEvent(false);
            _amazonSimpleWorkflow.Setup(
                w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(response).Callback(()=>@event.Set());
            return @event;
        }
    }
}