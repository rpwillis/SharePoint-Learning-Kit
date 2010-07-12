@ECHO OFF
ECHO "Uninstalling features"
call UninstallFeatures
ECHO "Retracting the solution"
call RetractSolution
:BEGINRETRACT
choice /c:delay /t 5 /d d > NUL
call EnumSolutions | findstr /c:"<State>"
if %ERRORLEVEL% EQU 0 GOTO BEGINRETRACT
call EnumSolutions | findstr /c:"<Deployed>TRUE</Deployed>"
if %ERRORLEVEL% EQU 0 GOTO ERROR

ECHO Deleting the solution
call DeleteSolution
ECHO Adding the solution
call AddSolution
ECHO Deploying the solution
call DeploySolution

ECHO Resetting IIS
iisreset
ECHO Fixing the breadcrumbs
call UpdateSolutionNavigation
GOTO END
:ERROR
Echo There was an error deploying the solution
call EnumSolutions
EXIT /B 1
:END
