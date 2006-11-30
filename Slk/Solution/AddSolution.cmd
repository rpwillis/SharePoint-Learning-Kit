rem -- adds the SharePointLearningKit solution to WSS; use DeploySolution.cmd to deploy it
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
if exist Debug\SharePointLearningKit.wsp goto debug
goto release
:debug
%SPDIR%\bin\stsadm -o addsolution -filename Debug\SharePointLearningKit.wsp
goto done
:release
%SPDIR%\bin\stsadm -o addsolution -filename Release\SharePointLearningKit.wsp
:done
@ECHO OFF
ECHO:
ECHO -- Don't forget to use DeploySolution.cmd to deploy this solution.
ECHO -- Alternatively, use SharePoint Central Administration.
ECHO:
