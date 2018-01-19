// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ErrorHandlerTests
    {
        [Test]
        public void Default_handler_test()
        {
            var defaultHandler = ErrorHandler.Default(e => ErrorAction.Retry);

            Assert.That(defaultHandler.OnError(new Error()), Is.EqualTo(ErrorAction.Retry));
        }

        [Test]
        public void Call_fallback_handler_when_default_handler_does_not_handle_error()
        {
            var defaultHandler = ErrorHandler.Default(e => ErrorAction.Unhandled).WithFallback(ErrorHandler.Default(e=>ErrorAction.Continue));

            Assert.That(defaultHandler.OnError(new Error()), Is.EqualTo(ErrorAction.Continue));
        }

        [Test]
        public void Does_not_call_fallback_handler_when_default_handler_handles_the_error()
        {
            var defaultHandler = ErrorHandler.Default(e => ErrorAction.Retry).WithFallback(ErrorHandler.Default(e => ErrorAction.Continue));

            Assert.That(defaultHandler.OnError(new Error()), Is.EqualTo(ErrorAction.Retry));
        }
    }
}