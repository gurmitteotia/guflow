using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace NetPlayground
{
    public static class DecisionAssertExtension
    {
        public static void AssertThatActivityIsScheduled(this IEnumerable<Decision> workflowStartedDecisions, string activityName, string activityVersion, string taskListName)
        {
            var activityScheduleDecision = workflowStartedDecisions.First(d => d.DecisionType == DecisionType.ScheduleActivityTask);
            var scheduleAttributes = activityScheduleDecision.ScheduleActivityTaskDecisionAttributes;
            Assert.AreEqual(activityName, scheduleAttributes.ActivityType.Name);
            Assert.AreEqual(activityVersion, scheduleAttributes.ActivityType.Version);
        }

        public static void AssertThatWorkflowIsCompleted(this IEnumerable<Decision> workflowStartedDecisions, string result)
        {
            var activityScheduleDecision = workflowStartedDecisions.First(d => d.DecisionType == DecisionType.CompleteWorkflowExecution);
            var decisionAttributes = activityScheduleDecision.CompleteWorkflowExecutionDecisionAttributes;
           
            Assert.AreEqual(result,decisionAttributes.Result);
        }
    }
}