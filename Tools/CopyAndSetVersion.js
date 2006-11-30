/* Copyright (c) Microsoft Corporation. All rights reserved. */

// CopyAndSetVersion.js
//
// This script copies the file specified by argument 1 to the path specified by argument 2.
// In the process, the string "%VERSION4%" is replaced with the 4-part version number, e.g.
// "1.0.123.0".
//

// initialize helper COM objects
fs = new ActiveXObject("Scripting.FileSystemObject");
shell = new ActiveXObject("WScript.Shell");

// parse command-line arguments
if (WScript.Arguments.Length != 2)
{
	WScript.Echo("Usage: CopyAndSetVersion <input-file> <output-file>");
	WScript.Quit(1);
}
var strInputFileName = WScript.Arguments(0);
var strOutputFileName = WScript.Arguments(1);

// set <strScriptPath> to the full path to this .js file
var strScriptPath = WScript.ScriptFullName;

// set <strScriptFolderPath> to the full path to the folder containing this
// .js file
var strScriptFolderPath = fs.GetParentFolderName(strScriptPath);

// set <strVersion> to the version number of the current build, e.g.
// "1.0.286.0"
var strVersionCsPath = fs.BuildPath(strScriptFolderPath,
	"..\\Src\\Shared\\Version.cs");
var stm = fs.OpenTextFile(strVersionCsPath);
var strVersionCs = stm.ReadAll();
var strVersion = strVersionCs.replace(/^.*"(.*)".*$/, "$1");
stm.Close();

// read the input file into <strFileContents>
var stm = fs.OpenTextFile(strInputFileName);
var strFileContents = stm.ReadAll();
stm.Close();

// replace "%VERSION4%" in <strFileContents>
strFileContents = strFileContents.replace(/%VERSION4%/g, strVersion);

// write the updated file to the output location
var stm = fs.OpenTextFile(strOutputFileName, 2/*ForWriting*/, true);
stm.Write(strFileContents);
stm.Close();

// done
WScript.Echo("Wrote: " + strOutputFileName);

