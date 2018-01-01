namespace Guflow.Decider
{
    /// <summary>
    /// Represent the timer to schedule in a workflow.
    /// </summary>
    public interface ITimerItem : IWorkflowItem
    {
        /// <summary>
        /// Returns name of timer.
        /// </summary>
        string Name { get; }
    }
}