<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Application Language="C#" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Security.Principal" %>
<%@ Import Namespace="System.Web.Configuration" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup...
        
        // store the WindowsIdentity of the application pool account in the <Application> object;
		// this account must have access to the directory specified in <appSettings
		// key="packageStoreDirectoryPath"> in Web.config; note that while debugging within
		// Visual Studio this WindowsIdentity will be the identity of the Visual Studio user
        Application["AppPoolIdentity"] = WindowsIdentity.GetCurrent();

		// set <m_pstoreDirectoryPath> to the full path to the directory maintained by PackageStore
		// that contains unzipped package files, and make sure that directorory exists
		string pstoreDirectoryPath = WebConfigurationManager.AppSettings
			["packageStoreDirectoryPath"];
		if (!Directory.Exists(pstoreDirectoryPath))
			Directory.CreateDirectory(pstoreDirectoryPath);
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
