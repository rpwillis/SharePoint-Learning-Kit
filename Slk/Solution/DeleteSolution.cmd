rem -- deletes the SharePointLearningKit solution from WSS; use RetractSolution.cmd first
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deletesolution -name SharePointLearningKit.wsp
