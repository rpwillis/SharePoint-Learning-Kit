@echo off

echo.Installing Course Manager...
echo.
echo. >> log.txt
echo. >> log.txt
echo.%date% %time%: Installing Course Manager... > log.txt
echo. >> log.txt

SolutionsAndFeaturesTest.exe 1.3.0.2 3.5.0.0 %1
if %ERRORLEVEL%==1 goto ERROR
echo.%date% %time%: Sharepoint Learning Kit (SLK) Version 1.3.1 Checked Succesfully >> log.txt
echo.%date% %time%: Microsoft .Net Framework Version 3.5 Checked Succesfully >> log.txt
echo. >> log.txt
echo.

@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"

echo.Copying the file SPDocumentSelectorIFrame.js...
echo.%date% %time%: Copying the file SPDocumentSelectorIFrame.js... >> log.txt
copy /y Files\SPDocumentSelectorIFrame.js %SPDIR%\TEMPLATE\LAYOUTS\1033
if %ERRORLEVEL%==1 goto ERROR
echo.The file was copied succesfully
echo.
echo.%date% %time%: The file was copied succesfully >> log.txt
echo. >> log.txt

::echo.Adding the solution Course_Manager_Configure.wsp...
::%SPDIR%\bin\stsadm -o addsolution -filename Solution\Course_Manager_Configure.wsp
::if %ERRORLEVEL%==-1 goto ERROR
::echo.

echo.Deploying Course_Manager_Configure.wsp to %1...
echo.%date% %time%: Deploying Course_Manager_Configure.wsp to %1... >> log.txt
%SPDIR%\bin\stsadm -o deploysolution -name Course_Manager_Configure.wsp -url %1 -immediate -allowGacDeployment -force
%SPDIR%\bin\stsadm -o execadmsvcjobs
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
if %ERRORLEVEL%==1 goto ERROR
echo.Course_Manager_Configure.wsp was deployed correctly at %1
echo.
echo.%date% %time%: Course_Manager_Configure.wsp was deployed correctly at %1 >> log.txt
echo. >> log.txt

echo.Activating the feature Course_Manager_Configure.wsp on %1...
echo.%date% %time%: Activating the feature Course_Manager_Configure.wsp on %1... >> log.txt
%SPDIR%\bin\stsadm -o activatefeature -name Course_Manager_Configure -url %1 -force
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
if %ERRORLEVEL%==1 goto ERROR
echo.Feature Course_Manager_Configure.wsp was activated correctly at %1
echo.
echo.%date% %time%: Feature Course_Manager_Configure.wsp was activated correctly at %1 >> log.txt
echo. >> log.txt

echo.Adding the webpart CourseManagerWebParts.cab...
echo.%date% %time%: Adding the webpart CourseManagerWebParts.cab... >> log.txt
%SPDIR%\bin\stsadm -o addwppack -filename Files\CourseManagerWebParts.cab -force
::if %ERRORLEVEL%==-1 goto ERROR
echo.WebPart CourseManagerWebParts.cab was added correctly
echo.
echo.%date% %time%: WebPart CourseManagerWebParts.cab was added correctly >> log.txt
echo. >> log.txt

echo.Deploying CourseManagerWebParts.cab to %1...
echo.%date% %time% Deploying CourseManagerWebParts.cab to %1... >> log.txt
%SPDIR%\bin\stsadm -o deploywppack -name CourseManagerWebParts.cab -url %1 -immediate -force
%SPDIR%\bin\stsadm -o execadmsvcjobs
if %ERRORLEVEL%==-2 goto ERROR
if %ERRORLEVEL%==-1 goto ERROR
if %ERRORLEVEL%==1 goto ERROR
echo.CourseManagerWebParts.cab was deployed correctly at %1
echo.
echo.%date% %time%: CourseManagerWebParts.cab was deployed correctly at %1 >> log.txt
echo. >> log.txt

@SET SPDIR=
iisreset
echo.
echo.Course Manager was installed succesfully...
echo. >> log.txt
echo.%date% %time%: Course Manager was installed succesfully... >> log.txt
echo. >> log.txt
echo. >> log.txt
goto END

:ERROR
echo.
echo.ERROR: Course Manager could not be installed correctly
echo.
echo. >> log.txt
echo.%date% %time%: ERROR: Course Manager could not be installed correctly >> log.txt
echo. >> log.txt
echo. >> log.txt

:END
@echo on
