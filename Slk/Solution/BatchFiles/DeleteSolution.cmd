rem -- deletes the SharePointLearningKit solution from WSS; use RetractSolution.cmd first
@SET SPDIR="%commonprogramfiles%\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deletesolution -name SharePointLearningKit.wsp
