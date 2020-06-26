Signal("name").IsTriggered vs IWorkflowItem.IsSignalled:

Signal("name").IsTriggered or Signal("name").IsTimedout are handy APIs and are supposed to be used immediately after waiting item. In the following example Signal("name").IsTriggered or Signal("name").IsTimedout does not make sense because these APIs are not used immediately after the WaitForSignal API.


```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));

       ScheduleLambda("RecordAttempt").AfterLambda("SendEmail");
       
       //Here both Signal("name").IsTriggered and Signal("name").IsTimedout will always return false.
       ScheduleLambda("ActivateUser").AfterLambda("RecordAttempt")
        .When(_=>Signal("EmailConfirmed").IsTriggered());
       
       ScheduleLambda("FailActivation").AfterLambda("RecordAttempt")
        .When(_=>Signal("EmailConfirmed").IsSignalTimedout());
    }
  }

```

However IWorkflowItem.IsSignalled and IWorkflowItem.IsSignalTimedout APIs can be used anywhere, including [workflow event handler](workflow-event), in the workflow and offer you more flexibility and sometime let you handle [complex scenarios](https://github.com/gurmitteotia/guflow-samples/blob/master/ServerlessManualApproval/Workflows/PermitIssueWorkflow.cs) which are otherwise not possible with former APIs.  Following example shows that IWorkflowItem.IsSignalled/IsSignalTimedout APIs can be used anywhere in the workflow:

```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));

       ScheduleLambda("RecordAttempt").AfterLambda("SendEmail");
       
       ScheduleLambda("ActivateUser").AfterLambda("RecordAttempt")
        .When(_=>Lambda("SendEmail").IsSignalled("EmailConfirmed"));
       
       ScheduleLambda("FailActivation").AfterLambda("RecordAttempt")
        .When(_=>Lambda("SendEmail").IsSignalTimedout("EmailConfirmed"));
    }
  }

```
Also shown in this [example](https://github.com/gurmitteotia/guflow-samples/blob/master/ServerlessManualApproval/Workflows/PermitIssueWorkflow.cs) certain scenarios can be handled only using IWorkflowItem.IsSignalled/IsSignalTimedout APIs.


However IWorkflowItem.IsSignalled and IWorkflowItem.IsSignalTimedout can be used any where, including [workflow event handler](workflow-event), in the workflow. Following is the valid example:


```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));
       
       ScheduleLambda("ActivateUser").AfterLambda("SendEmail")
        .When(_=>Lambda("SendEmail").IsSignalled("EmailConfirmed"));
       
       ScheduleLambda("FailActivation").AfterLambda("SendEmail")
        .When(_=>Lambda("SendEmail").IsSignalTimedout("EmailConfirmed"));
    }
  }

```


```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));

       ScheduleLambda("RecordAttempt").AfterLambda("SendEmail");
       
       ScheduleLambda("ActivateUser").AfterLambda("RecordAttempt")
        .When(_=>Lambda("SendEmail").IsSignalled("EmailConfirmed"));
       
       ScheduleLambda("FailActivation").AfterLambda("RecordAttempt")
        .When(_=>Lambda("SendEmail").IsSignalTimedout("EmailConfirmed"));
    }
  }

```

 



You can use two types of APIs to determine which signal is triggered and timedout. All of the above examples use Signal("name").IsTrigger or Signal("name").IsTimedout APIs. However you can also use IWorkflowItem.IsSignalled or IWorkflowItem.IsSignalTimedout and the following example shows the usage of later APIs:

```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));
       
       ScheduleLambda("ActivateUser").AfterLambda("SendEmail")
        .When(_=>Lambda("SendEmail").IsSignalled("EmailConfirmed"));
       
       ScheduleLambda("FailActivation").AfterLambda("SendEmail")
        .When(_=>Lambda("SendEmail").IsSignalTimedout("EmailConfirmed"));
    }
  }

```
Functionality wise both UserActivateWorkflow are same. 
Signal("name").IsTriggered or Signal("name").IsTimedout are handy APIs are supposed to be used immediately after waiting item. In the following example Signal("name").IsTriggered or Signal("name").IsTimedout does not make sense.


```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));

       ScheduleLambda("RecordAttempt").AfterLambda("SendEmail");
       
       //Here both Signal("name").IsTriggered and Signal("name").IsTimedout will always return true.
       ScheduleLambda("ActivateUser").AfterLambda("RecordAttempt")
        .When(_=>Signal("EmailConfirmed").IsTriggered());
       
       ScheduleLambda("FailActivation").AfterLambda("RecordAttempt")
        .When(_=>Signal("EmailConfirmed").IsSignalTimedout());
    }
  }

```

However IWorkflowItem.IsSignalled and IWorkflowItem.IsSignalTimedout can be use any where, including [workflow event handler](workflow-event), in the workflow. Following is the valid example:

```cs
public class UserActivateWorkflow: Workflow
  {
    public UserActivateWorkflow()
    {
       ScheduleLambda("SendEmail").WithInput(_=>new{Id})
        .OnCompletion(e => e.WaitForSignal("EmailConfirmed").For(TimeSpan.FromHours(12)));

       ScheduleLambda("RecordAttempt").AfterLambda("SendEmail");
       
       ScheduleLambda("ActivateUser").AfterLambda("RecordAttempt")
        .When(_=>Lambda("SendEmail").IsSignalled("EmailConfirmed"));
       
       ScheduleLambda("FailActivation").AfterLambda("RecordAttempt")
        .When(_=>Lambda("SendEmail").IsSignalTimedout("EmailConfirmed"));
    }
  }

```
Also shown in this [example](https://github.com/gurmitteotia/guflow-samples/blob/master/ServerlessManualApproval/Workflows/PermitIssueWorkflow.cs) certain scenarios can be handled only using IWorkflowItem.IsSignalled/IsSignalTimedout APIs.



Readme->Hosting.
 Because workflow state is maintained in [Amazon SWF](https://aws.amazon.com/swf/) you can downsize or upsize the workflow host machine in production.

 