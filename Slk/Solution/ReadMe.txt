/* Copyright (c) Microsoft Corporation. All rights reserved. */



                  SharePoint Learning Kit Application Installation Instructions


This directory and its subdirectories contain files needed to install the SharePoint Learning Kit
(SLK) "solution" into Windows SharePoint Services 3.0.

Note: Several executables in this directory tree, including slkadm.exe, won't work until SLK has
been installed.


+--------------------------------------------------------------------------------------------------
| To Install SharePointLearningKit.wsp into SharePoint
+--------------------------------------------------------------------------------------------------

The following steps must be performed in a command window (cmd.exe).

NOTE: As an alternative to steps 2 through 4, you can use SharePoint Central Administration:
Click Operations, Solution management (under Global Configuration), sharepointlearningkit.wsp,
and use the Deploy Solution button to start deployment.  HOWEVER, if you want site navigation
("breadcrumbs") to work for SLK pages, you need to perform step 5.

AddSolution.cmd (or its equivalent) must currently be done using command-line commands.
(AddSolution.cmd installs SLK; DeploySolution makes it "active".)

  1. Run AddSolution.cmd.

  2. To see if AddSolution.cmd worked, run EnumSolutions.cmd and make sure you see
     <Solution Name="sharepointlearningkit.wsp">.  Note that the solution isn't "deployed" yet --
     it's just stored in SharePoint, waiting to be deployed.  To deploy it, proceed to the next
     step.

  3. Run DeploySolution.cmd.

  4. Run EnumSolutions.cmd, and look for a block of XML in the output that looks like this:

         <Solution Name="sharepointlearningkit.wsp">
            <Id>00057001-c978-11da-ba52-00042350e42e</Id>
            <File>sharepointlearningkit.wsp</File>
            <Deployed>TRUE</Deployed>
            <WebApplicationSpecific>FALSE</WebApplicationSpecific>
            <ContainsGlobalAssembly>TRUE</ContainsGlobalAssembly>
            <LastOperationResult>DeploymentSucceeded</LastOperationResult>
            <LastOperationTime>...</LastOperationTime>
         </Solution>

     Repeat this step until you see this.  Specifically, look for "DeploymentSucceeded".
     If deployment fails, you can often determine the cause by reviewing the latest .log file
     in the WSS LOGS directory (typically "C:\Program Files\Common Files\Microsoft Shared\web
     server extensions\12\LOGS").  To cancel a deployment job, go to SharePoint Central
     Administration, Operations, Timer Job Definitions, click on the deployment job, and click
     "Delete".

     To verify that deployment worked, make sure the following exist:

       -- Microsoft.LearningComponents* and Microsoft.SharePointLearningKit in the GAC.

       -- In "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12":
            -- TEMPLATE\ADMIN\SharePointLearningKit directory
            -- TEMPLATE\LAYOUT\SharePointLearningKit directory
            -- TEMPLATE\FEATURES\SharePointLearningKit directory
            -- TEMPLATE\FEATURES\SharePointLearningKitAdmin directory

       -- SharePoint Central Administration, Application Management, SharePoint Learning Kit pages.

  4. Run UpdateSolutionNavigation.cmd -- this will make site navigation ("breadcrumbs") work for
     SharePoint Learning Kit pages.


+--------------------------------------------------------------------------------------------------
| To configure SharePoint Learning Kit:
+--------------------------------------------------------------------------------------------------

  1. If you already have an SLK database (named "SharePointLearningKit" by default) created using
     a previous version, it may be incompatible.  If so, delete it.  If in doubt, delete it.
     You can also delete your SLK package cache, located in "%windir%\Temp\SharePointLearningKit".

  2. Go to SharePoint Central Administration, Application Management, Configure SharePoint Learning
     Kit, select the SPSite (site collection) to configure (*** NOTE -- YOU MUST MAKE SURE YOU'VE
     GOT THE RIGHT WEB APPLICATION, I.E. NO PORT NUMBER, E.G. HTTP://YOURPC/ ***), and click OK.
     (The default settings are generally fine.  It's OK that "New SLK Settings file" is blank.)


+--------------------------------------------------------------------------------------------------
| To uninstall SharePointLearningKit.wsp:
+--------------------------------------------------------------------------------------------------

The following steps must be performed in a command window (cmd.exe).

NOTE: As an alternative to the steps below, you can use SharePoint Central Administration:
Click Operations, Solution management (under Global Configuration), sharepointlearningkit.wsp,
and use the Retract Solution button to start retraction.  When that's done (refresh the page
that shows sharepointlerningkit.wsp status), use the Remove Solution button to remove it.

  1. If you activated the SharePoint Learning Kit feature in any SPWeb, deactivate it in each
     SPWeb you activated it in.

  2. Run RetractSolution.cmd.

  3. Run EnumSolutions.cmd, and look for a block of XML in the output that looks like this:

         <Solution Name="sharepointlearningkit.wsp">
            <Id>00057001-c978-11da-ba52-00042350e42e</Id>
            <File>sharepointlearningkit.wsp</File>
            <Deployed>FALSE</Deployed>
            <WebApplicationSpecific>FALSE</WebApplicationSpecific>
            <ContainsGlobalAssembly>TRUE</ContainsGlobalAssembly>
            <LastOperationResult>RetractionSucceeded</LastOperationResult>
            <LastOperationTime>...</LastOperationTime>
         </Solution>

     Repeat this step until you see this.  Specifically, look for "RetractionSucceeded".
     If retraction fails, you can often determine the cause by reviewing the latest .log file
     in the WSS LOGS directory (typically "C:\Program Files\Common Files\Microsoft Shared\web
     server extensions\12\LOGS").  To cancel a retraction job, go to SharePoint Central
     Administration, Operations, Timer Job Definitions, click on the retraction job, and click
     "Delete".

     To verify that retraction worked, make sure the following *DON'T* exist:

       -- Microsoft.LearningComponents* and Microsoft.SharePointLearningKit in the GAC.
          (Of course you may have older or newer versions -- retraction will only delete the
          versions installed by the deployment process, I think.)

       -- In "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12":
            -- TEMPLATE\ADMIN\SharePointLearningKit directory
            -- TEMPLATE\LAYOUT\SharePointLearningKit directory
            -- TEMPLATE\FEATURES\SharePointLearningKit directory
            -- TEMPLATE\FEATURES\SharePointLearningKitAdmin directory

       -- SharePoint Central Administration, Application Management, SharePoint Learning Kit pages.

  4. Run DeleteSolution.cmd.

     To verify that deletion worked, run EnumSolutions.cmd and make sure you don't see
     <Solution Name="sharepointlearningkit.wsp">.

