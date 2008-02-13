using System;
using Microsoft.Build.Utilities;

namespace Microsoft.Education.Services
{
    /// <summary>
    /// Provides console output for an MSBuild engine
    /// </summary>
    internal class ConsoleLogger : Logger
    {
        public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
        {
            eventSource.ProjectStarted += eventSource_ProjectStarted;
            eventSource.TaskStarted +=new Microsoft.Build.Framework.TaskStartedEventHandler(eventSource_TaskStarted);
            eventSource.MessageRaised +=new Microsoft.Build.Framework.BuildMessageEventHandler(eventSource_MessageRaised);
            eventSource.WarningRaised +=new Microsoft.Build.Framework.BuildWarningEventHandler(eventSource_WarningRaised);
            eventSource.ErrorRaised +=new Microsoft.Build.Framework.BuildErrorEventHandler(eventSource_ErrorRaised);
            eventSource.ProjectFinished += new Microsoft.Build.Framework.ProjectFinishedEventHandler(eventSource_ProjectFinished);
        }

        void eventSource_ProjectFinished(object sender, Microsoft.Build.Framework.ProjectFinishedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        void eventSource_ErrorRaised(object sender, Microsoft.Build.Framework.BuildErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ResetColor();
        }

        void eventSource_WarningRaised(object sender, Microsoft.Build.Framework.BuildWarningEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(e.Message);
            Console.ResetColor();
        }

        void eventSource_MessageRaised(object sender, Microsoft.Build.Framework.BuildMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        void eventSource_TaskStarted(object sender, Microsoft.Build.Framework.TaskStartedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        void eventSource_ProjectStarted(object sender, Microsoft.Build.Framework.ProjectStartedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
