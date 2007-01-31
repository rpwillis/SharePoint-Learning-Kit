rem -- deploys the SharePointLearningKitResources solution to WSS; use RetractSolution.cmd to reverse this
net start spadmin
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
@SET /P LCID= < lcid.txt
%SPDIR%\bin\stsadm -o deploysolution -name SharePointLearningKit.wsp -immediate -allowGacDeployment -force -lcid %LCID%
@ECHO OFF
ECHO:
ECHO  ***** NOTE -- THIS OPERATION MAKE TAKE A MINUTE TO COMPLETE *****
ECHO  ** Use EnumSolutions.cmd to check the status of this operation **
ECHO:
