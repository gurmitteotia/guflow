namespace Guflow
{
    public interface IPostExecutionEvents
    {
        void Completed(string workflowId, string workflowRunId, string result);
    }
}