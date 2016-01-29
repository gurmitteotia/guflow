using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class ActivityCompletedEventTests
    {
        private const string _result = "result";
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private IEnumerable<HistoryEvent> _completedActivityEventGraph;

        [SetUp]
        public void Setup()
        {
            _completedActivityEventGraph = CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, _identity , _result);
        }

        [Test]
        public void Populate_activity_details_from_history_events()
        {
            var activityCompletedEvent = new ActivityCompletedEvent(_completedActivityEventGraph.First(), _completedActivityEventGraph);
           
            Assert.That(activityCompletedEvent.Result, Is.EqualTo(_result));
            Assert.That(activityCompletedEvent.Name, Is.EqualTo(_activityName));
            Assert.That(activityCompletedEvent.Version, Is.EqualTo(_activityVersion));
            Assert.That(activityCompletedEvent.PositionalName, Is.EqualTo(_positionalName));
            Assert.That(activityCompletedEvent.Identity, Is.EqualTo(_identity));
        }

        [Test]
        public void Throws_exception_when_completed_activity_is_not_found_in_workflow()
        {
            var activityCompletedEvent = new ActivityCompletedEvent(_completedActivityEventGraph.First(), _completedActivityEventGraph);
            var incompatibleWorkflow = new IncompatibleWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(()=> activityCompletedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new TestWorkflow();
            var activityCompletedEvent = new ActivityCompletedEvent(_completedActivityEventGraph.First(), _completedActivityEventGraph);

            var decisions = activityCompletedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions.Count(), Is.EqualTo(2));
            decisions.AssertThatActivityIsScheduled("Transcode", "2.0");
            decisions.AssertThatActivityIsScheduled("Sync", "2.1");
        }

        [Test]
        public void Return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new TestWorkflow();
            var activityCompletedEvent = new ActivityCompletedEvent(_completedActivityEventGraph.First(), _completedActivityEventGraph);

            var decisions = activityCompletedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions.Count(), Is.EqualTo(2));
            decisions.AssertThatActivityIsScheduled("Transcode", "2.0");
            decisions.AssertThatActivityIsScheduled("Sync", "2.1");
        }

        [Test]
        public void Return_empty_decision_when_childern_can_not_be_scheduled()
        {

        }

        private IEnumerable<HistoryEvent> CreateActivityCompletedEventGraph(string activityName, string version, string positionalName, string identity, string result)
        {
            var historyEvents = new List<HistoryEvent>();
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCompleted,
                EventId = 11,
                ActivityTaskCompletedEventAttributes = new ActivityTaskCompletedEventAttributes()
                {
                    Result = result,
                    StartedEventId = 10,
                    ScheduledEventId = 9
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = 10,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                    {
                        Identity = identity,
                        ScheduledEventId = 9

                    }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = 9,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityName, Version = version },
                    Control = (new ScheduleData() { PN = positionalName }).ToJson()
                }
            });
            return historyEvents;
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                AddActivity(_activityName,_activityVersion,_positionalName);

                AddActivity("Transcode", "2.0").DependsOn(_activityName,_activityVersion,_positionalName);
                AddActivity("Sync", "2.1").DependsOn(_activityName, _activityVersion, _positionalName);
            }
        }

        private class TestWorkflowWithMultipleParents : Workflow
        {
            public TestWorkflowWithMultipleParents()
            {
                AddActivity(_activityName, _activityVersion, _positionalName);
                AddActivity("Sync", "2.0");
                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName).DependsOn("Sync", "2.0");
            }
        }

        private class IncompatibleWorkflow : Workflow
        {
            public IncompatibleWorkflow()
            {
                AddActivity("Transcode", "1.0");
            }
        }
    }
}