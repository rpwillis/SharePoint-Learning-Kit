using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;


using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Trace
{
    public class LogLevel
    {
        public const int HighPriority = 0,
        MediumPriority = 1,
        LowPriority = 2;
    }
    public class clsLog
    {
        #region Base Class
        /// <summary>
        /// Write a log information in the text file
        /// </summary>
        /// <param name="logTypeGUID"></param>
        /// <param name="logInfo"></param>
        public clsLog(string logTypeGUID, string logInfo)
        {
            WriteLog(logTypeGUID, logInfo);
        }
        /// <summary>
        /// Write the message in the database
        /// </summary>
        /// <param name="message"></param>
        public static void Trace(string message)
        {
            Trace(message, LogLevel.HighPriority);
        }
        public static void Trace(string message, int level)
        {
            Trace(message, level, "","","");
        }
        public static void Trace(string message, int level,string Action,string PortalGUID,string ProjectGUID)
        {
            int traceLevel;
            string trace = (string)clsConfigurationProfile.Current.getPropertyValue("TraceLevel");
            if (trace == null)
                traceLevel = 999;
            else
                traceLevel = Convert.ToInt32(trace);
            if (level >= traceLevel)
            {
                clsGenericLog GenericLog = clsGenericLog.NewObject();
                GenericLog.Description = message;
                GenericLog.Action = Action;
                GenericLog.PortalGUID = PortalGUID;
                GenericLog.ProjectGUID = ProjectGUID;
                GenericLog.Level = level;
                GenericLog.EntryDate = DateTime.Now;
                string DomainUser = "";
                string Domain = "";
                string User = "";
                HttpContext ctx = HttpContext.Current;
                if (ctx == null)
                {
                    //windows app
                    DomainUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name; ;
                    int Index = DomainUser.IndexOf("\\");

                    if (Index == -1)
                    {
                        User = DomainUser;
                    }
                    else
                    {
                        Domain = DomainUser.Substring(0, Index);
                        User = DomainUser.Substring(Index + 1);
                    }

                    GenericLog.Domain = Domain;
                    GenericLog.User = User;
                    GenericLog.Module = "DocumentList";                   
                }
                else
                {
                    //web app
                    DomainUser = ctx.Request.ServerVariables["AUTH_USER"];
                    int Index = DomainUser.IndexOf("\\");
                    if (Index == -1)
                    {
                        User = DomainUser;
                    }
                    else
                    {
                        Domain = DomainUser.Substring(0, Index);
                        User = DomainUser.Substring(Index + 1);
                    }
                    GenericLog.Domain = Domain;
                    GenericLog.User = User;
                    GenericLog.Module = ctx.Request.Url.AbsolutePath;                    
                }
                

                GenericLog.Save();
            }
        }
        public static void WriteLog(string logTypeGUID, string logInfo)
        {
            try
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx == null)
                { 
                    //windows app
                    StoreLogObject(logTypeGUID, logInfo, "DocumentList", System.Environment.UserName);
                }
                else
                { 
                    //web app
                    StoreLogObject(logTypeGUID, logInfo, ctx.Request.Url.AbsolutePath, ctx.Request.ServerVariables["AUTH_USER"]);
                }
                
            }
            // Catch all exceptions when writing the log
            catch (Exception e)
            {
            }
        }
        private static void StoreLogObject( string logTypeGuid, string logInfo, string logModule, string logUser)
        {
            // Open the current file (based on the date)
            string buffer = DateTime.Now.ToString();
            buffer += " | " + logTypeGuid.ToString();
            buffer += " | " + logUser;
            buffer += " | " + logInfo;
            buffer += " | " + logModule;
            // buffer += "\n";
            WriteLog(buffer);
        }
        public static void WriteLog(string buffer)
        {
            lock (typeof(clsLog))
            {
                try
                {
                    string fileName = "MIPLOG" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" +
                        DateTime.Now.Day.ToString() + ".txt";

                    fileName = "C:\\LOGS\\" + fileName; FileStream fs = File.Open(fileName, FileMode.Append, FileAccess.Write);
                    StreamWriter streamWriter = new StreamWriter(fs);

                    streamWriter.WriteLine(buffer);
                    streamWriter.Close();
                    streamWriter.Dispose();
                    fs.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }
        #endregion

        #region Support Log Methods

        #endregion
    }
}
