// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ActivityArgsTests
    {
        [Test]
        public void Validation()
        {
            Assert.Throws<ArgumentException>(() => new ActivityArgs("", "", "id", "runid", "token"));
            Assert.Throws<ArgumentException>(() => new ActivityArgs("", "id", "", "runid", "token"));
            Assert.Throws<ArgumentException>(() => new ActivityArgs("", "id", "id", "", "token"));
            Assert.Throws<ArgumentException>(() => new ActivityArgs("", "id", "id", "rid", ""));
            Assert.DoesNotThrow(()=> new ActivityArgs("", "id", "id", "rid", "id"));
        }
    }
}