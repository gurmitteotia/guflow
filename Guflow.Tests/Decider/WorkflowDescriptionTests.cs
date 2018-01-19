// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Configuration;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowDescriptionTests
    {
        [Test]
        public void Throws_exception_when_attribute_is_not_applied()
        {
            Assert.Throws<WorkflowDescriptionMissingException>(() => WorkflowDescriptionAttribute.FindOn<WorkflowWithoutAttribute>());
        }

        [Test]
        public void Timeouts_are_null_when_not_set()
        {
            var workflowDescription = WorkflowDescriptionAttribute.FindOn<WorkflowWithoutTimeoutSet>();
           
            Assert.IsNull(workflowDescription.DefaultExecutionStartToCloseTimeout);
            Assert.IsNull(workflowDescription.DefaultTaskStartToCloseTimeout);
        }

        [Test]
        public void Timeouts_are_non_empty_when_set()
        {
            var workflowDescription = WorkflowDescriptionAttribute.FindOn<WorkflowWithTimeoutSet>();
           
            Assert.That(workflowDescription.DefaultExecutionStartToCloseTimeout,Is.EqualTo("10"));
            Assert.That(workflowDescription.DefaultTaskStartToCloseTimeout, Is.EqualTo("0"));
        }

        [Test]
        public void Throws_exception_version_is_empty()
        {
            Assert.Throws<ConfigurationErrorsException>(() => WorkflowDescriptionAttribute.FindOn<WorkflowWithEmptyVersion>());
        }

        [Test]
        public void Throws_exception_when_type_does_not_derive_from_workflow_class()
        {
           Assert.Throws<NonWorkflowTypeException>(()=> WorkflowDescriptionAttribute.FindOn(typeof (NonWorkflow)));
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentNullException>(() => WorkflowDescriptionAttribute.FindOn(null));
        }

        private class WorkflowWithoutAttribute : Workflow
        {
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithoutTimeoutSet : Workflow
        {
            
        }

        [WorkflowDescription("1.0", DefaultExecutionStartToCloseTimeoutInSeconds = 10, DefaultTaskStartToCloseTimeoutInSeconds = 0)]
        private class WorkflowWithTimeoutSet : Workflow
        {

        }

        [WorkflowDescription("")]
        private class WorkflowWithEmptyVersion : Workflow
        {

        }

        private class NonWorkflow
        {
            
        }
    }
}