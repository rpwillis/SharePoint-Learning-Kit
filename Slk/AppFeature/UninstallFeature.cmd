rem -- uninstalls the SharePointLearningKit feature from WSS
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o uninstallfeature -name SharePointLearningKit
