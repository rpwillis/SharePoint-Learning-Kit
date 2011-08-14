<%-- MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. --%>

<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

// SampleWebService.asmx
//
// This file is part of the SharePoint Learning Kit "WebService" sample code.  See ReadMe.txt
// for more information.
//      
// This sample code is located in Samples\SLK\WebService within SLK-SDK-n.n.nnn-ENU.zip.
//

<%-- NOTE: If this page displays "File Not Found", check the build number (1.0.nnn.0) below --%>
<%@ Assembly Name="Microsoft.LearningComponents, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.LearningComponents.SharePoint, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.LearningComponents.Storage, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<%@ WebService Language="C#" Class="SlkSamples.SampleWebService" %>

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePointLearningKit;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;

namespace SlkSamples
{

public class SampleWebService : System.Web.Services.WebService
{
    [WebMethod]
    public float? GetLearnerAssignmentScore(Guid learnerAssignmentId)
    {
        using (SPWeb spWeb = SPControl.GetContextWeb(HttpContext.Current))
        {
            StoredLearningSession learningSession = GetLearningSession(spWeb, learnerAssignmentId);
            return learningSession.CurrentActivityDataModel.Score.Scaled;
        }
    }

    [WebMethod]
    public void SetLearnerAssignmentScore(Guid learnerAssignmentId, float score)
    {
        using (SPWeb spWeb = SPControl.GetContextWeb(HttpContext.Current))
        {
            StoredLearningSession learningSession = GetLearningSession(spWeb, learnerAssignmentId);
            learningSession.CurrentActivityDataModel.Score.Scaled = score;
            learningSession.CommitChanges();
        }
    }

    StoredLearningSession GetLearningSession(SPWeb spWeb, Guid learnerAssignmentId)
    {
        SlkStore slkStore = SlkStore.GetStore(spWeb);
        LearnerAssignmentProperties assignmentProperties =
            slkStore.GetLearnerAssignmentProperties(
                learnerAssignmentId, SlkRole.Learner);
        return new StoredLearningSession(Microsoft.LearningComponents.SessionView.Execute,
            assignmentProperties.AttemptId, slkStore.PackageStore);
    }
}

}

