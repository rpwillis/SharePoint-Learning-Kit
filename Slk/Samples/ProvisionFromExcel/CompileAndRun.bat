rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- Compile the program...
%windir%\Microsoft.NET\Framework\v2.0.50727\csc.exe /r:..\..\..\Debug\Microsoft.LearningComponents.dll /r:..\..\..\Debug\Microsoft.LearningComponents.SharePoint.dll /r:..\..\..\Debug\Microsoft.LearningComponents.Storage.dll /r:..\..\..\Debug\Microsoft.SharePointLearningKit.dll /r:%windir%\assembly\GAC_MSIL\Microsoft.SharePoint\12.0.0.0__71e9bce111e9429c\Microsoft.SharePoint.dll ProvisionFromExcel.cs
if errorlevel 1 goto exit

rem -- Run the program...
ProvisionFromExcel.exe

:exit
pause
