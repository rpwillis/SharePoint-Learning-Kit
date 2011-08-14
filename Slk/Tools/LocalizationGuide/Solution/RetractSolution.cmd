rem -- undoes deployment of the SharePointLearningKitResources solution to WSS; use DeleteSolution.cmd to
rem -- fully delete the solution
net start spadmin
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
@SET /P LCID= < lcid.txt
%SPDIR%\bin\stsadm -o retractsolution -name SharePointLearningKit.wsp -immediate -lcid %LCID%
@ECHO OFF
ECHO:
ECHO  ***** NOTE -- THIS OPERATION MAKE TAKE A MINUTE TO COMPLETE *****
ECHO  ** Use EnumSolutions.cmd to check the status of this operation **
ECHO:
