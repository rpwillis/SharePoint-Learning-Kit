rem -- undoes deployment of the SharePointLearningKit solution to SharePoint; use DeleteSolution.cmd to
rem -- fully delete the solution
rem Change http://localhost to reference the web application SLK is deployed to
%SPDIR%\bin\stsadm -o retractsolution -name SharePointLearningKit.wsp -url http://localhost -immediate
Uninstall-SPSolution –Identity SharePointLearningKit.wsp –WebApplication http://localhost
