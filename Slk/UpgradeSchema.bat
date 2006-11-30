rem  Copyright (c) Microsoft Corporation. All rights reserved. 

REM -- This batch file upgrades ONLY the *views* and *security rules* of the SharePointLearningKit
REM -- database.  Don't use this batch file if anything else in SlkSchema.xml changed.
..\Src\SchemaCompiler\bin\Debug\SchemaCompiler.exe SlkSchema.xml /OutputUpgrade TempSlkUpgrade.sql /SchemaNamespace Microsoft.SharePointLearningKit.Schema /Namespace Microsoft.SharePointLearningKit
if errorlevel 1 goto exit
sqlcmd -d SharePointLearningKit -i TempSlkUpgrade.sql
if errorlevel 1 goto exit
iisreset
:exit
