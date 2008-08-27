using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerUITools;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Web.UI;
using Axelerate.BusinessLayerUITools.BaseClasses;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptContextMenu : clsCtrlWebPartBase, INamingContainer
    {
        private clsBaseLayoutWP m_Provider;
        private ArrayList CommandsToExecute = new ArrayList();
        
        private ArrayList Layouts
        {
            get
            {
                return (ArrayList)ViewState["ContextLayout"];
            }
            set
            {
                ViewState["ContextLayout"] = value;
            }
        }

        private bool EnableCommands
        {
            get
            {
                if (ViewState["EnableCommands"] == null)
                {
                    ViewState["EnableCommands"] = false;
                }
                return (bool)ViewState["EnableCommands"];
            }
            set
            {
                ViewState["EnableCommands"] = value;
            }
        }
        
        /// <summary>
        /// Image used as link as maximize button for this control if the correct java script is writed by the client.
        /// </summary>
        private Image MaximizeBT = null;

        /// <summary>
        /// Image used as button for collapse(minimize)/restore the control.
        /// </summary>
        private Image MinimizeRestoreBt = null;

        protected Label m_ErrorLabel;

        /// <summary>
        /// Panel to encapsulate the layout and buttons, is like the workspace (without the header) of the control.
        /// </summary>
        private Panel ClipZone = null;

        [ConnectionConsumer("Context Consumer", "ContextConsumer")]
        public void RegisterProvider(clsBaseLayoutWP Provider)
        {
            if (Provider != null)
            {
                m_Provider = Provider;
                Layouts = Provider.ContextMenuLayouts;
                EnableCommands = Provider.EnabledCommands;
                foreach (CommandEventArgs ev in CommandsToExecute)
                {
                    m_Provider.ExecuteCommand(this, ev, true);
                }
                CreateChildControls();
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            EnsureUpdatePanelFixups();
            
            ClipZone = new Panel(); //<div>
            ClipZone.Width = new Unit(100, UnitType.Percentage);
            ClipZone.Style.Value = "position:relative";
            ClipZone.CssClass = BodyCssClass;

            UpdatePanel container = new UpdatePanel();
            ClipZone.Controls.Add(container);
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
                    //if (item != null && item.GetType().IsAssignableFrom(typeof(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)))
                    //{
                    //    HeaderLabel.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString((Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)item, Title, ClassName, ((BLBusinessBase)item).DataKey.ToString());
                    //}
                    //else
                    //{
                        HeaderLabel.Text = Title;
                    //}
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

                //ClipZone.Controls.Add(m_ErrorLabel);
               // CheckParameters(item);



                TableCell HeaderRightCell = new TableCell();
                HeaderRightCell.CssClass = "headerrightcell";
                // HeaderRightCell.Width = new Unit(0, UnitType.Pixel);
                HeaderRow.Cells.Add(HeaderRightCell);

                Header.Controls.Add(HeaderRow);
                Controls.Add(Header);
            }
            m_ErrorLabel = new Label();
            m_ErrorLabel.CssClass = "ErrorMessage";
            container.ContentTemplateContainer.Controls.Add(m_ErrorLabel);

            if (Layouts != null)
            {
                wptGrid grid = new wptGrid();
                foreach (String xml in Layouts)
                {
                    XmlDocument XMLDoc = new XmlDocument();
                    XMLDoc.LoadXml(xml);
                    foreach (XmlNode node in XMLDoc.ChildNodes)
                    {
                        foreach (XmlNode gnode in node.ChildNodes)
                        {
                            switch (gnode.Name.ToUpper())
                            {
                                case "MENU":
                                    grid.CreateMenu(gnode, container.ContentTemplateContainer, null, false, new MenuEventHandler(OnDoMenu), -1, EnableCommands);
                                    break;
                                case "TOOLBAR":
                                    grid.CreateToolBar(gnode, container.ContentTemplateContainer, null, false, new CommandEventHandler(OnDoCommand), -1, EnableCommands);
                                    break;
                            }
                        }
                    }
                }
            }
            Controls.Add(ClipZone);
      //      base.CreateChildControls();
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


        protected void OnDoMenu(object sender, MenuEventArgs e)
        {
            String[] value = e.Item.Value.Split('|');
            String command = value[1];
            String arguments = value[0] + "|";
            if (value.Length >= 3)
            {
                arguments += value[2];
            }
            CommandsToExecute.Add(new CommandEventArgs(command, arguments));
            //ExecuteCommand(sender, e);
        }

        protected void OnDoCommand(object sender, CommandEventArgs e)
        {
            CommandsToExecute.Add(e);
            //ExecuteCommand(sender, e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (MinimizeRestoreBt != null)
            {
                MinimizeRestoreBt.Attributes["OnClick"] = "javascript:BLWPCollapseExpand('" + ClipZone.ClientID + "','" + MinimizeRestoreBt.ClientID + "','" + MinimizeButtonImage + "','" + RestoreButtonImage + "')";
            }
        }

        /*       private void FindButtons(ControlCollection control)
               {
                   foreach (Control cont in control)
                   {
                       if (typeof(IButtonControl).IsAssignableFrom(Type.GetType(cont.GetType().AssemblyQualifiedName)))
                       {
                           Button newButton = new Button();
                           IButtonControl but = (IButtonControl)cont;
                           but = newButton;
                           newButton.Command += new CommandEventHandler(NewButton_Command);
                       }
                       FindButtons(cont.Controls);
                   }
               }*/
        #region "Header Properties"
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
    }
}
