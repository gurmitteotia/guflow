// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowItemTests
    {
        private Identity _identity;

        [SetUp]
        public void Setup()
        {
            _identity = Identity.New("workflow", "1.0");
        }

        [Test]
        public void Invalid_arguments()
        {
            var childWorkflowItem = new ChildWorkflowItem(_identity, Mock.Of<IWorkflow>());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnCompletion(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnFailure(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnCancelled(null));
        }
    }
}