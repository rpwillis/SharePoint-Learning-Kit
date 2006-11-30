/* Copyright (c) Microsoft Corporation. All rights reserved. */

NOTE: Instead of following the directions below, you should probably be installing
SharePointLearningKit.wsp, which includes AppFeature.  See ..\Solution\ReadMe.txt.

To install the SharePointLearningKit application feature into the root SPWeb:
  1. Run AddFeature.cmd to copy the feature files into SharePoint's Features directory.
  2. Run InstallFeature.cmd to make Site Settings aware of the feature.
  3. Run ActivateFeature.cmd to activate the feature in the root SPWeb.

To uninstall the SharePointLearningKit application feature from the root SPWeb:
  1. Run DectivateFeature.cmd to deactivate the feature in the root SPWeb.
  2. Run UninstallFeature.cmd to remove the feature from Site Settings.
  3. Run DeleteFeature.cmd to delete the feature files from SharePoint's Features directory.

To edit the .xml files in the directory, use Visual Studio and attach the following .xsd:
C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\Template\XML\wss12.xsd

Troubleshooting:
  -- If an SPWeb's feature list gets corrupted, i.e. you get an error when you try to view the
     list features, delete and re-create the SPWeb.  (If it's the root SPWeb, after deleting it
	 go to SharePoint Central Administration and create the root site collection.)

