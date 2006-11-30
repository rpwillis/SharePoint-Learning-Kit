/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.SharePointLearningKit.WebControls;
using System.Text;
using Microsoft.LearningComponents;


namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    

    public class CustomControlTest : SlkAppBasePage
    {
        //protected ErrorBanner errorBanner;
        //protected GradingList gradingList;
        protected TextBox txtLearnerAssignmentId;
        protected RadioButtonList radSuccessStatus;
        protected TextBox txtGradedScore;
        protected void Page_Load(object sender, EventArgs e)
        {
           // ParseChildrenAttribute p =
           //(ParseChildrenAttribute)Attribute.GetCustomAttribute(typeof(CollectionPropertyControl),
           //   typeof(ParseChildrenAttribute));

           // StringBuilder sb = new StringBuilder();
           // sb.Append("The DefaultProperty property is " + p.DefaultProperty.ToString() + "<br>");
           // sb.Append("The ChildrenAsProperties property is " + p.ChildrenAsProperties.ToString() + "<br>");
           // sb.Append("The IsDefaultAttribute method returns " + p.IsDefaultAttribute().ToString());
           // Response.Write(sb.ToString());

            //((ASP.customcontroltest_aspx)(this)).Button1.Visible = false;          

            //errorBanner.AddError(ErrorType.Warning, "Msft");
            //errorBanner.AddError(ErrorType.Info, "Msft");
            long learnerAssignmentId;

            QueryString.Parse("LearnerAssignmentId", out learnerAssignmentId, false);


            LearnerAssignmentItemIdentifier learnerAssignmentItemIdentifier
                                = new LearnerAssignmentItemIdentifier(learnerAssignmentId);

            LearnerAssignmentProperties learnerAssignmentProperties
                = SlkStore.GetLearnerAssignmentProperties(learnerAssignmentItemIdentifier, SlkRole.Instructor);

            txtLearnerAssignmentId.Text = learnerAssignmentId.ToString();
            if (learnerAssignmentProperties.GradedPoints.HasValue)
            {
                txtGradedScore.Text = String.Format("{0:R}", learnerAssignmentProperties.GradedPoints.Value);
            }

            radSuccessStatus.Items.Add(
                new ListItem(SuccessStatus.Passed.ToString(), SuccessStatus.Passed.ToString()));
            radSuccessStatus.Items.Add(
                new ListItem(SuccessStatus.Failed.ToString(), SuccessStatus.Failed.ToString()));
            radSuccessStatus.Items.Add(
                new ListItem(SuccessStatus.Unknown.ToString(), SuccessStatus.Unknown.ToString()));

            radSuccessStatus.SelectedValue = learnerAssignmentProperties.SuccessStatus.ToString();

            //GradingItem gradeItem = new GradingItem();

            //gradeItem.AssignmentID  = 100;

            //gradeItem.LearnerName = "Learner" ;

            //gradeItem.Status = AssignmentStatus.NotStarted;

            //gradeItem.Comments = "Hello, World";
            //gradeItem.AutoGradedScore = 1000000 ;
            //gradeItem.AutoGradeToolTip = "100";
            //gradeItem.FinalScore = 99.9999999 ;
            //gradeItem.ActionState =true ;
            //gradeItem.ActionText = "Collect";

            //gradingList.Add(gradeItem);

            //gradeItem = new GradingItem();

            //gradeItem.AssignmentID = 200;

            //gradeItem.LearnerName = "Greg Aranov";

            //gradeItem.Status = AssignmentStatus.Final;

            //gradeItem.Comments = "Hello, World";
            //gradeItem.AutoGradedScore = 80;
            //gradeItem.AutoGradeToolTip = "80";
            //gradeItem.FinalScore = 100;
            //gradeItem.ActionState = false;
            //gradeItem.ActionText = "Reactivate";
            //gradingList.Add(gradeItem);


        }
    }
}
