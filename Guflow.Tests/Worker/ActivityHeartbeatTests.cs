// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading;
using Amazon.Runtime;
using Amazon.SimpleWorkflow.Model;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ActivityHeartbeatTests
    {
        private ActivityHeartbeat _activityHearbeat;
        
        private const int HeartbeatIntervel = 10;
        private const string HearbeatDetails = "details";
        private const int WaitPeriod = HeartbeatIntervel * 500;
        [SetUp]
        public void Setup()
        {
            _activityHearbeat = new ActivityHeartbeat();
            _activityHearbeat.ProvideDetails(()=> HearbeatDetails);
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
            var heartbeatApi = new TestHeartbeatSwfApi(() => false);
            StartHearbeat(heartbeatApi);
            Assert.That(heartbeatApi.Details, Is.EqualTo(HearbeatDetails));
        }

        [Test]
        public void Do_not_send_hearbeat_to_amazon_swf_when_no_interval_is_set()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(() => false);
            var hearbeat = new ActivityHeartbeat(); 
            hearbeat.StartHeartbeatIfEnabled(heartbeatApi, "token");

            Assert.IsFalse(heartbeatApi.Wait(HeartbeatIntervel * 100));
            Assert.That(heartbeatApi.HearbeatRecorded, Is.False);
        }

        [Test]
        public void Raise_cancellation_requested_when_activity_is_requested_to_cancel()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(()=>true);
            var cancellelationRequestedEvent = new ManualResetEvent(false);
            _activityHearbeat.CancellationRequested += (s, e) => cancellelationRequestedEvent.Set();

            StartHearbeat(heartbeatApi);

            Assert.That(cancellelationRequestedEvent.WaitOne(WaitPeriod), "Cancellation request event is not raised");
        }

        [Test]
        public void Raise_activity_terminated_when_resource_not_found_exception_is_raised()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(()=>throw new UnknownResourceException(""));

            var terminatedEvent = new ManualResetEvent(false);
            _activityHearbeat.ActivityTerminated += (s, e) => terminatedEvent.Set();

            StartHearbeat(heartbeatApi);

            Assert.That(terminatedEvent.WaitOne(WaitPeriod), "Activity terminated event is not raised");
        }

        [Test]
        public void Stop_reporting_hearbeat_when_resource_not_found_exception_is_raised()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(() => throw new UnknownResourceException(""));

            StartHearbeat(heartbeatApi);

            Assert.That(heartbeatApi.HearbeatRecordedTimes, Is.EqualTo(1));
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(() => throw new AmazonServiceException("network error"), 4);

            int retryAttempts = 0;
            HandleError errorHandler = e => { retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.OnError(errorHandler);

            StartHearbeat(heartbeatApi);

            Assert.That(retryAttempts, Is.GreaterThanOrEqualTo(2));
            Assert.That(heartbeatApi.HearbeatRecordedTimes, Is.GreaterThan(1));
        }

        private void StartHearbeat(TestHeartbeatSwfApi heartbeatApi)
        {
            try
            {
                _activityHearbeat.StartHeartbeatIfEnabled(heartbeatApi, "token");
                Assert.IsTrue(heartbeatApi.Wait(WaitPeriod));
            }
            finally
            {
                _activityHearbeat.StopHeartbeat();
            }
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried_using_fallback_error_handler()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(() => throw new AmazonServiceException("network error"), 4);
            int retryAttempts = 0;
            HandleError errorHandler = e => { retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.SetFallbackErrorHandler(ErrorHandler.Default(errorHandler));

            StartHearbeat(heartbeatApi);

            Assert.That(retryAttempts, Is.GreaterThanOrEqualTo(2));
            Assert.That(heartbeatApi.HearbeatRecordedTimes, Is.GreaterThan(1));
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_retried_using_fallback_error_handler_even_if_was_set_before_user_handler()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(() => throw new AmazonServiceException("network error"), 4);
            int retryAttempts = 0;
            HandleError errorHandler = e => { retryAttempts = e.RetryAttempts; return ErrorAction.Retry; };
            _activityHearbeat.SetFallbackErrorHandler(ErrorHandler.Default(errorHandler));
            _activityHearbeat.OnError(e=>ErrorAction.Unhandled);

            StartHearbeat(heartbeatApi);

            Assert.That(retryAttempts, Is.GreaterThanOrEqualTo(2));
            Assert.That(heartbeatApi.HearbeatRecordedTimes, Is.GreaterThan(1));
        }

        [Test]
        public void Error_raised_in_reporting_the_heartbeat_can_be_continued()
        {
            var heartbeatApi = new TestHeartbeatSwfApi(() => throw new AmazonServiceException("network error"),4);

            int retryAttempts = 0;
            HandleError errorHandler = e => {retryAttempts = e.RetryAttempts; return ErrorAction.Continue;};
            _activityHearbeat.OnError(errorHandler);

            StartHearbeat(heartbeatApi);

            Assert.That(retryAttempts, Is.EqualTo(0));
            Assert.That(heartbeatApi.HearbeatRecordedTimes, Is.GreaterThan(1));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(()=>_activityHearbeat.OnError(null));
            Assert.Throws<ArgumentNullException>(() => _activityHearbeat.ProvideDetails(null));
            Assert.Throws<ArgumentException>(() => _activityHearbeat.Enable(TimeSpan.MinValue));
            Assert.Throws<ArgumentException>(() => _activityHearbeat.Enable(TimeSpan.Zero));
        }
    }

  
}