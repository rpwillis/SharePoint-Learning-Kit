rem -- installs the SharePointLearningKit feature directory from WSS; do AddFeature.cmd first
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o installfeature -name SharePointLearningKit
