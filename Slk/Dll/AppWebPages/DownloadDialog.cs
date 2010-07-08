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
        long? m_assignmentId;

        AssignmentItemIdentifier AssignmentItemIdentifier
        {
            get
            {
                if (AssignmentId == null)
                {
                     return null;
                }
                else
                {
                    return new AssignmentItemIdentifier(AssignmentId.Value);
                }
            }
        }

        long? AssignmentId
        {
            get
            {
                if (m_assignmentId == null)
                {
                    m_assignmentId = QueryString.ParseLong(QueryStringKeys.AssignmentId, null);
                }
                return m_assignmentId;
            }
        }

        // TODO: Needs error handling. Remove catch all exceptions and ignore as that is worse than none.
        protected override void OnLoad(EventArgs e)
        {
            AssignmentProperties assignmentProperties = SlkStore.GetAssignmentProperties(AssignmentItemIdentifier, SlkRole.Instructor);
            using (AssignmentDownloader downloader = new AssignmentDownloader(assignmentProperties))
            {
                string zippedFileName = downloader.ZippedFileName;
                String userAgent = Request.Headers["User-Agent"];
                if (userAgent.Contains("MSIE 7.0"))
                {
                    zippedFileName = zippedFileName.Replace(" ", "%20");
                }   

                using (Stream iStream = new FileStream(downloader.ZippedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Buffer to read 10K bytes in chunk:
                    byte[] buffer = new Byte[10000];

                    // Total bytes to read:
                    long dataToRead = iStream.Length;

                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader("Content-Disposition", "attachment; filename=\"" + zippedFileName + ".zip\"");

                    // Read the bytes.
                    while (dataToRead > 0)
                    {
                        // Verify that the client is connected.
                        if (Response.IsClientConnected)
                        {
                            int length = iStream.Read(buffer, 0, 10000);
                            Response.OutputStream.Write(buffer, 0, length);
                            Response.Flush();

                            dataToRead = dataToRead - length;
                        }
                        else
                        {
                            //prevent infinite loop if user disconnects
                            dataToRead = -1;
                        }
                    }
                }
                Response.Close();
            }
        }
    }
}
