rem -- deactivates the SharePointLearningKit feature in the root SPWeb
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deactivatefeature -name SharePointLearningKit -url http://localhost
