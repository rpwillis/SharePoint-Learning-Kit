rem  Copyright (c) Microsoft Corporation. All rights reserved. 

echo --
echo -- This batch file deletes ALL USER WEB LIST ITEMS in the SharePointLearningKit database
echo --
pause
sqlcmd -d SharePointLearningKit -Q "DELETE FROM UserWebListItem"
pause
