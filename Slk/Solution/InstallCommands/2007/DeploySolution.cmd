rem -- deploys the SharePointLearningKit solution to WSS; use RetractSolution.cmd to reverse this
net start spadmin
@SET SPDIR="%commonprogramfiles%\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deploysolution -name SharePointLearningKit.wsp -url http://localhost -immediate -allowGacDeployment
%SPDIR%\bin\stsadm -o execadmsvcjobs
