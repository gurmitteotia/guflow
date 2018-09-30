### Guflow
A C#.NET library, built on [Amazon SWF](https://aws.amazon.com/swf/) ( Simple Workflow Service ), let you coordinate the execution of serverless AWS Lambda functions, activities and timers with ease.

[![Build status](https://ci.appveyor.com/api/projects/status/github/gurmitteotia/guflow?svg=true)](https://ci.appveyor.com/project/gurmitteotia/guflow/branch/master)
### Installation
```
 Install-Package Guflow
     OR
 dotnet add package Guflow
 ```
 ### Description
 Guflow provides high level and easy to use APIs to program [Amazon SWF](https://aws.amazon.com/swf/). In Guflow you write-
* Workflows to orchestrate the scheduling of AWS lambda functions, activities and timers
* Activities to carry out the task.

You will find Guflow to be an alternative to Amazon Step functions and Amazon Flow framework and it provides simplicity and flexbility in one place. If you want some feature to be included in Guflow library or something is not working for you pleae file an [issue](https://github.com/gurmitteotia/guflow/issues). Your ideas, suggestion and collorbation will greatly help this library and its users.

### Features:
Guflow:
* Allows you to create complex workflows with ease and flexibility
* Use async task for polling for new decisions and activity tasks
* Provide equal supports for scheduling the activities written in other framework/language
* Supports async/sync activity method
* Supports activity throttling
* Supports parallel execution of lambda, activities and timers in workflow
* Supports fork and join of workflow branches
* Supports loops around an individual item(lambda, activity, timer) or whole execution branch
* Supported by behavioural unit tests and continuously released
* Designed to support all the relevant features of Amazon SWF. At the time of writing this document Guflow supports:
  * Lambda
  * Activity
  * Timer
  * Signal
  * Workflow cancellation support
  * Marker
  * Activity heartbeat and cancellation
  * Handling of all error events.


### Example
Following example shows BookHolidays workflow using AwsLambda:
     
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
            ScheduleLambda("BookFlight").When(_=>Input.BookFlight);
            ScheduleLambda("ChooseSeat").AfterLambda("BookFlight");

            ScheduleLambda("BookHotel").When(_ => Input.BookHotel);
            ScheduleLambda("BookDinner").AfterLambda("BookHotel");

            ScheduleLambda("ChargeCustomer").AfterLambda("ChooseSeat").AfterLambda("BookDinner");
            
            ScheduleLambda("SendEmail").AfterLambda("ChargeCustomer");
        }
        [WorkflowEvent(EventName.WorkflowStarted)]
        private WorkflowAction OnStart()
        {
            if (!Input.BookFlight && !Input.BookHotel)
                return CompleteWorkflow("Nothing to do");

            return StartWorkflow();
        }
    }             
          
```
Above workflow has four possible execution scenarios:
* 1. User has choosen to book both flight and hotel: In this case two branches "BookFlight->ChoosSeat" and "BookHotel->BookDinner" will schedule in parallel and ChargeCustomer lambda function will be scheduled only after completion of ChooseSeat and BookDinner lambda functions.
* 2. User has choosen to book the flight only: In this case ChargeCustomer lambda will be scheduled after ChooseSeat lambda.
* 3. User has choosen to book the hotel only: In this case ChargeCustomer lambda will be scheduled after BookDinner lambda.
* 4. User has choosen not to book flight or hotel: Workflow will be completed immediately.

You can implement the above workflow using SWF activities with same ease. You can also mix activities, lambdas or timers in a workflow. 


To understand the scheduling of lambda functions, activities and timers in a workflow, please read about [Delflow algorithm](https://github.com/gurmitteotia/guflow/wiki/Deflow-algorithm) and [workflow branches](https://github.com/gurmitteotia/guflow/wiki/Execution-branches). Default implementation will good enough for majority of complex scenarios, however you have flexibility to customize every aspect of it.

**Features in pipeline:** Please look at [project board](https://github.com/gurmitteotia/guflow/projects/1) to see what is is coming next.

### Documentation
Guflow is supported by [tutorial](https://github.com/gurmitteotia/guflow/wiki/Tutorial), [documentation](https://github.com/gurmitteotia/guflow/wiki) and [samples](https://github.com/gurmitteotia/guflow-samples) to get you started easily.

### Supported .NET frameworks:
dotnet standard 1.3 onwards

### Getting help
Please raise [issue](https://github.com/gurmitteotia/guflow/issues) in github
