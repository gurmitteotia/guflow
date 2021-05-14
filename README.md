### Guflow
A C#.NET library, built on [Amazon SWF](https://aws.amazon.com/swf/) ( Simple Workflow Service ), lets you write distributive, fault tolerant and scalable workflows using Lambda functions and self hosted activities with ease.

[![Build status](https://ci.appveyor.com/api/projects/status/github/gurmitteotia/guflow?svg=true)](https://ci.appveyor.com/project/gurmitteotia/guflow/branch/master)
### Installation
```
 Install-Package Guflow
     OR
 dotnet add package Guflow
 ```
 ### Description
In Guflow you can write-
* Workflows to orchestrate the scheduling of Lambda functions, activities, child workflows and timers
* Activities to carry out the task.

Guflow not only supports all the features of [Amazon SWF](https://aws.amazon.com/swf/) but also continue to add its own custom APIs to take care of real world requirments with ease.

### Features:
Guflow:
* Allows you to create complex workflows with ease and flexibility
* Supports parallel execution of the Lambda functions, activities, child workflows and timers in the workflow
* Supports fork and join of [workflow branches](/wiki/Workflow-branches)
* Supports recursion around an individual workflow item or an execution branch
* Provides equal supports for scheduling the activities written in other framework/language
* Provides rich [signal APIs](wiki/Workflow-signals) to cater for varied requirments including [human approval](wiki/Wait-for-signals)
* Supports child workflow.
* Supports async/sync [activity method](wiki/Activity-method)
* Supports activity throttling
* Supports activity [heartbeat and cancellation](wiki/Activity-heartbeat-and-cancellation)


### Example 1
Following example starts two parallel branches which later join to execute the workflow in a single branch.
     
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
Above example is further evolved in [workflow branching](wiki/Workflow-branches).


### Example 2
In the following example, workflow execution will pause after ApproveExpense Lambda function is completed and it will wait for either "Accepted" or "Rejected" signal for 5 days.
```cs
                         ApproveExpense          
                              |
                              |
                              v
         |````````````````````|``````````````````|
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
              .WithInput(_=>new{Id}); 
         
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
In following example, workflow execution will pause when "ReserveItem" is failed with "NotAvailable" reason and reschedule the Lambda function "ReserveItem" on receiving the signal "InventoryUpdated":
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
              .WithInput(_=>new{Id});
         
            ScheduleLambda("ChargeCustomer").AfterLambda("ReserveItem");
          
            ScheduleLambda("ShipItem").AfterLambda("ChargeCustomer");
    }             
          
```
You can use the [signal APIs](wiki/Workflow-signals) along with AWS Lambda functions to implement human approvals in your workflows.  A good number of examples involving manual approvals are provided in the [example](https://github.com/gurmitteotia/guflow-samples/tree/master/ServerlessManualApproval) project.

### Example 4
In following example, "ProcessLog" Lambda function is executed recursively within 1 hour of interval and up to 100 times.
```cs

    [WorkflowDescription("1.0")]
    public class ProcessLogWorkflow : Workflow
    {
        public ProcessLogWorkflow()
        {
            ScheduleLambda("ProcessLog")
				.OnCompletion(e=>Reschedule(e).After(Timespan.FromHours(1)).UpTo(times:100));
		}
    }             
          
```

**Features in pipeline:** Please look at [project board](https://github.com/gurmitteotia/guflow/projects/1) to see what is is coming next. If you want some feature to be included in Guflow library or something is not working for you please file an [issue](https://github.com/gurmitteotia/guflow/issues). Your ideas, suggestion and collorbation will greatly help this library and its users.

### Documentation
Guflow is supported by [tutorial](https://github.com/gurmitteotia/guflow/wiki/Tutorial), [documentation](https://github.com/gurmitteotia/guflow/wiki) and [samples](https://github.com/gurmitteotia/guflow-samples) to get you started easily.

### Supported .NET frameworks:
dotnet standard 1.3 onwards

### Hosting
You will host workflows execution in either an EC2 intance or in a docker container and for the workers you will either use Lambda functions or self hosted activities. You don't need a high spec machine (docker or EC2 instance) to execute workflows. It is worth to start with lower end machine (docker or EC2 instance) for executing the workflows. You should also explore the option to host the workflow execution on a shared machine instead of a dedicated machine to keep the cost down.

### Costs
Primarily while using Guflow you will pay for:
1. The usage of [Amazon SWF](https://aws.amazon.com/swf/pricing/)
1. The docker container or EC2 instance for hosting the workflow executions

Other costs may involve the usage of the Lambda functions, EC2 or docker container for running self hosted activites or any other AWS service (database, S3) you're using to support the workflows. This [document](wiki/Choosing-between-Lambda-functions-and-activities) can help you to choose between Lamdba functions and self hosted activites for your workers.

### Alternatives
On AWS stack alternatives are: Amazon [Step functions](https://aws.amazon.com/step-functions/) and Amazon [Flow framework](https://docs.aws.amazon.com/amazonswf/latest/awsflowguide/welcome.html)

### Getting help
Enable the [logging](wiki/Logging) to look for any obvious error and if problem persist then please raise an [issue](https://github.com/gurmitteotia/guflow/issues) in github
