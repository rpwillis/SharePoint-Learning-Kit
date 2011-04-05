/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationHelper
{
    /// <summary>
    /// Performs output to a text file using StreamWriter.
    /// If the file exists it will be overwritten.
    /// </summary>

    public class LogFile
    {
        private string m_fileName = "";
        private System.IO.StreamWriter m_writer;

        /// <summary>
        /// Creates file and opens it for writing, if the file already exists it will be overwritten.
        /// </summary>
        /// <param name="ObjComm">File name for the log file. It will be created in the working directory</param>
        public LogFile(string fileName)
        {
            m_fileName = fileName;
            string logFileFullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + m_fileName;
            m_writer = System.IO.File.CreateText(logFileFullPath);
        }

        /// <summary>
        /// Writes string into log file. Every record written is prepended with current UTC date and time
        /// </summary>
        /// <param name="LogRecord">String to be written into log file</param>
        public void WriteToLogFile(string logRecord)
        {
            m_writer.WriteLine(System.DateTime.UtcNow + "   " + logRecord);
        }

        /// <summary>
        /// Closes log file.
        /// </summary>
        public void FinishLogging()
        {
            m_writer.Close();
        }

       
    }
}
