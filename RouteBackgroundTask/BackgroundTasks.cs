using Windows.ApplicationModel.Background;

namespace RouteBackgroundTask
{
    public sealed class BackgroundTasks : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //
            // TODO: Insert code to start one or more asynchronous methods using the
            //       await keyword, for example:
            //
            // await ExampleMethodAsync();
            //

            _deferral.Complete();
        }

        //var builder = new BackgroundTaskBuilder();
        //builder.Name = exampleTaskName;
        //builder.TaskEntryPoint = "RuntimeComponent1.ExampleBackgroundTask";
        //builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));

        public static BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();

            return task;
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var key = task.TaskId.ToString();
            var message = settings.Values[key].ToString();
        }
    }
}
