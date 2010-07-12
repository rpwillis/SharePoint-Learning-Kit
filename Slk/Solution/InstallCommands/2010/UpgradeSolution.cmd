rem --  This script upgrades the SharePointLearningKit solution to the build located in this directory.
@SET WORKINGDIR=%~dp0
Update-SPSolution –Identity SharePointProject.wsp –LiteralPath %WORKINGDIR%\SharePointLearningKit.wsp -GACDeployment
