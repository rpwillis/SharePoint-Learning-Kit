### Frequently Asked Questions About SLK

**Where do I get support for SharePoint Learning Kit?**

SharePoint Learning Kit is a community source project. As such, product support is provided via this community Codeplex site. While Microsoft engineers are actively involved in the community development and leadership, Microsoft product support does not provide support for SharePoint Learning Kit. Please post any issues or questions to the SLK Codeplex forums. 

**How do you get checkin rights for SLK? Who decides what goes into SLK and what stays out?**
See [Community Roles](Community-Roles)

**How do I check in a bug fix or new feature for SLK?**
See [Checking In](Checking-In)

*How do I test someone else's patch for a bugfix or a new feature?
See [Test a Patch](Test-a-Patch)

**How do I help localize SLK?**
See [HowToCheckInTranslationXMLFiles](HowToCheckInTranslationXMLFiles)

#### Installation, Configuration and Deployment

**I'm having trouble deploying SLK**

Several of you have had trouble deploying SLK under certain configuraitons, most notably on a domain controller. If you are having this problem, try editing the installation/deployment CMD files to replace 'localhost' with the fully-qualified domain name of your server or server farm. We have not tested this yet, but on one of the discussion threads, a member of our community suggested that it might work.

**When I run EnumSolutions.cmd, it tells me that deployment failed, or deployment has been pending for a long time**

If deployment fails, you can often determine the cause by reviewing the latest .log file in the WSS LOGS directory (typically "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\LOGS"). Regardless of the reason, you must cancel the deployment job before you can try again. To cancel a deployment job, go to SharePoint Central Administration, Operations, Timer Job Definitions, click on the deployment job, and click "Delete".

**I've installed and configured SLK, but "E-Learning Actions" is not showing up on my document library menu**

In order to assign content from a document library, the site that the document library is on must have the SharePoint Learning Kit feature activated. If you don't see the 'E-Learning Actions' item, it's usually because you haven't activated the feature, or you activated it, but the document library you're using isn't on the site that you activated it on. To be sure it's activated on the right site:

# Be sure you are signed in to SharePoint with permissions that allow you to change site settings
# Navigate to the document library that you want to assign from
# On the _Site Actions_ menu, click _Site Settings_
# Under _Site Administration_, click _Site features_
# On the _SharePoint Learning Kit_ row in the list of site features, click _Activate_

**I've installed and configured SLK, but the Assignment List Web Part is not showing up in my Web Part Gallery**

In order for the ALWP to show up in the Web Part Gallery on a site collection, it has to be added to the gallery on that site collection. The procedure for this is documented in Section 2.2.3 of the GettingStarted document (at the bottom of Page 6):

# Navigate to the top-level site in your site collection
# On the _Site Actions_ menu, click _Site Settings_
# Under _Galleries_, click _Web parts_
# Click _New_
# Check _Microsoft.SharePointLearningKit.WebParts.AssignmentListWebPart_, and then click _Populate Gallery_
