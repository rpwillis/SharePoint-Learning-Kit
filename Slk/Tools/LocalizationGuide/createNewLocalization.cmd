set /p culture=Enter The Culture:
set folder=translatedXMLs\%culture%
mkdir %folder%
echo %culture% > %folder%\culture.txt
copy ..\..\solution\resources\SLK.resx %folder%
copy ..\..\SlkSettings.xml %folder%\SlkSettings.xml.dat
call ExtractStrings.cmd
copy bin\out\*.xml %folder%
echo Created %folder%

REM Add the culture to the xml files
localize r %folder%\Microsoft.LearningComponents.xml %folder%\Microsoft.LearningComponents.xml %culture%
localize r %folder%\Microsoft.SharePointLearningKit.xml %folder%\Microsoft.SharePointLearningKit.xml %culture%
localize r %folder%\Microsoft.LearningComponents.SharePoint.xml %folder%\Microsoft.LearningComponents.SharePoint.xml %culture%
localize r %folder%\Microsoft.LearningComponents.Storage.xml  %folder%\Microsoft.LearningComponents.Storage.xml %culture%
