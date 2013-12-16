/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

This directory contains SharePoint Learning Kit sample code which demonstrates
how to add instructors to SharePoint Learning Kit assignments without the user
being one of those instructors.  (The SLK user interface only allows instructors
on an assignment to change the list of instructors on that assignment.)

Imagine this scenario: an instructor has created a number of assignments, with
him or herself as the only instructor on those assignments, then unexpectedly quit
and moved far away.  How can an administrator reassign that instructor's
assignments to a new instructor?  Even if the new instructor has the "SLK Instructor"
permission on the SharePoint Web sites containing the assignments, that only gives
the new instructor the ability to create new assignments -- the SLK user interface
won't allow the new instructor to access the old instructor's assignments.

This sample code demonstrates one possible user interface for solving this problem.
The sample, packaged as a Web page, first validates that the user is a SharePoint
Web farm administrator (i.e. someone with sufficient permission to access SharePoint
Central Administration) -- since the operations about to be performed violate the
"normal" security rules of SLK -- and then lets the user select (a) the old
instructor, and (b) one or more new instructors.  The page then searches for all SLK
assignments for which the old instructor is an instructor, and adds the new
instructor(s) as instructors to those assignments.  The scope of the search is the
SharePoint Web site collection in which the page is run, and all new instructors must
already be listed in "All People" on that site collection.  The list of old
instructors is compiled from the list of instructors on all assignments in the site
collection.

A key architectural point to understand: when a user interacts with SLK, SLK creates
a row in an interal SLK UserItem table containing a small amount of information about
the user.  If the user is later removed from "All People", the UserItem table is not
affected.  So, when this sample code searches for assignments and lists the names of
the instructors, it's looking at the names in SLK's UserItem table, not the names in
SharePoint's "All People" list.

To use this sample, do the following:

  1. This sample require that new LearningStore views be added to the SLK database
     schema, as follows:

       (a) The schema is represented in an XML file, SlkSchema.xml; in the SLK SDK this
           should be located in ..\..\..SLK\SlkSchema.xml.  You need to add the views
           located in SchemaUpdate.xml to SlkSchema.xml; add them before the
           "</StoreSchema>" XML end element.

       (b) Compile the schema, and install it into SLK.  (SLK allows simple schema
           upgrades, consisting of only view and/or security rule updates, to be
           installed in-place.  More complex schema changes require existing SLK
           databases to be deleted and re-created.)  Do this by running the
           UpgradeSchema.bat batch file.  Note that this batch file assumes that your
           SLK database server is the current computer, and the database is named
           "SharePointLearningKit"; change the batch file if your configuration is
           different.

  2. In a Visual Studio 2005 Command Prompt, execute the following.  This will allow
     AddInstructorsSample.dll to be loaded even though it's not code-signed.

         sn -Vr *,fac4eacf658b669a

  3. Open AddInstructorsSample.sln in Visual Studio 2005, and build it.  This will
     create bin\Debug\AddInstructorsSample.dll, and will automatically copy it to
     the Global Assembly Cache (using a post-build event).

     NOTE: The post-build event mentioned above uses gacutil.exe.  Therefore,
     gacutil.exe must be in an accessible location when AddInstructorsSample.sln
     is built.

  4. Copy AddInstructors.aspx to the directory containing SharePoint Learning Kit
     application pages, typically:

         C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\TEMPLATE\LAYOUTS\SharePointLearningKit

  5. To try out AddInstructors.aspx, navigate to any SLK page in the site collection
     in which you'd like to perform the assignment search operation (described
     above).  For example, click an assignment in Assignment List Web Part, and
     then replace then replace the name of the .aspx page (e.g. "Lobby.aspx" or
     "Grading.aspx") plus any following query parameters (e.g.
     "?LearnerAssignmentId=123") with "AddInstructors.aspx".  Follow the instructions
     on the page.  Note that this page requires you to be a SharePoint Farm
     administrator -- someone who can access SharePoint Central Administration.

To debug the sample, open AddInstructorsSample.sln in Visual Studio, and click Debug,
Attach to Process, and select all w3wp.exe instances you see in the list.  (Tip, click
the Process list header to sort in reverse alphabetical order.)  Click Attach, set
breakpoints, and execute the AddInstructors.aspx page as described above.  If you
change C# code, recompile and run iisreset.exe to force SharePoint to reload the DLL.

If you add controls to the .aspx code, declare them in the list of controls at the top
of AddInstructors.aspx.cs, then recompile, iisreset.exe, but then, *after* that, cause
the .aspx file to be saved (so it's last-modified date changes) to force ASP.NET to
recompile the .aspx file when it's next loaded.  (If you don't do this, ASP.NET won't
"bind" to the new control declarations in the .cs file.)

This sample code is located in Samples\SLK\AddInstructors within SLK-SDK-n.n.nnn-ENU.zip.

