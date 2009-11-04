rem  Copyright (c) Microsoft Corporation. All rights reserved. 

echo --
echo -- This batch file deletes ALL PACKAGES in the SharePointLearningKit database
echo --
pause
sqlcmd -d SharePointLearningKit -Q "DELETE FROM PackageItem"
sqlcmd -d SharePointLearningKit -Q "DELETE FROM GlobalObjectiveItem"
rmdir /s /q "%windir%\Temp\SharePointLearningKit\PackageCache"
pause
