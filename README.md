### Guflow
A light weight C#.Net library to write distributed workflows and activities using [Amazon SWF](https://aws.amazon.com/swf/)


### Installation
 Install-Package Guflow

### Usage example
Following example shows on how you will create a workflow to transcode a video file:
```cs
[WorkflowDescription("1.0")]
public class TranscodeWorkflow : Workflow
{
  public TranscodeWorkflow()
  {
	ScheduleActivity<Download>()
		.OnFailure(e=>Reschedule(e).After(TimeSpan.FromSecond(2));
	
	//Schedule two transcode activities in parallel after Download activity
	ScheduleActivity<Transcode>("MP4").After<Download>()
		.WithInput(a=>new {Format = "MP4"});
				
	ScheduleActivity<Transcode>("MOV").After<Download>()
		.WithInput(a=>new {Format = "MOV"});

	//SendEmail activity will be scheduled once both Transcode activities are completed
    ScheduleActivity<SendEmail>().AfterActivity<Transcode>("MP4")
	    .AfterActivity<Transcode>("MOV")	
  }
}
```
Following example show how to create an acivity:
```cs
[ActivityDescription("1.0"]
public class DownloadActivity : ActivityDescription
{
  //You can write async and sync method.
  //Supports activity cancellation
  [Execute]
  public async DownloadedInfo ExecuteAsync(DownloadInput input, CancellationToken token)
  {
        await IO.DownloadFromS3Async(input.Location, token);
		
		return new DownloadedInfo("path");
  }
}
```
