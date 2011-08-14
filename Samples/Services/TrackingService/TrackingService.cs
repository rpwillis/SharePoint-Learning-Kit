using System;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SharePointLearningKit.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TrackingService : TrackingContract
    {
        /// <summary>
        /// Do something with the incoming ActivityAttempt.  In our case we pass it to the SLKAssignmentBroker for storage in the SLK DB
        /// </summary>
        /// <param name="trackingData"></param>
        public void Submit(ActivityAttempt trackingData)
        {
            Trace.WriteLine(
                String.Format("Assignment Uploaded: ID = {0}, Status = {1}, ScaledScore = {2}, Identity = {3}",
                trackingData.LearnerAssignmentIdentifier,
                trackingData.CompletionStatus,
                trackingData.Score.Scaled,
                System.Security.Principal.WindowsIdentity.GetCurrent().Name
                ));

            try
            {
                SLKAssignmentBroker.DBConnection = ServiceConfiguration.Default.DatabaseConnection;
                SLKAssignmentBroker.ProcessActivityAttempt(trackingData);
            }
            catch (SLKAssignmentException slkae)
            {
                Trace.WriteLine(
                    String.Format("Error submitting assignment: ID = {0}, Error = {1}",
                    trackingData.LearnerAssignmentIdentifier,
                    slkae.Message
                    ));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = slkae.Message;
            }
            catch (Exception exception)
            {
                Trace.WriteLine(
                    String.Format("Error submitting assignment: ID = {0}, Error = {1}",
                    trackingData.LearnerAssignmentIdentifier,
                    exception.ToString()
                    ));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = exception.Message;
            }
        }
    }
}