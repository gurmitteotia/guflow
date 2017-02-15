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
        
        [SetUp]
        public void Setup()
        {
            _activityHearbeat = new ActivityHeartbeat();
            _activityHearbeat.ProvideDetails(()=>"details");
            _simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            _activityHearbeat.SetInterval(TimeSpan.FromMilliseconds(HeartbeatIntervel));
        }

        [TearDown]
        public void Teardown()
        {
            _activityHearbeat.StopHeartbeat();
        }

        [Test]
        public void Sends_heartbeat_to_amazon_swf_when_started()
        {
            SetupAmazonSwfToRecordHeartbeat("details", "token");

            _activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");
            Thread.Sleep(HeartbeatIntervel * 50);

           _simpleWorkflow.VerifyAll();
        }

        [Test]
        public void Do_not_send_hearbeat_to_amazon_swf_when_no_interval_is_set()
        {
            SetupAmazonSwfToRecordHeartbeat("details", "token");
            var activityHearbeat = new ActivityHeartbeat();

            activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");
            Thread.Sleep(HeartbeatIntervel * 50);

            AsserThatHearbeatIsNotRecorded();
        }

        [Test]
        public void Raise_cancellation_requested_when_activity_is_requested_to_cancel()
        {
            AmazonSwfReturnActivityCancellationRequested();
            var cancellelationRequestedEvent = new ManualResetEvent(false);
            _activityHearbeat.CancellationRequested += (s, e) => cancellelationRequestedEvent.Set();
            
            _activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");

            Assert.That(cancellelationRequestedEvent.WaitOne(HeartbeatIntervel * 50), "Cancellation request event is not raised");
        }

        [Test]
        public void Raise_activity_terminated_when_resource_not_found_exception_is_raised()
        {
            RecordHeartbeatThrows(new UnknownResourceException("Activity terminated"));
            var terminatedEvent = new ManualResetEvent(false);
            _activityHearbeat.ActivityTerminated += (s, e) => terminatedEvent.Set();

            _activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");

            Assert.That(terminatedEvent.WaitOne(HeartbeatIntervel * 50), "Activity terminated event is not raised");
        }

        [Test]
        public void Stop_reporting_hearbeat_when_resource_not_found_exception_is_raised()
        {
            RecordHeartbeatThrows(new UnknownResourceException("Activity terminated"));
          
            _activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");
            Thread.Sleep(HeartbeatIntervel * 80);
            
            AsserThatHearbeatIsRecordedOnleOnce();
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried()
        {
            RecordHeartbeatThrows(new AmazonServiceException("network error"));
            int retryAttempts = 0;
            HandleError errorHandler = e => { retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.OnError(ErrorHandler.Default(errorHandler));
           
            _activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");
            Thread.Sleep(HeartbeatIntervel * 50);

            Assert.That(retryAttempts, Is.GreaterThan(2));
            AsserThatHearbeatIsRecordedMultipleTimes();
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_continued()
        {
            RecordHeartbeatThrows(new AmazonServiceException("network error"));
            int retryAttempts = 0;
            HandleError errorHandler = e => {retryAttempts = e.RetryAttempts; return ErrorAction.Continue;};
            _activityHearbeat.OnError(ErrorHandler.Default(errorHandler));
            
            _activityHearbeat.StartHeartbeatAsync(_simpleWorkflow.Object, "token");
            Thread.Sleep(HeartbeatIntervel * 50);

            Assert.That(retryAttempts, Is.EqualTo(0));
            AsserThatHearbeatIsRecordedMultipleTimes();
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(()=>_activityHearbeat.OnError((HandleError) null));
            Assert.Throws<ArgumentNullException>(() => _activityHearbeat.OnError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => _activityHearbeat.ProvideDetails(null));
            Assert.Throws<ArgumentException>(() => _activityHearbeat.SetInterval(TimeSpan.MinValue));
            Assert.Throws<ArgumentException>(() => _activityHearbeat.SetInterval(TimeSpan.Zero));
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

        private void RecordHeartbeatThrows(Exception exception)
        {
            _simpleWorkflow.Setup(
                w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>())).Throws(exception);
        }
        private void AmazonSwfReturnActivityCancellationRequested()
        {
            var response = new RecordActivityTaskHeartbeatResponse()
            {
                ActivityTaskStatus = new ActivityTaskStatus() {CancelRequested = true}
            };

            _simpleWorkflow.Setup(
                w => w.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
        }

        private void SetupAmazonSwfToRecordHeartbeat(string details, string taskToken)
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
            _simpleWorkflow.Setup(w => w.RecordActivityTaskHeartbeatAsync(It.Is<RecordActivityTaskHeartbeatRequest>(r => req(r)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
        }
    }
}