* Use CPC ([CodePlex Client](http://www.codeplex.com/CodePlexClient)) to download in the SLK source tree using _cpc checkout slk_ [(help)](http://www.codeplex.com/CodePlexClient/Wiki/View.aspx?title=Workflow&referringTitle=Home#checkout)
* Download the patch from Source Code -> Patches.
* Apply the patch using _cpc applypatch patchname.patch_.
* Review the changes to the source code using _cpc status_.
* Build the changed SLK using _nmake_ from the root directory of the source code.
* [Deploy](Fresh-Install-of-your-build) the build and test the fix(es) listed for the patch.
* Run through the [Build Validation Tests](Build-Validation-Tests).
* Post to the work item that you've tested the patch and sign off on it.