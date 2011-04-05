rem -- adds the SharePointLearningKitResources solution to WSS; use DeploySolution.cmd to deploy it
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
@SET /P LCID= < lcid.txt
%SPDIR%\bin\stsadm -o addsolution -filename SharePointLearningKit.wsp -lcid %LCID%
@ECHO OFF
ECHO:
ECHO -- Don't forget to use DeploySolution.cmd to deploy this solution.
ECHO -- Alternatively, use SharePoint Central Administration.
ECHO:
