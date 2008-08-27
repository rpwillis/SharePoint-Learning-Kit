using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using System.Xml;


namespace Axelerate.SlkCourseManagerLogicalLayer.WebControls
{
    /// <summary>
    /// Class representing a Document Selector using SharePoint's Document Library
    /// </summary>
    public class SPDocumentLibrarySelector : Microsoft.SharePoint.WebPartPages.WebPart 
    {
        /// <summary>
        /// Override on the Init Method to load the proper information
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Page.Request.Params["DocLibName"] != null)
            {
                SPSecurity.CodeToRunElevated ClearElevatedGetSitesAndGroups = new SPSecurity.CodeToRunElevated(ClearOldWebparts);
                SPSecurity.RunWithElevatedPrivileges(ClearElevatedGetSitesAndGroups);
                SPSecurity.CodeToRunElevated elevatedGetSitesAndGroups = new SPSecurity.CodeToRunElevated(AddWebpart);
                SPSecurity.RunWithElevatedPrivileges(elevatedGetSitesAndGroups);
            }
        }

        /// <summary>
        /// Clear any old Document Library instances loaded on the site.
        /// </summary>
        private void ClearOldWebparts()
        {
            SPWeb myWebSite = SPContext.Current.Web;
            myWebSite.AllowUnsafeUpdates = true;
            SPFile file = myWebSite.GetFile(Page.Request.RawUrl);
            SPLimitedWebPartManager WebPartMgr = file.GetLimitedWebPartManager(PersonalizationScope.Shared);// myWebSite.GetLimitedWebPartManager(Page.Request.RawUrl, PersonalizationScope.Shared); 
            //SPLimitedWebPartManager WebPartMgr = myWebSite.GetLimitedWebPartManager(Page.Request.RawUrl, PersonalizationScope.Shared); 

            //Clear all the old webparts

            string DocListName = "Documentos";
            if (Page.Request.Params["DocLibName"] != null)
            {
                DocListName = Page.Server.HtmlDecode(Page.Request.Params["DocLibName"]);
            }
            List<System.Web.UI.WebControls.WebParts.WebPart> OldWebParts = new List<System.Web.UI.WebControls.WebParts.WebPart>();
            foreach (System.Web.UI.WebControls.WebParts.WebPart wp in WebPartMgr.WebParts)
            {
                if (!wp.Equals(this))
                {
                    OldWebParts.Add(wp);
                }
            }
            foreach (System.Web.UI.WebControls.WebParts.WebPart wp in OldWebParts)
            {
                WebPartMgr.DeleteWebPart(wp);
            }

            myWebSite.Update();
            
        }

        /// <summary>
        /// Add the Document Library Webpart to the Page.
        /// </summary>
        private void AddWebpart()
        {
            SPWeb myWebSite = SPContext.Current.Web;
            myWebSite.AllowUnsafeUpdates = true;
            SPFile file = myWebSite.GetFile(Page.Request.RawUrl);
            SPLimitedWebPartManager WebPartMgr = file.GetLimitedWebPartManager(PersonalizationScope.Shared);// myWebSite.GetLimitedWebPartManager(Page.Request.RawUrl, PersonalizationScope.Shared); 
            //SPLimitedWebPartManager WebPartMgr = myWebSite.GetLimitedWebPartManager(Page.Request.RawUrl, PersonalizationScope.Shared); 

            
            //Get DocList
            string DocListName = "Documentos";
            if (Page.Request.Params["DocLibName"] != null)
            {
                DocListName = Page.Server.HtmlDecode(Page.Request.Params["DocLibName"]);
            }
            
            SPList list = myWebSite.Lists[DocListName];
            if (list == null) return;

            SPFile viewPage = myWebSite.GetFile(list.DefaultViewUrl);
            Guid listGuid = list.ID;
            ListViewWebPart listView = null;

            if (WebPartMgr.WebParts.Count == 0)
            {
                listView = new ListViewWebPart();
                listView.ID = "docListWP";
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
                view.Toolbar = "none";
                //listView.ListViewXml = listView.s
                listView.ViewGuid = view.ID.ToString("B").ToUpper();
            }
            myWebSite.Update();
        }
    }
}
