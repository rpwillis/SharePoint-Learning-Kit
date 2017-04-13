**Best Practices for Testing & Debugging SLK Builds**

I strongly prefer to have my development environment on a separate machine from my server environment.  The primary reason I do this is so I can wipe/restore my server environment back to a well known state for testing purposes or in case I mess something up a little too deeply.  Installing Windows Server 2003 from scratch takes about two hours.  Installing a complete development machine with all its tools takes about two days.  Big difference.

I also like to use Virtual Machine technology to host my server environment.  This lets me use one physical machine for dev and test (provided it's beefy enough -- kids, don't try this at home without at least 2 GB or RAM -- I run x64 bit dev boxes with 4 GB of RAM because I like to run not only a server VM but also a clean client VM when testing).  Using VM technology with Snapshots (VMWare's terminology for rolling back a VM to a previous set of bits on disk -- aka before you installed SLK) makes testing from a well known state very easy.

OK, let's assume you've now set up your server on a separate machine -- VM or physical.  Now we need to deploy and debug.  Deploy is as easy as copying files and running the cmd scripts per the Getting Started Guide.  I do this manually because the steps for performing this in the Getting Started Guide don't lend themselves well to automation.  Some day, perhaps some genius will write an automated SLK deployment tool -- on that day, I will be happy.  Until then, I've learned to enjoy the 10 minutes it takes me to install SLK from a machine that's been wiped back to Windows + SQL + WSS.

For debugging, I use Visual Studio Remote Debugger.  This is quick and easy to install (you can put it into your snapshot image too).

**After installing SLK:**
* Start the Remote Debugger
* Use a web browser to open the SLK Web Site.  This initializes the IIS Worker Process and loads the DLLs into memory so you have something to attach to.
* Switch to your dev machine and use Tools -> Attach To Process.  Set the machine name to the name of your SLK server.  Attach to W3WP.exe.  Sometimes there is more than one of these processes -- you can attach to all of them.
* Look at Modules.  Make sure you see the SLK DLLs in the Modules list.  Load Symbols for the SLK DLLs.  (Note: I know you should be able to use ~\SLK\Solution\DebugFiles, but for some reason I have better luck if I load the symbols from \\MySlkServer\c$\Install\DebugFiles.  Go figure.)
* Open the SLK source code file in question.  Set a breakpoint.
* If all has worked out, the breakpoint will be a solid red circle (meaning VS.Net sees everything properly and this breakpoint will be hit) rather than a hollow red circle (meaning VS.Net doesn't think it has been attached to a process that has this source code loaded).