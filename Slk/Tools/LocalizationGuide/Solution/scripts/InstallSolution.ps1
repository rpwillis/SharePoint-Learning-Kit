Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
$file = Get-ChildItem SharePointLearningKit.wsp
$lcid = Get-Content lcid.txt
Add-SPSolution -LiteralPath $file.FullName -Language $lcid
Install-SPSolution -Identity SharePointLearningKit.wsp -Language $lcid -GACDeployment
Write-Host "Solution added and deployed"'/>
Write-Host "Press any key to continue ..."'/>
