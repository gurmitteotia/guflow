### Guflow
A C#.Net library to write distributed workflows and activities using [Amazon SWF](https://aws.amazon.com/swf/)

[![Build status](https://ci.appveyor.com/api/projects/status/github/gurmitteotia/guflow?svg=true)](https://ci.appveyor.com/project/gurmitteotia/guflow/branch/master)

### Installation
```
 Install-Package Guflow
 ```
### Description
[Amazon SWF](https://aws.amazon.com/swf/) allows you to create distributable, elastic and fault tolerant workflows and acivities. While you can program Amazon SWF directly using REST api or Amazon .NET SDK, it can be very time consuming and error prone approach. Guflow will provide you high level and easy to use APIs to program the Amazon SWF.

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

            ScheduleLambda("ChargeCustomer").AfterLambda("ChoosSeat").AfterLambda("BookDinner");
            
            ScheduleLambda("SendEmail").AfterLambda("ChargeCustomer");
        }
    }             
          
```
Above workflow has three possible execution scenarios:
* 1. User has choosen to book both flight and hotel: In this case ChargeCustomer lambda function will be scheduled only after completion of ChoosSeat and BookDinner lambda functions.
* 2. User has choosen to book the flight only: In this case ChargeCustomer lambda will be scheduled after ChoosSeat lambda.
* 3. User has choosen to book the hotel only: In this case ChargeCustomer lambda will be scheduled after BookDinner lambda.  

You can implement the above workflow using SWF activities with same ease. You can also mix and match activities, lambdas or timers in a workflow with ease. You have the flexibility to customize the workflow executions to deal with complex real life scenarios. Please look at [examples](https://github.com/gurmitteotia/guflow-samples) and [custom workflow actions](https://github.com/gurmitteotia/guflow/wiki/workflow-actions) for more ideas.

### Features:
Guflow:
* Allows you to create complex workflows with ease and flexibility
* Use async task for polling for new decisions and activity tasks
* Provide equal supports for scheduling the activities written in other framework/language
* Supports async/sync activity method
* Supports activity throttling
* Supports parallel execution of activities and branches in workflow
* Supported by behavioural unit tests and continuously released
* Designed to support all the relevant features of Amazon SWF. At the time of writing this document Guflow supports:
  * Lambda
  * Activity
  * Timer
  * Signal
  * Workflow cancellation support
  * Marker
  * Activity heartbeat and cancellation

**Features in pipeline:** Please look at [project board](https://github.com/gurmitteotia/guflow/projects/1) to see what is is coming next.

### Documentation
Guflow is supported by [tutorial](https://github.com/gurmitteotia/guflow/wiki/Tutorial), [documentation](https://github.com/gurmitteotia/guflow/wiki) and [samples](https://github.com/gurmitteotia/guflow-samples) to get you started easily.

### Supported .NET frameworks:
dotnet core and .NET 4.5 onwards.

### Getting help
Please post your messages to [google group](https://groups.google.com/forum/#!forum/guflow)
