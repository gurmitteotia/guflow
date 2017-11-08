using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ActivityHeartbeatTests
    {
        private ActivityHeartbeat _activityHearbeat;
        private Mock<IAmazonSimpleWorkflow> _simpleWorkflow;
        private const int HeartbeatIntervel = 10;
        private AutoResetEvent _heartbeatReportedToSwf;
        private readonly int _waitTimeForEvent = HeartbeatIntervel * 200;
        
        [SetUp]
        public void Setup()
        {
            _heartbeatReportedToSwf = new AutoResetEvent(false);
            _activityHearbeat = new ActivityHeartbeat();
            _activityHearbeat.ProvideDetails(()=>"details");
            _simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            _activityHearbeat.Enable(TimeSpan.FromMilliseconds(HeartbeatIntervel));
        }

        [TearDown]
        public void Teardown()
        {
            _activityHearbeat.StopHeartbeat();
        }

        [Test]
        public void Sends_heartbeat_to_amazon_swf_when_started()
        {
            SetupAmazonSwfToRecordHeartbeat("details", "token", CallbackForTimes(3));

            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));

           _simpleWorkflow.VerifyAll();
        }

        [Test]
        public void Do_not_send_hearbeat_to_amazon_swf_when_no_interval_is_set()
        {
            SetupAmazonSwfToRecordHeartbeat("details", "token", CallbackForTimes(3));
            var activityHearbeat = new ActivityHeartbeat();

            activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsFalse(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));


            AsserThatHearbeatIsNotRecorded();
        }

        [Test]
        public void Raise_cancellation_requested_when_activity_is_requested_to_cancel()
        {
            SetupAmazonSwfToReturnActivityCancellationRequested(CallbackForTimes(3));
            var cancellelationRequestedEvent = new ManualResetEvent(false);
            _activityHearbeat.CancellationRequested += (s, e) => cancellelationRequestedEvent.Set();
            
            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));

            Assert.That(cancellelationRequestedEvent.WaitOne(HeartbeatIntervel * 50), "Cancellation request event is not raised");
        }

        [Test]
        public void Raise_activity_terminated_when_resource_not_found_exception_is_raised()
        {
            RecordHeartbeatThrows(new UnknownResourceException("Activity terminated"), CallbackForTimes(1));
            var terminatedEvent = new ManualResetEvent(false);
            _activityHearbeat.ActivityTerminated += (s, e) => terminatedEvent.Set();

            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");

            Assert.That(terminatedEvent.WaitOne(_waitTimeForEvent *2), "Activity terminated event is not raised");
        }

        [Test]
        public void Stop_reporting_hearbeat_when_resource_not_found_exception_is_raised()
        {
            RecordHeartbeatThrows(new UnknownResourceException("Activity terminated"), CallbackForTimes(1));
          
            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));

            AsserThatHearbeatIsRecordedOnleOnce();
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried()
        {
            RecordHeartbeatThrows(new AmazonServiceException("network error"), CallbackForTimes(4));
            int retryAttempts = 0;
            HandleError errorHandler = e => { Console.WriteLine($"Attempts {e.RetryAttempts}"); retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.OnError(errorHandler);
           
            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));

            Assert.That(retryAttempts, Is.GreaterThanOrEqualTo(2));
            AsserThatHearbeatIsRecordedMultipleTimes();
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried_using_fallback_error_handler()
        {
            RecordHeartbeatThrows(new AmazonServiceException("network error"), CallbackForTimes(4));
            int retryAttempts = 0;
            HandleError errorHandler = e => { retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.SetFallbackErrorHandler(ErrorHandler.Default(errorHandler));

            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));


            Assert.That(retryAttempts, Is.GreaterThanOrEqualTo(2));
            AsserThatHearbeatIsRecordedMultipleTimes();
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried_using_fallback_error_handler_even_if_was_set_before_user_handler()
        {
            RecordHeartbeatThrows(new AmazonServiceException("network error"), CallbackForTimes(4));
            int retryAttempts = 0;
            HandleError errorHandler = e => { retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.SetFallbackErrorHandler(ErrorHandler.Default(errorHandler));
            _activityHearbeat.OnError(e=>ErrorAction.Unhandled);

            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));


            Assert.That(retryAttempts, Is.GreaterThanOrEqualTo(2));
            AsserThatHearbeatIsRecordedMultipleTimes();
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_continued()
        {
            RecordHeartbeatThrows(new AmazonServiceException("network error"), CallbackForTimes(3));
            int retryAttempts = 0;
            HandleError errorHandler = e => {retryAttempts = e.RetryAttempts; return ErrorAction.Continue;};
            _activityHearbeat.OnError(errorHandler);
            
            _activityHearbeat.StartHeartbeatIfEnabled(_simpleWorkflow.Object, "token");
            Assert.IsTrue(_heartbeatReportedToSwf.WaitOne(_waitTimeForEvent));

            Assert.That(retryAttempts, Is.EqualTo(0));
            AsserThatHearbeatIsRecordedMultipleTimes();
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(()=>_activityHearbeat.OnError(null));
            Assert.Throws<ArgumentNullException>(() => _activityHearbeat.ProvideDetails(null));
            Assert.Throws<ArgumentException>(() => _activityHearbeat.Enable(TimeSpan.MinValue));
            Assert.Throws<ArgumentException>(() => _activityHearbeat.Enable(TimeSpan.Zero));
        }

        private Action CallbackForTimes(int callbackTimes)
        {
            var callbackEvent = new CallbackEvent(_heartbeatReportedToSwf, callbackTimes);
            return callbackEvent.Call;
        }
        private void AsserThatHearbeatIsRecordedMultipleTimes()
        {
            _simpleWorkflow.Verify(
                w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }

        private void AsserThatHearbeatIsRecordedOnleOnce()
        {
            _simpleWorkflow.Verify(
                w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        private void AsserThatHearbeatIsNotRecorded()
        {
            _simpleWorkflow.Verify(
                w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        private void RecordHeartbeatThrows(Exception exception, Action callback)
        {
            _simpleWorkflow.Setup(
                    w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception).Callback(callback);

        }
        private void SetupAmazonSwfToReturnActivityCancellationRequested(Action callback)
        {
            var response = new RecordActivityTaskHeartbeatResponse()
            {
                ActivityTaskStatus = new ActivityTaskStatus() {CancelRequested = true}
            };

            _simpleWorkflow.Setup(
                    w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                        It.IsAny<CancellationToken>())).ReturnsAsync(response)
                .Callback(callback);
        }

        private void SetupAmazonSwfToRecordHeartbeat(string details, string taskToken, Action callback)
        {
            var response = new RecordActivityTaskHeartbeatResponse()
            {
                ActivityTaskStatus = new ActivityTaskStatus()
            };

            Func<RecordActivityTaskHeartbeatRequest, bool> req = r =>
            {
                Assert.That(r.Details, Is.EqualTo(details));
                Assert.That(r.TaskToken, Is.EqualTo(taskToken));
                return true;
            };
            _simpleWorkflow
                .Setup(w => w.RecordActivityTaskHeartbeatAsync(It.Is<RecordActivityTaskHeartbeatRequest>(r => req(r)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response).Callback(callback);
        }

        private class CallbackEvent
        {
            private readonly AutoResetEvent _event;
            private readonly int _times;
            private int _currentInvokedTimes = 0;

            public CallbackEvent(AutoResetEvent @event, int times)
            {
                _event = @event;
                _times = times;
            }

            public void Call()
            {
                if (++_currentInvokedTimes >= _times)
                    _event.Set();
            }
        }
    }

  
}