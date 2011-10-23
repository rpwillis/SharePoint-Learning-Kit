rem -- deletes the SharePointLearningKitResources solution from WSS; use RetractSolution.cmd first
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
@SET /P LCID= < lcid.txt
%SPDIR%\bin\stsadm -o deletesolution -name SharePointLearningKit.wsp -lcid %LCID%
