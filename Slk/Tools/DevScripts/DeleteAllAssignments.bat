rem  Copyright (c) Microsoft Corporation. All rights reserved. 

echo --
echo -- This batch file deletes ALL ASSIGNMENTS in the SharePointLearningKit database
echo --
pause
sqlcmd -d SharePointLearningKit -Q "DELETE FROM InstructorAssignmentItem"
sqlcmd -d SharePointLearningKit -Q "DELETE FROM LearnerAssignmentItem"
sqlcmd -d SharePointLearningKit -Q "DELETE FROM AssignmentItem"
pause
