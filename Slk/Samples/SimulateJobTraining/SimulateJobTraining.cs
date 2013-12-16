/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SimulateJobTraining.cs
//
// This is SharePoint Learning Kit sample code that compiles into a console application.  You can
// compile this application using Visual Studio 2005, or you can compile and run this application
// without Visual Studio installed by double-clicking CompileAndRun.bat.
//
// This sample code is located in Samples\SLK\SimulateJobTraining within SLK-SDK-n.n.nnn-ENU.zip.
//
// This application creates a simulated job training program: it creates a number of local machine
// accounts (corresponding to simulated users), SharePoint Web sites, and SharePoint Learning Kit
// assignments.  This sample application is primarily designed to be used for database load
// testing.
//
// Almost all the SLK-specific work in this application is in ..\SimulateClass\SimulateClass.cs.
// In fact, the source file you're reading now isn't much of an SLK sample -- it mostly accesses
// SharePoint and Windows to set up an environment in which SimulateClass.cs can run.
//
// The simulation is set up as follows.  (The constants used below are defined within the program.)
//
//   -- The job training program has <NumberOfLearners> learners -- e.g. employees who need to
//      learn job-related skills.  One local machine account, named "SlkLearner<n>" (where <n> is a
//      number from 1 to <NumberOfLearners> inclusive), is created for each simulated learner, with
//      randomly-generated passwords.  If an account already exists it is not re-created; because
//      of this, if you'd like to log in as a simulated learner you can manually change the
//      password.
//
//   -- The job training program has <NumberOfInstructors> instructors.  Local machine accounts are
//      created for instructors (named "SlkInstructor<n>") the same way as for learners.
//
//   -- The job training program has <NumberOfWebs> SharePoint Web sites.  Each site corresponds to
//      a simulated job role that requires training.  The Web sites are named "SlkSampleWeb<n>",
//      where <n> is between 1 and <NumberOfWebs> inclusive.  The Web sites are parents of the
//      SharePoint Web site with URL <ParentWebUrl>.  If a given Web site already exists, it is
//      not re-created, but all role assignments (permissions) for accounts named "SlkLearner<n>"
//      and "SlkInstructor<n>" are removed.
//
//   -- Each learner is a learner on <WebsPerLearner> of the <NumberOfWebs> SharePoint Web sites,
//      randomly assigned.
//
//   -- Each instructor is an instructor on <WebsPerLearner> of the <NumberOfWebs> SharePoint Web
//      sites, randomly assigned.  All instructors on the same Web site are instructors for all
//      assignments on that site.
//
//   -- Once all the above is done, the SimulateClass sample application is execute once for each
//      of the <NumberOfWebs> Web sites.  See ..\SimulateClass\SimulateClass.cs for more
//      information.  You can set the number of assignments per Web site, and other simulation
//      parameters, by editing SimulateClass.cs.
//   
// Before running this program, the Web site <ParentWebUrl> (below) must be a valid SharePoint Web
// site, accessible to the current user (i.e. the person running the program) and configured in
// SharePoint Central Administration for use with SharePoint Learning Kit.  Also, the e-learning
// packages and non-e-learning documents listed in <PackageUrls> in SimulateClass.cs (in
// the directory ..\SimulateClass) must exist.
//
// NOTE: This program will give each created instructor account read access to each document
// listed in <PackageUrls>.  If any such document inherits permissions from its parent folder or
// library, that inheritence will be broken -- after that, permissions on that document are
// managed directly, independent of the parent folder or library.
//
// P.S. If you'd like to delete all the Web sites created by this program, un-comment
// "#define DeleteWebsOnly" (below) and run this program.  This can be used to "clean up" after
// running this program.
//

//#define DeleteWebsOnly

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;

class SimulateJobTrainingProgram
{
    /// <summary>
    /// The SharePoint Web site (SPWeb) of the class.  This site must exist before running this
    /// program.  NOTE: This site should be a Windows SharePoint Services "Team Site", or a
    /// similar site, which does not include one navigation "tab" for each child site, or else
    /// the many tabs will make the page very wide and unusable.
    /// </summary>
    const string ParentWebUrl = "https://localhost:123";

    /// <summary>
    /// The number of instructors.  One local machine account, named "SlkInstructor(n)" (where (n)
	/// is a number from 1 to (NumberOfInstructors) inclusive), is created for each simulated
	/// instructor (if the account doesn't already exist).
	/// </summary>
    const int NumberOfInstructors = 50;

    /// <summary>
    /// The number of learners.  One local machine account, named "SlkLearner(n)" (where (n) is a
	/// number from 1 to (NumberOfLearners) inclusive), is created for each simulated learner (if
	/// the account doesn't already exist).
    /// </summary>
    const int NumberOfLearners = 3500;

    /// <summary>
    /// The number of SharePoint Web sites to create (if they don't already exist).  Each Web site
	/// is a simulated online classroom for training learners on one job role.
    /// </summary>
    const int NumberOfWebs = 100;

    /// <summary>
    /// How many of the (NumberOfWebs) Web sites each instructor is an instructor of.
    /// </summary>
    const int WebsPerInstructor = 10;

    /// <summary>
    /// How many of the (NumberOfWebs) Web sites each learner is a learner of.
    /// </summary>
    const int WebsPerLearner = 3;

    /// <summary>
    /// Instructor login names begin with this string.
    /// </summary>
    const string InstructorLoginNamePrefix = "SlkInstructor";

    /// <summary>
    /// Learner login names begin with this string.
    /// </summary>
    const string LearnerLoginNamePrefix = "SlkLearner";

    /// <summary>
    /// Deterministic random number generator -- it will return the same sequence of random numbers
    /// if seeded with the same seed each time.
    /// </summary>
    static Random s_deterministicRandomNumberGenerator = new Random(123456);

    /// <summary>
    /// Cryptographically strong random number generator, used to create passwords.
    /// </summary>
    static RandomNumberGenerator s_secureRandomNumberGenerator = RandomNumberGenerator.Create();

    /// <summary>
    /// Directory entries that correspond to objects that are children of the root computer object.
    /// </summary>
    static DirectoryEntries s_computerChildren;

    /// <summary>
    /// The directory entry that corresponds to the "Guests" local machine group.
    /// </summary>
    static DirectoryEntry s_guests;

    /// <summary>
    /// The SharePoint SPWeb object corresponding to (ParentWebUrl).
    /// </summary>
    static SPWeb s_parentWeb;

    /// <summary>
    /// The SharePoint Learning Kit store corresponding to the SharePoint site collection
    /// containing (s_parentWeb).
    /// </summary>
    static SlkStore s_slkStore;

    static void Main(string[] args)
    {
        Stack<IDisposable> disposer = new Stack<IDisposable>();
        try
        {
            // "log in" to SharePoint and SLK as the current user
            SPSite spSite = new SPSite(ParentWebUrl);
            disposer.Push(spSite);
            s_parentWeb = spSite.OpenWeb();
            disposer.Push(s_parentWeb);
            s_slkStore = SlkStore.GetStore(s_parentWeb);

            // set <instructorPermission> and <learnerPermission> to the name of the SharePoint
            // permission (or "role definition", to be more precise) that indicates that a user
            // is a SLK instructor or learner, respectively; examples: "SLK Instructor",
            // "SLK Learner"
            string instructorPermission = s_slkStore.Mapping.InstructorPermission;
            string learnerPermission = s_slkStore.Mapping.LearnerPermission;

            // initialize <s_computerChildren> and <s_guests>, used by CreateUser
            DirectoryEntry computer = new DirectoryEntry(
                String.Format("WinNT://{0},computer", Environment.MachineName));
            disposer.Push(computer);
            s_computerChildren = computer.Children;
            s_guests = s_computerChildren.Find("Guests", "group");
            disposer.Push(s_guests);

            // if <DeleteWebsOnly> is true, just delete the <NumberOfWebs> Web sites and exit
#if DeleteWebsOnly
            for (int webNumber = 1; webNumber <= NumberOfWebs; webNumber++)
                DeleteWeb(String.Format("SlkSampleWeb{0}", webNumber));
            return;
#else

            // create SharePoint Web sites for simulated training classes, if they don't exist
            // yet; if they do exist, remove the instructors and learners on those sites
            for (int webNumber = 1; webNumber <= NumberOfWebs; webNumber++)
            {
                CreateWeb(String.Format("SlkSampleWeb{0}", webNumber),
                    String.Format("SLK Sample Web Site {0}", webNumber));
            }

            // create local machine accounts for simulated instructors (if they don't exist yet),
            // and add them as instructors on <WebsPerInstructor> randomly-chosen Web sites; also,
            // ensure each instructor has read access to each file listed in
			// <SimulateClassProgram.PackageUrls> (in ..\SimulateClass\SimulateClass.cs)
            for (int instructorNumber = 1;
                 instructorNumber <= NumberOfInstructors;
                 instructorNumber++)
            {
                // create the user account
                string loginName = String.Format(InstructorLoginNamePrefix + "{0}",
                    instructorNumber);
                string fullName = String.Format("SLK Sample Instructor {0}", instructorNumber);
                CreateUser(loginName, fullName);

                // add the user as instructors on <WebsPerInstructor> randomly-chosen Web sites
                foreach (int webNumber in ShuffledNumbers(1, NumberOfWebs, WebsPerInstructor))
                {
                    string relativeUrl = String.Format("SlkSampleWeb{0}", webNumber);
                    AddUserToWeb(loginName, relativeUrl, instructorPermission);
                }

				// ensure the instructor has read access to each file listed in
				// <SimulateClassProgram.PackageUrls> (in ..\SimulateClass\SimulateClass.cs)
				for (int packageIndex = 0;
				     packageIndex < SimulateClassProgram.PackageUrls.Length;
					 packageIndex++)
				{
					string packageUrl = SimulateClassProgram.PackageUrls[packageIndex];
					GiveUserReadAccessToFile(loginName, packageUrl);
				}
            }

            // create local machine accounts for simulated learners (if they don't exist yet),
            // and add them as learners on <WebsPerLearner> randomly-chosen Web sites
            for (int learnerNumber = 1;
                 learnerNumber <= NumberOfLearners;
                 learnerNumber++)
            {
                // create the user account
                string loginName = String.Format(LearnerLoginNamePrefix + "{0}",
                    learnerNumber);
                string fullName = String.Format("SLK Sample Learner {0}", learnerNumber);
                CreateUser(loginName, fullName);

                // add the user as learners on <WebsPerLearner> randomly-chosen Web sites
                foreach (int webNumber in ShuffledNumbers(1, NumberOfWebs, WebsPerLearner))
                {
                    string relativeUrl = String.Format("SlkSampleWeb{0}", webNumber);
                    AddUserToWeb(loginName, relativeUrl, learnerPermission);
                }
            }

            // ensure all our Web sites have at least one instructor and one learner -- if any
            // don't, randomly select an instructor and/or learner as needed
            IEnumerator<int> shuffledInstructorNumbers =
                ShuffledNumbers(1, NumberOfInstructors, int.MaxValue).GetEnumerator();
            IEnumerator<int> shuffledLearnerNumbers =
                ShuffledNumbers(1, NumberOfLearners, int.MaxValue).GetEnumerator();
            for (int webNumber = 1; webNumber <= NumberOfWebs; webNumber++)
            {
                string relativeUrl = String.Format("SlkSampleWeb{0}", webNumber);
                if (!DoesWebHaveAnyoneWithPermission(relativeUrl, instructorPermission))
                {
                    shuffledInstructorNumbers.MoveNext();
                    string loginName = String.Format(InstructorLoginNamePrefix + "{0}",
                        shuffledInstructorNumbers.Current);
                    AddUserToWeb(loginName, relativeUrl, instructorPermission);
                }
                if (!DoesWebHaveAnyoneWithPermission(relativeUrl, learnerPermission))
                {
                    shuffledLearnerNumbers.MoveNext();
                    string loginName = String.Format(LearnerLoginNamePrefix + "{0}",
                        shuffledLearnerNumbers.Current);
                    AddUserToWeb(loginName, relativeUrl, learnerPermission);
                }
            }

            // create assignments in each of the Web sites we created
            for (int webNumber = 1; webNumber <= NumberOfWebs; webNumber++)
            {
                string relativeUrl = String.Format("SlkSampleWeb{0}", webNumber);
                Console.WriteLine("Creating assignments in Web site {0} of {1}:  {2}", webNumber,
                    NumberOfWebs, relativeUrl);
                using (SPWeb spWeb = s_parentWeb.Webs[relativeUrl])
				{
                    SimulateClassProgram.RunProgram(spWeb.Url);
				}
            }
#endif
        }
        finally
        {
            // dispose of objects used by this method
            while (disposer.Count > 0)
                disposer.Pop().Dispose();
        }
    }

    /// <summary>
    /// Creates a SharePoint Web site (SPWeb) for simulating an online classroom that provides
    /// training for one job role.  The Web site is not created if it already exists.
    /// </summary>
    ///
    /// <param name="relativeUrl">The relative URL of the SPWeb, e.g. "SlkSampleWeb123".</param>
    /// 
    /// <param name="title">The title of the new Web site, e.g. "SLK Sample Learner 123".</param>
    ///
    static void CreateWeb(string relativeUrl, string title)
    {
        // if the Web site with name <relativeUrl> already exists, do nothing except delete all
        // permissions for users whose login names contain <InstructorLoginNamePrefix> or
        // <LearnerLoginNamePrefix>
        Console.WriteLine("Finding or creating Web site: {0}", relativeUrl);
        SPWeb spWeb;
        try
        {
            using (spWeb = s_parentWeb.Webs[relativeUrl])
            {
                if (!spWeb.Exists)
                    throw new System.IO.FileNotFoundException();
                Console.WriteLine("...exists already - deleting sample account role assignments");
                List<SPUser> usersToRemove = new List<SPUser>();
                foreach (SPRoleAssignment roleAssignment in spWeb.RoleAssignments)
                {
                    SPUser spUser = roleAssignment.Member as SPUser;
                    if (spUser != null)
                    {
                        string loginName = spUser.LoginName.ToLowerInvariant();
                        if (loginName.Contains(InstructorLoginNamePrefix.ToLowerInvariant()) ||
                            loginName.Contains(LearnerLoginNamePrefix.ToLowerInvariant()))
                        {
                            string domainName = String.Format(@"{0}\{1}",
                                s_parentWeb.Site.HostName, loginName);
                            usersToRemove.Add(spUser);
                        }
                    }
                }
                foreach (SPUser spUser in usersToRemove)
                    spWeb.RoleAssignments.Remove(spUser);
            }
            return;
        }
        catch (System.IO.FileNotFoundException)
        {
            // Web site doesn't exist -- create it below
        }
        
        // create the Web site
        string description = "SharePoint Learning Kit Sample Site";
        spWeb = s_parentWeb.Webs.Add(relativeUrl, title, description,
            (uint) s_parentWeb.Locale.LCID, "STS", true, false);
        spWeb.Dispose();
        Console.WriteLine("...created");
    }

    /// <summary>
    /// Deletes a SharePoint Web site (SPWeb), if it exists.
    /// </summary>
    ///
    /// <param name="relativeUrl">The relative URL of the SPWeb, e.g. "SlkSampleWeb123".</param>
    ///
    static void DeleteWeb(string relativeUrl)
    {
        Console.WriteLine("Deleting Web site (if exists): {0}", relativeUrl);
        SPWeb spWeb;
        try
        {
            using (spWeb = s_parentWeb.Webs[relativeUrl])
            {
                if (!spWeb.Exists)
                    throw new System.IO.FileNotFoundException();
                Console.WriteLine("...exists - deleting it");
                spWeb.Delete();
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            // Web site doesn't exist -- create it below
            Console.WriteLine("...doesn't exist");
        }
    }

    /// <summary>
    /// Creates a local machine account with a randomly-assigned password.  The account is not
    /// created if it already exists.
    /// </summary>
    ///
    /// <param name="loginName">The login name, e.g. "SlkLearner123".</param>
    /// 
    /// <param name="fullName">The full name, e.g. "SLK Sample Learner 123".</param>
    ///
    static void CreateUser(string loginName, string fullName)
    {
        // add the user as a local user of this computer; set <existed> to true if the user
        // already existed
        Console.WriteLine("Finding or creating user account: {0}", loginName);
        DirectoryEntry user;
        bool existed;
        try
        {
            user = s_computerChildren.Find(loginName, "user");
            existed = true;
            Console.WriteLine("...exists already");
        }
        catch (COMException)
        {
            user = s_computerChildren.Add(loginName, "user");
            existed = false;
        }

        using (user)
        {
            // if the user didn't exist, set up their account
            if (!existed)
            {
                // set properties of the user
                byte[] passwordBytes = new byte[30];
                s_secureRandomNumberGenerator.GetNonZeroBytes(passwordBytes);
                string password = Convert.ToBase64String(passwordBytes);
                user.Invoke("SetPassword", new object[] { password });
                user.Invoke("Put", new object[] { "FullName", fullName });
                user.Invoke("Put", new object[] { "Description",
                    "* Created by SharePoint Learning Kit sample code *" });
                user.CommitChanges();

                // add the user to the Guests group
                try
                {
                    s_guests.Invoke("Add", new object[] { user.Path });
                }
                catch (TargetInvocationException)
                {
                    // probably the user is already a member of the group
                }
            }

            // add the user to SharePoint
            string domainName = String.Format(@"{0}\{1}", s_parentWeb.Site.HostName, loginName);
            s_parentWeb.SiteUsers.Add(domainName, String.Empty, fullName, String.Empty);
        }

        if (!existed)
            Console.WriteLine("...created");
    }

    /// <summary>
    /// Creates a role assignment for a given user on a given SharePoint Web site.
    /// </summary>
    ///
    /// <param name="loginName">The login name, e.g. "SlkLearner123".</param>
    ///
    /// <param name="relativeUrl">The relative URL of the SPWeb, e.g. "SlkSampleWeb123".</param>
    ///
    /// <param name="permission">The name of the permission to use for the role assignment, e.g.
    ///     "SLK Instructor".</param>
    ///
    static void AddUserToWeb(string loginName, string relativeUrl, string permission)
    {
        Console.WriteLine("Adding {0} as {1} on {2}", loginName, permission, relativeUrl);
        using (SPWeb spWeb = s_parentWeb.Webs[relativeUrl])
        {
            using(SPSite spSite = s_parentWeb.Site)
            {
                string domainName = String.Format(@"{0}\{1}", spSite.HostName, loginName);
                SPUser spUser = s_parentWeb.SiteUsers[domainName];
                SPRoleAssignment roleAssignment = new SPRoleAssignment(spUser);
                SPRoleDefinition roleDefinition = spWeb.RoleDefinitions[permission];
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                spWeb.RoleAssignments.Add(roleAssignment);
            }
        }
    }

    /// <summary>
    /// Returns true if a given SharePoint Web site has anyone on it with a given permission.
    /// </summary>
    ///
    /// <param name="relativeUrl">The relative URL of the SPWeb, e.g. "SlkSampleWeb123".</param>
    ///
    /// <param name="permission">The name of the permission to look for, e.g. "SLK Instructor".
    ///     </param>
    ///
    static bool DoesWebHaveAnyoneWithPermission(string relativeUrl, string permission)
    {
        using (SPWeb spWeb = s_parentWeb.Webs[relativeUrl])
        {
            SPRoleDefinition roleDefinition = spWeb.RoleDefinitions[permission];
            foreach (SPRoleAssignment roleAssignment in spWeb.RoleAssignments)
            {
                if (roleAssignment.RoleDefinitionBindings.Contains(roleDefinition))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gives a user read access to a file in a SharePoint document library.
    /// </summary>
	///
    /// <param name="loginName">The login name, e.g. "SlkLearner123".</param>
	///
    /// <param name="packageUrl">The URL of the file to give the user access to.  This file must
	/// 	exist within a SharePoint document library, and the current user (i.e. the user
	/// 	running this program) must have sufficient permissions to grant the specified user
	/// 	access to the file.</param>
	///
	static void GiveUserReadAccessToFile(string loginName, string packageUrl)
	{
        Stack<IDisposable> disposer = new Stack<IDisposable>();
        try
        {
			// make <spListItem> refer to the file specified by <packageUrl>
            SPSite packageSite = new SPSite(packageUrl);
            disposer.Push(packageSite);
            SPWeb packageWeb = packageSite.OpenWeb();
            disposer.Push(packageWeb);
            Uri packageUri = new Uri(packageUrl);
            SPListItem spListItem = packageWeb.GetListItem(packageUri.AbsolutePath);

            // make <spUser> refer to the user specified by <loginName>
            SPUser spUser;
            string domainName = String.Format(@"{0}\{1}", s_parentWeb.Site.HostName, loginName);
            try
            {
                spUser = packageWeb.SiteUsers[domainName];
            }
            catch (SPException)
            {
                // if user doesn't exist on <packageWeb>, add them
				packageWeb.SiteUsers.Add(domainName, String.Empty, String.Empty, String.Empty);
                spUser = packageWeb.SiteUsers[domainName];
            }

            // if <spListItem> inherits permissions from its parent folder or document library,
            // break that inheritance
            if (!spListItem.HasUniqueRoleAssignments)
                spListItem.BreakRoleInheritance(true);

			// grant <spUser> read access to <spListItem>
            SPRoleDefinition roleDefinition = packageWeb.RoleDefinitions["Read"];
            SPRoleAssignment roleAssignment = new SPRoleAssignment(spUser);
            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
            spListItem.RoleAssignments.Add(roleAssignment);
        }
        finally
        {
            // dispose of objects used by this method
            while (disposer.Count > 0)
                disposer.Pop().Dispose();
        }
	}

    /// <summary>
    /// Enumerates a series of shuffled integers within a given range.  No duplicates are returned
    /// unless more numbers are requested than are within the range.
    /// </summary>
    ///
    /// <param name="minValue">The smallest integer in the range.</param>
    ///
    /// <param name="maxValue">The largest integer in the range.</param>
    /// 
    /// <param name="count">The count of numbers to return.  May be int.MaxValue for a virtually
    ///     infinite series of numbers.</param>
    ///
    static IEnumerable<int> ShuffledNumbers(int minValue, int maxValue, int count)
    {
        int[] numbers = new int[maxValue - minValue + 1];
        for (int index = 0; index < numbers.Length; index++)
            numbers[index] = minValue + index;
        for (int resultIndex = 0; resultIndex < count; resultIndex++)
        {
            int index = resultIndex % numbers.Length;
            int otherIndex = s_deterministicRandomNumberGenerator.Next(index, numbers.Length);
            int chosenNumber = numbers[otherIndex];
            numbers[otherIndex] = numbers[index];
            numbers[index] = chosenNumber;
            yield return chosenNumber;
        }
    }
}

