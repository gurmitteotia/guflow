// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;
using ChildPolicy = Guflow.Decider.ChildPolicy;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RestartWorkflowDecisionTests
    {
        [Test]
        public void Can_return_swf_decision_with_populated_properties_to_continue_the_workflow_as_new()
        {
            var restartWorkflowAction = new RestartWorkflowAction();
           restartWorkflowAction.ChildPolicy = ChildPolicy.RequestCancel;
           restartWorkflowAction.ExecutionStartToCloseTimeout = TimeSpan.FromSeconds(2);
           restartWorkflowAction.Input = "input";
           restartWorkflowAction.AddTag("tags1");
           restartWorkflowAction.AddTag("tags2");
           restartWorkflowAction.TaskList = "task list";
           restartWorkflowAction.TaskPriority = 4;
           restartWorkflowAction.TaskStartToCloseTimeout = TimeSpan.FromSeconds(3);
           restartWorkflowAction.WorkflowTypeVersion = "1.0";

           var restartWorkflowDecision = new RestartWorkflowDecision(restartWorkflowAction);
           

            var decision = restartWorkflowDecision.Decision();

            Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.ContinueAsNewWorkflowExecution));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.ChildPolicy, Is.EqualTo(Amazon.SimpleWorkflow.ChildPolicy.REQUEST_CANCEL));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.ExecutionStartToCloseTimeout, Is.EqualTo("2"));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.Input, Is.EqualTo("input"));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TagList, Is.EqualTo(new List<string>{"tags1", "tags2"}));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TaskList.Name, Is.EqualTo("task list"));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TaskPriority, Is.EqualTo("4"));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TaskStartToCloseTimeout, Is.EqualTo("3"));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.WorkflowTypeVersion, Is.EqualTo("1.0"));
        }

        [Test]
        public void Return_swf_decision_to_continue_the_workflow_as_new()
        {
            var restartWorkflowDecision = new RestartWorkflowDecision(new RestartWorkflowAction());
            

            var decision = restartWorkflowDecision.Decision();

            Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.ContinueAsNewWorkflowExecution));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.ChildPolicy, Is.Null);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.ExecutionStartToCloseTimeout, Is.Null);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.Input, Is.Null);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TagList, Is.Empty);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TaskList, Is.Null);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TaskPriority, Is.Null);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.TaskStartToCloseTimeout, Is.Null);
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.WorkflowTypeVersion, Is.Null);
        }
    }
}