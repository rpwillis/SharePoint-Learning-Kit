/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

This directory contains sample SharePoint Learning Kit reports, written as
C# ASP.NET pages designed to be used within SharePoint.

To try out these report pages, copy them to the directory containing
SharePoint Learning Kit application pages, typically:

C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\TEMPLATE\LAYOUTS\SharePointLearningKit

Then, view a report as follows: Sign into SharePoint as an instructor**
and navigate to a Create Assignment, Edit Assignment, or Grade Assignment
page of any assignment, then replace the name of the .aspx page (e.g.
"AssignmentProperties.aspx") plus any following query parameters (e.g.
"?AssignmentId=123") with the name of the report (for example,
"Report1.aspx").

** SiteCollectionInfo.aspx requires that your Windows account have access
to the SLK database, since that report demonstrates direct database access.

This sample code is located in Samples\SLK\ReportPages within SLK-SDK-n.n.nnn-ENU.zip.

