/* Copyright (c) Microsoft Corporation. All rights reserved. */


This directory builds SharePoint Learning Kit (SLK) functionality.

  -- Dll: Builds Microsoft.SharePointLearningKit.dll, which implements functionality used by SLK
     web pages.  This DLL also implements the SLK web part(s).

  -- App: Contains the .aspx/.cs pages that comprise the SLK application, but not including 
     administration features.

  -- Admin: Contains the .aspx/.cs pages that comprise the SLK Administration feature within
     SharePoint Central Administration.

  -- Localization

  -- Samples: Contains sample applications showing the use of the API.

  -- Solution: Builds SharePointLearningKit.wsp, the WSS "Solution" that implements all SLK
     features (both application and adminstration).

  -- slkadm: Contains the source code for the SlkAdm project.

  -- Tools: Contains tools used in the building of the project.

  -- SlkSchema.xml: Defines the SLK LearningStore schema.  This file is used as input to the
     Microsoft Learning Components (MLC) SchemaCompiler.exe; the output is SlkSchema.sql and
     Dll\SlkSchema.cs.

  -- SlkSettings.xml: The default out-of-box SLK Settings file, which is stored in an SLK database
     and which defines various SLK application settings and query definitions.  Customers can
     customize this file and upload a new version to the database.

  -- SlkSettings.xsd, .xsx: The XML schema of SlkSettings.xml.

To do development in this directory:

  1. Prerequisite: All projects in ..\Src (i.e. all Microsoft Learning Components DLLs) need to
     be built.  To ensure this, run "msbuild" (to build both debug and release versions of all DLLs), 
     or at least "msbuild /t:Debugd" (to build debug versions).

  2. If SLK components (in this directory tree) haven't been built yet, run "msbuild" (to build both
     debug and release versions of everything), or at least "msbuild /t:Debug" (to build debug versions).

  3. Deploy the SharePointLearningKit.wsp solution.  See Solution\ReadMe.txt.

  4. Do desired development, follow the instructions in the ReadMe.txt file in the appropriate
     subdirectory.

