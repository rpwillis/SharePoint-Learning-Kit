/* Copyright (c) Microsoft Corporation. All rights reserved. */

// FixSdkDoc.js
//
// This script copies works around a problem (1345) in the generated SDK documentation set
// (intermittently, loading Documentation.htm in Internet Explorer causes an infinite loop).
//

// initialize helper COM objects
fs = new ActiveXObject("Scripting.FileSystemObject");
shell = new ActiveXObject("WScript.Shell");

// set <strScriptPath> to the full path to this .js file
var strScriptPath = WScript.ScriptFullName;

// set <strScriptFolderPath> to the full path to the folder containing this .js file
var strScriptFolderPath = fs.GetParentFolderName(strScriptPath);

// set <strPagesPath> to the full path to the SdkDoc\Pages directory
var strPagesPath = fs.BuildPath(fs.GetParentFolderName(strScriptFolderPath), "SdkDoc\\Pages");

// fix each file that needs to be fixed
FixFile(fs.BuildPath(strPagesPath, "Default_Contents.htm"));
FixFile(fs.BuildPath(strPagesPath, "Default_Index.htm"));

function FixFile(strPath)
{
	// read the file into <strFileContents>
	var stm = fs.OpenTextFile(strPath);
	var strFileContents = stm.ReadAll();
	stm.Close();

	// make the repair
	strFileContents = strFileContents.replace(/if \(parent\.DocTopic == undefined\)/, "if (false)");

	// write the updated file
	var stm = fs.OpenTextFile(strPath, 2/*ForWriting*/, true);
	stm.Write(strFileContents);
	stm.Close();

	// done
	WScript.Echo("Fixed: " + strPath);
}

