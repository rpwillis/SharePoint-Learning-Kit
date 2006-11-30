/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

BasicWebPlayer is Microsoft Learning Components (MLC) sample code that runs as a compile-on-demand
ASP.NET Web application.  (MLC is a set of components distributed with SharePoint Learning Kit.)
The instructions below explain how to run this sample.

This sample code is located in Samples\BasicWebPlayer within SLK-SDK-n.n.nnn-ENU.zip.

BasicWebPlayer is an e-learning content playback application.  In some ways BasicWebPlayer is
similar to SharePoint Learning Kit -- both support playback of SCORM 2004, SCORM 1.2, and Class
Server LRM and IMS+ content -- but there are some differences, such as the following:

  -- SLK requires Windows SharePoint Services 2007, SQL Server 2005, and Windows Server 2003.
     BasicWebPlayer does not require SharePoint (but does require SQL Server and Windows Server).

  -- Both SLK and BasicWebPlayer support the concepts of self-assignment, i.e. a user assigning
     content to themself so they can "execute" it.  SLK also supports the concept of assigning
     content to other users; BasicWebPlayer does not.

  -- In SLK, a package to be executed is uploaded to a document library where it can be
     self-assigned or assigned to others.  (SLK also maintains a file system cache to improve
     access performance.)  Each assignment causes the creation of a LearnerAssignmentItem in the
     SLK database -- a SLK extension to the MLC base schema.  Multiple LearnerAssignmentItem items
     can refer to the same package, which is referred to by a PackageItem item in the SLK
     database -- another SLK extension to the MLC base schema.  When the user executes a package,
     an MLC "attempt" is created that corresponds one-to-one with that LearnerAssignmentItem.

     In MLC, each package uploaded by a user is used for one attempt, and that user "owns" that
     package.  There is no LearnerAssignmentItem table; instead, the standard MLC PackageItem table
     is extended by BasicWebPlayer to contain a reference to the user who owns the package.
     Package files are stored in a file system directory, C:\BasicWebPlayerPackages by default.

     The schema differences described above illustrate the flexibility of MLC: different
     applications of MLC (such as SLK and BasicWebPlayer, in this case) can extend the base schema
     in different ways, to suit their specific needs.

     NOTE: The BasicWebPlayer extensions to the MLC base schema are defined in the Schema.xml
     file in the BasicWebPlayer directory.

  -- SLK supports assignment (including self-assignment) of non-e-learning content, such as
     Microsoft Word documents.  BasicWebPlayer does not.  (Non-e-learning content is a feature of
     the SLK application, not of MLC.)

The following instructions explain how to set up and configure BasicWebPlayer.  (Unlike SLK, which
is packaged as ready-to-install SharePoint "solution", BasicWebPlayer is sample code which requires
manual configuration.)

-------------------------------------------------------------------------------
To copy the MLC assemblies into Bin, and perform an initial compilation of
BasicWebPlayer's Schema.xml, run the following in a command window, in this
directory:

Init.bat

-------------------------------------------------------------------------------
To create the Training database used by this sample, copy and paste the
following into a command window.  This assumes that Microsoft SQL Server 2005
is installed on this computer.  NOTE: This batch file also creates the
directory "C:\BasicWebPlayerPackages", which is where this application stores
training package files.

CreateDatabase.bat

-------------------------------------------------------------------------------
To delete the Training database, run the following.  NOTE: This batch file also
deletes the directory "C:\BasicWebPlayerPackages", which is where this
application stores training package files.

DeleteDatabase.bat

-------------------------------------------------------------------------------
If you update Schema.xml, you'll need to recompile it (which generates
Schema.sql and App_Code\Schema.cs) and recreate the database:

CompileSchema.bat
DeleteDatabase.bat
CreateDatabase.bat

-------------------------------------------------------------------------------
To test this sample in the debugger, open BasicWebPlayer.sln and click Start,
Debugging.

-------------------------------------------------------------------------------
To test this sample using Internet Information Services (IIS):

If you haven't yet installed ASP.NET 2.0, do so by running the following
(click Start, Run, enter this command):

     %windir%\Microsoft.NET\Framework\v2.0.50727\aspnet_regiis.exe -i

Next, create a virtual directory pointing to this physical directory:

  1. In Control Panel, go to Administrative Tools and open Internet Information
     Services (IIS) Manager.

  2. Expand "(local computer)" and "Web Sites", right-click on Default Web
     Site, select New, Virtual Directory. 

  3. In the Virtual Directory Creation Wizard:
       (a) On the "Welcome" page Click Next.
       (b) On the "Virtual Directory Alias" page enter "BasicWebPlayer" and
           Click Next.
       (c) On the "Web Site Content Directory" page enter the full path to
           this directory.
       (d) On the "Virtual Directory Access Permissions" page make sure
           "Read" and "Run scripts" are checked.
       (e) On the last page click Finish.

  4. If you have SharePoint installed, make sure to exclude the BasicWebPlayer
     virtual directory from the list of directories that SharePoint manages.

  5. In a browser, navigate to: http://localhost/BasicWebPlayer

Finally, use regedit.exe to create the following registry key.  (There doesn't
need to be any values in this key.)  This allows BasicWebPlayer to write to the
event log.  If you've previously run BasicWebPlayer from Visual Studio this
registry key may already exist.

  HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog\Application\BasicWebPlayer

