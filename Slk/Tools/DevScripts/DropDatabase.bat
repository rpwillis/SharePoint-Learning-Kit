rem  Copyright (c) Microsoft Corporation. All rights reserved. 

echo --
echo -- This batch file DELETES the SharePointLearningKit database
echo --
pause
iisreset
sqlcmd -Q "DROP DATABASE SharePointLearningKit"
pause
