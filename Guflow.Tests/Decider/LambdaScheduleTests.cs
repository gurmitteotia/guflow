// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaScheduleTests
    {
        private const string ParentActivityName = "Activity1";
        private const string ParentActivityVersion = "1.0";


        private const string LambdaName = "LambdaName";
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }

        [Test]
        public void Lambda_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new LambdaAfterActivityWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(LambdaName),"input")}));
        }

        private IEnumerable<HistoryEvent> ActivityEventGraph()
        {
            return _builder.ActivityCompletedGraph(Identity.New(ParentActivityName, ParentActivityVersion), "id",
                "res");
        }

        private class LambdaAfterActivityWorkflow : Workflow
        {
            public LambdaAfterActivityWorkflow()
            {
                ScheduleActivity(ParentActivityName, ParentActivityVersion);
                ScheduleLambda(LambdaName).AfterActivity(ParentActivityName, ParentActivityVersion);
            }
        }
    }
}