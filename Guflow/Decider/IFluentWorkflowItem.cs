using Guflow.Worker;

namespace Guflow.Decider
{
    public interface IFluentWorkflowItem<out T>
    {
        T AfterTimer(string name);
        T AfterActivity(string name, string version, string positionalName = "");
        T AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity;
    }
}