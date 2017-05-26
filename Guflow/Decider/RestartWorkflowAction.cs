using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    public class RestartWorkflowAction : WorkflowAction
    {
        private readonly List<string> _tags = new List<string>();

        public int? TaskPriority { get;set; }
        public IEnumerable<string> TagList { get { return _tags; }}
        public string ChildPolicy { get; set; }
        public string Input { get; set; }
        public TimeSpan? ExecutionStartToCloseTimeout { get; set; }
        public string TaskList { get; set; }
        public TimeSpan? TaskStartToCloseTimeout { get; set; }
        public string WorkflowTypeVersion { get; set; }

        public void AddTag(string tag)
        {
            _tags.Add(tag);
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {new RestartWorkflowDecision(this), };
        }
    }
}