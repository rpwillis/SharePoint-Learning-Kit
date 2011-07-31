/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Globalization;
using System.Web;

#endregion

/*
 * This file contains various database-related helper classes
 * 
 * Internal error numbers: 1200-1399
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a SqlCommand (and a corresponding SqlDataReader) that can
    /// write debug output to a TextWriter
    /// </summary>
    internal class LogableSqlCommand: IDisposable
    {
        // One big goal of this class: It must not read any data that the caller
        // doesn't read.  If it did that, it could change the results received by
        // the caller (e.g., in some situations, SQL Server requires that data be
        // read in the correct order).
        //
        // Another goal: It must not log anything that is "thrown away" at
        // a later time.  For example, SQL may return a result set and then an
        // error.  The calling code throws the result set away once the error
        // is found, so this class shouldn't log the result set either.
        //
        // What those goals means in term of implementation:  All logging happens
        // as late as possible.  Examples:
        //   - The log is output in the Dispose method (i.e., once everything
        //     has been read by the caller).  Until that point, the data to be
        //     output is maintained in a set of StringBuilders.
        //   - The code never directly reads data from the SqlCommand or
        //     SqlReader that could change the results received from the
        //     caller.  Instead, it "intercepts" the data as the caller
        //     reads it.

        /// <summary>
        /// The "real" SqlCommand
        /// </summary>
        private SqlCommand m_command;

        /// <summary>
        /// Destination for the log, or null if a log shouldn't be kept.
        /// </summary>        
        private TextWriter m_log;
        
        /// <summary>
        /// The "real" SqlDataReader
        /// </summary>
        /// <remarks>
        /// Null until the command is executed
        /// </remarks>
        private SqlDataReader m_reader;
        
        /// <summary>
        /// String containing the HTML for the start of the command, the command text,
        /// and the input parameters.
        /// </summary>
        /// <remarks>
        /// Always null if <Fld>m_log</Fld>=null.
        /// </remarks>
        private StringBuilder m_commandPart;
        
        /// <summary>
        /// String containing the HTML for the result sets
        /// </summary>
        /// <remarks>
        /// Always null if <Fld>m_log</Fld>=null.
        /// </remarks>
        private StringBuilder m_resultSetPart;
        
        /// <summary>
        /// String containing the HTML for the exception information
        /// </summary>
        /// <remarks>
        /// Always null if <Fld>m_log</Fld>=null.
        /// </remarks>
        private StringBuilder m_exceptionPart;
        
        /// <summary>
        /// True if the opening tag(s) related to a result set have been written
        /// to <Fld>m_resultSetPart</Fld> and the corresponding closing tag(s)
        /// have not been written.
        /// </summary>
        /// <remarks>
        /// Always null if <Fld>m_log</Fld>=null.
        /// </remarks>
        private bool m_openResultSetTag;
        
        /// <summary>
        /// True if the opening tag(s) related to a result set row have been
        /// written to <Fld>m_resultSetPart</Fld> and the corresponding
        /// closing tag(s) have not yet been written.
        /// </summary>
        /// <remarks>
        /// Always null if <Fld>m_log</Fld>=null.
        /// </remarks>
        private bool m_openResultSetRowTag;
        
        /// <summary>
        /// Current result set index
        /// </summary>
        /// <remarks>
        /// Always null if <Fld>m_log</Fld>=null.
        /// </remarks>
        private int m_resultSetCount;

        /// <summary>
        /// Index of the field that will next be written to <Fld>m_resultSetPart</Fld>
        /// </summary>
        private int m_nextFieldIndex;

        /// <summary>
        /// Create a new instance of a Logable SQL command
        /// </summary>
        /// <param name="connection">Connection on which the command will execute</param>
        /// <param name="log">Location in which the log should be output, or null
        ///    if no log should be output</param>                
        public LogableSqlCommand(SqlConnection connection, TextWriter log)
        {
            // Create the command
            m_command = new SqlCommand();
            m_command.Connection = connection;
            
            // Remember the log
            m_log = log;    
        }
        
        /// <summary>
        /// Identical to calling SqlCommand.Parameters.Add(<paramref name="parameter"/>)
        /// on the command
        /// </summary>
        /// <param name="parameter"></param>
        public void AddParameter(SqlParameter parameter)
        {
            // Fail if the command has already been executed
            if(m_reader != null)
                throw new LearningComponentsInternalException("LSTR1200");
                
            m_command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Identical to setting the CommandText of the SqlCommand to <paramref name="commandText"/>
        /// and then calling ExecuteReader on the SqlCommand.
        /// </summary>
        /// <param name="commandText"></param>
        public void Execute(string commandText)
        {
            // Fail if the command has already been exectued
            if(m_reader != null)
                throw new LearningComponentsInternalException("LSTR1210");
                
            // Save the command text
            m_command.CommandText = commandText;

            // Log
            if(m_log != null)
                LogCommandTextAndInputParameters();
            
            // Execute
            try
            {
                m_reader = m_command.ExecuteReader(CommandBehavior.SequentialAccess);
            }
            catch(SqlException e)
            {
                // Log
                if (m_log != null)
                    LogException(e);
                
                throw;                
            }
        }

        /// <summary>
        /// Identical to calling Read on the SqlDataReader returned from
        /// SqlCommand.ExecuteReader
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            // Fail if the command hasn't been executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1220");
                
            // Log
            if(m_log != null)
            {
                // Add the opening tags for the result set if it hasn't
                // already been added (i.e., if this is the first row
                // in the result set)
                LogResultSetStart();
                
                // Add the closing tags for a result set row if it
                // hasn't already been added (i.e., if this is not the
                // first row in the result set)
                LogResultSetRowEnd();
            }
                        
            bool returnValue;
            try
            {
                returnValue = m_reader.Read();
            }
            catch(SqlException e)
            {
                // Log
                if (m_log != null)
                    LogException(e);
                
                throw;                
            }
            
            // Log
            if(returnValue && (m_log != null))
                LogResultSetRowStart();
                
            return returnValue;
        }        

        /// <summary>
        /// Identical to calling NextResult on the SqlDataReader returned
        /// from SqlCommand.ExecuteReader
        /// </summary>
        /// <returns></returns>
        public bool NextResult()
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1230");
                
            // Log
            if(m_log != null)
            {
                // Add the closing tags for any rows
                LogResultSetRowEnd();
                
                // Add the closing tags for the result set
                LogResultSetEnd();
            }

            try
            {
                return m_reader.NextResult();
            }
            catch(SqlException e)
            {
                // Log
                if (m_log != null)
                    LogException(e);

                throw;
            }
        }

        /// <summary>
        /// Identical to calling IsDBNull on the SqlDataReader returned
        /// from SqlCommand.ExecuteReader
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsDBNull(int index)
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1240");
                
            // Get the value
            bool returnValue = m_reader.IsDBNull(index);
            
            // Log
            if(m_log != null)
            {
                if(returnValue)
                {
                    // It is null -- so that needs to be added to the output
                    LogResultSetValue(index, DBNull.Value);
                }
            }
            
            return returnValue;
        }

        /// <summary>
        /// Identical to calling GetInt32 on the SqlDataReader returned
        /// from SqlCommand.ExecuteReader
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetInt32(int index)
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1250");

            // Get the value
            int returnValue = m_reader.GetInt32(index);

            // Log
            if (m_log != null)
                LogResultSetValue(index, returnValue);

            return returnValue;
        }

        /// <summary>
        /// Identical to calling GetInt64 on the SqlDataReader returned
        /// from SqlCommand.ExecuteReader
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long GetInt64(int index)
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1260");
                
            // Get the value
            long returnValue = m_reader.GetInt64(index);
            
            // Log
            if(m_log != null)
                LogResultSetValue(index, returnValue);

            return returnValue;
        }

        /// <summary>
        /// Identical to calling GetSqlXml on the SqlDataReader returned
        /// from SqlCommand.ExecuteReader
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SqlXml GetSqlXml(int index)
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1270");
                
            // Get the value
            SqlXml returnValue = m_reader.GetSqlXml(index);
            
            // Log
            if(m_log != null)
                LogResultSetValue(index, returnValue);
                
            return returnValue;
        }

        /// <summary>
        /// Identical to calling GetValue on the SqlDataReader returned
        /// from SqlCommand.ExecuteReader
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetValue(int index)
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1290");
                
            // Get the value
            object returnValue = m_reader.GetValue(index);
            
            // Log
            if (m_log != null)
                LogResultSetValue(index, returnValue);

            return returnValue;
        }

        /// <summary>
        /// Identical to reading the FieldCount property on the
        /// SqlDataReader returned from SqlCommand.ExecuteReader
        /// </summary>
        /// <returns></returns>
        public int GetFieldCount()
        {
            // Fail if the command wasn't executed
            if (m_reader == null)
                throw new LearningComponentsInternalException("LSTR1300");
                
            return m_reader.FieldCount;
        }

        /// <summary>
        /// Identical to reading SqlCommand.Parameters.Count
        /// </summary>
        /// <returns></returns>
        public int GetParameterCount()
        {
            return m_command.Parameters.Count;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <remarks>
        /// Actually writes to the log output
        /// </remarks>
        public void Dispose()
        {
            if(m_log != null)
            {
                try
                {
                    // Write everything into a StringBuilder first, so that everything
                    // can be written to the log in one call (which is good for multithreading)
                    StringBuilder sb = new StringBuilder();
                    
                    // Write the command text, input parameters, etc.
                    sb.Append(m_commandPart);
                    if(m_exceptionPart != null)
                    {
                        // There was an exception -- so just write that
                        sb.Append(m_exceptionPart);
                    }
                    else
                    {
                        // There wasn't an exception
                        
                        // Close all the result set and result set row tags
                        // (so the "full" data is stored in m_resultSetPart)
                        LogResultSetEnd();
                        
                        // Write the result set info
                        sb.Append(m_resultSetPart);
                        
                        // Write the output parameters
                        AppendParameters(sb, m_command, true);
                    }
                    
                    // Write the close tags
                    sb.Append("</div>");
                    
                    // Now send everything to the log
                    m_log.Write(sb.ToString());
                    m_log.Flush();
                }
                catch(ObjectDisposedException)
                {
                    // Since the caller controls the lifetime of the TextWriter,
                    // we shouldn't fail if the TextWriter has been disposed.
                }
            }
            
            if (m_command != null)
                m_command.Dispose();
            if (m_reader != null)
                m_reader.Dispose();
        }

        
        /// <summary>
        /// Save the command text and input parameter information for
        /// later inclusion in the log output
        /// </summary>
        private void LogCommandTextAndInputParameters()
        {
            // Opening tags
            m_commandPart = new StringBuilder("<div style=\"border-bottom: 2px solid rgb(200, 200, 200);\">\r\n");

            // Add the command text
            m_commandPart.Append("<b>Command:</b><pre>");
            m_commandPart.Append(HttpUtility.HtmlEncode(m_command.CommandText));
            m_commandPart.Append("</pre><p/>\r\n");

            // Add the stack trace
            m_commandPart.Append("<b>Stack:</b><pre>");
            StackTrace stackTrace = new StackTrace(true);
            m_commandPart.Append(HttpUtility.HtmlEncode(stackTrace.ToString()));
            m_commandPart.Append("</pre><p/>\r\n");
            
            // Add the list of input parameters
            AppendParameters(m_commandPart, m_command, false);        
        }

        /// <summary>
        /// Save the exception information for later inclusion in the log output
        /// </summary>
        /// <param name="e">Exception that was thrown.</param>        
        private void LogException(SqlException e)
        {
            m_exceptionPart = new StringBuilder("<b>Exception:</b><br/>");
            m_exceptionPart.Append("Message="+HttpUtility.HtmlEncode(e.Message)+"<br/>");
            m_exceptionPart.Append("Number="+e.Number.ToString(CultureInfo.CurrentCulture)+"<br/>");
            m_exceptionPart.Append("Class=" + e.Class.ToString(CultureInfo.CurrentCulture) + "<br/>");
            m_exceptionPart.Append("State=" + e.State.ToString(CultureInfo.CurrentCulture) + "<br/>");
            m_exceptionPart.Append("<p/>\r\n");
        }

        /// <summary>
        /// Save the start of a result set for later inclusion in the log output
        /// </summary>
        private void LogResultSetStart()
        {
            // We're done if there has already been a open tag
            if(m_openResultSetTag)
                return;
                
            m_resultSetCount++;
            
            // Create the StringBuilder if necessary
            if(m_resultSetPart == null)
                m_resultSetPart = new StringBuilder();
                
            // Write the output
            m_resultSetPart.Append("<b>Result set #" + m_resultSetCount.ToString(CultureInfo.CurrentCulture) + ":</b><br/>");
            m_resultSetPart.Append("<table border=\"1\"><tr>");
            for(int i=0; i<m_reader.FieldCount; i++)
            {
                m_resultSetPart.Append("<td><b>" + HttpUtility.HtmlEncode(m_reader.GetName(i)) + "</b>(" +
                    HttpUtility.HtmlEncode(m_reader.GetDataTypeName(i)) + ")</td>");
            }
            m_resultSetPart.Append("</tr>");
            
            m_openResultSetTag = true;
        }

        /// <summary>
        /// Save the end of a result set for later inclusion in the log output
        /// </summary>
        private void LogResultSetEnd()
        {
            // We're done if there isn't an open tag
            if(!m_openResultSetTag)
                return;
            
            // Add a close tag for the row if needed
            LogResultSetRowEnd();
                
            m_resultSetPart.Append("</table><p/>\r\n");
            
            m_openResultSetTag = false;
        }
                
        /// <summary>
        /// Save the start of a result set row for later inclusion in the log output
        /// </summary>
        private void LogResultSetRowStart()
        {
            // We're done if there has already been an open tag
            if(m_openResultSetRowTag)
                throw new LearningComponentsInternalException("LSTR1305");
            
            // Add open tag for the result set if needed
            LogResultSetStart();
                
            m_resultSetPart.Append("<tr>");
            
            m_nextFieldIndex = 0;
            m_openResultSetRowTag = true;
        }
        
        /// <summary>
        /// Save the end of a result set row for later inclusion in the log output
        /// </summary>
        private void LogResultSetRowEnd()
        {
            // We're done if there isn't an open tag
            if(!m_openResultSetRowTag)
                return;

            // If we didn't add a column in the output for each field, add it
            // now
            while(m_nextFieldIndex < m_reader.FieldCount)
            {
                m_resultSetPart.Append("<td>&lt;Unknown&gt;</td>");
                m_nextFieldIndex++;
            }
                            
            m_resultSetPart.Append("</tr>");
            
            m_openResultSetRowTag = false;
        }

        /// <summary>
        /// Save a value in a result set for later inclusion in the log output
        /// </summary>
        /// <param name="index">Index of the field in the row.</param>
        /// <param name="value">Value of the field.</param>        
        private void LogResultSetValue(int index, object value)
        {
            if(index < m_nextFieldIndex)
                throw new LearningComponentsInternalException("LSTR1307");
                
            // Add values for columns that we're skipping
            while(m_nextFieldIndex < index)
            {
                m_resultSetPart.Append("<td>&lt;Unknown&gt;</td>");
                m_nextFieldIndex++;                
            }
            
            m_resultSetPart.Append("<td>");
            AppendValue(m_resultSetPart,value);
            m_resultSetPart.Append("</td>");
            
            m_nextFieldIndex = index+1;
        }

        /// <summary>
        /// Helper function that appends parameter tags/text to a StringBuilder
        /// </summary>
        /// <param name="sb">StringBuilder to be written to</param>
        /// <param name="command">Command from which to retrieve parameters</param>
        /// <param name="outputParameters">If true, only the output parameters are added.
        ///     If false, then only the input parameters are added.</param>        
        private static void AppendParameters(StringBuilder sb, SqlCommand command, bool outputParameters)
        {
            // Remember if a parameter has been found (if no parameters are found, then
            // we shouldn't output anything)
            bool parameterFound = false;
            
            // Enumerate through each parameter
            foreach (SqlParameter parameter in command.Parameters)
            {
                // Skip if the caller is asking for output parameters, and this is an input parameter
                if ((outputParameters) && (parameter.Direction == ParameterDirection.Input))
                    continue;
                    
                // Skip if the caller is asking for input parameters, and this is an output parameter
                if ((!outputParameters) && (parameter.Direction == ParameterDirection.Output))
                    continue;

                // If this is the first parameter, add some header tags
                if (!parameterFound)
                {
                    sb.Append("<b>");
                    if (outputParameters)
                        sb.Append("Output");
                    else
                        sb.Append("Input");
                    sb.Append(" Parameters:</b><br/><table border=\"1\"><tr><td><b>Name/Type</b></td><td><b>Value</b></td></tr>");
                    parameterFound = true;
                }

                // Add the parameter name and value
                sb.Append("<tr><td>" + HttpUtility.HtmlEncode(parameter.ParameterName) + "(" + 
                    HttpUtility.HtmlEncode(parameter.SqlDbType.ToString()) + ")</td><td>");
                AppendValue(sb, parameter.Value);
                sb.Append("</td>");
                sb.Append("</tr>\r\n");
            }
            
            // If we added some header tags, then finish with some footer tags
            if (parameterFound)
                sb.Append("</table><p/>\r\n");
        }

        /// <summary>
        /// Helper function that appends a value to a StringBuilder
        /// </summary>
        /// <param name="sb">StringBuilder to be written to</param>
        /// <param name="value">Value to be written</param>
        private static void AppendValue(StringBuilder sb, object value)
        {
            if (value == null)
                throw new LearningComponentsInternalException("LSTR1308");
            else if (Object.ReferenceEquals(value, DBNull.Value))
                sb.Append("DBNull.Value");
            else if (value.GetType() == typeof(string))
            {
                string stringValue = (string)value;
                if(stringValue.Length == 0)
                    sb.Append("<br/>");
                else if (stringValue.Length > 200)
                    sb.Append(HttpUtility.HtmlEncode(stringValue.Substring(0, 200)) + "...");
                else
                    sb.Append(HttpUtility.HtmlEncode(stringValue));
            }
            else if ((value.GetType() == typeof(long)) ||
                    (value.GetType() == typeof(bool)) ||
                    (value.GetType() == typeof(Single)) ||
                    (value.GetType() == typeof(Double)) ||
                    (value.GetType() == typeof(DateTime)) ||
                    (value.GetType() == typeof(Guid)) ||
                    (value.GetType() == typeof(int)))
            {
                sb.Append(HttpUtility.HtmlEncode(value.ToString()));
            }
            else if (value.GetType() == typeof(SqlXml))
            {
                string xmlString = ((SqlXml)value).Value;
                if (xmlString.Length > 200)
                    sb.Append(HttpUtility.HtmlEncode(xmlString.Substring(0, 200)) + "...");
                else if(xmlString.Length == 0)
                    sb.Append("<br/>");
                else
                    sb.Append(HttpUtility.HtmlEncode(xmlString));
            }
            else if (value.GetType() == typeof(byte[]))
            {
                byte[] bytes = (byte[])value;
                if (bytes.Length > 30)
                    sb.Append(HttpUtility.HtmlEncode(BitConverter.ToString(bytes, 0, 30)) + "...");
                else if(bytes.Length == 0)
                    sb.Append("<br/>");
                else
                    sb.Append(HttpUtility.HtmlEncode(BitConverter.ToString(bytes)));
            }
            else
                throw new LearningComponentsInternalException("LSTR1310");
        }
        
    }
}
