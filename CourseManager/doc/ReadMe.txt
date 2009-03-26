/* Copyright 2008 Microsoft Corporation. All rights reserved. */


SharePoint Learning Kit Course Manager 1.0 Read Me
July 11, 2008


The SharePoint Learning Kit (SLK) is a SCORM 2004 certified e-learning delivery and tracking 
application built as a Windows SharePoint Services 3.0 solution. It works with either 
Windows SharePoint Services 3.0 or Microsoft Office SharePoint Server 2007. 

The SharePoint Learning Kit Course Manager is an add-on that provides additional features 
to help instructors with course planning, assignments, and grading.

The SharePoint Learning Kit Course Manager includes the following two Web Parts:
*  Plan and Assign page
*  Monitor and Assess page

These Web Parts help instructors do the following:
*  Plan courses and activities
*  Distribute, collect, and monitor the status of assigned activities
*  Grade activities and monitor student performance

+-------------------------------------------------------------------------------------------
| System Requirements
+-------------------------------------------------------------------------------------------

SharePoint Learning Kit Course Manager requires: 

*  SharePoint Learning Kit (Alpha 1.3.1)
     Available as a free download from http://www.codeplex.com/SLK
     Must be deployed and activated in the site where you want to install the Course Manager

*  Microsoft .NET Framework 3.5
     Available as a free download from http://www.microsoft.com/downloads
     Must be installed on the target server


+-------------------------------------------------------------------------------------------
| Permission Requirements
+-------------------------------------------------------------------------------------------

To install and configure the SLK Course Manager, you must be a server farm administrator. To 
enable site features and add Web Parts, you must have full control of the site.


+-------------------------------------------------------------------------------------------
| Known Issues
+-------------------------------------------------------------------------------------------

If you try to access a site when you are installing the SLK Course Manager on that site 
(using installer.bat), you will see an error. You can access the site again after the 
installer has completed.

--------------------------------------------------------------------------------------------

When you are following the steps in the SLK Course Manager Installation Guide to activate 
SLK Course Manager, and you are at the SharePoint Central Administration site, there are two 
issues that may arise.

1) On the Configure Course Manager page, after you create the database for the
   site collection and click "OK," you may not receive the “Configuration Complete” 
   confirmation. If you do not, one or both of the following issues has occurred:
             
      * The database was not created correctly
      * The necessary registry keys were not created

   As a result, the Course Manager Web Parts do not work correctly, and using them results in
   the following message: 

      "An unexpected error has occurred.

      Web Parts Maintenance Page: If you have permissions, you can use this page to 
      temporarily close Web Parts or remove personal settings. For more information, 
      contact your site administrator.

      Troubleshoot issues with Windows SharePoint Services."

   To troubleshoot the problem, you can verify that the database was created correctly by 
   checking to see that the following tables were created:

      dbo.Activities
      dbo.ActivityGroups
      dbo.ActivityGroupStatuses
      dbo.Activity.Statuses
      dbo.AssignedActivityStatuses
      dbo.ConfigurationProfiles
      dbo.ConfigurationProperties
      dbo.Profiles_Properties
      dbo.UILayouts
      dbo.UserRoles

   To solve the problem, do one of the following:

   If the database was not created correctly, then the account used to configure SLK Course
   Manager may not have sufficient permissions to create databases within SQL. Verify the
   account permissions; it must have dbcreator, securityadmin and sysadmin SQL security 
   privileges. Then, repeat the steps to configure SLK Course Manager.


   If the database was created correctly and you still encounter the error when accessing 
   the Course Manager Web Parts, then the issue may be that the registry keys are not being 
   created correctly. For the Course Manager Web Parts to correctly function, the following 
   two registry keys must exist under HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SLKCourseManager\Settings

      String Value: Shared<site collection GUID>
      Value Data: Data Source=<SQL Server Name>;Initial Catalog=<Database Name>;Integrated Security=SSPI;Timeout=120
      
      String Value: SLKCM<site collection GUID>
      Value Data: Data Source=<SQL Server Name>;Initial Catalog=<Database Name>;Integrated Security=SSPI;Timeout=120

      Where: 
        <site collection GUID> is the GUID of the site collection where the Web application resides
        <SQL Server Name> is the name of the SQL server that contains the database
        <Database Name> is the database name specified for Course Manager when Configure Course Manager is run 

      For Example: 
        String Value: Shared8f874782-6a02-4a0b-aea0-b0a8699586f6
        Value Data: Data Source=SQLServer;Initial Catalog=Course_Manager;Integrated Security=SSPI;Timeout=120

        String Value: SLKCM8f874782-6a02-4a0b-aea0-b0a8699586f6
        Value Data:  Data Source=SQLServer;Initial Catalog=Course_Manager;Integrated Security=SSPI;Timeout=120
  
   If those registry keys were not created, you can manually create them within the registry 
   using the format specified above. 

      To determine the site collection GUID, you can perform the following steps:
      
      *  Access Application Management from within SharePoint Central Administration
      *  Select Site Collection Quotas and Locks under the SharePoint Site Management section
      *  Under the Site Collection section, change the Site Collection and choose the 
         Web Application that contains the Course Manager Web Parts. When you make the change, 
         you see the GUID of the site collection at the end of the URL in your browser’s 
         address bar. 
           
         For example:
         http://myslkcm:8944/_admin/sitequota.aspx?SiteId=8f874782-6a02-4a0b-aea0-b0a8699586f6
         The GUID of the site collection is at the end of the URL: 8f874782-6a02-4a0b-aea0-b0a8699586f6 

         NOTE: You may need to select a different Web application, and then reselect the 
               destination Web application, for the Site GUID to be reflected in the URL. 


2) When accessing the Course Manager Web Parts within your SharePoint Site, you may encounter 
   the following error: 

        “Error
        An error occurred during the processing of . Unknown server tag 'asp:ScriptManager'.”

   The error indicates the Configure Course Manager component did not correctly write all the 
   necessary information to the web.config for the Web application that corresponds to where 
   the Course Manager Web Parts are being deployed to. 

   To solve the problem, refer to Appendix B of the SLK Course Manager Installation Guide and 
   inspect the appropriate web.config file to determine which entries are missing. You can 
   copy and paste the missing sections into your web.config file directly from Appendix B. 
   
   NOTE: Back-up your existing web.config file before performing any manual edits and ensure that 
         you paste the entries into the correct sections within your web.config. 

   After pasting the missing elements into your web.config, you need to ensure that the 
   WebSiteGUID key under the <appSettings> section has the correct Site Collection GUID, which 
   will be the same as the one identified in the previous known issue. 

   For example: The value in this app setting key needs to be the site collection GUID.
                <add key="WebSiteGUID" value="8f874782-6a02-4a0b-aea0-b0a8699586f6" />
                 

   NOTE: After making any changes to the web.config, you need to perform an iisreset for 
         the changes to take effect. 

--------------------------------------------------------------------------------------------

For information about installing the SLK Course Manager, please refer to the 
SharePoint Learning Kit Course Manager Installation Guide.

For information about how to use SLK Course Manager, see the 
SharePoint Learning Kit Course Manager User Guide.

For more information about the SharePoint Learning Kit, FAQs, licensing, and future releases,
visit http://www.codeplex.com/SLK.