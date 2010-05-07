rem --  This script upgrades the SharePointLearningKit solution to the build located in this directory.
net start spadmin
@SET SPDIR="%commonprogramfiles%\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o upgradesolution -name SharePointLearningKit.wsp -filename SharePointLearningKit.wsp -immediate -allowGacDeployment
%SPDIR%\bin\stsadm -o execadmsvcjobs
