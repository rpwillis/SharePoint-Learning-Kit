Below is a slightly more SLK specific version of [How To Contribute To A CodePlex Project](http://www.codeplex.com/CodePlexClient/Wiki/View.aspx?title=HowToContribute&referringTitle=Home)

* Enter an Issue Tracker item. Discuss the problem/feature and volunteer to do the work.
* Use CPC ([CodePlex Client](http://www.codeplex.com/CodePlexClient)) to download in the SLK source tree using _cpc checkout slk_ [(help)](http://www.codeplex.com/CodePlexClient/Wiki/View.aspx?title=Workflow&referringTitle=Home#checkout)
* Make modifications to the code as needed to fix the issue.
* Build, [Deploy](Fresh-Install-of-your-build), and Test your fix.  It's strongly recommended that you "walk the code" by [attaching a debugger](attaching-a-debugger) and stepping through your changes.
* Run through the [Build Validation Tests](Build-Validation-Tests).
* Use _cpc status_ to review the changes you've made, verifying that you didn't change something else and accidentally include it in the patch.
* Create a Patch file using _cpc makepatch mypatchname.patch_.
* Upload the Patch file to Source Code -> Patches. Update the Issue Tracker with the fact that the patch has been uploaded.
* Get at least one other community member to download & [test the patch](Test-a-Patch).
* 2nd community member adds a comment to the Issue Tracker saying they sign off on the patch.
* Patch is reviewed by an SLK Coordinator. Coordinator can approve, request changes, or reject the patch. If approved, the patch is applied to the codebase and will be in the next release build.