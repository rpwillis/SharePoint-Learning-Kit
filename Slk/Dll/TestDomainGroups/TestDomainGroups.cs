/* Copyright (c) Microsoft Corporation. All rights reserved. */

// TestDomainGroups.cs
//
// Console application that tests ..\EnumerateDomainGroups outside the context of SLK.
//
// To use this test program, provide the name of a domain group ("domain\groupname") on the
// command line.  Don't forget to put it in quotes if it has spaces.
//

using System;
using System.Collections.Generic;
using Microsoft.SharePointLearningKit;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
            throw new Exception("Usage: TestDomainGroups <domain-name>");
        ICollection<SPUserInfo> users = DomainGroupUtilities.EnumerateDomainGroup(args[0],
            new TimeSpan(0, 1, 0));
        foreach (SPUserInfo user in users)
            Console.WriteLine("{0}: Name={1} Email={2}", user.LoginName, user.Name, user.Email);
    }
}

// fake version of SharePoint's SPUserInfo structure
class SPUserInfo
{
    public string LoginName;
    public string Name;
    public string Email;
}

// fake string resources
static class AppResources
{
    public static string DomainGroupEnumTimeout = "DomainGroupEnumTimeout";
    public static string DomainGroupEnumFailed = "DomainGroupEnumFailed Path={0}";
    public static string DomainGroupNameHasNoBackslash = "DomainGroupNameHasNoBackslash Name={0}";
}
