@echo off
rem  Copyright (c) HunterStone. All rights reserved. 
rem -- installs SLK DLLs into the Global Assembly Cache

iisreset /stop
net stop iisadmin
net stop sptimerv3
net stop SPAdmin

gacutil /nologo /u  CourseManagerConfCs

pause

gacutil /nologo /i "CourseManagerConfCs\bin\Debug\CourseManagerConfCs.dll"

dir c:\windows\assembly\temp

echo Hit ENTER TO PROCEED
pause

iisreset
net start iisadmin
net start sptimerv3
net start SPAdmin

echo Restarting IIS...
waitfor BLUE /T15
