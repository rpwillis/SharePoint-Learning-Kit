using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Web.UI.WebControls;
using System.Web.UI;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Interfaces;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    [XmlRoot(Namespace = "Axelerate.BusinessLayerUIToolsTest")]
    [ToolboxData("<{0}:clsBOSearchResults runat=server />")]
    public class wptBOSearchResults : clsCtrlWebPartBase, IWebEditable
    {
        private Control _WebPart;
        private UpdatePanel up = new UpdatePanel();  

        protected override void CreateChildControls()
        {
            base.Controls.Clear();
            base.CreateChildControls();
            generateFactContent();

            EnsureUpdatePanelFixups();
            up.ID = "Async";
            if (_WebPart != null)
            {
                up.UpdateMode = UpdatePanelUpdateMode.Always;
                up.ContentTemplateContainer.Controls.Add(_WebPart);
                this.Controls.Add(up);
            }
        }

        /// <summary>
        /// ascx path to be loaded
        /// </summary>
        private String ClassPath
        {
            get
            {

                return "wptBOSearchResults.ascx";
            }
        }

        /// <summary>
        /// Business Object used by the ascx
        /// </summary>
        [Category("Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ClassName")]
        [DefaultValue("")]
        [WebDisplayName("Class name:")]
        [WebDescription("Enter the class name.")]
        public override String ClassName
        {
            get
            {
                return base.ClassName;
            }
            set
            {
                if (_WebPart != null)
                {
                    ((ICtrlWebPartBase)_WebPart).ClassName = value;
                }
                base.ClassName = value;
            }
        }

        /// <summary>
        /// WebPart Title
        /// </summary>
        [Category("Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Title")]
        [DefaultValue("")]
        [WebDisplayName("Title:")]
        [WebDescription("Enter the WebPart title.")]
        public override String Title
        {
            get
            {
                if (_WebPart != null)
                {
                    return ((IWebPart)_WebPart).Title;
                }
                else
                {
                    return base.Title;
                }
            }
            set
            {
                if (_WebPart != null)
                {
                    ((IWebPart)_WebPart).Title = value;
                }
                base.Title = value;
            }
        }

        /// <summary>
        /// WebPart Title URL
        /// </summary>
        /// 
        [Category("Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "TitleURL")]
        [DefaultValue("")]
        [WebDisplayName("TitleURL:")]
        [WebDescription("Enter the WebPart titleurl.")]
        public override string TitleUrl
        {
            get
            {
                if (_WebPart != null)
                {
                    return ((IWebPart)_WebPart).TitleUrl;
                }
                else
                {
                    return base.TitleUrl;
                }
            }
            set
            {
                if (_WebPart != null)
                {
                    ((IWebPart)_WebPart).TitleUrl = value;
                }
                base.TitleUrl = value;
            }
        }

        /// <summary>
        /// Catalog Icon
        /// </summary>
        [Category("Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "CatalogIconImageUrl")]
        [DefaultValue("")]
        [WebDisplayName("Catalog icon image URL:")]
        [WebDescription("Enter the WebPart title.")]
        public override string CatalogIconImageUrl
        {
            get
            {
                if (_WebPart != null)
                {
                    return ((IWebPart)_WebPart).CatalogIconImageUrl;
                }
                else
                {
                    return base.CatalogIconImageUrl;
                }
            }
            set
            {
                if (_WebPart != null)
                {
                    ((IWebPart)_WebPart).CatalogIconImageUrl = value;
                }
                base.CatalogIconImageUrl = value;
            }
        }

       

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        private void generateFactContent()
        {
            Table table = new Table();
            TableRow trBody = new TableRow();
            TableCell cell = new TableCell();
            try
            {
                if (Page != null)
                {
                    _WebPart = Page.LoadControl(ClassPath);
                    ((ICtrlWebPartBase)_WebPart).FactoryParameters = base.FactoryParameters ;
                    ((ICtrlWebPartBase)_WebPart).FactoryMethod = base.FactoryMethod;
                    ((IWebPart)_WebPart).CatalogIconImageUrl = base.CatalogIconImageUrl;
                    ((IWebPart)_WebPart).TitleUrl = base.TitleUrl;
                    ((IWebPart)_WebPart).Title = base.Title;
                    ((ICtrlWebPartBase)_WebPart).ClassName = base.ClassName;
                    cell.Controls.Add(_WebPart);
                    trBody.Cells.Add(cell);
                    table.Rows.Add(trBody);
                    Controls.Add(table);
                }
            }
            catch (Exception ex)
            {
            }
        }

        [ConnectionConsumer("Search Consumer", "default")]
        public void RegisterProvider(ISearchProvider Provider)
        {
            if (_WebPart != null)
            {
                ((ISearchConsumer)_WebPart).RegisterProvider(Provider);
                
            }
        }

  /*      #region IWebEditable Members

        EditorPartCollection IWebEditable.CreateEditorParts()
        {
            List<EditorPart> editors = new List<EditorPart>();
            editors.Add(new tlpBusinessSelector());
            return new EditorPartCollection(editors);
        }

        object IWebEditable.WebBrowsableObject
        {
            get { return this; }
        }

        #endregion*/
    }
}
