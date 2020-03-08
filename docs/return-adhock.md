On receiving a signal you can interrupt the workflow execution or schedule any workflow item. You need to define the signal event handler as explained [here](Workflow-events#Signal).

In the following example workflow can be cancelled if the "CancelOrder" is received within the grace period of 1 hour.

```cs
public class OrderWorkflow :Workflow
{
  public OrderWorkflow()
  {
   ScheduleTimer("GracePeriod").FireAfter(TimeSpan.FromHours(1)); 
   ScheduleLambda("ReserveOrder").AfterTimer("GracePeriod");
   ScheduleLambda("OrderCancelled").AfterTimer("GracePeriod").When(_=>false);
   ... 
  }
  [SignalEvent]
  public WorkflowAction CancelOrder() 
  {
    var timer = Timer("GracePeriod");
    if timer.IsActive()
      return CancelRequest.For(timer) + Jump.ToLambda("OrderCancelled");
    return Ignore;
  }
}
```
