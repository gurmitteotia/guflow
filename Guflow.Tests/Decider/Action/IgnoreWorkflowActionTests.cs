﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using NUnit.Framework;
using System.Linq;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class IgnoreWorkflowActionTests
    {
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
        }
        //[Test]
        //public void Equality_tests()
        //{
        //    Assert.That(WorkflowAction.Ignore.Equals(WorkflowAction.Ignore));

        //    Assert.That(WorkflowAction.Ignore.Equals(WorkflowAction.Ignore(false)));

        //    Assert.IsFalse(WorkflowAction.Ignore(true).Equals(WorkflowAction.Ignore(false)));
        //}
        [Test]
        public void Return_empty_decisions()
        {
            var workflowAction = WorkflowAction.Ignore(null);
            Assert.That(workflowAction.Decisions(),Is.Empty);
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowReturningStartWorkflowAction();
            var activityCompletedEvent = CreateCompletedActivityEvent(WorkflowReturningStartWorkflowAction.ActivityName, WorkflowReturningStartWorkflowAction.ActivityVersion);

            var workflowAction = activityCompletedEvent.Interpret(workflow);

            Assert.That(workflowAction.Decisions(), Is.Empty);
        }
        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion)
        {
            var activityIdentity = Identity.New(activityName, activityVersion, string.Empty).ScheduleId();
            var allHistoryEvents = _builder.ActivityCompletedGraph(activityIdentity, "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private class WorkflowReturningStartWorkflowAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public WorkflowReturningStartWorkflowAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion).OnCompletion(e => Ignore);
            }
        }
    }
}