using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Web.UI.WebControls;
using System.Web.UI;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Interfaces;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    [XmlRoot(Namespace = "Axelerate.BusinessLayerUIToolsTest")]
    [ToolboxData("<{0}:clsBOGenericWebPart runat=server />")]
    public class wptBOGenericWebPart : clsCtrlWebPartBase, IWebEditable 
    {
        private Control _WebPart;
        private Table table;

        protected override void  CreateChildControls()
        {
            Controls.Clear();
            base.CreateChildControls();
            generateFactContent();
        }

        internal Control WPart
        {
            get
            {
                EnsureChildControls();
                return _WebPart;
            }
            set
            {
                _WebPart = value;
            }
        }

        /// <summary>
        /// ascx path to be loaded
        /// </summary>
        [Category("Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ClassPath")]
        [DefaultValue("")]
        [WebDisplayName("Class path name:")]
        [WebDescription("Enter the class path name.")]
        public String ClassPath
        {
            get
            {
                if (ViewState["ClassPath"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["ClassPath"].ToString();
                }
            }
            set
            {
                ViewState["ClassPath"] = value;
                generateFactContent();
            }
        }

        protected void RecreateChildControls()
        { 
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
        /// 
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
        [DefaultValue("")]
        [Personalizable(PersonalizationScope.Shared)]
        [WebDisplayName("Catalog icon image URL:")]
        [WebDescription("Enter the WebPart title.")]
        [XmlElement(ElementName = "CatalogIconImageUrl")]
        [WebBrowsable(true)]
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
            table = new Table();
            table.Width = new Unit("100%");
            table.CellPadding = 0;
            table.CellSpacing = 0;
            TableRow trBody = new TableRow();
            TableCell cell = new TableCell();
            try
            {
                if (Page != null && ClassPath != "")
                {
                    _WebPart = Page.LoadControl(ClassPath);
                    ((ICtrlWebPartBase)_WebPart).FactoryParameters = base.FactoryParameters ;
                    ((ICtrlWebPartBase)_WebPart).FactoryMethod = base.FactoryMethod;
                    ((IWebPart)_WebPart).CatalogIconImageUrl = base.CatalogIconImageUrl;
                    ((IWebPart)_WebPart).TitleUrl = base.TitleUrl;
                    ((IWebPart)_WebPart).Title = base.Title;
                    ((ICtrlWebPartBase)_WebPart).ClassName = base.ClassName;
                    cell.Controls.Add(_WebPart);
                  
                }
            }
            catch (Exception ex)
            {
            }
            trBody.Cells.Add(cell);
            table.Rows.Add(trBody);
 /*           if (Controls.Count == 1)
            {
                Controls.RemoveAt(0);

            }*/
            Controls.Add(table);
        }

        #region IWebEditable Members

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

        #endregion
    }
}
