rem --  This script upgrades the SharePointLearningKit solution to the build located in this directory.
net start spadmin
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
if exist Debug\SharePointLearningKit.wsp goto debug
goto release
:debug
%SPDIR%\bin\stsadm -o upgradesolution -name SharePointLearningKit.wsp -filename Debug\SharePointLearningKit.wsp -immediate -allowGacDeployment
goto done
:release
%SPDIR%\bin\stsadm -o upgradesolution -name SharePointLearningKit.wsp -filename Release\SharePointLearningKit.wsp -immediate -allowGacDeployment
:done
@ECHO OFF
ECHO:
ECHO  ***** NOTE -- THIS OPERATION MAKE TAKE A MINUTE TO COMPLETE *****
ECHO  ** Use EnumSolutions.cmd to check the status of this operation **
ECHO:
