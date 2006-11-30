/* Copyright (c) Microsoft Corporation. All rights reserved. */

NOTE: Instead of following the directions below, you should probably be installing
SharePointLearningKit.wsp, which includes AdminFeature.  See ..\Solution\ReadMe.txt.

To install the SharePointLearningKit Administration feature into the root SPWeb:
  1. Run AddFeature.cmd to copy the feature files into SharePoint's Features directory.
  2. Run InstallFeature.cmd to make Site Settings aware of the feature.

To uninstall the SharePointLearningKit Administration feature from the root SPWeb:
  1. Run UninstallFeature.cmd to remove the feature from Site Settings.
  2. Run DeleteFeature.cmd to delete the feature files from SharePoint's Features directory.

To edit the .xml files in the directory, use Visual Studio and attach the following .xsd:
C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\Template\XML\wss12.xsd

