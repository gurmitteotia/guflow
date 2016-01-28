using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace NetPlayground
{
    public static class DecisionAssertExtension
    {
        public static void AssertThatActivityIsScheduled(this IEnumerable<Decision> workflowStartedDecisions, string activityName, string activityVersion)
        {
            var activityScheduledAttributes = workflowStartedDecisions.Where(d => d.DecisionType == DecisionType.ScheduleActivityTask).Select(a => a.ScheduleActivityTaskDecisionAttributes);
            var scheduleAttribute = activityScheduledAttributes.FirstOrDefault(a => a.ActivityType.Name.Equals(activityName) && a.ActivityType.Version.Equals(activityVersion));
            Assert.That(scheduleAttribute,Is.Not.Null,string.Format("Activity by name {0} and version {1} is not scheduled",activityName,activityVersion));
        }

        public static void AssertThatWorkflowIsCompleted(this IEnumerable<Decision> workflowStartedDecisions, string result)
        {
            var activityScheduleDecision = workflowStartedDecisions.First(d => d.DecisionType == DecisionType.CompleteWorkflowExecution);
            var decisionAttributes = activityScheduleDecision.CompleteWorkflowExecutionDecisionAttributes;
           
            Assert.AreEqual(result,decisionAttributes.Result);
        }
    }
}