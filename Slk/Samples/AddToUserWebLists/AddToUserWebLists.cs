/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// AddToUserWebLists.cs
//
// This is SharePoint Learning Kit sample code that compiles into a console application.  You can
// compile this application using Visual Studio 2005, or you can compile and run this application
// without Visual Studio installed by double-clicking CompileAndRun.bat.
//
// This sample code is located in Samples\SLK\AddToUserWebLists within SLK-SDK-n.n.nnn-ENU.zip.
//
// This application adds Web sites to the user Web lists (visible in the E-Learning Actions page in
// the SLK user interface) of a given set of users.  The input is a two-column spreadsheet in
// Microsoft Excel XML Spreadsheet format:
//
//   1. The first column is the login name of the user, including the domain name if needed.
//      You can use ".\<login-name>" if <login-name> is a local machine account.
//
//   2. The second column is the URL of the SharePoint Web site to add to the user's Web list.
//      If the Web site is already in their list, its timestamp is updated so it appears at the
//      top of the most-recently-used list in E-Learning Actions.
//
// The spreadsheet file name can be specified on the command line.  If it's not, the default is
// "UserWebList.xls" in the same directory as AddToUserWebLists.exe.  There should be a worksheet
// (i.e. tab in Exce) named "User Web Lists".
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;

class Program
{
    static void Main(string[] args)
    {
        // load the XML spreadsheet into memory
        XPathNavigator rootNode;
        string path;
        if (args.Length == 1)
            path = args[0];
        else
        {
            path = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "UserWebList.xls");
        }
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite))
        {
            rootNode = new XPathDocument(stream).CreateNavigator();
        }

        // create a namespace manager for accessing the XML spreadsheet
        string ssNamespace = "urn:schemas-microsoft-com:office:spreadsheet";
        XmlNamespaceManager nsm = new XmlNamespaceManager(rootNode.NameTable);
        nsm.AddNamespace("ss", ssNamespace);

        // set <worksheetNode> to the "User Web Lists" worksheet
        XPathNavigator worksheetNode = rootNode.SelectSingleNode(
            "ss:Workbook/ss:Worksheet[@ss:Name = 'User Web Lists']", nsm);

        // loop once for each row in the worksheet
        const int ExpectedCellCount = 2;
        List<string> cells = new List<string>(ExpectedCellCount);
        foreach (XPathNavigator rowNode in worksheetNode.Select("ss:Table/ss:Row", nsm))
        {
            // set <cells> to the cells of this row
            cells.Clear();
            foreach (XPathNavigator cellNode in rowNode.Select("ss:Cell", nsm))
            {
                if (cellNode.MoveToAttribute("Index", ssNamespace))
                {
                    while (cells.Count < cellNode.ValueAsInt - 1)
                        cells.Add(String.Empty);
                    cellNode.MoveToParent();
                }
                XPathNavigator dataNode = cellNode.SelectSingleNode("ss:Data", nsm);
                cells.Add(dataNode.Value.Trim());
            }

            // ensure there are at least <ExpectedCellCount> cells
            while (cells.Count < ExpectedCellCount)
                cells.Add(String.Empty);

            // get the login name (i.e. "<domain>\<login-name>", or ".\<login-name>" for
            // local machine accounts), and the URL of the SharePoint Web site, listed in
            // this row of the worksheet; skip this row if either is blank
            string loginName = cells[0];
            string webUrl = cells[1];
            if ((loginName.Length == 0) || (webUrl.Length == 0))
                continue;

            // "log in" to SharePoint as the user running this program, and set <spUser> to the
            // SPUser of the user specified by this worksheet row
            SPUser spUser;
            using (SPSite anonymousSite = new SPSite(webUrl))
            {
                if (loginName.StartsWith(@".\"))
                    loginName = anonymousSite.HostName + loginName.Substring(1);
                using (SPWeb rootWeb = anonymousSite.RootWeb)
                {
                    spUser = rootWeb.AllUsers[loginName];
                }
            }

            // "log in" to SharePoint as the user <spUser>, then add <webUrl> to the SLK user Web
            // list of that user
            using (SPSite spSite = new SPSite(webUrl, spUser.UserToken))
            {
                using (SPWeb spWeb = spSite.OpenWeb())
                {
                    Console.WriteLine("Adding {0} to the user Web list of {1}",
                        spWeb.Url, spUser.Name);
                    SlkStore slkStore = SlkStore.GetStore(spWeb);
                    slkStore.AddToUserWebList(spWeb);
                }
            }
        }
    }
}

