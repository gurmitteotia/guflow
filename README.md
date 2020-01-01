### Guflow
A C#.NET library, built on [Amazon SWF](https://aws.amazon.com/swf/) ( Simple Workflow Service ), lets you write distributive and scalable workflows using serverless AWS Lambda functions, activities, child workflows and timers with ease.

[![Build status](https://ci.appveyor.com/api/projects/status/github/gurmitteotia/guflow?svg=true)](https://ci.appveyor.com/project/gurmitteotia/guflow/branch/master)
### Installation
```
 Install-Package Guflow
     OR
 dotnet add package Guflow
 ```
 ### Description
In Guflow you can write-
* Elastic and scalable workflows to orchestrate the scheduling of lambda functions, activities, child workflows and timers
* 
* Activities to carry out the task.

Guflow, supporting all the features of [Amazon SWF](https://aws.amazon.com/swf/), is an alternative to Amazon Step functions and the Amazon Flow framework and it provides simplicity and flexbility in one place.

### Features:
Guflow:
* Allows you to create complex workflows with ease and flexibility
* Supports parallel execution of lambda functions, activities, child workflow and timers in the workflow
* Supports fork and join of [workflow branches](https://github.com/gurmitteotia/guflow/wiki/Workflow-branches)
* Supports recursion around an individual item (lambda function, activity, child workflow and timer) or whole execution branch
* Provides equal supports for scheduling the activities written in other framework/language
* Provides intuitive [signal APIs](https://github.com/gurmitteotia/guflow/wiki/Workflow-signals) to easily implement human approvals using serverless lambda functions.
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
Following example starts two parallel branches which later joins to execute the workflow in a single branch.
     
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
Above example is discussed in more details in [workflow branching](wiki/Workflow-branches).


### Example 2
In the following example, workflow execution will pause after ApproveExpense lambda is completed and it will wait for either "Accepted" or "Rejected" signal for 5 days.
```cs
             ApproveExpense          
                  |
                  |
                  v
         |````````````````````|``````````````````
    <Accepted>            <Rejected>         <Timedout> 
         |                    |					 |
         |                    |					 |
         v                    v					 v	
    SubmitToAccount     SendRejectEmail     EscalateExpense         
            
              
    [WorkflowDescription("1.0")]
    public class ExpenseWorkflow : Workflow
    {
        public ExpenseWorkflow()
        {
            ScheduleLambda("ApproveExpense")
              .OnCompletion(e=>e.WaitForAnySignal("Accepted", "Rejected").For(TimeSpan.FromDays(5))
              .WithInput(_=>new{Id});  //Send workflow id to lambda functions to send signals to this workflow.
         
            ScheduleLambda("SubmitToAccount").AfterLambda("ApproveExpense")
              .When(_=>Signal("Accepted").IsTriggered());

            ScheduleLambda("SendRejectEmail").AfterLambda("ApproveExpense")
              .When(_=>Signal("Rejected").IsTriggered());

			ScheduleLambda("EscalateExpense").AfterLambda("ApproveExpenses")
			  .When(_=>AnySignal("Accepted","Rejected").IsTimedout())
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
You can use the [signal APIs](https://github.com/gurmitteotia/guflow/wiki/Workflow-signals) along with AWS lambda functions to implement human approvals/signals. You will no more need to use self hosted activities for getting the human signals in your workflows.
A good number of examples involving manual approvals are provided in the [example](https://github.com/gurmitteotia/guflow-samples/tree/master/ServerlessManualApproval) project.


**Features in pipeline:** Please look at [project board](https://github.com/gurmitteotia/guflow/projects/1) to see what is is coming next. If you want some feature to be included in Guflow library or something is not working for you please file an [issue](https://github.com/gurmitteotia/guflow/issues). Your ideas, suggestion and collorbation will greatly help this library and its users.

### Documentation
Guflow is supported by [tutorial](https://github.com/gurmitteotia/guflow/wiki/Tutorial), [documentation](https://github.com/gurmitteotia/guflow/wiki) and [samples](https://github.com/gurmitteotia/guflow-samples) to get you started easily.

### Supported .NET frameworks:
dotnet standard 1.3 onwards

### Hosting
You need to self host workflows in either a EC2 intance or a docker container. However for 

### Getting help
Enable the [logging](https://github.com/gurmitteotia/guflow/wiki/Logging) to look for any obvious error and if problem persist then please raise an [issue](https://github.com/gurmitteotia/guflow/issues) in github
