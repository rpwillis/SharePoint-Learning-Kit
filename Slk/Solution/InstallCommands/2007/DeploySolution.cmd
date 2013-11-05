rem -- deploys the SharePointLearningKit solution to WSS; use RetractSolution.cmd to reverse this
net start spadmin
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deploysolution -name SharePointLearningKit.wsp -url http://localhost -immediate -allowGacDeployment
@ECHO OFF
ECHO:
ECHO  ***** NOTE -- THIS OPERATION MAY TAKE A MINUTE TO COMPLETE *****
ECHO  ** Use EnumSolutions.cmd to check the status of this operation **
ECHO:
