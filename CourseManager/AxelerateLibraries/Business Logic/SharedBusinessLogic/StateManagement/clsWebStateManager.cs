using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using System.Reflection;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.StateManagement
{

    /// <summary>
    //
    /// </summary>
    public class clsWebStateManager
    {
        /// <summary>
        /// Indicates the type of storage that is going to be used for the state management
        /// </summary>
        public enum StateStorageType
        {
            //The session state will be used for storage. This storage will last as long as current user session expires.
            SessionState,

            //The application state will be used for storage. This storage will last until the application is shut down.
            ApplicationState
        }
        #region "Static Properties and Methods"

        private static clsWebStateManager m_Current = null;

        public static  clsWebStateManager Current()
        {
            if (m_Current == null)
            {
                m_Current = new clsWebStateManager();
            }
            return m_Current;
        }
        #endregion

        #region "Cross Page Parameter Passing"

        private void UntypedXPagePushParameter(string pParemeterName, object pParameter, StateStorageType pStateStorage) 
        {
            
            switch (pStateStorage)
            {
                case StateStorageType.ApplicationState:
                    break;
                case StateStorageType.SessionState:
                    HttpContext.Current.Session[pParemeterName] = pParameter;
                    break;
            }            
        }

        private object UntypedXPagePopParameter(string pParemeterName, StateStorageType pStateStorage)
        {            
            
            switch (pStateStorage)
            {
                case StateStorageType.ApplicationState:
                    break;
                case StateStorageType.SessionState:
                    object ToReturn = HttpContext.Current.Session[pParemeterName];
                    HttpContext.Current.Session[pParemeterName] = null;
                    return ToReturn;
                    break;
            }
            return null;
        }

        public T XPagePopParameter<T>(string ParemeterName, StateStorageType pStateStorage)
        {
            return (T) UntypedXPagePopParameter(ParemeterName, pStateStorage);
        }

        public void XPagePushParameter<T>(string ParemeterName, T pParameter, StateStorageType pStateStorage) 
        {
            UntypedXPagePushParameter(ParemeterName, pParameter, pStateStorage);
        }
        
        #endregion

        #region "Business Object Caching"
        public T BOFromQueryString<T>(string pQueryParameterName) where T : GUIDTemplate<T>, new()
        {
            string GUIDQueryParameterName = pQueryParameterName + "GUID";
            try 
            {
                System.Web.SessionState.HttpSessionState Session = HttpContext.Current.Session;            
                string GUID = HttpContext.Current.Request.Params[pQueryParameterName];
                bool Refresh = false;
                if (Session[GUIDQueryParameterName] != null)
                {
                    if (Session[GUIDQueryParameterName].ToString() != GUID)
                    {
                        Session[GUIDQueryParameterName] = GUID;
                        Refresh = true;
                    }
                    
                } else
                {
                    Refresh = true;
                }

                if (Refresh)
                {
                    /* T BusinessObject = null;
                    MethodInfo FactoryMethod = typeof(T).GetMethod("GetObjectByGUID");
                    
                    object[] FactoryParameters = new object[2];
                    FactoryParameters[0] = GUID;
                    FactoryParameters[1] = null;
                    FactoryMethod .Invoke(null, 

                    ReflectionHelper.InvokeBusinessFactoryMethod(
                    BusinessObject.GetObjectByGUID()
                    BLCriteria T.G
                    pQueryParameterName = ;
                    
                    BusinessObject.GUID;

                    return */
                }
            } catch (System.Exception ex)
            {
            };
            return null;
        }
        




        #endregion


    }
}
