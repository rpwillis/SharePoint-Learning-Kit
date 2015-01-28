rem  Copyright (c) Microsoft Corporation. All rights reserved. 
rem Usage: updateTranslations.cmd <culture> <debug/release>

..\Localize Update %1\Microsoft.SharePointLearningKit.xml ..\bin\out\Microsoft.SharePointLearningKit.xml %1\Microsoft.SharePointLearningKit.xml
..\Localize Update %1\Microsoft.LearningComponents.SharePoint.xml ..\bin\out\Microsoft.LearningComponents.SharePoint.xml %1\Microsoft.LearningComponents.SharePoint.xml
..\Localize Update %1\Microsoft.LearningComponents.Storage.xml ..\bin\out\Microsoft.LearningComponents.Storage.xml %1\Microsoft.LearningComponents.Storage.xml
..\Localize Update %1\Microsoft.LearningComponents.xml ..\bin\out\Microsoft.LearningComponents.xml %1\Microsoft.LearningComponents.xml
