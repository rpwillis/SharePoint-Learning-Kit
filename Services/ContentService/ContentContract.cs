using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SharePointLearningKit.Services
{
    [ServiceContract]
    public interface GetContentContract
    {
        [OperationContract, WebGet(UriTemplate = "{name}")]
        Stream GetFile(string name);

        [OperationContract, WebInvoke(UriTemplate = "{name}", Method="HEAD")]
        void GetFileHeaders(string name);
    }

    [ServiceContract(SessionMode=SessionMode.Required)]
    public interface PutContentContract
    {
        [OperationContract]
        void BeginPutFile(Uri documentLibrary, string name);

        [OperationContract]
        void PutFileChunk(byte[] chunk);
        
        [OperationContract]
        void EndPutFile();
    }
}