rem -- undoes deployment of the SharePointLearningKit solution to WSS; use DeleteSolution.cmd to
rem -- fully delete the solution
net start spadmin
@SET SPDIR="%commonprogramfiles%\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o retractsolution -name SharePointLearningKit.wsp -url http://localhost -immediate
@ECHO OFF
ECHO:
ECHO  ***** NOTE -- THIS OPERATION MAKE TAKE A MINUTE TO COMPLETE *****
ECHO  ** Use EnumSolutions.cmd to check the status of this operation **
ECHO:
