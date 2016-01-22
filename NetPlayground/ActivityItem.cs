using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityItem: SchedulableItem
    {
        private readonly ActivityName _activityName;
        private string _taskListName;
        
        public ActivityItem(string name, string version)
        {
            _activityName = new ActivityName(name,version);
        }

        internal override Decision GetDecision()
        {
            return new Decision()
            {
                ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                {
                    ActivityType = new ActivityType() {Name = _activityName.Name, Version = _activityName.Version},
                    TaskList = new TaskList() {  Name = _taskListName}
                },
                DecisionType = DecisionType.ScheduleActivityTask
            };
        }

        public void ScheduleOnTaskList(string taskListName)
        {
            _taskListName = taskListName;
        }
    }
}