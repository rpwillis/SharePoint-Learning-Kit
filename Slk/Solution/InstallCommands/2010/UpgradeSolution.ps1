$file = Get-ChildItem "SharePointLearningKit.wsp"
Update-SPSolution –Identity SharePointProject.wsp –LiteralPath $file.FullName -GACDeployment
