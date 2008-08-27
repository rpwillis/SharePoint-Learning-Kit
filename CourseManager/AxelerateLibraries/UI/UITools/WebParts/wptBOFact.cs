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

using Axelerate.BusinessLayerFrameWork.BLCore;
using System.ComponentModel;
using System.Xml.Serialization;

using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    [XmlRoot(Namespace = "Axelerate.SharepointBusinessLayerUIToolsTest")]
    [ToolboxData("<{0}:wptBOFact runat=server />")]
    [Serializable]
    public class wptBOFact : clsCtrlWebPartBase, INamingContainer
    {
        #region Enums
        public enum FactContentType
        {
            Property = 0,
            HTML = 1,
            ProgressBar = 2,
            Image = 5
        }

        public enum wptBehavior
        {
            EditableOnFocus = 0,
            NoEditable = 1,
            AlwaysEditable = 2
        }
        #endregion

        #region Fact Members
        private FactContentType m_contentType = FactContentType.HTML;
        private Table tblContentInfo = null;
        /// <summary>
        /// Panel to encapsulate the layout and buttons, is like the workspace (without the header) of the control.
        /// </summary>
        private Panel ClipZone = null;

        /// <summary>
        /// Image used as link as maximize button for this control if the correct java script is writed by the client.
        /// </summary>
        private Image MaximizeBT = null;

        /// <summary>
        /// Image used as button for collapse(minimize)/restore the control.
        /// </summary>
        private Image MinimizeRestoreBt = null;

        protected Label m_ErrorLabel;
        #endregion

        #region FactProperties
        [Category("Fact Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ContentType")]
        [DefaultValue("")]
        [WebDisplayName("Fact content type:")]
        [WebDescription("Type of the fact content.")]
        public FactContentType ContentType
        {
            get
            {
                return m_contentType;
            }
            set
            {
                m_contentType = value;
            }
        }

        /// <summary>
        /// Image path to be applied to the Maximize button
        /// </summary>
        public String MaximizeBTImage
        {
            get
            {
                if (ViewState["MaximizeBTImage"] == null)
                {
                    ViewState["MaximizeBTImage"] = "../DevCommon/images/igpnl_up.gif";
                }
                return (String)ViewState["MaximizeBTImage"];
            }
            set
            {
                ViewState["MaximizeBTImage"] = value;
            }
        }

        [Category("Fact Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Content")]
        [DefaultValue("")]
        [WebDisplayName("Fact content:")]
        [WebDescription("Enter the fact content.")]
        public string Content
        {
            get
            {
                if (ViewState["FactContent"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["FactContent"];
                }
            }
            set
            {
                ViewState["FactContent"] = value;
            }
        }
        /// <summary>
        /// Set or Get if the control most show the maximize button
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most show the maximize button")]
        public bool ShowMaximizeButton
        {
            get
            {
                object val = ViewState["ShowMaximize"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["ShowMaximize"] = value;
            }
        }

        /// <summary>
        /// Set or Get if the control most show the maximize button
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most show the minimize button")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Show Collapse/Expand button")]
        [WebDescription("Show the collapse/expand button on title header.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "ShowMinimizeRestoreButton")]
        public bool ShowMinimizeRestoreButton
        {
            get
            {
                object val = ViewState["ShowMinimizeRestoreButton"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["ShowMinimizeRestoreButton"] = value;
            }
        }

        /// <summary>
        /// Set or Get if the control most show the header
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most show the header")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Show custom title")]
        [WebDescription("Show the custom title for this webpart. This title can use image for rounder borders appareance")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "ShowHeader")]
        public bool ShowHeader
        {
            get
            {
                object val = ViewState["ShowHeader"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["ShowHeader"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get Css Class image for the header.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Title Css Class")]
        [WebDescription("Css class to apply to custom title.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "HeaderCssClass")]
        public string HeaderCssClass
        {
            get
            {
                object val = ViewState["HeaderCssClass"];
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["HeaderCssClass"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get Css Class for the body of the webpart.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Body Css Class")]
        [WebDescription("Css class to apply to body of the webpart.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "BodyCssClass")]
        public string BodyCssClass
        {
            get
            {
                object val = ViewState["BodyCssClass"];
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["BodyCssClass"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get the Minimize Button Image.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Minimize button image")]
        [WebDescription("Image to use in minimize button on title.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "MinimizeButtonImage")]
        public string MinimizeButtonImage
        {
            get
            {
                object val = ViewState["MinimizeButtonImage"];
                if (val == null)
                {
                    val = clsConfigurationProfile.Current.getPropertyValue("WPMinimizeButtonImage");
                }
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["MinimizeButtonImage"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get the Restore Button Image.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Restore Button Image")]
        [WebDescription("Image to use in restore button on title.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "RestoreButtonImage")]
        public string RestoreButtonImage
        {
            get
            {
                object val = ViewState["RestoreButtonImage"];
                if (val == null)
                {
                    val = clsConfigurationProfile.Current.getPropertyValue("WPRestoreButtonImage");
                }
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["RestoreButtonImage"] = value;
            }
        }

        #endregion

        #region WebPart Events
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void CreateChildControls()
        {
            generateFactContent();
        }
        #endregion

        #region WebPart Functions

        private void generateFactContent()
        {
            Object item = GetBusinessClassInstance();
            if (item == null)
            {
                CheckParameters();
            }
            else
            {
                Table MainContainer = new Table();
                Controls.Add(MainContainer);
                tblContentInfo = new Table();

                MainContainer.Width = new Unit(100, UnitType.Percentage);

                TableRow MainContainerRow = new TableRow();
                TableCell MainContainerCell = new TableCell();
                MainContainer.Rows.Add(MainContainerRow);
                MainContainerRow.Cells.Add(MainContainerCell);

                ClipZone = new Panel(); //<div>
                ClipZone.Width = new Unit(100, UnitType.Percentage);
                ClipZone.Style.Value = "position:relative";
                ClipZone.CssClass = BodyCssClass;

                if (ShowHeader)
                {
                    Table Header = new Table();
                    Header.BorderStyle = BorderStyle.None;
                    Header.CellPadding = 0;
                    Header.CellSpacing = 0;
                    Header.Width = new Unit(100, UnitType.Percentage);
                    Header.CssClass = "igwgHdrBlue2k7";
                    TableRow HeaderRow = new TableRow();

                    //Title of the control
                    TableCell HeaderCell = new TableCell();
                    HeaderCell.CssClass = "headercell";
                    Label HeaderLabel = new Label();
                    if (Title != "")
                    {
                        if (item != null && item.GetType().IsAssignableFrom(typeof(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)))
                        {
                            HeaderLabel.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString((Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)item, Title, ClassName, ((BLBusinessBase)item).DataKey.ToString());
                        }
                        else
                        {
                            HeaderLabel.Text = Title;
                        }
                    }

                    if (HeaderCssClass != "")
                    {
                        Header.CssClass = HeaderCssClass;
                    }
                    else
                    {
                        HeaderLabel.CssClass = "MIP-GridTitles";
                    }
                    /*if (HeaderBackgroundImage != "")
                    {
                        Header.BackImageUrl = HeaderBackgroundImage;
                    }*/

                    HeaderCell.Controls.Add(HeaderLabel);

                    TableCell HeaderLeftCell = new TableCell();
                    HeaderLeftCell.CssClass = "headerleftcell";
                    //HeaderLeftCell.Width = new Unit(0, UnitType.Pixel);
                    HeaderRow.Cells.Add(HeaderLeftCell);
                    HeaderRow.Cells.Add(HeaderCell);

                    if (ShowMaximizeButton || ShowMinimizeRestoreButton)
                    {
                        //Maximaize button
                        HeaderCell = new TableCell();
                        HeaderCell.CssClass = "headercell";
                        HeaderCell.Width = new Unit(10, UnitType.Pixel);
                        HeaderCell.HorizontalAlign = HorizontalAlign.Center;
                        if (ShowMinimizeRestoreButton)
                        {
                            MinimizeRestoreBt = new Image();
                            MinimizeRestoreBt.ID = "CollapseExpandButton";
                            MinimizeRestoreBt.ImageUrl = MinimizeButtonImage;
                            MinimizeRestoreBt.Attributes["OnClick"] = "javascript:BLWPCollapseExpand(\"" + ID + "\",\"\")";
                            HeaderCell.Controls.Add(MinimizeRestoreBt);
                            RegisterCollapseScript();
                        }
                        HeaderRow.Controls.Add(HeaderCell);
                    }

                    m_ErrorLabel = new Label();
                    m_ErrorLabel.CssClass = "ErrorMessage";
                    ClipZone.Controls.Add(m_ErrorLabel);
                    CheckParameters(item);



                    TableCell HeaderRightCell = new TableCell();
                    HeaderRightCell.CssClass = "headerrightcell";
                    // HeaderRightCell.Width = new Unit(0, UnitType.Pixel);
                    HeaderRow.Cells.Add(HeaderRightCell);

                    Header.Controls.Add(HeaderRow);
                    MainContainerCell.Controls.Add(Header);
                }
                MainContainerCell.Controls.Add(ClipZone);

                this.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");

                TableRow trBody = new TableRow();
                TableCell cell = new TableCell();

                if (item != null)
                {
                    if (Content == null) //If the object property is null
                    {
                        Label nullLbl = new Label();
                        cell.Controls.Add(nullLbl);
                    }
                    else
                    {
                        switch (ContentType)
                        {
                            //Because we don't know the Type of the property, then create a Label object and set the text with the value using the proper format.
                            case FactContentType.Property:
                                Label lbl = new Label();
                                lbl.CssClass = CssClass;
                                lbl.Text = parseBOPropertiesString("{" + Content + "}");
                                cell.Controls.Add(lbl);
                                break;
                            //If the object is an HTML, then create a Label object and set the text with the value using the proper format.
                            case FactContentType.HTML:
                                Label lblHTML = new Label();
                                lblHTML.CssClass = CssClass;
                                lblHTML.Text = parseBOPropertiesString(Content);
                                cell.Controls.Add(lblHTML);
                                break;
                            //If the content is a double (as a number or a double or float type property), then create an Image object and set the location//
                            case FactContentType.ProgressBar:
                                Image imgPB = clsSharedMethods.SharedMethods.getImageProperty((BLBusinessBase)item, "{" + Content + "}", true);
                                cell.Controls.Add(imgPB);
                                break;
                            //If the object is an image (Bitmap), create an Image object and set the location.
                            case FactContentType.Image:
                                Image img = clsSharedMethods.SharedMethods.getImageProperty((BLBusinessBase)item, Content, false);
                                cell.Controls.Add(img);
                                break;
                            default:
                                break;
                        }
                    }
                    trBody.Cells.Add(cell);
                    tblContentInfo.Rows.Add(trBody);

                }
                else
                {
                    tblContentInfo.Rows.Add(trBody);
                }
                ClipZone.Controls.Add(tblContentInfo);
            }
        }

        protected void CheckParameters(object item)
        {
            m_ErrorLabel.Text = "";
            m_ErrorLabel.Visible = false;
            if (item != null)
            {
                m_ErrorLabel.Visible = true;
                m_ErrorLabel.Text = Resources.ErrorMessages.errInvalidDatasource;
                Type DatasourceType = Type.GetType(ClassName);
                if (DatasourceType == null)
                {
                    clsLog.Trace(Resources.ErrorMessages.errInvalidClassName + " " + ClassName, LogLevel.LowPriority);
                    if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                    {
                        throw new Exception(Resources.ErrorMessages.errInvalidClassName + " " + ClassName);
                    }
                    m_ErrorLabel.Text = Resources.ErrorMessages.errInvalidClassName + " " + ClassName;
                }
                else if (DatasourceType.GetMethod(FactoryMethod, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public) == null)
                {
                    clsLog.Trace(Resources.ErrorMessages.errInvalidFactoryMethod + " " + FactoryMethod, LogLevel.LowPriority);
                    if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                    {
                        throw new Exception(Resources.ErrorMessages.errInvalidFactoryMethod + " " + FactoryMethod);
                    }
                    m_ErrorLabel.Text = Resources.ErrorMessages.errInvalidFactoryMethod + " " + FactoryMethod;
                }
                clsLog.Trace(Resources.ErrorMessages.errInvalidDatasource, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Resources.ErrorMessages.errInvalidDatasource);
                }
            }
        }

        private void RegisterCollapseScript()
        {
            System.Text.StringBuilder Script = new System.Text.StringBuilder(200);
            Script.AppendLine("<script type=\"text/javascript\">");
            Script.AppendLine("     function BLWPCollapseExpand(PanelID, ImageID, CollapseImgUrl, ExpandImgUrl) ");
            Script.AppendLine("     { ");
            Script.AppendLine("         var Panel = document.getElementById(PanelID);");
            Script.AppendLine("         var Img = document.getElementById(ImageID);");
            Script.AppendLine("         if(Panel.style.display == \"none\") ");
            Script.AppendLine("         {");
            Script.AppendLine("             Panel.style.display = \"\";");
            Script.AppendLine("             Img.src = CollapseImgUrl;");
            Script.AppendLine("         }");
            Script.AppendLine("         else {");
            Script.AppendLine("             Panel.style.display = \"none\"; ");
            Script.AppendLine("             Img.src = ExpandImgUrl;");
            Script.AppendLine("         }");
            Script.AppendLine("      }");
            Script.AppendLine("</script>");

            ScriptManager.RegisterStartupScript(this, typeof(clsBaseLayoutWP), "BLWP_Collapse_Expand_Scritp", Script.ToString(), false);
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (MinimizeRestoreBt != null)
            {
                MinimizeRestoreBt.Attributes["OnClick"] = "javascript:BLWPCollapseExpand('" + ClipZone.ClientID + "','" + MinimizeRestoreBt.ClientID + "','" + MinimizeButtonImage + "','" + RestoreButtonImage + "')";
            }
        }

        #endregion
    }
}