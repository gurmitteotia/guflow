Latest release of Guflow supports the timeout for all wait of signal APIs. Now with minimum efforts you can support the human approvals in your workflows. Here is an example:

```
public class UserActivateWorkflow : Workflow
{
  public UserActivateWorkflow()
  {
     ScheduleLambda("SendLinkInEmail").WithInput(_=>{Id=Id})
      .OnCompletion(e=>e.WaitForSignal("EmailConfirmed").For(Timespan.FromHours(12)));

     ScheduleLambda("ActivateAccount").AfterLambda("SendLinkInEmail")
      .When(_=>Signal("EmailConfirmed").IsTriggered());
    
     ScheduleLambda("ActivationFailed").WithInput(_=>{ Reason="Link timedout"})
      .AfterLambda("SendLinkInEmail")
      .When(_=>Signal("EmailConfirmed").IsTimedout())

  }

}

```
Timeout is supported for all the wait for signal APIs: WaitForAnySignal and WaitForAllSignals. You can read more about it here.

Last few releases of Guflow were targetted to enhance signal handling capability. I'm quite confident that now you will get utmost flexibility int

 Guflow will let you use signals for the following three purposes:

- Human approvals
- Inter workflow communication
- Taking a custom action on receiving the signals.


Signal timeouts very well take care of scenarios where the workflow host can remains offline/down for some time either becuase of crash or hot upgrade. 


All the workflow state is maintined in Amazon SWF hence you can 

-Workflow Host is crashed/down immediately after executing the Lambda function "SendLinkInEmail" and it came up 

-Workflow is resumed/up after 6 hours: Guflow will calculate that 
