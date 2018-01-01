namespace Guflow.Decider
{
    /// <summary>
    /// Represents the activity child of workflow.
    /// </summary>
    public interface IActivityItem : IWorkflowItem
    {
        /// <summary>
        /// Returns name of activity.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Returns version of activity.
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Returns positional name of activity.
        /// </summary>
        string PositionalName { get; }
    }
}