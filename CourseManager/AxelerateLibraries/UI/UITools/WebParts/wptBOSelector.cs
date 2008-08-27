using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Web.UI.WebControls.WebParts;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Configuration;
using System.Collections;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Interfaces;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    [ToolboxData("<{0}:wptSearchTextbox runat=server />")]
    public class wptBOSelector : clsCtrlWebPartBase, INamingContainer, ISelectorProvider
    {
        Label Comment = new Label();
        Button btnSelector  = new Button();
        DropDownList DropDownList1  = new DropDownList();
        Table table = new Table();

        #region WebPart Properties


        /// <summary>
        /// Text displayed in the comment label
        /// </summary>
        [Category("Loaded Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "CommentText")]
        [DefaultValue("")]
        [WebDisplayName("Comment text:")]
        [WebDescription("Enter the comment text.")]
        public String CommentText
        {
            get
            {
                return Comment.Text;
            }
            set
            {
                Comment.Text = value;
            }
        }

        /// <summary>
        /// Sets the visibility of the comment label
        /// </summary>
        public bool CommentVisible
        {
            get
            {
                return Comment.Visible;
            }
            set
            {
                Comment.Visible = value;
            }
        }

        
        /// <summary>
        /// Text displayed in the button
        /// </summary>
        public String ButtonText
        {
            get
            {
                return btnSelector.Text;
            }
            set
            {
                btnSelector.Text = value;
            }
        }

        /// <summary>
        /// Sets the visibility of the button
        /// </summary>
        public bool ButtonVisible
        {
            get
            {
                return btnSelector.Visible;
            }
            set
            {
                btnSelector.Visible = value;
            }
        }

        /// <summary>
        /// bussiness object used to populate the DropDownList
        /// </summary>
        public String businessobj
        {
            get
            {
                if (ViewState["busssinesobj"] == null)
                {
                    return "";
                }
                else
                {
                    return (String)ViewState["busssinesobj"];
                }
            }
            set
            {
                ViewState["busssinesobj"] = value;
            }
        }

        /// <summary>
        /// bussiness object's property used as value of the DropDownList. It's not interpreted
        /// </summary>
        public String columnValue
        {
            get
            {
                if (ViewState["columnValue"] == null)
                {
                    return "";
                }
                else
                {
                    return (String)ViewState["columnValue"];
                }
            }
            set
            {
                ViewState["columnValue"] = value;
            }
        }

        /// <summary>
        /// bussiness object's property used as text of the DropDownList. It's not interpreted
        /// </summary>
        public String columnText
        {
            get
            {
                if (ViewState["columnText"] == null)
                {
                    return "";
                }
                else
                {
                    return (String)ViewState["columnText"];
                }
            }
            set
            {
                ViewState["columnText"] = value;
            }
        }

        
        #endregion

        #region WebPart methods

        /// <summary>
        /// Populates the DropDownList
        /// </summary>
        public void fill()
        {
            Type businessType = System.Type.GetType(businessobj, false, true);
            if (businessType != null)
            {
                IBLListBase activate = (IBLListBase)System.Activator.CreateInstance(businessType);
                IBLListBase collection = (IBLListBase)ReflectionHelper.GetSharedBusinessClassProperty(businessobj, "GetCollection", new object[] { GetCriteria() });
                DropDownList1.DataSource = collection;
                DropDownList1.DataTextField = columnText;
                DropDownList1.DataValueField = columnValue;
                DropDownList1.DataBind();
            }
        }
        #endregion

        #region WebPart Events
        protected void Page_Load(object sender, EventArgs e)
        {
            //sets the button event
            btnSelector.Command += new CommandEventHandler(NewButton_Command);
        }

        public event CommandEventHandler CommandEvent;
        void NewButton_Command(object sender, CommandEventArgs e)
        {
            if (CommandEvent != null)
            {
                CommandEvent(this, e);
            }
        }

        [ConnectionProvider("Selector Provider", "default")]
        public ISelectorProvider GetSelectorProvider()
        {
            return this;
        }
        #endregion

        #region WebPart Generator
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            generateBOSelectorContent();
            this.fill();
        }

        private void generateBOSelectorContent()
        {
            table.Width = new Unit("100%");
            TableRow trBody = new TableRow();
            TableCell tdCell1 = new TableCell();
            TableCell tdCell2 = new TableCell();
            TableCell tdCell3 = new TableCell();
            string DevCommonPath = clsConfigurationProfile.Current.getPropertyValue("DevCommonPath");
            Comment.Text = CommentText;
            Comment.Visible = CommentVisible;
            btnSelector.Text = ButtonText;
            btnSelector.Visible = ButtonVisible;

            btnSelector.ID = "btnSelector";
            btnSelector.Height = new Unit("22px");
            tdCell1.Controls.Add(Comment);
            tdCell2.Controls.Add(DropDownList1);
            tdCell3.Controls.Add(btnSelector);
            trBody.Cells.Add(tdCell1);
            trBody.Cells.Add(tdCell2);
            trBody.Cells.Add(tdCell3);
            table.Rows.Add(trBody);
            this.Controls.Add(table);
        }
        #endregion

        #region ISelectorProvider Members


        public IList SelectedItems
        {
            get { throw new Exception(Resources.ErrorMessages.errMethodNotImplemented); }
        }

        public string ReturnedProperty
        {
            get
            {
                throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
            }
            set
            {
                throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
            }
        }

        public IList NotSelectedItems
        {
            get { throw new Exception(Resources.ErrorMessages.errMethodNotImplemented); }
        }

        public void SelectItem(string ObjectDataKey)
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        public void DeselectItem(string ObjectDatakey)
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        #endregion

        #region ISelectorProvider Members

        public string ConsumerType
        {
            get
            {
                throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
            }
            set
            {
                throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
            }
        }

        #endregion

        
    }
}
