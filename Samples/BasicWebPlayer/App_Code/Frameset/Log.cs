/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Diagnostics.CodeAnalysis;

#if DEBUG
namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Summary description for Log
    /// </summary>
    public sealed class Log :IDisposable
    {
        // NOTE: Change return value to enable logging.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private bool IsLogEnabled { get { return false; } }
        private bool m_isDisposed;

        private const string FILENAME = @"c:\backup\framesetLog.txt";

        string m_filePath;
        StreamWriter m_writer; 
        /// <summary>
        /// Open the log. 
        /// </summary>
        /// <param name="filePath">The path to the log file.</param>
        private Log(string filePath)
        {
            // Even in debug builds don't log anything unless compiled to do so.
            if (!IsLogEnabled)
                return;

            m_writer = File.AppendText(filePath);
            m_filePath = filePath;
        }

        public Log()
            : this(FILENAME)
        {
        }

        public void Dispose()
        {
            if (m_isDisposed)
                return;

            // Even in debug builds don't log anything unless compiled to do so.
            if (!IsLogEnabled)
                return;

            if (m_writer != null)
            {
                m_writer.Dispose();
                m_writer = null;
            }
            m_isDisposed = true;

            GC.SuppressFinalize(this);
        }


        public void WriteMessage(string message)
        {
            // Even in debug builds don't log anything unless compiled to do so.
            if (!IsLogEnabled)
                return;

            m_writer.Write("\r\nLog Entry : ");
            m_writer.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                            DateTime.Now.ToLongDateString());
            m_writer.WriteLine("  :{0}", message);
            m_writer.WriteLine ("-------------------------------");
            // Update the underlying file.
            m_writer.Flush(); 
        }

        public string FilePath { get { return m_filePath; } }
    }
}
#endif