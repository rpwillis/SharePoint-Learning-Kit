using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Retrieves emails which need reminders and sends them.</summary>
    public class ReminderEmails
    {
        List<int> reminderDays;
        DateTime minDueDate;
        DateTime maxDueDate;


        /// <summary>Initializes a new instance of <see cref="ReminderEmails"/>.</summary>
        public ReminderEmails()
        {
        }

        /// <summary>Process the emails for a given url.</summary>
        public void Process(Uri url)
        {
            Process(url.ToString());
        }

        /// <summary>Process the emails for a given url.</summary>
        public void Process(string url)
        {
            using (SPSite site = new SPSite(url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SlkStore store = SlkStore.GetStore(web);
                    Process(store);
                }
            }
        }

        /// <summary>Process the emails for a given store.</summary>
        public void Process(ISlkStore store)
        {
            if (store.Settings.EmailSettings != null)
            {
                reminderDays = store.Settings.EmailSettings.ReminderDays;

                if (reminderDays.Count > 0)
                {
                    DetermineMaxAndMinDates();
                    DateTime today = DateTime.Now.Date;

                    IEnumerable<AssignmentProperties> collection = store.LoadAssignmentReminders(minDueDate, maxDueDate);

                    foreach (AssignmentProperties assignment in collection)
                    {
                        if (assignment.DueDate != null)
                        {
                            int daysTo = assignment.DueDate.Value.ToLocalTime().Date.Subtract(today).Days;

                            DateTime date = assignment.DueDate.Value.ToLocalTime().Date;
                            TimeSpan span = date.ToLocalTime().Subtract(today);

                            if (reminderDays.Contains(daysTo))
                            {
                                assignment.SendReminderEmail();
                            }
                        }
                    }
                }
            }
        }

        void DetermineMaxAndMinDates()
        {
            int maxDays = reminderDays[0];
            int minDays = reminderDays[0];

            foreach (int day in reminderDays)
            {
                if (maxDays < day)
                {
                    maxDays = day;
                }
                else if (minDays > day)
                {
                    minDays = day;
                }
            }

            DateTime today = DateTime.Now.Date;
            maxDueDate = today.AddDays(maxDays + 1);
            minDueDate = today.AddDays(minDays - 1);
        }
    }
}
