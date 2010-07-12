rem -- adds the SharePointLearningKit solution to WSS; use DeploySolution.cmd to deploy it
@SET SPDIR="%commonprogramfiles%\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o addsolution -filename SharePointLearningKit.wsp
@ECHO OFF
ECHO:
ECHO -- Don't forget to use DeploySolution.cmd to deploy this solution.
ECHO -- Alternatively, use SharePoint Central Administration.
ECHO:
