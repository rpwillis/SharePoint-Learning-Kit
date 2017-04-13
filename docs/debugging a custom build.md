**Objective**

Debug a custom build of SLK

**Steps**

# Disable Strong-Name validation on your Sharepoint server since these assemblies are unsigned.  You only need to do this once.
## Open a Command Prompt and type '\Program Files\Microsoft Visual Studio 8\SDK\v2.0\Bin\sn.exe -Vr *,abc4ed181d6d6a94'.  This assumes that Visual Studio is installed on the server.  If not, you need to find and run sn.exe from a machine that has the .NET 2.0 SDK installed.
# Install the newly created .WSP file using UpgradeSolution.cmd as per GettingStarted.doc.
## Note: If you're building the source code on your server, the SlkDll solution will automatically place the assembly for the web part in the GAC after a successful compile so you can skip the .WSP deployment step and save time.
# Restart IIS using iisreset.exe or whatever tool you're comfortable with.
# Open a web browser to the SLK page to force W3WP.exe to be restarted and the SLK assembly to be loaded.
# Use Visual Studio's Debug -> Attach to Process to attach the debugger to W3WP.exe.
## Sometimes there are more than one W3WP processes running.  Double-check that you have the right process by using Debug -> Windows -> Modules.  You should see Microsoft.LearningComponents.dll in the list.
# Load the symbols for your custom build.  Right click on Microsoft.LearningComponents.dll in the Modules window and select Load Symbols.  Use the Open File dialog to locate the debug symbols in the source code directory of the machine you compiled on.
# Open a source code file that you want to debug.  For example, SLK-SourceCode\SLK\Dll\AssignmentListWebPart.cs.  Set a breakpoint in the file (such as on the AssignmentListWebPart constructor).
# Perform the activity (such as reloading the page with the AssignmentListWebPart on it) and the breakpoint will be hit.


I've commonly seen errors when trying to deploy a custom build of "file copy failed" and if you look into the logs you'll see certificate validation failures.  I've found that if I use the WSS Admin tool to remove the SLK web part from the root web and manually uninstall it that the installation will then succeed.