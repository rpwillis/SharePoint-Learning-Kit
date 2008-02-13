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

        [OperationContract, WebGet(UriTemplate = "{name}/Headers")]
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