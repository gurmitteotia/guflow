### Guflow
A C#.NET library, built on [Amazon SWF](https://aws.amazon.com/swf/) ( Simple Workflow Service ), lets you coordinate the execution of serverless AWS Lambda functions, activities, child workflows and timers with ease.

[![Build status](https://ci.appveyor.com/api/projects/status/github/gurmitteotia/guflow?svg=true)](https://ci.appveyor.com/project/gurmitteotia/guflow/branch/master)
### Installation
```
 Install-Package Guflow
     OR
 dotnet add package Guflow
 ```
 ### Description
 Guflow provides high level and easy to use APIs to program [Amazon SWF](https://aws.amazon.com/swf/). In Guflow you can write-
* Elastic and scalable workflows to orchestrate the scheduling of lambda functions, activities, child workflows and timers
* Activities to carry out the task.

Guflow, supporting all the features of [Amazon SWF](https://aws.amazon.com/swf/), is an alternative to Amazon Step functions and the Amazon Flow framework and it provides simplicity and flexbility in one place.

### Features:
Guflow:
* Allows you to create complex workflows with ease and flexibility
* Supports parallel execution of lambda functions, activities, child workflow and timers in the workflow
* Supports fork and join of [workflow branches](https://github.com/gurmitteotia/guflow/wiki/Workflow-branches)
* Supports recursion around an individual item (lambda function, activity, child workflow and timer) or whole execution branch
* Provides equal supports for scheduling the activities written in other framework/language
* Provides robust [signal APIs](https://github.com/gurmitteotia/guflow/wiki/Workflow-signals) to easily implement manual approvals using serverless lambda functions.
* Encourage the development of worker using AWS lambda functions.
* Supports async/sync activity method
* Supports activity throttling
* Supported by behavioural unit tests and continuously released.
* Supports all the features of Amazon SWF:
  * Lambda function
  * Activity
  * Timer
  * Child workflows
  * Signal
  * Workflow/child-workflow cancellation
  * Marker
  * Activity heartbeat and cancellation
  * Handling of all error events.


### Example 1
Following example shows BookHolidays workflow using AWS Lambda functions:
     
```cs
        BookFlight          BookHotel
            |                   |
            |                   |
            v                   v
        ChoosSeat          BookDinner
            |                   |
            |                   |
            `````````````````````
                    |
                    v
              ChargeCustomer
                    |
                    v
               SendEmail
              
    [WorkflowDescription("1.0")]
    public class BookHolidaysWorkflow : Workflow
    {
        public BookHolidaysWorkflow()
        {
            ScheduleLambda("BookFlight")
            ScheduleLambda("ChooseSeat").AfterLambda("BookFlight");

            ScheduleLambda("BookHotel")
            ScheduleLambda("BookDinner").AfterLambda("BookHotel");

            ScheduleLambda("ChargeCustomer").AfterLambda("ChooseSeat").AfterLambda("BookDinner");
            
            ScheduleLambda("SendEmail").AfterLambda("ChargeCustomer");
        }
    }             
          
```

### Example 2
In the following example, workflow execution will pause after ApproveExpense lambda is completed and continue when either of Accepted or Rejected signal is received.
```cs
             ApproveExpense          
                  |
                  |
                  v
         |````````````````````|
    <Accepted>            <Rejected>
         |                    |
         |                    |
         v                    v
    SubmitToAccount     SendRejectEmail              
            
              
    [WorkflowDescription("1.0")]
    public class ExpenseWorkflow : Workflow
    {
        public ExpenseWorkflow()
        {
            ScheduleLambda("ApproveExpense")
              .OnCompletion(e=>e.WaitForAnySignal("Accepted", "Rejected"))
              .WithInput(_=>new{Id});  //Send workflow id to lambda functions to send signals to this workflow.
         
            ScheduleLambda("SubmitToAccount").AfterLambda("ApproveExpense")
              .When(_=>Signal("Accepted").IsTriggered());

            ScheduleLambda("SendRejectEmail").AfterLambda("ApproveExpense")
              .When(_=>Signal("Accepted").IsTriggered());
        }
    }             
          
```


### Example 3
In following example, workflow execution will pause when "ReserveItem" is failed with "NotAvailable" reason and reschedule the lambda function "ReserveItem" on receiving the signal "InventorUpdated":
```cs
             ReserveItem <----------------Reschedule         
                  |                        |
                  |                 <InvenetoryUpdated>
                  v                        |
         |````````````````````|            |
    <Success>               <Fail>-------NotAvailable
         |                    |       
         |                    |
         v                    v
    ChargeCustomer      FailWorkflow(on all other reasons)              
            
              
    [WorkflowDescription("1.0")]
    public class OrderWorkflow : Workflow
    {
        public OrderWorkflow()
        {
            ScheduleLambda("ReserveItem")
              .OnFailure(e=>e.Reason=="NotAvailable"
                           ?e.WaitForSignal("InventoryUpdated").ToReschedule()
                           :e.DefaultAction())
              .WithInput(_=>new{Id});//Send workflow id to lambda functions to send signals to this workflow.
         
            ScheduleLambda("ChargeCustomer").AfterLambda("ReserveItem");
          
            ScheduleLambda("ShipItem").AfterLambda("ChargeCustomer");
    }             
          
```
You can read about signal APIs [here](https://github.com/gurmitteotia/guflow/wiki/Workflow-signals) and find more examples about manual approvals in [sample](https://github.com/gurmitteotia/guflow-samples/tree/master/ServerlessManualApproval) project.


[Query APIs](https://github.com/gurmitteotia/guflow/wiki/Query-apis), [workflow actions](https://github.com/gurmitteotia/guflow/wiki/Workflow-actions) and flexible [branching](https://github.com/gurmitteotia/guflow/wiki/Workflow-branches) support will allow you to create challanging workflows.
Though Guflow supports all the features of Amazon SWF but it continues to add useful APIs, manual intervention and timer reset are examples of it and many


**Features in pipeline:** Please look at [project board](https://github.com/gurmitteotia/guflow/projects/1) to see what is is coming next. If you want some feature to be included in Guflow library or something is not working for you please file an [issue](https://github.com/gurmitteotia/guflow/issues). Your ideas, suggestion and collorbation will greatly help this library and its users.

### Documentation
Guflow is supported by [tutorial](https://github.com/gurmitteotia/guflow/wiki/Tutorial), [documentation](https://github.com/gurmitteotia/guflow/wiki) and [samples](https://github.com/gurmitteotia/guflow-samples) to get you started easily.

### Supported .NET frameworks:
dotnet standard 1.3 onwards

### Hosting
Guflow is a light weight and performant library, you should start with not so powerfull EC2 instance or docker container to host the Guflow decider.

### Getting help
Enable the [logging](https://github.com/gurmitteotia/guflow/wiki/Logging) to look for any obvious error and if problem persist then please raise an [issue](https://github.com/gurmitteotia/guflow/issues) in github
