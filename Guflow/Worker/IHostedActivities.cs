namespace Guflow.Worker
{
    internal interface IHostedActivities
    {
        Activity FindBy(string activityName, string activityVersion);
    }
}