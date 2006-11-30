/* Copyright (c) Microsoft Corporation. All rights reserved. */



DEBUGGING WITHIN TEST WEB APPLICATION

The easiest way to debug calls into Microsoft.SharePointLearningKit.dll (MS.SLK.dll) from these
.aspx pages is to use the Visual Studio's built-in web server.  Since these .aspx files use
application.master, a "fake" application.master is included in this project, purely for debugging
purposes -- this application.master includes the same content placeholders used in the "real"
application.master (in "C:\Program Files\Common Files\Microsoft Shared\web server
extensions\12\TEMPLATE").

To debug:
  1. Build the entire Slk project, i.e. run "fmake deb" (or "fmake" for both debug & release
     versions) in ..\Slk.
  2. Open SlkApp.sln in Visual Studio.
  3. Right-click on project "C:\...\App\" and select "Set as StartUp Project".
  4. Right-click on the .aspx file you want to debug and select "Set as Start Page".
     If you need to specify query parameters for the .aspx file, right-click "C:\...\App",
	 Property Pages, Start Options.
  5. Set breakpoints in SlkDll .cs files as needed.
  6. Press F5.

Troubleshooting:
  -- If you get strange warnings about source files being out of date, or VC/CLR won't let you
     set breakpoints, try the following things (these are all guesses at the moment):
       -- Remove Microsoft.LearningComponents* and Microsoft.SharePointLearningKit* from the GAC,
          rebuild, and try again.
       -- Also, try stopping the ASP.NET Development Server (appears in the taskbar near the
          clock) before rebuilding.
  -- If you add a control in a .aspx file, you need to add a protected member variable in the
     code-behind file.  (To find the type of that variable, stop in the debugger and look at
	 all fields on the auto-generated derived class.)  If the new protected member variables are
	 null, you may need to do a clean build and/or stop the Visual Studio "development server"
	 (in the task bar) to get the .aspx file to re-bind to the DLL.


DEBUGGING WITHIN SHAREPOINT

If you need to debug calls into MS.SLK.dll from these .aspx pages while running within SharePoint,
the instructions are the same as ..\Admin\ReadMe.txt, except these .aspx pages are in
...\12\TEMPLATE\LAYOUTS\SharePointLearningKit, not ...\12\TEMPLATE\LAYOUTS\SharePointLearningKit.

Other debugging notes for debugging within SharePoint:

  -- You can't delete the SLK database while it's in use.  You may need to stop the "ASP.NET
     Development Server" (in the taskbar) before attempting to delete the database.

  -- Use "nmake links" to make a hard link from .aspx files in this directory to the
     SharePointLearningKit application directory installed in SharePoint.  That way, you can
     make edits in the version installed in SharePoint while still allowing check in/out locally.
     Before beginning, you must check out the file locally.  WARNING: "nmake links" deletes the
     version in SharePoint.


WHEN YOU ADD A FILE...

If you add a file (e.g. .aspx or .gif) to this directory and it needs to be included on the
customer's front-end server machines, don't forget to update ..\Solution\manifest.xml and
..\Solution\Cab.ddf.


