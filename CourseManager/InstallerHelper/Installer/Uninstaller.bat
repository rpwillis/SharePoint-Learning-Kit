@echo off
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"

echo.Uninstalling Course Manager from %1...
echo. >> log.txt
echo. >> log.txt
echo.%date% %time%: Uninstalling Course Manager... >> log.txt
echo.Restoring the Web.config...
echo.%date% %time%: Restoring the Web.config... >> log.txt
RestoreWebConfig.exe %1
if %ERRORLEVEL%==1 goto ERROR
echo.Web.config Restored Succesfully
echo.
echo.%date% %time%: Web.config Restored Succesfully >> log.txt
echo. >> log.txt

echo.Retracting CourseManagerWebParts.cab...
echo.%date% %time%: Retracting CourseManagerWebParts.cab... >> log.txt
%SPDIR%\bin\stsadm -o retractwppack -name CourseManagerWebParts.cab -immediate
%SPDIR%\bin\stsadm -o execadmsvcjobs
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
echo.CourseManagerWebParts.cab retracted succesfully
echo.
echo.%date% %time%: CourseManagerWebParts.cab retracted succesfully >> log.txt
echo. >> log.txt

echo.Retracting Course_Manager_Configure.wsp...
echo.%date% %time%: Retracting Course_Manager_Configure.wsp... >> log.txt
%SPDIR%\bin\stsadm -o retractsolution -name Course_Manager_Configure.wsp -allcontenturls -immediate
%SPDIR%\bin\stsadm -o execadmsvcjobs
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
echo.Course_Manager_Configure.wsp retracted succesfully
echo.
echo.%date% %time%: Course_Manager_Configure.wsp retracted succesfully >> log.txt
echo. >> log.txt

echo.Deleting CourseManagerWebParts.cab...
echo.%date% %time%: Deleting CourseManagerWebParts.cab... >> log.txt
%SPDIR%\bin\stsadm -o deletewppack -name CourseManagerWebParts.cab
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
echo.CourseManagerWebParts.cab deleted succesfully
echo.
echo.%date% %time%: CourseManagerWebParts.cab deleted succesfully >> log.txt
echo. >> log.txt

echo.Deleting Course_Manager_Configure.wsp...
echo.%date% %time%: Deleting Course_Manager_Configure.wsp... >> log.txt
%SPDIR%\bin\stsadm -o deletesolution -name Course_Manager_Configure.wsp
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
echo.Course_Manager_Configure.wsp deleted succesfully
echo.
echo.%date% %time%: Course_Manager_Configure.wsp deleted succesfully >> log.txt
echo. >> log.txt

echo.Course Manager uninstalled succesfully
echo.
echo.%date% %time%: Course Manager uninstalled succesfully from %1>> log.txt
echo. >> log.txt
echo. >> log.txt
iisreset
goto END

:ERROR
echo.%date %time%: Course Manager did not uninstalled properly >> log.txt

:End
@echo on