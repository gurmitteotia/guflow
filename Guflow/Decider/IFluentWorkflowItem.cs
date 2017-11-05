using Guflow.Worker;

namespace Guflow.Decider
{
    public interface IFluentWorkflowItem<out T>
    {
        /// <summary>
        /// Schedule this item after the named timer is fired.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        T AfterTimer(string name);
        /// <summary>
        /// Schedule this item after named activity is completed.
        /// </summary>
        /// <param name="name">Parent activity name.</param>
        /// <param name="version">Parent activity version</param>
        /// <param name="positionalName">Parent activity positional name.</param>
        /// <returns></returns>
        T AfterActivity(string name, string version, string positionalName = "");
        /// <summary>
        /// Schedule this item after TActivity is compeleted.
        /// </summary>
        /// <typeparam name="TActivity">Activity type.</typeparam>
        /// <param name="positionalName">Positional name.</param>
        /// <returns></returns>
        T AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity;
    }
}