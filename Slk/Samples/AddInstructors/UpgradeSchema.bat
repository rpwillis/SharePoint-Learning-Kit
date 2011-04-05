rem  Copyright (c) Microsoft Corporation. All rights reserved. 

REM -- This batch file upgrades ONLY the *views* and *security rules* of the SharePointLearningKit
REM -- database.  Don't use this batch file if anything else in SlkSchema.xml changed.
..\..\..\Release\SchemaCompiler.exe ..\..\..\SLK\SlkSchema.xml /OutputUpgrade SlkUpgrade.sql /SchemaNamespace Microsoft.SharePointLearningKit.Schema /Namespace Microsoft.SharePointLearningKit
if errorlevel 1 goto exit
sqlcmd -d SharePointLearningKit -i SlkUpgrade.sql
if errorlevel 1 goto exit
iisreset
:exit
