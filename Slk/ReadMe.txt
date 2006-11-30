/* Copyright (c) Microsoft Corporation. All rights reserved. */


This directory builds SharePoint Learning Kit (SLK) functionality.

  -- Dll: Builds Microsoft.SharePointLearningKit.dll, which implements functionality used by SLK
     web pages.  This DLL also implements the SLK web part(s).

  -- App: Contains the .aspx/.cs pages that comprise the SLK application, including pages used
     within frames of SLK web part(s), but not including administration features.

  -- AppFeature: Contains files that implement the "App" directory above as a WSS "Feature".

  -- Admin: Contains the .aspx/.cs pages that comprise the SLK Administration feature within
     SharePoint Central Administration.

  -- AdminFeature: Contains files that implement the "Admin" directory above as a WSS "Feature".

  -- Solution: Builds SharePointLearningKit.wsp, the WSS "Solution" that implements all SLK
     features (both application and adminstration).

  -- SlkSchema.xml: Defines the SLK LearningStore schema.  This file is used as input to the
     Microsoft Learning Components (MLC) SchemaCompiler.exe; the output is SlkSchema.sql and
     Dll\SlkSchema.cs.

  -- SlkSettings.xml: The default out-of-box SLK Settings file, which is stored in an SLK database
     and which defines various SLK application settings and query definitions.  Customers can
     customize this file and upload a new version to the database.

  -- SlkSettings.xsd, .xsx: The XML schema of SlkSettings.xml.

To do development in this directory:

  1. Prerequisite: All projects in ..\Src (i.e. all Microsoft Learning Components DLLs) need to
     be built.  To ensure this, run "nmake" in the parent directory (to build both debug and
     release versions of all DLLs), or at least "nmake deb" (to build debug versions).

  2. If SLK components (in this directory tree) haven't been built yet, run "nmake" (to build both
     debug and release versions of everything), or at least "nmake deb" (to build debug versions).
     Among other things, this will copy MLC files to temporary locations within various
     subdirectories, and will compile the SLK LearningStore schema -- all of which is required
     before launching Visual Studio.

  3. Deploy the SharePointLearningKit.wsp solution.  See Solution\ReadMe.txt.

  4. Do desired development, follow the instructions in the ReadMe.txt file in the appropriate
     subdirectory.  To update the SLK LearningStore schema, edit SlkSchema.xml and then rebuild the
     solution ("nmake" or "nmake deb").

