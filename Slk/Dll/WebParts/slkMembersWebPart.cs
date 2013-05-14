using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>The Slk Members web part.</summary>
    public class SlkMembersWebPart : WebPart, IObserverConnection
    {
        bool show;
        bool showEvaluated;
        bool errorOccurred;
        SlkStore store;
        SortedDictionary<string, HyperLink> members = new SortedDictionary<string, HyperLink>();
        string connectedId;

#region properties
        SPWeb Web
        {
            get { return SPContext.Current.Web ;}
        }

        SlkStore Store 
        {
            get
            {
                if (store == null)
                {
                    store = SlkStore.GetStore(Web);
                }

                return store;
            }
        }

        /// <summary>See <see cref="IObserverConnection.UserId"/>.</summary>
        public int UserId 
        { 
            get { return 0; ;} 
        }
#endregion properties

#region constuctors
        /// <summary>Initializes a new instance of <see cref="SlkMembersWebPart"/>.</summary>
        public SlkMembersWebPart()
        {
        }
#endregion constuctors

#region protected methods
        /// <summary>Creates the child controls.</summary>
        protected override void CreateChildControls()
        {
            try
            {
                if (Show())
                {
                    SlkMemberships slkMembers = new SlkMemberships();
                    slkMembers.FindAllSlkMembers(Web, Store, false);

                    foreach (SlkUser learner in slkMembers.Learners)
                    {
                        HyperLink link = new HyperLink();
                        link.Text = learner.Name;
                        members.Add(learner.Name, link);
                    }

                }
            }
            catch (SafeToDisplayException e)
            {
                errorOccurred = true;
                Literal literal = new Literal();
                literal.Text = string.Format(CultureInfo.InvariantCulture, "<p class=\"ms-formvalidation\">{0}</p>", e.Message);
                Controls.Add(literal);
            }

            base.CreateChildControls();
        }

        /// <summary>Renders the web part.</summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (errorOccurred)
            {
                base.RenderContents(writer);
            }
            else
            {
                if (Show())
                {
                    writer.Write("<ul class=\"slk-members\">");

                    foreach (HyperLink link in members.Values)
                    {
                        writer.Write("<li>");
                        link.RenderControl(writer);
                        writer.Write("</li>");
                    }

                    writer.Write("</ul>");
                }
                else if (WebPartManager.DisplayMode == WebPartManager.BrowseDisplayMode)
                {
                    writer.Write("<style type='text/css'>#MSOZoneCell_WebPart{0} {1}</style>", ClientID, "{display:none;}");
                }
            }
        }

#endregion protected methods

#region public methods
        /// <summary>Tells the provider the id of the ALWP web part.</summary>
        public void AssignWebPartId(string id)
        {
            connectedId = id;
        }
#endregion public methods

#region private methods
        bool Show()
        {
            if (showEvaluated == false)
            {
                show =  Store.IsInstructor(Web);
                showEvaluated = true;
            }

            return show;
        }
#endregion private methods
    }
}
