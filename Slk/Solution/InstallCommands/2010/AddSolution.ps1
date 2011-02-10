Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
$file = Get-ChildItem "SharePointLearningKit.wsp"
Add-SPSolution -LiteralPath $file.FullName
