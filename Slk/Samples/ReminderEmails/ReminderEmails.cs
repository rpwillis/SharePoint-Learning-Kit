using System;

namespace Microsoft.SharePointLearningKit
{
    public class ProcessReminderEmails
    {
        public static void Main(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0 || string.IsNullOrEmpty(arguments[0]))
            {
                Console.WriteLine("You must pass a url of a site collection.");
            }
            else
            {
                try
                {
                    ReminderEmails reminder = new ReminderEmails();
                    reminder.Process(arguments[0]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
