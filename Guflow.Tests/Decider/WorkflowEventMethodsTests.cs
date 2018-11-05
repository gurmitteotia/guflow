// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowEventMethodsTests
    {
        private TestArgument _argument;

        private const EventName _eventName = EventName.Signal;

        [SetUp]
        public void Setup()
        {
            _argument = new TestArgument(10);
        }
        [Test]
        public void Can_invoke_the_public_method_with_matching_attribute()
        {
            var targetWorkflow = new TestClassWithPublicMethod();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.TestMethodInvoked);
        }

        [Test]
        public void Can_invoke_the_protected_method_with_matching_attribute()
        {
            var targetWorkflow = new TestClassWithProtectedMethod();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.TestMethodInvoked);
        }

        [Test]
        public void Can_invoke_the_private_method_with_matching_attribute()
        {
            var targetWorkflow = new TestClassWithPrivateMethod();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.TestMethodInvoked);
        }

        [Test]
        public void Returns_empty_workflow_action_when_target_method_returns_void()
        {
            var workflowMethod = WorkflowEventMethods.For(new TestClassWithPrivateMethod()).EventMethod(_eventName);

            var workflowAction =workflowMethod.Invoke(_argument);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Empty));
        }

        [Test]
        public void Returns_workflow_action_of_target_method()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;
            var workflowMethod = WorkflowEventMethods.For(new TestClassToReturnWorkflowAction(expectedWorkflowAction)).EventMethod(_eventName);

            var workflowAction = workflowMethod.Invoke(_argument);

            Assert.That(workflowAction, Is.EqualTo(expectedWorkflowAction));
        }

        [Test]
        public void Throws_exception_when_target_method_return_type_is_other_then_void_or_workflow_action()
        {
            Assert.Throws<InvalidMethodSignatureException>(()=>WorkflowEventMethods.For(new TestClassWithIncompatibleReturnType()).EventMethod(_eventName));
        }

        [Test]
        public void Pass_the_source_parameter_to_target_method_when_target_type_is_assignable_from_source_parameter()
        {
            var targetWorkflow = new MethodWithParameterOfBaseClass();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.InvokedWith, Is.EqualTo(_argument));
        }

        [Test]
        public void Pass_the_source_parameter_to_target_method_when_both_parameters_are_of_same_type()
        {
            var targetWorkflow = new MethodWithParameterOfSameClass();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.InvokedWith, Is.EqualTo(_argument));
        }

        [Test]
        public void Deserialize_the_properties_of_source_event_into_parameters_when_name_and_type_matches()
        {
            var targetWorkflow = new MethodWithDeserializedArguments();
            _argument.Reason = "reason3";
            _argument.EventId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.Argument1, Is.EqualTo("reason3"));
            Assert.That(targetWorkflow.Argument2, Is.EqualTo(56));
        }

        [Test]
        public void Can_pass_properties_of_source_event_along_with_event_to_target_method_properties()
        {
            var targetWorkflow = new MethodWithDeserializedArgumentsAndSourcEvent();
            _argument.Reason = "reason3";
            _argument.EventId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

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
            _argument.EventId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.Argument1, Is.Null);
            Assert.That(targetWorkflow.Argument2, Is.EqualTo(56));
        }

        [Test]
        public void Deserialize_the_json_string_in_to_complex_argument()
        {
            var targetWorkflow = new MethodDeserializeJsonInToComplextObject();

            _argument.Reason = new Reason(){Id=10, Details = "details"}.ToJson();
            _argument.EventId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.EventId, Is.EqualTo(56));
            Assert.That(targetWorkflow.Reason.Id, Is.EqualTo(10));
            Assert.That(targetWorkflow.Reason.Details, Is.EqualTo("details"));
        }

        [Test]
        public void Throws_exception_when_deserialized_property_can_not_be_assigned_to_method_parameter()
        {
            var targetWorkflow = new MethodWithIncompatibleArgumentType();
            _argument.Reason = "reason3";
            _argument.EventId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            Assert.Throws<InvalidMethodSignatureException>(()=> workflowMethod.Invoke(_argument));
        }

        [Test]
        public void Pass_default_values_to_parameters_when_not_found_in_event()
        {
            var targetWorkflow = new MethodWithNonExistingArgument();
            _argument.Reason = "reason3";
            _argument.EventId = 56;
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.NotExistString,Is.Null);
            Assert.That(targetWorkflow.NotExistLong, Is.EqualTo(0));
            Assert.That(targetWorkflow.NotExistBool, Is.EqualTo(false));
            Assert.That(targetWorkflow.NonExistReason, Is.Null);
            Assert.That(targetWorkflow.NonExistStructArg.Id, Is.EqualTo(0));
            Assert.That(targetWorkflow.NonExistStructArg.Details, Is.Null);
        }

        [Test]
        public void Throws_exception_when_multiple_methods_are_found_for_same_event()
        {
            Assert.Throws<AmbiguousWorkflowMethodException>(()=> WorkflowEventMethods.For(new TestClassWithMultipleMethods()).EventMethod(_eventName));
        }

        [Test]
        public void Can_return_null_when_target_method_not_found()
        {
            Assert.That(WorkflowEventMethods.For(new EmptyWorkflow()).EventMethod(_eventName),Is.Null);
        }

        [Test]
        public void Return_workflow_empty_action_when_target_method_returns_null()
        {
            var workflowMethod = WorkflowEventMethods.For(new MethodReturnsNull()).EventMethod(_eventName);

            var workflowAction = workflowMethod.Invoke(_argument);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Empty));
        }

        [Test]
        public void Throws_actual_exception_thrown_by_target_method()
        {
            var targetWorkflow = new MethodThrowsException(new Exception(""));
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);

            Assert.Throws<Exception>(()=>workflowMethod.Invoke(_argument));
        }

        [Test]
        public void Pass_default_value_in_struct_when_source_string_property_is_null()
        {
            var targetWorkflow = new MethodDeserialingStringInToStruct();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);
            _argument.Reason = null;

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.Reason.Id, Is.EqualTo(0));
            Assert.That(targetWorkflow.Reason.Details, Is.Null);
        }

        [Test]
        public void Can_pass_source_primitive_type_to_string_parameters()
        {
            var targetWorkflow = new MethodWithStringParamemtersForPrimitiveStructType();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);
            _argument.EventTime = DateTime.Now;

            workflowMethod.Invoke(_argument);

            Assert.That(targetWorkflow.EventId, Is.EqualTo(_argument.EventId.ToString()));
            Assert.That(targetWorkflow.EventTime, Is.EqualTo(_argument.EventTime.ToString()));
        }

        [Test]
        public void Can_pass_source_string_type_to_primitive_type_parameters()
        {
            var targetWorkflow = new MethodWithPrimitiveType();
            var workflowMethod = WorkflowEventMethods.For(targetWorkflow).EventMethod(_eventName);
            var argument = new ArgumentInStringFormat(10)
            {
                Duration = TimeSpan.FromSeconds(2).ToString(),
                EventId = 10.ToString(),
                EventTime = DateTime.Now.ToString(),
                Reason = true.ToString()
            };

            workflowMethod.Invoke(argument);

            Assert.That(targetWorkflow.EventId, Is.EqualTo(10));
            Assert.That(targetWorkflow.EventTime, Is.EqualTo(DateTime.Parse(argument.EventTime)));
            Assert.That(targetWorkflow.Duration, Is.EqualTo(TimeSpan.Parse(argument.Duration)));
            Assert.That(targetWorkflow.Reason, Is.EqualTo(bool.Parse(argument.Reason)));
        }

        private class TestClassWithPublicMethod : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod()
            {
                TestMethodInvoked = true;
            }
            public bool TestMethodInvoked { get; private set; }
        }
        private class TestClassWithProtectedMethod : Workflow
        {
            [WorkflowEvent(_eventName)]
            protected void TestMethod()
            {
                TestMethodInvoked = true;
            }
            public bool TestMethodInvoked { get; private set; }
        }
        private class TestClassWithPrivateMethod : Workflow
        {
            [WorkflowEvent(_eventName)]
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

            [WorkflowEvent(_eventName)]
            public WorkflowAction TestMethod()
            {
                return _workflowAction;
            }
        }

        private class TestClassWithIncompatibleReturnType : Workflow
        {
            [WorkflowEvent(_eventName)]
            private int TestMethod()
            {
                TestMethodInvoked = true;
                return 0;
            }
            public bool TestMethodInvoked { get; private set; }
        }

        private class MethodWithParameterOfBaseClass : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(WorkflowEvent testArgument)
            {
                InvokedWith = testArgument;
            }

            public WorkflowEvent InvokedWith { get; private set; }
        }

        private class MethodWithParameterOfSameClass : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(TestArgument testArgument)
            {
                InvokedWith = testArgument;
            }

            public TestArgument InvokedWith { get; private set; }
        }

        private class MethodWithDeserializedArguments : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(string reason, long eventId)
            {
                Argument1 = reason;
                Argument2 = eventId;
            }

            public string Argument1 { get; private set; }
            public long Argument2 { get; private set; }
        }

        private class MethodWithDeserializedArgumentsAndSourcEvent : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(string reason, long eventId, TestArgument @event)
            {
                Argument1 = reason;
                Argument2 = eventId;
                Argument3 = @event;
            }

            public string Argument1 { get; private set; }
            public long Argument2 { get; private set; }
            public TestArgument Argument3 { get; private set; }
        }

        private class MethodWithIncompatibleArgumentType : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(long reason, string eventId)
            {
            }
        }

        private class MethodWithNonExistingArgument : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(string notExistString, long notExistLong, bool notExistBool, Reason nonExistReason, StructArg nonExistStructArg)
            {
                NotExistString = notExistString;
                NotExistLong = notExistLong;
                NotExistBool = notExistBool;
                NonExistReason = nonExistReason;
                NonExistStructArg = nonExistStructArg;
            }

            public string NotExistString { get; private set; }
            public long NotExistLong { get; private set; }
            public bool NotExistBool { get; private set; }
            public Reason NonExistReason { get; private set; }
            public StructArg NonExistStructArg { get; private set; }
        }

        private class TestClassWithMultipleMethods : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(string reason, long eventId)
            {
            }
            [WorkflowEvent(_eventName)]
            public void TestMethod(string reason)
            {
            }
        }

        private class MethodReturnsNull : Workflow
        {
            [WorkflowEvent(_eventName)]
            public WorkflowAction TestMethod()
            {
                return null;
            }
        }

        private class MethodThrowsException : Workflow
        {
            private readonly Exception _exception;

            public MethodThrowsException(Exception exception)
            {
                _exception = exception;
            }

            [WorkflowEvent(_eventName)]
            public WorkflowAction TestMethod()
            {
                throw _exception;
            }
        }

        private class MethodDeserializeJsonInToComplextObject : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(Reason reason, long eventId)
            {
                Reason = reason;
                EventId = eventId;
            }

            public Reason Reason { get; private set; }
            public long EventId { get; private set; }
        }

        private class MethodDeserialingStringInToStruct : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(StructArg reason)
            {
                Reason = reason;
            }
            public StructArg Reason { get; private set; }
        }

        private class MethodWithStringParamemtersForPrimitiveStructType : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(string eventId, string eventTime)
            {
                EventId = eventId;
                EventTime = eventTime;
            }
            public string EventId { get; private set; }
            public string EventTime { get; private set; }
        }

        private class MethodWithPrimitiveType : Workflow
        {
            [WorkflowEvent(_eventName)]
            public void TestMethod(int eventId, DateTime eventTime, TimeSpan duration, bool reason)
            {
                EventId = eventId;
                EventTime = eventTime;
                Duration = duration;
                Reason = reason;
            }
            public int EventId { get; private set; }
            public DateTime EventTime { get; private set; }
            public TimeSpan Duration { get; private set; }
            public bool Reason { get; private set; }
        }

        private class Reason
        {
            public int Id;
            public string Details;
        }
        private struct StructArg
        {
            public int Id;
            public string Details;
        }
     
        private class TestArgument : WorkflowEvent
        {
            public TestArgument(long eventId) : base(eventId)
            {
            }

            internal override WorkflowAction Interpret(IWorkflow workflow)
            {
                throw new NotImplementedException();
            }

            public string Reason { get; set; }
            public long EventId { get; set; }
            public DateTime EventTime { get; set; }
            internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
            {
                throw new NotImplementedException();
            }
        }

        private class ArgumentInStringFormat : WorkflowEvent
        {
            public ArgumentInStringFormat(long eventId)
                : base(eventId)
            {
            }

            internal override WorkflowAction Interpret(IWorkflow workflow)
            {
                throw new NotImplementedException();
            }
            public string EventId { get; set; }
            public string EventTime { get; set; }
            public string Duration { get; set; }
            public string Reason { get; set; }
            internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
            {
                throw new NotImplementedException();
            }
        }
    }
}