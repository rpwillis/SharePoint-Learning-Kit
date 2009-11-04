rem  Copyright (c) Microsoft Corporation. All rights reserved. 

echo --
echo -- This batch file deletes ALL ASSIGNMENTS in the SharePointLearningKit database
echo --
pause
sqlcmd -d SharePointLearningKit -Q "DELETE FROM UserItem"
pause
