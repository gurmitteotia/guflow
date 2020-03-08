You can implement the human interactions in the workflow using signal APIs. Following three APIs will allow you to implement the human interaction for various requirments:
```
Good to know:
1. Signal name is case insensitive.
2. The signal is ignored if there is no waiting item
```
* **WaitForSignal**: Pause the workflow execution and wait for the specific signal to continue. 

    In the following example, workflow execution will be paused indefinitely on the completion of the Lambda function "SendEmail" and will continue on receiving the "EmailConfirmed" signal.

  ```cs
  public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed"));
       ScheduleLambda("ActivateUser").AfterLambda("SendEmail");
    }
  }
  ```

    **Note:** In above example Workflow.Id is passed as input to the "SendEmail" Lambda function because Amazon SWF does not automatically pass the workflow Id and RunId to the Lambda function like it does with activities. Inside the "SendEmail" Lambda function you will store the workflow Id and later on use it to send the signal back to workflow. This [example](https://github.com/gurmitteotia/guflow-samples/tree/master/ServerlessManualApproval) can give you further ideas on how to implement the signal APIs.

    In the following example, workflow execution will be paused for 12 hours and will continue on either receiving the "EmailConfirmed" signal or signal timeout.

  ```cs
  public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));
       
       ScheduleLambda("ActivateUser").AfterLambda("SendEmail")
        .When(_=>Signal("EmailConfirmed").IsTriggered());
       
       ScheduleLambda("FailActivation").AfterLambda("SendEmail")
        .When(_=>Signal("EmailConfirmed").IsTimedout());
    }
  }
  ```

* **WaitForAnySignal**: Pause the workflow execution until one of the specific signal is received. 

    In the following example workflow will be paused indefinitely after the execution of the Lambda function- "ApproveExpenses" and will continue on receiving either "Accepted" or "Rejected" signal and it will selectively schedule the lambda functions based on the signal received.

  ```cs
  public class ExpenseWorkflow : Workflow
  {
     public ExpenseWorkflow()
     {
        ScheduleLambda("ApproveExpenses").WithInput(_=>new {Id})
          .OnCompletion(e => e.WaitForAnySignal("Accepted", "Rejected"));

        ScheduleLambda("SendToAccount").AfterLambda("ApproveExpenses")
          .When(_ => Signal("Accepted").IsTriggered());
        ScheduleLambda("SendBackToEmp").AfterLambda("ApproveExpenses")
          .When(_ => Signal("Rejected").IsTriggered());
     }
  }
  ```

    In the following examples workflow will wait for either "Accepted" or "Rejected" signal for 2 days.

  ```cs
  public class ExpenseWorkflow : Workflow
  {
     public ExpenseWorkflow()
     {
        ScheduleLambda("ApproveExpenses").WithInput(_=>new {Id})
          .OnCompletion(e => e.WaitForAnySignal("Accepted", "Rejected").For(TimeSpan.FromDays(2)));

        ScheduleLambda("SendToAccount").AfterLambda("ApproveExpenses")
          .When(_ => Signal("Accepted").IsTriggered());

        ScheduleLambda("SendBackToEmp").AfterLambda("ApproveExpenses")
          .When(_ => Signal("Rejected").IsTriggered());
        
        ScheduleLambda("Escalate").AfterLambda("ApproveExpenses")
          .When(_ => AnySignal("Accepted", "Rejected").IsTimedout());
     }
  }

  ```
  Note: In above example you can also use

* **WaitForAllSignals**: Pause the workflow execution and wait for all the specific signals to continue the execution. 
In the following example workflow will pause the execution after the execution of the "PromoteEmployee" lambda function and will continue when "HRApproved" and "ManagerApproved" signals are received.

  ```cs
  public class PromotionWorkflow : Workflow
  {
     public PromotionWorkflow()
     {
        ScheduleLambda("PromoteEmployee").WithInput(_=>new{Id})
          .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved"));
        ScheduleLambda("Promoted").AfterLambda("PromoteEmployee");
        ScheduleLambda("SendForReviewToHR").AfterLambda("Promoted");
     }
  }
  ```
  In this following examples workflow will wait for both signals- HRApproved and ManagerApproved for 2 days:

    ```cs
  public class PromotionWorkflow : Workflow
  {
     public PromotionWorkflow()
     {
        ScheduleLambda("PromoteEmployee").WithInput(_=>new{Id})
          .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved").For(TimeSpan.FromDays(2)));
        ScheduleLambda("Promoted").AfterLambda("PromoteEmployee");
        ScheduleLambda("SendForReviewToHR").AfterLambda("Promoted");
        ScheduleLambda("EscalateApproval").AfterLambda("PromoteEmployee")
          .When(_=>AnySignal("HRApproved", "ManagerApproved").IsTimedout());
     }
  }
  ```

  You can call above three "WaitForSignal" APIs anywhere in the workflow. In the following example workflow waits for the signals after completion of the child workflow:

  ```cs
  public class PromotionWorkflow : Workflow
  {
    public PromotionWorkflow()
    {
       ScheduleChildWorkflow<PromoteWorkflow>()
         .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved"));
       ScheduleLambda("Promoted").AfterLambda("PromoteEmployee");
       ScheduleLambda("SendForReviewToHr").AfterLambda("Promoted");
    }
  }
  ``` 
 
**Wait in parallel branches**: You can wait for a signal at the same time in parallel workflow branches and workflow will trigger the execution of only one of the waiting item. In this hypothetical example workflow has two parallel execution branches and it waits for the same signal "Accepted" in both branches. Two "Accepted" signals need to be sent to trigger the execution on both branches.

  ```cs
  public class TestWorkflow : Workflow
  {
     ScheduleLambda("Lambda1").WithInput(_=>new{Id})
       .OnCompletion(e => e.WaitForSignal("Accepted"));
     ScheduleLambda("Lambda2").AfterLambda("Lambda1");

     ScheduleLambda("Lambda3").WithInput(_=>new{Id})
       .OnCompletion(e => e.WaitForAllSignals("Accepted", "OtherAccepted"));
     ScheduleLambda("Lambda4").AfterLambda("Lambda3");
  }
  ```

**Reschedule on signal(s)**: By default workflow execution is continued i.e. child workflow items are scheduled on receiving the signal(s) however you can also reschedule the waiting item on receiving the signal as shown in the following example:

  ```cs
  public class OrderWorkflow : Workflow
  {
     public OrderWorkflow()
     {
       ScheduleLambda("ReserveItem").OnFailure(e=>e.Reason=="NotAvailable"
                        ?e.WaitForSignal("InventoryUpdated").ToReschedule()
                        :e.DefaultAction())
      .WithInput(_=>new{Id});
       ScheduleLambda("ChargeCustomer").AfterLambda("ReserveItem");
       ScheduleLambda("ShipItem").AfterLambda("ChargeCustomer");
  }   
  ```
**Customisation**: 

  Following sets of APIs should give you enough flexibility to implement the complex workflows. These APIs are also used internally to support the default signal behaviour.:
  * **IWorkflowItem.IsWaitingForSignal**: You can use it to determine if the target workflow item is waiting for the specific signal.
  * **IWorkflowItem.IsWaitingForAnySignal**: You can use it to determine if the target workflow item is waiting for any signal at all.
  * **IWorkflowItem.Resume:** Resume the workflow execution from the target workflow item. If workflow item expects more signals (e.g. you're using WaitForAllSignals API) then it will just record the specified signal as received and it will not continue the execution. It will only continue the execution when all the expected signals are received. This API will throw an exception if the targeted workflow item is not waiting for the given signal.
  * **IWorkflowItem.IsSignalled**: You can use it to determine if the targeted workflow item has received the specific signal. You can call this API anywhere in the workflow.
  * **Workflow.WaitingItems**: Returns all workflow items waiting for the specific signal.
  * **Workflow.WaitingItem**: Returns one of the workflow item which will resume the execution on receiving the given signal. If there are multiple workflow items waiting for the specific signal (can happen in parallel branches) then it will return the one, waiting for the longest period.

   Following examples will clarify the usage of these APIs:

  **Example 1:** Following example does what workflow will do by default on receiving the signal.

  ```cs
  public class UserActivateWorkflow: Workflow
  {
     public UserActivateWorkflow()
     {
        ScheduleLambda("SendEmail").WithInput(_=>new{Id})
           .OnCompletion(e => e.WaitForSignal("EmailConfirmed"));
        ScheduleLambda("ActivateUser").AfterLambda("SendEmail");
     }
     [SignalEvent]
     public WorkflowAction EmailConfirmed(WorkflowSignaledEvent @event)
     {
       return Lambda("SendEmail").IsWaitingForSignal(@event.SignalName)
           ? Lambda("SendEmail").Resume(@event) : Ignore;
     }
  }
  ``` 
  **Example 2:** In the following hypothetical example it will resume all the waiting workflow items on receiving the signal. By default, only one of the waiting item is resumed on receiving the signal.

  ```cs
  public class TestWorkflow: Workflow
  {
    public TestWorkflow()
    {
       ScheduleLambda("Lambda1").WithInput(_=>new{Id})
          .OnCompletion(e => e.WaitForSignal("Confirmed"));
       ScheduleLambda("Lambda2").AfterLambda("Lambda1");

       ScheduleLambda("Lambda2").WithInput(_=>new{Id})
          .OnCompletion(e => e.WaitForSignal("Confirmed"));
       ScheduleLambda("Lambda3").AfterLambda("Lambda2");

   }
   [SignalEvent]
   public WorkflowAction Confirmed(WorkflowSignaledEvent @event)
   {
      WorkflowAction result = Ignore;
      foreach(var item in WaitingItems(@event.SignalName)
        result+=item.Resume(@event);
      return result;
   }
  }
  ``` 
 
**IWorkflowItem.IsSignalled vs Signal("name).IsTriggered**:

"Signal(name).IsTriggerd" is a handy API and is supposed to be used only after the waiting item, it returns true if the "continuation" of the workflow is triggered by the specific signal. Usage of this API makes sense only after the waiting workflow item. 

While "IWorkflowItem.IsSignalled" API can be used anywhere in the workflow, you can anytime use it to check if the workflow item has received the given signal. This API will let you handle some complex requirements as shown in the [PermitIssueWorkflow](https://github.com/gurmitteotia/guflow-samples/tree/master/ServerlessManualApproval) example.

**Things to take care**
1. Do not mix and match multiple "Wait" APIs in response to the same event. e.g.
   ```cs
     public class UserActivateWorkflow: Workflow
     {
        public UserActivateWorkflow()
        {
            ScheduleLambda("SendEmail").WithInput(_=>new{Id})
                .OnCompletion(e => e.WaitForSignal("EmailConfirmed")+ e.WaitForSignal("PhoneConfirmed"));
            ScheduleLambda("ActivateUser").AfterLambda("SendEmail");
        }
     } 
   ```
   At the moment workflow will ignore the second "WaitForSignal" action but it can be changed in future.

2. Do not schedule a workflow item, waiting for signals, directly. It may introduce non-deterministic behaviour in some situations. e.g "SendEmail" lambda function will wait for the signal "EmailConfirmed" after execution. It should be resumed by sending the "EmailedConfirmed" signal and not scheduled again by jumping to it:
   ```cs
     public class UserActivateWorkflow: Workflow
     {
        public UserActivateWorkflow()
        {
            ScheduleLambda("SendEmail").WithInput(_=>new{Id})
                .OnCompletion(e => e.WaitForSignal("EmailConfirmed");
            ScheduleLambda("ActivateUser").AfterLambda("SendEmail");
        }

        [SignalEvent] //Don't jump to "SendEmail" instead Resume it.
        public WorkflowAction Signal1() => Jump.ToLambda("SendEmail");
     } 
   ```