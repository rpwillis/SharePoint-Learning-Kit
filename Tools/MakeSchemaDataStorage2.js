/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MakeSchemaDataStorage2.js
//
// This creates Src\Schema\SchemaDataStorage2.cs to be a copy of Src\Schema\SchemaDataStorage.cs
// but with XML documentation references changed to rather to SLK types than MLC types.
//

// initialize helper COM objects
fs = new ActiveXObject("Scripting.FileSystemObject");
shell = new ActiveXObject("WScript.Shell");

// set <strScriptPath> to the full path to this .js file
var strScriptPath = WScript.ScriptFullName;

// set <strScriptFolderPath> to the full path to the folder containing this
// .js file
var strScriptFolderPath = fs.GetParentFolderName(strScriptPath);

// set <strSchemaPath> to the full path to the ApiRef directory
var strSchemaPath = fs.BuildPath(
	fs.GetParentFolderName(strScriptFolderPath), "Src\\Schema");

// read SchemaDataStorage.cs into <strFileContents>
var strInputFileName = fs.BuildPath(strSchemaPath, "SchemaDataStorage.cs");
var stm = fs.OpenTextFile(strInputFileName);
var strFileContents = stm.ReadAll();
stm.Close();

// change MLC documentation references to SLK
strFileContents = strFileContents.replace(
	/<Typ>\/Microsoft\.LearningComponents\.Storage\.BaseSchema\./g,
	"<Typ>/Microsoft.SharePointLearningKit.Schema.");

// write <strFileContents> to SchemaDataStorage2.cs
var strOutputFileName = fs.BuildPath(strSchemaPath, "SchemaDataStorage2.cs");
var stm = fs.CreateTextFile(strOutputFileName, true);
stm.Write(strFileContents);
stm.Close();

// done
WScript.Echo("Generated SchemaDataStoage2.cs");

