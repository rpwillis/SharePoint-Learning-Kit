using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axelerate.BusinessLayerUITools.WebParts;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;

namespace Axelerate.SlkCourseManagerLogicalLayer.WebParts
{
    /// <summary>
    /// This class represents the Plan And Assign Web Part.
    /// </summary>
    public class PlanAndAssign : Microsoft.SharePoint.WebPartPages.WebPart
    {
        wptHyperGrid MainGrid = new wptHyperGrid();
        wptHyperGrid SlkActGrid = new wptHyperGrid();

        /// <summary>
        /// Default constructor
        /// </summary>
        public PlanAndAssign()
            : base()
        {
            MainGrid.ID = this.ID + "MAINGRID";
            MainGrid.LayoutName = "PlanAndAssignGrid";
            MainGrid.FactoryMethod = "GetCollectionByCourse";
            MainGrid.FactoryParameters = "[CRITERIA]";
            MainGrid.ClassName = "Axelerate.SlkCourseManagerLogicalLayer.clsActivityGroups, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4";
            MainGrid.AutoGenerateSelectButton = true;
            MainGrid.AllowEdit = true;
            this.AllowEdit = true;

            SlkActGrid.ID = this.ID + "SLKACTGRID";
            SlkActGrid.LayoutName = "SLKActivitiesGrid";
            SlkActGrid.FactoryMethod = "GetSLKAssignmentsNotInCM";
            SlkActGrid.FactoryParameters = "";
            SlkActGrid.ClassName = "Axelerate.SlkCourseManagerLogicalLayer.clsSLKAssignments, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4";
            SlkActGrid.AutoGenerateSelectButton = false;
            SlkActGrid.AllowEdit = false;
        }

        /// <summary>
        /// Gets or Sets the Document List Web URL used by the Document Selector
        /// </summary>
        [Category("Behavior")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "DocumentListWebUrl", Namespace = "Axelerate.SlkCourseManagerLogicalLayer.WebParts")]
        [DefaultValue("")]
        [WebDisplayName("Document List Website URL")]
        [WebDescription("URL of the Document List Owner Website.")]
        public string DocumentListWebUrl
        {
            get
            {
                if (ViewState["DocumentListWebUrl"] != null)
                {
                    return (string)ViewState["DocumentListWebUrl"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (ViewState["DocumentListWebUrl"] == null || ViewState["DocumentListWebUrl"].ToString() != value)
                {
                    ViewState["DocumentListWebUrl"] = value;

                }
            }
        }

        /// <summary>
        /// Gets or Sets the Document List Name used by the Document Selector
        /// </summary>
        [Category("Behavior")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "DocumentListName", Namespace = "Axelerate.SlkCourseManagerLogicalLayer.WebParts")]
        [DefaultValue("")]
        [WebDisplayName("Document List Name")]
        [WebDescription("Name of the document list for select resources.")]
        public string DocumentListName
        {
            get
            {
                if (ViewState["DocumentListName"] != null)
                {
                    return (string)ViewState["DocumentListName"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (ViewState["DocumentListName"] == null || ViewState["DocumentListName"].ToString() != value)
                {
                    ViewState["DocumentListName"] = value;
                    
                }
            }
        }

        /// <summary>
        /// Override to the OnInit Method to set the Document List with the Document List Web Url and Name values
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

        
            SPSecurity.CodeToRunElevated elevatedGetSitesAndGroups = new SPSecurity.CodeToRunElevated(setDocumentList);
            SPSecurity.RunWithElevatedPrivileges(elevatedGetSitesAndGroups);
            
        }

        /// <summary>
        /// Override to the CreateChilControls method to handle the rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            try
            {
                base.CreateChildControls();
                if (CurrentUserIsInstructor)
                {
                    Controls.Add(MainGrid);
                    System.Web.UI.WebControls.Label SlkActTitle = new System.Web.UI.WebControls.Label();
                    SlkActTitle.Text = "<br /><br /><br /><br /><br />"+ Resources.Messages.strSlkActivities +"<br />";
                    SlkActTitle.CssClass = "ms-standardheader ms-WPTitle";
                    System.Web.UI.WebControls.Label SlkActDescription = new System.Web.UI.WebControls.Label();
                    SlkActDescription.Text = "<br />" + Resources.Messages.strSlkReadOnlyActivities + "<br /><br />";
                    Controls.Add(SlkActTitle);
                    Controls.Add(SlkActDescription);
                    Controls.Add(SlkActGrid);
                }
                else
                {
                    System.Web.UI.WebControls.Label lbl = new System.Web.UI.WebControls.Label();
                    lbl.Text = Resources.ErrorMessages.UserNotInstructorOnSiteError;
                    Controls.Add(lbl);
                }
            }
            catch (Exception e)
            {
                System.Web.UI.WebControls.Label lbl = new System.Web.UI.WebControls.Label();
                lbl.Text = e.Message;
                lbl.ForeColor = System.Drawing.Color.Red;
                Controls.Add(lbl);
            }
        }

        /// <summary>
        /// Sets the Document List on the Document Selector component
        /// </summary>
        private void setDocumentList()
        {
            string DocListName = DocumentListName;
            if (SPContext.Current != null)
            {
                try
                {
                    SPWeb myWebSite = null;
                    if (DocumentListWebUrl == null || DocumentListWebUrl == "")
                    {
                        myWebSite = SPContext.Current.Web;
                    }
                    else
                    {
                        myWebSite = SPContext.Current.Site.OpenWeb(DocumentListWebUrl);
                    }
                    if (DocListName == null || DocListName == "")
                    {
                        DocListName = "Shared Documents";
                    }
                    SPList list = myWebSite.Lists[DocListName];
                    if (list != null)
                    {
                        DocumentListName = DocListName;
                        SPContext.Current.Web.AllowUnsafeUpdates = true;
                        SPFile file = SPContext.Current.Web.GetFile(Page.Request.Url.AbsoluteUri.Remove(Page.Request.Url.AbsoluteUri.LastIndexOf("/") + 1) + "DocumentViewer.aspx");
                        SPLimitedWebPartManager WebPartMgr = file.GetLimitedWebPartManager(PersonalizationScope.Shared);// myWebSite.GetLimitedWebPartManager(Page.Request.RawUrl, PersonalizationScope.Shared); 
                        //SPLimitedWebPartManager WebPartMgr = myWebSite.GetLimitedWebPartManager(Page.Request.RawUrl, PersonalizationScope.Shared); 

                        List<System.Web.UI.WebControls.WebParts.WebPart> OldWebParts = new List<System.Web.UI.WebControls.WebParts.WebPart>();
                        foreach (System.Web.UI.WebControls.WebParts.WebPart wp in WebPartMgr.WebParts)
                        {
                            if (!wp.Equals(this))
                            {
                                if (typeof(ListViewWebPart).Equals(wp.GetType()))
                                {
                                    if (((ListViewWebPart)wp).ListName != list.ID.ToString("B").ToUpper())
                                    {
                                        OldWebParts.Add(wp);
                                    }
                                    else
                                    {
                                        OldWebParts.Add(wp);
                                    }
                                }
                                else
                                {
                                    OldWebParts.Add(wp);
                                }

                            }
                        }
                        foreach (System.Web.UI.WebControls.WebParts.WebPart wp in OldWebParts)
                        {
                            WebPartMgr.DeleteWebPart(wp);
                        }
                        SPContext.Current.Web.Update();


                        //Get DocList


                        //SPFile viewPage = myWebSite.GetFile(list.DefaultViewUrl);
                        Guid listGuid = list.ID;
                        ListViewWebPart listView = null;

                        if (WebPartMgr.WebParts.Count == 0)
                        {
                            listView = new ListViewWebPart();
                            listView.ID = "docListWP";
                            listView.WebId = myWebSite.ID;
                            listView.ListName = listGuid.ToString("B").ToUpper();
                            listView.AllowClose = false;
                            listView.AllowHide = false;

                            SPView view = list.DefaultView;

                            // view.Toolbar = "none";
                            listView.ListViewXml = view.HtmlSchemaXml.Replace("Toolbar Type=\"Standard\"", "Toolbar Type=\"None\"");
                            //listView.ViewGuid = view.ID.ToString("B").ToUpper();

                            //listView.FilterString = "ID = " + item["ID"].ToString();

                            WebPartMgr.AddWebPart(listView, "Main", 0);
                        }
                        else
                        {
                            listView = (ListViewWebPart)WebPartMgr.WebParts["docListWP"];
                            listView.ListName = listGuid.ToString("B").ToUpper();
                            listView.AllowClose = false;
                            listView.AllowHide = false;

                            SPView view = list.Views[0];
                            listView.ListViewXml = view.HtmlSchemaXml.Replace("Toolbar Type=\"Standard\"", "Toolbar Type=\"None\"");

                            //listView.ListViewXml = listView.s
                            listView.ViewGuid = view.ID.ToString("B").ToUpper();

                        }
                        SPContext.Current.Web.Update();
                    }
                }
                catch
                {
                    if (DocumentListName != null)
                    {
                        //MainGrid.SetE SetError(string.Format("Can't set the document library {0} as resource for activities", DocumentListName.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Gets if the current user has Instructor permissions.
        /// </summary>
        private bool CurrentUserIsInstructor
        {
            get
            {
                try
                {
                    return clsUser.IsInstructor();
                }
                catch
                {
                    return false;
                }                
            }
        }

        /// <summary>
        /// Override to the OnPreRender method to handle the rendering.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            Controls.Clear();
            base.OnPreRender(e);
            CreateChildControls();
        }
    }
}
