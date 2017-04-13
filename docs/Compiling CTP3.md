**Objective**

Enable Codeplex community developers to compile and debug SLK using existing CTP 3 files posted to the community site.
This document outlines the steps that are needed to 'fix up' the source code to make it buildable.  Future releases will have most of these fixes already in place.

**Steps**

# Download and unzip the CTP 3 Runtime Binaries, Documentation, and Source Code from the Codeplex site.
# Install Windows Sharepoint Services 2007 Beta 2 with SLK following the steps in the GettingStarted.doc file found in the Runtime Binaries zip.
# Patch the source code files so that they are usable outside of the build environment:
## Create a "References" directory at the root of source code directory (SLK-SourceCode-1.1.0.633-ENU\References)
### Copy Microsoft.Sharepoint.dll from your Sharepoint Server's hard drive into the References directory.  This DLL is found at C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\ISAPI\Microsoft.Sharepoint.dll
### Copy the Microsoft.LearningComponents.Compression.dll into the References directory.  This file is found in the SharePointLearningKit.wsp file that is in the Runtime Binaries directory SLK-Install-1.1.0.633-ENU\Release\SharePointLearningKit.wsp.  To extract the file, make a copy of the WSP file and rename it to end in .CAB.  Open the CAB file using Windows Explorer.  Now you can copy the file using standard Explorer file copy operations.
## Copy SlkSchema.xml, SlkSchema.sql, SlkSettings.xml, and SlkSettings.xsd from the Documentation directory SLK-SDK-CTP3\SLK into the source code SLK-SourceCode-1.1.0.633-ENU\SLK
## Fix-up the csproj files:
### SLK-SourceCode-1.1.0.633-ENU\Src\LearningComponents\LearningComponents.csproj:
#### Remove the broken reference to Compression.vcproj, replace it with a reference to SLK-SourceCode-1.1.0.633-ENU\References\Microsoft.LearningComponents.Compression.dll using "Browse".
### SLK-SourceCode-1.1.0.633-ENU\Src\SharePoint\SharePoint.csproj
#### Remove the broken reference to Compression.vcproj, replace it with a reference to SLK-SourceCode-1.1.0.633-ENU\References\Microsoft.LearningComponents.Compression.dll using "Browse".
#### Remove the broken references to Microsoft.Sharepoint.Library and Microsoft.Web.Design.Server.
### SLK-SourceCode-1.1.0.633-ENU\Slk\Dll\SlkDll.csproj
#### Remove the broken reference to Compression.vcproj, replace it with a reference to SLK-SourceCode-1.1.0.633-ENU\References\Microsoft.LearningComponents.Compression.dll using "Browse".
#### Remove the broken references to Microsoft.Sharepoint.Library and Microsoft.Web.Design.Server.
## Fix-up the sln files:
### SLK-SourceCode-1.1.0.633-ENU\Src\LearningComponents\LearningComponents.sln
#### Remove the broken reference to Compression.vcproj.
#### Remove the broken references to MiscTestUtilities, UnitTests
### Remove redundant SLN files:
#### SLK-SourceCode-1.1.0.633-ENU\Src\Storage\Storage.sln
#### SLK-SourceCode-1.1.0.633-ENU\Src\Sharepoint\Sharepoint.sln
# Open SLK-SourceCode-1.1.0.633-ENU\Slk\Dll\SlkDll.sln and build.  This will build other dependent projects.
## If you compiled the source code on your server, the updated SLK assembly has been placed in the GAC and you are now ready to debug.
# Optionally, you can create a new WSP file:
## Edit SLK-SourceCode-1.1.0.633-ENU\Slk\Solution\makefile in notepad.  Comment out using a leading # all lines that start with  ..\..\Tools\SignCode.
## Copy all GIF files from the SharePointLearningKit.CAB file that you created from the .WSP file above from Layouts\SharePointLearningKit\Frameset\Images into SLK-SourceCode-1.1.0.633-ENU\Slk\App\Frameset\Images.
## Open a Visual Studio 2005 Command Prompt.  Change directory into SLK-SourceCode-1.1.0.633-ENU\Slk\Solution.  Type 'nmake deb' to build a debug build of the WSP file.