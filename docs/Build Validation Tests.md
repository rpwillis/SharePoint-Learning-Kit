**Build BVT**
Perform a full build from the root using _nmake_. (Normally we'd require that this build with no warnings, but the code is far off of this right now.  A bug on this is filed.)

**Fresh BVT**
Perform a [Fresh Install of your build](Fresh-Install-of-your-build).
Upload a document to the document library.
Assign document to self.
Begin assignment.
Resume assignment.
Complete assignment.
Check status on the My Assignments web part.

**Upgrade BVT**
Perform an install of the previous release.
Upload a document to the document library.
Assign document to self.
Upgrade to the new build using the _UpdateSolution.cmd_ steps from the Getting Started Guide.
Check the assignment to make sure it still looks good.
Begin/Resume/Complete assignment.
Check status.
Create a new assignment to self.
Begin/Resume/Complete assignment.
Check status.

**SCORM Playback BVT** -- run after changing version
Upload the Solitare.zip sample SCORM content to the document library.
Assign to self.
Verify that the content loads and plays.

**AD Groups BVT** -- run after making changes to assignments or AD code
Go to People and Groups -> Site Permissions -> Add Users
In Users/Groups, add the name of an Active Directory group in your domain
Give users permissions directly -> Read, SLK Learner -> OK
Go to the Document Library, select a document -> E-Learning Actions
Assign it to someone else -> Current Site
If no error, then BVT has passed.