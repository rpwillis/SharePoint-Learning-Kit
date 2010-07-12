rem -- deploys the SharePointLearningKit solution to SharePoint; use RetractSolution.cmd to reverse this
rem Change http://localhost to reference the web application SLK should be deployed
Install-SPSolution –Identity SharePointLearningKit.wsp –WebApplication http://localhost -GACDeployment
