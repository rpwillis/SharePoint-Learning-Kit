rem -- activates the SharePointLearningKit feature in the root SPWeb; do InstallFeature.cmd first
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o activatefeature -name SharePointLearningKit -url http://localhost
