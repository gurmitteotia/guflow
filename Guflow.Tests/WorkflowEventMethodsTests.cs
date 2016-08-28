﻿using System;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowEventMethodsTests
    {
        private TestArgument _argument;

        [SetUp]
        public void Setup()
        {
            _argument = new TestArgument(10);
        }
        [Test]
        public void Can_invoke_the_public_method_with_matching_attribute()
        {
            var targetWorkflow = new TestClassWithPublicMethod();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.TestMethodInvoked);
        }

        [Test]
        public void Can_invoke_the_protected_method_with_matching_attribute()
        {
            var targetWorkflow = new TestClassWithProtectedMethod();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.TestMethodInvoked);
        }

        [Test]
        public void Can_invoke_the_private_method_with_matching_attribute()
        {
            var targetWorkflow = new TestClassWithPrivateMethod();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.TestMethodInvoked);
        }

        [Test]
        public void Returns_ignore_workflow_action_when_target_method_return_type_is_void()
        {
            var workflowMethod = WorkflowEventMethods.For(new TestClassWithPrivateMethod()).FindFor<TestMethodAttribute>();

            var workflowAction =workflowMethod.Invoke(_argument);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Ignore));
        }

        [Test]
        public void Returns_workflow_action_of_target_method()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;
            var workflowMethod = WorkflowEventMethods.For(new TestClassToReturnWorkflowAction(expectedWorkflowAction)).FindFor<TestMethodAttribute>();

            var workflowAction = workflowMethod.Invoke(_argument);

            Assert.That(workflowAction, Is.EqualTo(expectedWorkflowAction));
        }

        [Test]
        public void Throws_exception_when_target_method_return_type_is_other_then_void_or_workflow_action()
        {
            Assert.Throws<InvalidMethodSignatureException>(()=>WorkflowEventMethods.For(new TestClassWithIncompatibleReturnType()).FindFor<TestMethodAttribute>());
        }

        [Test]
        public void Pass_the_source_parameter_to_target_method_when_target_type_is_assignable_from_source_parameter()
        {
            var targetWorkflow = new MethodWithParameterOfBaseClass();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.InvokedWith, Is.EqualTo(_argument));
        }

        [Test]
        public void Pass_the_source_parameter_to_target_method_when_both_parameters_are_of_same_type()
        {
            var targetWorkflow = new MethodWithParameterOfSameClass();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.InvokedWith, Is.EqualTo(_argument));
        }

        [Test]
        public void Deserialize_the_properties_of_source_event_into_parameters_when_name_and_type_matches()
        {
            var targetWorkflow = new MethodWithDeserializedArguments();
            _argument.Reason = "reason3";
            _argument.SomeId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.Argument1, Is.EqualTo("reason3"));
            Assert.That(targetWorkflow.Argument2, Is.EqualTo(56));
        }

        [Test]
        public void Can_pass_properties_of_source_event_along_with_event_to_target_method_properties()
        {
            var targetWorkflow = new MethodWithDeserializedArgumentsAndSourcEvent();
            _argument.Reason = "reason3";
            _argument.SomeId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.Argument1, Is.EqualTo("reason3"));
            Assert.That(targetWorkflow.Argument2, Is.EqualTo(56));
            Assert.That(targetWorkflow.Argument3, Is.EqualTo(_argument));
        }

        [Test]
        public void Can_deserialize_properties_with_null_values_in_to_parameter_arguments()
        {
            var targetWorkflow = new MethodWithDeserializedArguments();
            _argument.Reason = null;
            _argument.SomeId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.Argument1, Is.Null);
            Assert.That(targetWorkflow.Argument2, Is.EqualTo(56));
        }

        [Test]
        public void Throws_exception_when_deserialized_property_can_not_be_assigned_to_method_parameter()
        {
            var targetWorkflow = new MethodWithIncompatibleArgumentType();
            _argument.Reason = "reason3";
            _argument.SomeId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).FindFor<TestMethodAttribute>();

            Assert.Throws<InvalidMethodSignatureException>(()=> workflowMethod.Invoke(_argument));
        }

        [Test]
        public void Throws_exception_when_multiple_methods_are_found_for_same_event()
        {
            Assert.Throws<AmbiguousWorkflowMethodException>(()=> WorkflowEventMethods.For(new TestClassWithMultipleMethods()).FindFor<TestMethodAttribute>());
        }

        [Test]
        public void Can_return_null_when_target_method_not_found()
        {
            Assert.That(WorkflowEventMethods.For(new EmptyWorkflow()).FindFor<TestMethodAttribute>(),Is.Null);
        }

        [Test]
        public void Return_workflow_ignore_action_when_target_method_returns_null()
        {
            var workflowMethod = WorkflowEventMethods.For(new MethodReturnsNull()).FindFor<TestMethodAttribute>();

            var workflowAction = workflowMethod.Invoke(_argument);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Ignore));
        }

        private class TestClassWithPublicMethod : Workflow
        {
            [TestMethod]
            public void TestMethod()
            {
                TestMethodInvoked = true;
            }
            public bool TestMethodInvoked { get; private set; }
        }
        private class TestClassWithProtectedMethod : Workflow
        {
            [TestMethod]
            protected void TestMethod()
            {
                TestMethodInvoked = true;
            }
            public bool TestMethodInvoked { get; private set; }
        }
        private class TestClassWithPrivateMethod : Workflow
        {
            [TestMethod]
            private void TestMethod()
            {
                TestMethodInvoked = true;
            }
            public bool TestMethodInvoked { get; private set; }
        }

        private class TestClassToReturnWorkflowAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public TestClassToReturnWorkflowAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [TestMethod]
            public WorkflowAction TestMethod()
            {
                return _workflowAction;
            }
        }

        private class TestClassWithIncompatibleReturnType : Workflow
        {
            [TestMethod]
            private int TestMethod()
            {
                TestMethodInvoked = true;
                return 0;
            }
            public bool TestMethodInvoked { get; private set; }
        }

        private class MethodWithParameterOfBaseClass : Workflow
        {
            [TestMethod]
            public void TestMethod(WorkflowEvent testArgument)
            {
                InvokedWith = testArgument;
            }

            public WorkflowEvent InvokedWith { get; private set; }
        }

        private class MethodWithParameterOfSameClass : Workflow
        {
            [TestMethod]
            public void TestMethod(TestArgument testArgument)
            {
                InvokedWith = testArgument;
            }

            public TestArgument InvokedWith { get; private set; }
        }

        private class MethodWithDeserializedArguments : Workflow
        {
            [TestMethod]
            public void TestMethod(string reason, long someId)
            {
                Argument1 = reason;
                Argument2 = someId;
            }

            public string Argument1 { get; private set; }
            public long Argument2 { get; private set; }
        }

        private class MethodWithDeserializedArgumentsAndSourcEvent : Workflow
        {
            [TestMethod]
            public void TestMethod(string reason, long someId, TestArgument @event)
            {
                Argument1 = reason;
                Argument2 = someId;
                Argument3 = @event;
            }

            public string Argument1 { get; private set; }
            public long Argument2 { get; private set; }
            public TestArgument Argument3 { get; private set; }
        }

        private class MethodWithIncompatibleArgumentType : Workflow
        {
            [TestMethod]
            public void TestMethod(long reason, string someId)
            {
            }

        }

        private class TestClassWithMultipleMethods : Workflow
        {
            [TestMethod]
            public void TestMethod(string reason, long someId)
            {
            }
            [TestMethod]
            public void TestMethod(string reason)
            {
            }
        }

        private class MethodReturnsNull : Workflow
        {
            [TestMethod]
            public WorkflowAction TestMethod()
            {
                return null;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class TestMethodAttribute : Attribute
        {
        }

        private class TestArgument : WorkflowEvent
        {
            public TestArgument(long eventId) : base(eventId)
            {
            }

            internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
            {
                throw new NotImplementedException();
            }

            public string Reason { get; set; }

            public long SomeId { get; set; }
        }

        
    }
}