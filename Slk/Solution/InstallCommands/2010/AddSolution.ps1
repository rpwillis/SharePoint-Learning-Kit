$file = Get-ChildItem "SharePointLearningKit.wsp"
Add-SPSolution -LiteralPath $file.FullName
