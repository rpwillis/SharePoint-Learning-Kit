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

using System.Drawing;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Interfaces;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    [ToolboxData("<{0}:wptSearchTextbox runat=server />")]
    public class wptBOSearchTextbox : clsCtrlWebPartBase, INamingContainer, ISearchProvider
    {
        #region WebPart properties
        private TextBox txtBoxSearch = new TextBox();
        private ImageButton ImgSearch = new ImageButton();
        private Table tblContentInfo = new Table();
        private Label label = new Label();
        private UpdatePanel AsyncPanel = new UpdatePanel();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                object value = HttpContext.Current.Session["ListSearchText"];
                HttpContext.Current.Session["ListSearchText"] = null;
                if (value != null)
                {
                    txtBoxSearch.Text = (string)value;
                }
                txtBoxSearch.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) __doPostBack('" + ImgSearch.UniqueID + "','')");
                this.DataBind();
               
            }
        }

        /// <summary>
        /// The business object's property that represents the title of each search result. Its interpreted
        /// </summary>
        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Resultstitle")]
        [DefaultValue("")]
        [WebDisplayName("Results title:")]
        [WebDescription("Enter the property that will be diplayed as the results' title.")]
        public String title
        {
            get
            {
                if (ViewState["Resultstitle"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Resultstitle"].ToString();
                }
            }
            set
            {
                ViewState["Resultstitle"] = value;
            }
        }

        /// <summary>
        /// The business object's property that represents the description of each search result. Its interpreted
        /// </summary>
        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Resultsdescription")]
        [DefaultValue("")]
        [WebDisplayName("Results description:")]
        [WebDescription("Enter the property that will be diplayed as the results' description.")]
        public String description
        {
            get
            {
                if (ViewState["Resultsdescription"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Resultsdescription"].ToString();
                }
                
            }
            set
            {
                ViewState["Resultsdescription"] = value;
            }
        }
        

        /// <summary>
        /// The business object's property that represents the details of each search result. Its interpreted
        /// </summary>
        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "details")]
        [DefaultValue("")]
        [WebDisplayName("Results details:")]
        [WebDescription("Enter the property that will be diplayed as the results' details.")]
        public String details
        {
            get
            {
                if (ViewState["Details"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Details"].ToString();
                }
            }
            set
            {
                ViewState["Details"] = value;
            }
        }


        [Category("Search TextBox Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "columnsString")]
        [DefaultValue("")]
        [WebDisplayName("Search by property:")]
        [WebDescription("Enter the property that will be searched.")]
        public String columnsString
        {
            get
            {
                if (columns.Length > 0)
                {
                    string columnsString = "";
                    foreach (string column in columns)
                    {
                        columnsString += column + ",";
                    }
                    return columnsString.Remove(columnsString.Length - 1);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                columns = value.Split(new char[] { ',' });
            }
        }

        /// <summary>
        /// The business object's properties involved in the search, used by the criteria property.
        /// </summary>
    /*    [Category("Search TextBox Properties")]
//        [DefaultValue("")]
        [Personalizable(Personalizable(PersonalizationScope.Shared))]
        [WebDisplayName("Search by property:")]
        [WebDescription("Enter the property that will be searched.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "columns")]*/
        [WebBrowsable(false)]
        [Browsable(false)]
        public String[] columns
        {
            get
            {
                if (ViewState["Columns"] == null)
                {
                    return new string[0];
                }
                else
                {
                    return (String[])ViewState["Columns"];
                }
            }
            set
            {
                ViewState["Columns"] = value;
            }
        }
        

        /// <summary>
        /// bussiness object name. Specifies where the search is going to be done
        /// </summary>
        [Category("Search TextBox Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "businessobj")]
        [DefaultValue("")]
        [WebDisplayName("Search business object name:")]
        [WebDescription("Enter the business object name that will be used by the search.")]
        public String businessobj
        {
            get
            {
                if (ViewState["BusinessObj"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["BusinessObj"].ToString();
                }
            }
            set
            {
                ViewState["BusinessObj"] = value;
            }
        }
        private String _busssinesobj;

        /// <summary>
        /// Search Textbox title
        /// </summary>
        [Category("Search TextBox Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "searchTitle")]
        [DefaultValue("")]
        [WebDisplayName("Search title:")]
        [WebDescription("Enter the search title.")]
        public String searchTitle
        {
            get
            {
                return label.Text;
            }
            set
            {
                label.Text = value;
            }
        }
        
        /// <summary>
        /// Page path where the selected result is going to be diplayed
        /// </summary>

        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "redirect")]
        [DefaultValue("")]
        [WebDisplayName("Datailed results page URL:")]
        [WebDescription("Enter the page URL where the detailed results are going to be displayed.")]
        public String redirect
        {
            get
            {
                if (ViewState["Redirect"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Redirect"].ToString();
                }
            }
            set
            {
                ViewState["Redirect"] = value;
            }
        }

        [Category("Search TextBox Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ResultsPath")]
        [DefaultValue("")]
        [WebDisplayName("Results page URL:")]
        [WebDescription("Enter the results URL.")]
        public String ResultsPath
        {
            get
            {
                if (ViewState["ResultsPath"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["ResultsPath"].ToString();
                }
            }
            set
            {
                ViewState["ResultsPath"] = value;
            }
        }


        #region ISearchProvider Members

        /// <summary>
        /// search criteria
        /// </summary>
        public BLCriteria Criteria
        {
            get
            {
                //gets the kind of business object 
                Type businessType = System.Type.GetType(businessobj, false, true);
                if (typeof(IBLListBase).IsAssignableFrom(businessType))
                {
                    BLCriteria nCriteria = new BLCriteria(businessType);
                    string[] words = new string[1];
                    if (txtBoxSearch.Text != null)
                    {
                        words = txtBoxSearch.Text.Split(null);         //splits the search text in order to make a seach of each word
                    }
                    IBLListBase activate = (IBLListBase)System.Activator.CreateInstance(businessType);
                    String suffix = activate.DataLayer.DataLayerFieldSuffix;

                    BLCriteriaExpression.ListExpression[] columnexp = new BLCriteriaExpression.ListExpression[columns.Length];  //handles a search for each column
                    if (words.Length > 0)
                    {
                        //fills the columnexp criteria expresion for each column
                        for (int index = 0; index != columns.Length; index++)
                        {
                            if (!columns[index].EndsWith(suffix))
                            {
                                columns[index] += suffix;
                            }
                            BLCriteriaExpression.BinaryOperatorExpression expName = new BLCriteriaExpression.BinaryOperatorExpression("Name_Criteria_0", columns[index], "Name", "like", "%" + words[0] + "%");
                            //Add to the list with None as separetor, because is the first expression.
                            columnexp[index] = new BLCriteriaExpression.ListExpression(index.ToString());
                            columnexp[index].AddExpression(expName, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                        }
                    }
                    //search each word on each column
                    for (int index = 0; index != columns.Length; index++)
                    {
                        //makes a search on column[index] for each word.
                        for (int i = 1; i < words.Length; i++)
                        {
                            //Create base filter for name
                            BLCriteriaExpression.BinaryOperatorExpression expName = new BLCriteriaExpression.BinaryOperatorExpression("Name_Criteria_" + i.ToString(), columns[index], "Name", "like", "%" + words[i] + "%");
                            //Add to the list with OR as concatenate operator.
                            columnexp[index].AddExpression(expName, BLCriteriaExpression.BLCriteriaOperator.OperatorOr);
                        }
                    }
                    BLCriteriaExpression.ListExpression WordsSearchExpression = new BLCriteriaExpression.ListExpression("Words Expressions");
                    WordsSearchExpression.AddExpression(columnexp[0], BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                    //Add to the list with None as separetor, because is the first expression.
                    for (int index = 1; index != columns.Length; index++)
                    {
                        WordsSearchExpression.AddExpression(columnexp[index], BLCriteriaExpression.BLCriteriaOperator.OperatorOr);
                    }
                    //adds the criteria expresions made to the nCriteria
                    nCriteria.AddExpression(WordsSearchExpression, BLCriteriaExpression.BLCriteriaOperator.OperatorOr);
                    return nCriteria;
                }
                return null;
            }
        }

        #endregion

        [Category("Search TextBox Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "RedirectToItemList")]
        [DefaultValue("")]
        [WebDisplayName("Redirect to results page:")]
        [WebDescription("Check if you want to redirect to the results URL.")]
        public bool RedirectToItemList
        {
            get
            {
                object value = ViewState["PSTRedirectToList"];
                if (value == null)
                {
                    return false;
                }
                else
                {
                    return (bool)value;
                }
            }
            set
            {
                ViewState["PSTRedirectToList"] = value;
            //    CreateChildControls();
            }
        }

        #endregion

        #region WebPart events
        [ConnectionProvider("Search Provider", "default")]
        public ISearchProvider GetSearchProvider()
        {
            return this;
        }

        protected void ImgSearch_Click1(object sender, EventArgs e)
        {
            this.DataBind();
            NewSearch = true;
            if (RedirectToItemList)
            {
                HttpContext.Current.Session["ListSearchText"] = txtBoxSearch.Text; 
                HttpContext.Current.Response.Redirect(ResultsPath);
            }
        }
        #endregion

        #region WebPart functions
        private void generateContent()
        {
            TableRow trBody = new TableRow();
            TableCell tdCell1 = new TableCell();
            TableCell tdCell2 = new TableCell();
            TableCell tdCell3 = new TableCell();
            TableCell tdCell4 = new TableCell();
            this.ImgSearch.Width = new Unit("22px");
            this.ImgSearch.Height = new Unit("22px");
            string DevCommonPath = clsConfigurationProfile.Current.getPropertyValue("DevCommonPath");

            this.ImgSearch.ImageUrl = DevCommonPath + "/Images/MasterPageImages/top_search.gif";
            this.ImgSearch.TabIndex = 100;
            if (!RedirectToItemList)
            {
                //this.ImgSearch.AutoSubmit = true;
                //NOT SET CLIENT EVENT this.ImgSearch.ClientSideEvents.Click = "ImgSearch_Click";
            }
            else
            {
            //    this.ImgSearch.AutoSubmit = false;
            //    this.ImgSearch.ClientSideEvents.Click = "ImgSearch_Click";
                this.ImgSearch.OnClientClick = "ImgSearch_Click";
            }
            
            this.ImgSearch.Click +=new ImageClickEventHandler(ImgSearch_Click1);

            this.txtBoxSearch.BorderWidth = new Unit("1px");
            this.txtBoxSearch.BorderStyle = BorderStyle.Solid;
            label.Text = this.searchTitle;
            label.CssClass = "MIP-Small";
            this.txtBoxSearch.CssClass = "MIP-Small";
            label.ForeColor = Color.FromArgb(0x1952a4);
            this.txtBoxSearch.BorderColor = Color.FromArgb(0x7f9db9);
            trBody.HorizontalAlign = HorizontalAlign.Right;
            tdCell1.HorizontalAlign = HorizontalAlign.Right;
            tdCell2.HorizontalAlign = HorizontalAlign.Right;
            tdCell3.HorizontalAlign = HorizontalAlign.Left;
            this.txtBoxSearch.Width = new Unit("165px");
            tdCell1.Width = new Unit("100%");
            tdCell4.Width = new Unit("10px");
            this.txtBoxSearch.Height = new Unit("19px");
            tdCell1.Height = new Unit("26px");
            tdCell2.Height = new Unit("26px");
            tdCell3.Height = new Unit("26px");
            tdCell4.Height = new Unit("26px");
            tdCell1.Controls.Add (label);
            tdCell2.Controls.Add(this.txtBoxSearch);
            tdCell3.Controls.Add(this.ImgSearch);
            trBody.Cells.Add(tdCell1);
            trBody.Cells.Add(tdCell2);
            trBody.Cells.Add(tdCell3);
            trBody.Cells.Add(tdCell4);
            tblContentInfo.Rows.Add(trBody);

            
        }

        protected override void CreateChildControls()
        {
            try
            {
                EnsureUpdatePanelFixups();
                this.Controls.Clear();
                generateContent();
                if (!RedirectToItemList)
                {
//                    AsyncPanel.Width = this.Width;
  //                  AsyncPanel.Height = this.Height;
                    AsyncPanel.UpdateMode = UpdatePanelUpdateMode.Always;
                    AsyncPanel.ContentTemplateContainer.Controls.Add(tblContentInfo);
                    this.Controls.Add(AsyncPanel);
                }
                else
                {
                    this.Controls.Add(tblContentInfo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Bad parameters");
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            
            System.Text.StringBuilder Script = new System.Text.StringBuilder();

            Script.AppendLine("<script id=\"Infragistics\" type=\"text/javascript\">");
            Script.AppendLine("<!--");
            Script.AppendLine("function ImgSearch_Click(oButton, oEvent){");
            Script.AppendLine("__doPostBack(oButton.getUniqueID(), \"\");");
            Script.AppendLine("}");
            Script.AppendLine("-->");
            Script.AppendLine("</script>");

            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "wptSearchTextbox", Script.ToString());

        }
        #endregion

        #region ISearchProvider Members


        public bool NewSearch
        {
            get
            {
                if (ViewState["NewSearch"] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["NewSearch"]);
                }
            }
            set
            {
                ViewState["NewSearch"] = value;
            }
        }

        #endregion
    }
}
