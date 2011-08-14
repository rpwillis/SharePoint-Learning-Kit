Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
$file = Get-ChildItem "SharePointLearningKit.wsp"
Update-SPSolution –Identity SharePointLearningKit.wsp –LiteralPath $file.FullName -GACDeployment
