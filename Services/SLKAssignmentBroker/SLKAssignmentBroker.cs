using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace SharePointLearningKit.Services
{
    /// <summary>
    /// Functionality to save an ActivityAttempt to an SLK Assignment in the SLK DB.
    /// </summary>
    public static class SLKAssignmentBroker
    {
        public static string DBConnection = @"Data Source=localhost;Initial Catalog=SharePointLearningKit;Integrated Security=True";

        /// <summary>
        /// Convert an incoming ActivityAttempt into SLK Database records
        /// </summary>
        /// <param name="activityAttempt">Microsoft.Education.Services.ActivityAttempt record containing CMI datamodel tracking information</param>
        /// <exception cref="SLKAssignmentException">Thrown if the Assignment cannot be found or if the Assignment Status will not allow additional attempts</exception>
        public static void ProcessActivityAttempt(ActivityAttempt activityAttempt)
        {
            SLKAssignmentsDataContext db = new SLKAssignmentsDataContext(DBConnection);

            IQueryable<LearnerAssignmentItem> learnerAssignmentQuery = from p in db.LearnerAssignmentItems where p.Id == long.Parse(activityAttempt.LearnerAssignmentIdentifier) select p;

            if (learnerAssignmentQuery.Count() == 0)
                throw new SLKAssignmentException("LearnerAssignment was not found");

            LearnerAssignmentItem learnerAssignmentItem = learnerAssignmentQuery.Single<LearnerAssignmentItem>();

            if (learnerAssignmentItem.NonELearningStatus == (int)CompletionStatus.Completed || learnerAssignmentItem.IsFinal)
                throw new SLKAssignmentException("Cannot modify a completed assignment");

            AssignmentItem assignmentItem = learnerAssignmentItem.AssignmentItem;
            ActivityPackageItem activityPackageItem = null;

            if (assignmentItem.ActivityPackageItem == null)
            {
                #region Create stub PackageItem and ActivityPackageItem for a newly uploaded Grava Package.
                #region Obtain the gravaPackageFormat
                PackageFormat gravaPackageFormat = null;
                IQueryable<PackageFormat> gravaPackageFormatQuery = from p in db.PackageFormats where p.Name == "Grava" select p;
                if (gravaPackageFormatQuery.Count() == 0)
                {
                    gravaPackageFormat = new PackageFormat();
                    db.PackageFormats.InsertOnSubmit(gravaPackageFormat);

                    // TODO Get the SLK Schema converted so that PackageFormat.Id is an Identity w/ Auto-Increment to get rid of this poo-poo.
                    // See http://www.codeplex.com/SLK/WorkItem/View.aspx?WorkItemId=14657
                    gravaPackageFormat.Id = 3;
                    gravaPackageFormat.Name = "Grava";
                }
                else
                {
                    gravaPackageFormat = gravaPackageFormatQuery.Single<PackageFormat>();
                }
                #endregion

                PackageItem packageItem = new PackageItem();
                db.PackageItems.InsertOnSubmit(packageItem);
                packageItem.PackageFormat1 = gravaPackageFormat;
                packageItem.Location = "Grava Null Package Location";
                packageItem.Manifest = XElement.Parse("<GravaNullManifest />");

                activityPackageItem = new ActivityPackageItem();
                db.ActivityPackageItems.InsertOnSubmit(activityPackageItem);
                activityPackageItem.ActivityIdFromManifest = "Grava Null Activity Id";
                activityPackageItem.OriginalPlacement = 0;
                activityPackageItem.Title = "Grava Null Activity Title";
                activityPackageItem.PackageItem = packageItem;
                #endregion
            }
            else
            {
                activityPackageItem = assignmentItem.ActivityPackageItem;
            }

            AttemptItem attempt = new AttemptItem();
            db.AttemptItems.InsertOnSubmit(attempt);
            attempt.UserItem = learnerAssignmentItem.UserItem;
            attempt.ActivityPackageItem1 = activityPackageItem; // RootActivityPackageItem, designer generated name
            attempt.PackageItem = activityPackageItem.PackageItem;

            ActivityAttemptItem activityAttemptItem = new ActivityAttemptItem();
            db.ActivityAttemptItems.InsertOnSubmit(activityAttemptItem);
            activityAttemptItem.AttemptItem = attempt;
            activityAttemptItem.ActivityPackageItem = activityPackageItem;

            // We're assuming one Activity per Package so we just set the Package CompletionStatus to the Activity's CompletionStatus
            attempt.CompletionStatus = (int)activityAttempt.CompletionStatus;

            activityAttemptItem.CompletionStatus = (int)activityAttempt.CompletionStatus;
            activityAttemptItem.RawScore = (float)activityAttempt.Score.Raw;
            activityAttemptItem.MaxScore = (float)activityAttempt.Score.Max;
            activityAttemptItem.ScaledScore = (float)activityAttempt.Score.Scaled;

            // Update the LearnerAssignmentItem's NonELearningStatus and FinalPoints with this data so it shows up on the Grading screen
            learnerAssignmentItem.NonELearningStatus = (int)activityAttempt.CompletionStatus;
            learnerAssignmentItem.FinalPoints = (float)activityAttempt.Score.Scaled * 100;

            db.SubmitChanges();
        }
    }
    
    /// <summary>
    /// An error occured while processing the SLKAssignment
    /// </summary>
    public class SLKAssignmentException : Exception
    {
        public SLKAssignmentException(string message) : base(message) { }
    }
}
