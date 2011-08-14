/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

This directory contains a sample Web service which accesses SharePointLearningKit.

To try out SampleWebService.asmx, copy it to the directory containing SharePoint
Learning Kit application pages, typically:

C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\TEMPLATE\LAYOUTS\SharePointLearningKit

Then, do the following:

  1. Copy HelloWorld.zip (a very basic single-activity SCORM 2004 package) into a
     SLK-enabled document library.  Open the HelloWorld.zip context menu, select
     E-Learning Actions, and assign it to yourself.  Note that this sample assumes the
     content is assigning e-learning content, not non-e-learning content such as a
     Microsoft Word document.

  2. Navigate a Web browser to the "lobby" page for that assignment (for example,
     from an Assignment List Web Part that displays learner assignments, click on
     the assignment you created).

  3. If you haven't started the assignment yet, click Begin Assignment.  This sample
     assumes that the assignment is already started when you use the Web service.

  4. Make a note of the LearnerAssignmentId value in the address bar.  You'll need it
     below.

  5. In the Web browser address bar, replace the name of the .aspx page (e.g.
     "Lobby.aspx") plus any following query parameters (e.g. "?LearnerAssignmentId=123")
     with "SampleWebService.asmx".  The page you see is the standard ASP.NET UI
     for .asmx files.  (This isn't how a client application will typically access a
     Web service, but it's a convenient way to test the Web service.)

  6. To set the SCORM "scaled score" on the current activity of the assignment to
     a given value (typically a fraction between 0.0 and 1.0), click the
     SetLearnerAssignmentScore link, set the "learnerAssignmentId" parameter to the
     LearnerAssignmentId value you made a note of above, and set the "score" parameter
     to the desired score (e.g. "0.75").  Click Invoke.  If the operation is successful,
     you'll see a blank page.

  7. To get the SCORM "scaled score" that you just set, return to the original
     SampleWebService.asmx page, click the GetLearnerAssignmentScore link, and set the
     "learnerAssignmentId" parameter to the LearnerAssignmentId value.  Click Invoke.
     If the operation is successful, you'll see a small XML document containing the score.

  8. If you navigate back to the "lobby" page for the assignment and click "Mark as
     Complete", the Score will appear to be the score value you entered, converted to a
     percentage value.

Note that in a real Web service you could access all SLK functionality available to the
user; for example, you could set arbitrary SCORM data model values, perform queries
(similar to Assignment List Web Part), etc.  Authentication may be performed automatically
(by having the client application send network credentials -- that's what's happening in
the example above), or the Web service code can "log into" SharePoint (as shown in other
SLK code samples), if the application has a secure way of identifying the caller.

This sample code is located in Samples\SLK\WebService within SLK-SDK-n.n.nnn-ENU.zip.

