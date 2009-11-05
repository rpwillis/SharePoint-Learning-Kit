/* Copyright (c) 2009. All rights reserved. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.SharePointLearningKit.WebControls;
using Resources.Properties;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.ObjectModel;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    public class DownloadDialog : SlkAppBasePage
    {
        private AssignmentItemIdentifier AssignmentItemIdentifier
        {
            get
            {
                AssignmentItemIdentifier assignmentItemId = null;
                if (AssignmentId != null)
                {
                    assignmentItemId = new AssignmentItemIdentifier(AssignmentId.Value);
                }
                return assignmentItemId;
            }
        }
         private long? m_assignmentId;
        private long? AssignmentId
        {
            get
            {
                if (m_assignmentId == null)
                {
                    long id;
                    QueryString.Parse(QueryStringKeys.AssignmentId, out id, false);
                    m_assignmentId = id;
                }
                return m_assignmentId;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            AssignmentProperties assignmentProperties = SlkStore.GetAssignmentProperties(
                                                                 AssignmentItemIdentifier,
                                                                 SlkRole.Instructor);
            string finalTempFolderPath;
            string tempZippedFilePath;
            string zippedFileName;

            AssignmentDownloader assignmentDownloader = new AssignmentDownloader();
            assignmentDownloader.DownloadAssignment(assignmentProperties, out finalTempFolderPath,
                                                    out zippedFileName, out tempZippedFilePath);
            System.IO.Stream iStream = null;

            // Buffer to read 10K bytes in chunk:
            byte[] buffer = new Byte[10000];

            // Length of the file:
            int length;

            // Total bytes to read:
            long dataToRead;

            try
            {
                // Open the file.
                iStream = new System.IO.FileStream(tempZippedFilePath, System.IO.FileMode.Open,
                            System.IO.FileAccess.Read, System.IO.FileShare.Read);


                // Total bytes to read:
                dataToRead = iStream.Length;

                String userAgent = Request.Headers["User-Agent"];
                if (userAgent.Contains("MSIE 7.0"))
                {
                    zippedFileName = zippedFileName.Replace(" ", "%20");
                }   

                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + zippedFileName + ".zip\"");

                // Read the bytes.
                while (dataToRead > 0)
                {
                    // Verify that the client is connected.
                    if (Response.IsClientConnected)
                    {
                        // Read the data in buffer.
                        length = iStream.Read(buffer, 0, 10000);

                        // Write the data to the current output stream.
                        Response.OutputStream.Write(buffer, 0, length);

                        // Flush the data to the HTML output.
                        Response.Flush();

                        buffer = new Byte[10000];
                        dataToRead = dataToRead - length;
                    }
                    else
                    {
                        //prevent infinite loop if user disconnects
                        dataToRead = -1;
                    }
                }
            }
            catch (Exception)
            {
               
            }
            finally
            {
                if (iStream != null)
                {
                    //Close the file.
                    iStream.Close();
                }
                Response.Close();
            }
            //Response.Clear();
            //Response.AppendHeader(
            //   "content-disposition",
            //   "attachment; filename=" + zippedFileName + ".zip");
            //Response.WriteFile(tempZippedFilePath);
            //Response.Flush();
            //Response.Close();

            assignmentDownloader.DeleteTemp(finalTempFolderPath, tempZippedFilePath);
        }
    }
}
