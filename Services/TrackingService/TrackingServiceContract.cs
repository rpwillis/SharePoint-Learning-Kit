using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SharePointLearningKit.Services
{
    [ServiceContract]
    public interface TrackingContract
    {
        [OperationContract, WebInvoke]
        void Submit(ActivityAttempt trackingData);
    }
}