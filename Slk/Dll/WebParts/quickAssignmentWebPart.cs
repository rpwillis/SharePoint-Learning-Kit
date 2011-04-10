using System;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>The quick assignment web part.</summary>
    public class QuickAssignmentWebPart : WebPart
    {
        TextBox titleBox;
        Button submit;

#region protected methods
        /// <summary>Creates the child controls.</summary>
        protected override void CreateChildControls()
        {
            titleBox = new TextBox();
            titleBox.MaxLength = 100;
            titleBox.Rows = 2;
            Controls.Add(titleBox);

            submit = new Button();
            submit.Text = "Assign";
            submit.Click += AssignClick;
            Controls.Add(submit);

            base.CreateChildControls();
        }
#endregion protected methods

#region click event
        /// <summary>The action on clicking assign.</summary>
        public void AssignClick(object sender, EventArgs e)
        {
            try
            {
                SPWeb web = SPContext.Current.Web;
                string url = SlkUtilities.UrlCombine(web.ServerRelativeUrl, "_layouts/SharePointLearningKit/AssignmentProperties.aspx");
                string encodedTitle = HttpUtility.UrlEncode(titleBox.Text);
                url = String.Format(CultureInfo.InvariantCulture, "{0}?Location={1}&Title={2}", url, AssignmentProperties.noPackageLocation, encodedTitle);

                Page.Response.Redirect(url, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
        }
#endregion click event
    }
}
