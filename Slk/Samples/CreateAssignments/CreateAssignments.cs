/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// CreateAssignments.cs
//
// This is SharePoint Learning Kit sample code that compiles into a console application.  You can
// compile this application using Visual Studio 2005, or you can compile and run this application
// without Visual Studio installed by double-clicking CompileAndRun.bat.
//
// This sample code is located in Samples\SLK\CreateAssignments within SLK-SDK-n.n.nnn-ENU.zip.
//
// This application demonstrates creating assignments.
//

using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePointLearningKit;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            AssignmentItemIdentifier assignmentId;

            assignmentId = CreateAssignment(
                "http://laptop01/sites/slk/staff/Learning%20Objects/Solitaire.zip", 0,
                "http://laptop01/sites/slk/classes/englisha",
                "Assignment for one 'A' students in Math 1A",
                @"demo\teacherone", // instructor
                @"demo\pupilone");
            Console.WriteLine("Created assignment: ID={0}", assignmentId.GetKey());

            /*
            assignmentId = CreateAssignment(
                "http://localhost/districts/bellevue/elm/library/Shared%20Documents/Solitaire.zip", 0,
                "http://localhost/districts/bellevue/elm/math1a",
                "Assignment for two 'A' students in Math 1A",
                @".\EllenAdams", // instructor
                @".\BengtHasselgren",
                @".\BrianValentine");
            Console.WriteLine("Created assignment: ID={0}", assignmentId.GetKey());

            assignmentId = CreateAssignment(
                "http://localhost/districts/bellevue/elm/library/Shared%20Documents/Solitaire.zip", 0,
                "http://localhost/districts/bellevue/elm/math1b",
                "Assignment for six 'J' students in Math 1B",
                @".\EllenAdams", // instructor
                @".\JamesRHamilton",
                @".\JayHenningsen",
                @".\JeffHay",
                @".\JesperHerp",
                @".\JoeHealy",
                @".\JonathanHaas");
            Console.WriteLine("Created assignment: ID={0}", assignmentId.GetKey());
            */
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Creates a SharePoint Learning Kit assignment.
    /// </summary>
    ///
    /// <param name="packageUrl">The URL of the e-learning package or non-e-learning document
    ///     to assign.  This file must be located within a SharePoint document library.</param>
    ///
    /// <param name="organizationIndex">If <paramref name="packageUrl"/> refers to an e-learning
    ///     package (e.g. a SCORM .zip file), <paramref name="organizationIndex"/> should be
    ///     the zero-based index of the organization to assign.  (Use 0 to assign the first
    ///     organization.)  If <paramref name="packageUrl"/> is a non-e-learning document,
    ///     <paramref name="organizationIndex"/> should be <c>null</c>.</param>
    ///
    /// <param name="assignmentWebUrl">The URL of the SharePoint Web site that the new assignment
    ///     will be associated with.</param>
    ///
    /// <param name="title"></param>
    ///
    /// <param name="instructorLoginName">The SharePoint login name of the instructor of the
    ///     assignment.  If the instructor account is a local machine account, the caller can
    ///     specify @".\account-name".  This user must have read access to the file specified by
    ///     <paramref name="packageUrl"/>, and must have the "SLK Instructor" permission on the
    ///     SharePoint Web site specified by <paramref name="assignmentWebUrl"/>.</param>
    ///
    /// <param name="learnerLoginNames">The SharePoint login names of the learners of the
    ///     assignment.  If a learner account is a local machine account, the caller can
    ///     specify @".\account-name".  Learners need not have access to the file specified by
    ///     <paramref name="packageUrl"/>, but they must have the "SLK Learner" permission on the
    ///     SharePoint Web site specified by <paramref name="assignmentWebUrl"/>.</param>
    ///
    /// <returns>
    /// The <c>AssignmentItemIdentifier</c> of the newly-created SharePoint Learning Kit
    /// assignment.
    /// </returns>
    ///
    static AssignmentItemIdentifier CreateAssignment(string packageUrl, int? organizationIndex,
        string assignmentWebUrl, string title, string instructorLoginName,
        params string[] learnerLoginNames)
    {
        Stack<IDisposable> disposer = new Stack<IDisposable>();
        try
        {
            // set <instructorToken> to the SPUserToken of the instructor
            SPUser instructor;
            SPUserToken instructorToken;
            SPSite anonymousSite = new SPSite(packageUrl);
            disposer.Push(anonymousSite);
            if (instructorLoginName.StartsWith(@".\"))
                instructorLoginName = anonymousSite.HostName + instructorLoginName.Substring(1);
            disposer.Push(anonymousSite.RootWeb);
            instructor = anonymousSite.RootWeb.AllUsers[instructorLoginName];
            instructorToken = instructor.UserToken;

            // set <packageWeb> to the SPWeb of the SharePoint Web site containing the package
            // or document to assign
            SPSite packageSite = new SPSite(packageUrl, instructorToken);
            disposer.Push(packageSite);
            SPWeb packageWeb = packageSite.OpenWeb();
            disposer.Push(packageWeb);

            // set <spFile> to the SPFile of the package or document to assign
            SPFile spFile = packageWeb.GetFile(packageUrl);

            // set <packageLocation> to the SharePointPackageStore-format location string that
            // uniquely identifies the current version of the <spFile>
            string packageLocation = new SharePointFileLocation(packageWeb,
                spFile.UniqueId, spFile.UIVersion).ToString();

            // set <assignmentWeb> to the SPWeb of the SharePoint Web site that the new assignment
            // will be associated with
            SPSite assignmentSite = new SPSite(assignmentWebUrl, instructorToken);
            disposer.Push(assignmentSite);
            SPWeb assignmentWeb = assignmentSite.OpenWeb();
            disposer.Push(assignmentWeb);

            // set <slkStore> to the SharePoint Learning Kit store associated with the SPSite of
            // <assignmentWeb>
            SlkStore slkStore = SlkStore.GetStore(assignmentWeb);

            // set <assignmentProperties> to the default assignment properties for the package or
            // document being assigned; some of these properties will be overridden below
            LearningStoreXml packageWarnings;
            AssignmentProperties assignmentProperties = slkStore.GetNewAssignmentDefaultProperties(
                assignmentWeb, packageLocation, organizationIndex, SlkRole.Instructor,
                out packageWarnings);

            // set the assignment title
            assignmentProperties.Title = title;

            // set <allLearners> to a dictionary that maps SharePoint user login names to SlkUser
            // objects, for all users that have the "SLK Learner" permission on <assignmentWeb>
            SlkMemberships memberships = slkStore.GetMemberships(assignmentWeb, null, null);
            Dictionary<string, SlkUser> allLearners = new Dictionary<string, SlkUser>(
                StringComparer.OrdinalIgnoreCase);
            foreach (SlkUser learner in memberships.Learners)
                allLearners.Add(learner.SPUser.LoginName, learner);

            // set the learners of the assignment to be <learnerLoginNames>
            assignmentProperties.Learners.Clear();
            foreach (string rawLearnerLoginName in learnerLoginNames)
            {
                string learnerLoginName;
                if (rawLearnerLoginName.StartsWith(@".\"))
                    learnerLoginName = anonymousSite.HostName + rawLearnerLoginName.Substring(1);
                else
                    learnerLoginName = rawLearnerLoginName;

                SlkUser slkUser;
                if (allLearners.TryGetValue(learnerLoginName, out slkUser))
                    assignmentProperties.Learners.Add(slkUser);
                else
                    throw new Exception(String.Format("Not a learner: {0}", learnerLoginName));
            }

            // create the assignment
            AssignmentItemIdentifier assignmentId = slkStore.CreateAssignment(assignmentWeb,
                packageLocation, organizationIndex, SlkRole.Instructor, assignmentProperties); 

            // return the ID of the new assignment
            return assignmentId;
        }
        finally
        {
            // dispose of objects used by this method
            while (disposer.Count > 0)
                disposer.Pop().Dispose();
        }
    }
}
