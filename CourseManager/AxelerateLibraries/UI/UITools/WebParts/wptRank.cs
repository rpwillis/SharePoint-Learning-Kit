using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using Axelerate.BusinessLogic.SharedBusinessLogic.Security;
using Axelerate.BusinessLogic.SharedBusinessLogic;
using System.Web.UI.WebControls;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Drawing;
using System.Web;
using Axelerate.BusinessLayerUITools.BaseClasses;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptRank : clsCtrlWebPartBase, INamingContainer
    {
        private Label lblError = new Label();
        private Label lblcomment = new Label();
        private TextBox txtComment = new TextBox();
        private HiddenField hdfNewRank = new HiddenField();
        private Table mainTable = new Table();
        private Button btnNext = new Button();
        private Button WebImageButton1 = new Button();
        private String starClientID;
        Panel div2CellTable1 = new Panel();
       
        public int ActualRanking = 0;
        //private IRankable RankableObject
        #region WebPart properties

        protected void Page_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// Guid of the object to be ranked
        /// </summary>
        public string ObjectGUID
        {
            get
            {
                if (ViewState["RankObjectGUID"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["RankObjectGUID"];
                }
            }
            set
            {
                ViewState["RankObjectGUID"] = value;
            }
        }

        /// <summary>
        /// Message to be displayed if an error occurs
        /// </summary>
        [Category("Loaded Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "MsgError")]
        [DefaultValue("")]
        [WebDisplayName("Custom error message text:")]
        [WebDescription("Enter the error text.")]
        public string MsgError
        {
            get
            {
                return lblError.Text;
            }
            set
            {
                lblError.Text = value;
            }
        }

        /// <summary>
        /// Css to be applied to the comment label
        /// </summary>
        [Category("Loaded Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "CssLabel")]
        [DefaultValue("")]
        [WebDisplayName("Style name:")]
        [WebDescription("Enter the style name.")]
        public String CssLabel
        {
            get
            {
                return lblcomment.CssClass;
            }
            set
            {

                lblcomment.CssClass = value;
            }
        }

        /// <summary>
        /// Css to be applied to the error label
        /// </summary>
        [Category("Loaded Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "CssError")]
        [DefaultValue("")]
        [WebDisplayName("Error style name:")]
        [WebDescription("Enter the error style name.")]
        public String CssError
        {
            get
            {
                return lblError.CssClass;
            }
            set
            {
                lblError.CssClass = value;
            }
        }

        #endregion

        #region WebPart Events

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            setActualRank();
            setJavaScript();
        }


        private void setJavaScript()
        {
            System.Text.StringBuilder Script = new System.Text.StringBuilder();

            Script.AppendLine("<script language=\"javascript\" type=\"text/javascript\">");
            Script.AppendLine("var ActualRanking = " + ActualRanking + ";");
            Script.AppendLine("var hiddenFieldID = \"" + hdfNewRank.ClientID + "\";");
            Script.AppendLine("var Star1ID = \"" + starClientID + "\";");
            Script.AppendLine("var commentID = \"" + div2CellTable1.ClientID + "\";");
            Script.AppendLine("function init()");
            Script.AppendLine("{");
            Script.AppendLine("mouseout();");
            Script.AppendLine("}");
            Script.AppendLine("function monclick(newrank)");
            Script.AppendLine("{");
            Script.AppendLine("var cmd = document.getElementById(commentID);");
            Script.AppendLine("var hdfNewRank = document.getElementById(hiddenFieldID);");
            Script.AppendLine("hdfNewRank.innerText = newrank;");
            Script.AppendLine("cmd.style.display = \"inline\";");
            Script.AppendLine("}");
            Script.AppendLine("function mouseout()");
            Script.AppendLine("{");
            Script.AppendLine("var rank = 5;");
            Script.AppendLine("while (rank != ActualRanking)");
            Script.AppendLine("{");
            Script.AppendLine("var mstar = document.getElementById(Star1ID.substr(0,Star1ID.length-1) + rank);");
            Script.AppendLine("if (mstar != null)");
            Script.AppendLine("{");
            Script.AppendLine("mstar.style.backgroundImage = \"\";");
            Script.AppendLine("}");
            Script.AppendLine("rank--;");
            Script.AppendLine("}");
            Script.AppendLine("while (rank != 0)");
            Script.AppendLine("{");
            Script.AppendLine("var mstar = document.getElementById(Star1ID.substr(0,Star1ID.length-1) + rank);");
            Script.AppendLine("mstar.style.backgroundImage = \"url('DevCommon/images/Raking/star.png')\";");
            Script.AppendLine("rank--;");
            Script.AppendLine("}");
            Script.AppendLine("}");
            Script.AppendLine("function setbackground(rank)");
            Script.AppendLine("{");
            Script.AppendLine("while (rank != 0)");
            Script.AppendLine("{");
            Script.AppendLine("var mstar = document.getElementById(Star1ID.substr(0,Star1ID.length-1) + rank);");
            Script.AppendLine("mstar.style.backgroundImage = \"url('DevCommon/images/Raking/starPoint.png')\";");
            Script.AppendLine("rank--;");
            Script.AppendLine("}");
            Script.AppendLine("}");
            Script.AppendLine("init();");
            Script.AppendLine("</script>");
            //Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "wptRank", Script.ToString());
            HttpContext.Current.Response.Write(Script.ToString());
        }

        private void setActualRank()
        {
            try
            {
                //gets the object to rank
                IRankeable RankableObject = (IRankeable)ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", new object[] { ObjectGUID, null });

                //gets the actual rank
                if (RankableObject != null)
                {
                    clsADUser ActualUser = clsADUser.TryGetObjectByGUID("3", null); //TODO: 3 replaces GetCurrentUserNameGUID()
                    ActualRanking = RankableObject.ActualRank(ActualUser);
                }
                //  Title = ((string)ViewState["OriginalTitle"]).Replace("{Nose}", "Daniel Rojas");
                // TitleUrl = TitleUrl.Replace("{algo}", "Otra cosa");
            }
            catch
            {
            }
        }

        protected void btAccept_Click(object sender, EventArgs e)
        {
            try
            {
                IRankeable RankableObject = (IRankeable)ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", new object[] { ObjectGUID, null });
                //if the ObjectGUID belongs to a business object, ranks the object
                if (RankableObject != null)
                {
                    clsADUser ActualUser = clsADUser.TryGetObjectByGUID("3", null); //TODO: 3 replaces GetCurrentUserNameGUID()
                    RankableObject.Rank(ActualUser, int.Parse(hdfNewRank.Value), txtComment.Text);
                    lblError.Visible = false;
                }
            }
            catch
            {
                //if the business object couldn't be ranked (do to a business object rule broken) displays the error message
                lblError.Visible = true;
            }
        }
        protected void WebImageButton1_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
        }
        #endregion

        #region WebPart Creation
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            generateRankContent();
            //setJavaScript();
        }

        private void generateRankContent()
        {
            mainTable.Width = new Unit("100%");
            this.Controls.Add(mainTable);
            TableRow rowTable1= new TableRow();
            mainTable.Rows.Add(rowTable1);
            TableCell cellTable1 = new TableCell();
            rowTable1.Cells.Add(cellTable1);
            Table table2 = new Table();
            TableRow rowTable2 = new TableRow();
            table2.Rows.Add(rowTable2);
            TableCell cellTable2 = new TableCell();
            rowTable2.Cells.Add(cellTable2);
            Table table3 = new Table();
            TableRow rowTable3 = new TableRow();
            table3.Rows.Add(rowTable3);
            TableCell cell1Table3 = new TableCell();
            TableCell cell2Table3 = new TableCell();
            TableCell cell3Table3 = new TableCell();
            rowTable3.Cells.Add(cell1Table3);
            rowTable3.Cells.Add(cell2Table3);
            rowTable3.Cells.Add(cell3Table3);
            Table table4 = new Table();
            TableRow rowTable4 = new TableRow();
            table4.Rows.Add(rowTable4);
            TableCell cell1Table4 = new TableCell();
            TableCell cell2Table4 = new TableCell();
            rowTable4.Cells.Add(cell1Table4);
            rowTable4.Cells.Add(cell2Table4);

            Panel divCellTable1 = new Panel();
            cellTable1.Controls.Add(divCellTable1);
            divCellTable1.Style.Value = " position:relative; padding: 10px 0px 10px 0px; width:100%; height:100%;";
            divCellTable1.Controls.Add(table2);

            table2.CellPadding = 10;
            table2.Width = new Unit("100%");

            Panel divCellTable2 = new Panel();
            cellTable2.Controls.Add(divCellTable2);
            divCellTable2.Style.Value = "background-image:url(DevCommon/images/Raking/stars_back.png); background-repeat:no-repeat; width:100%; height:100%;";

            CreateStars(divCellTable2);

            lblError.Font.Name = "Verdana";
            lblError.ForeColor = Color.Red;
            lblError.Text = "Error";

            divCellTable1.Controls.Add(div2CellTable1);
            div2CellTable1.ID = "getComment";
            div2CellTable1.CssClass = "MIP-SemiTransparentDiv";
            div2CellTable1.Style.Value = "width:100%; height:100%;";
            div2CellTable1.Controls.Add(table3);

            table3.BorderWidth = new Unit("0");
            table3.CellPadding = 5;
            table3.CellSpacing = 0;
            table3.Width = new Unit("100%");
            table3.Style.Value = "vertical-align:middle; height:100%";

            cell1Table3.Controls.Add(lblcomment);
            lblcomment.Font.Name = "Verdana";
            lblcomment.Text = "Comment: ";
            lblcomment.CssClass = "MIP-Normal";

            cell2Table3.Style.Value = "width:100%";
            cell2Table3.Controls.Add(txtComment);

            txtComment.Width = new Unit("100%");

            cell3Table3.Controls.Add(table4);
            table4.Width = new Unit("150px");
            btnNext.ID = "btnNext";
            btnNext.Text = "Accept";
            btnNext.Height = new Unit("22px");
   //         btnNext.ImageDirectory = "DevCommon/Images/WebImageButtonOnWhite/";
   //         btnNext.UseBrowserDefaults = false;
            this.btnNext.Click +=new EventHandler(btAccept_Click);
   /*         btnNext.Alignments.VerticalImage = VerticalAlign.Bottom;
            btnNext.RoundedCorners.HeightOfBottomEdge = 0;
            btnNext.RoundedCorners.HoverImageUrl = "igwib_bg-office12_blue_hover.gif";
            btnNext.RoundedCorners.ImageUrl = "igwib_bg-office12_blue_normal.gif";
            btnNext.RoundedCorners.MaxHeight = 22;
            btnNext.RoundedCorners.MaxWidth = 200;
            btnNext.RoundedCorners.PressedImageUrl = "igwib_bg-office12_blue_pressedl.gif";
            btnNext.RoundedCorners.RenderingType = ButtonRoundedCornersType.FileImages;*/
            cell1Table4.Controls.Add(btnNext);

            WebImageButton1.ID = "WebImageButton1";
            WebImageButton1.Text = "Cancel";
            WebImageButton1.Height = new Unit("22px");
  //          WebImageButton1.ImageDirectory = "DevCommon/Images/WebImageButtonOnWhite/";
  //          WebImageButton1.UseBrowserDefaults = false;
            this.WebImageButton1.Click +=new EventHandler(WebImageButton1_Click);
  /*          WebImageButton1.Alignments.VerticalImage = VerticalAlign.Bottom;
            WebImageButton1.RoundedCorners.HeightOfBottomEdge = 0;
            WebImageButton1.RoundedCorners.HoverImageUrl = "igwib_bg-office12_blue_hover.gif";
            WebImageButton1.RoundedCorners.ImageUrl = "igwib_bg-office12_blue_normal.gif" ;
            WebImageButton1.RoundedCorners.MaxHeight = 22;
            WebImageButton1.RoundedCorners.MaxWidth = 200;
            WebImageButton1.RoundedCorners.PressedImageUrl = "igwib_bg-office12_blue_pressedl.gif";
            WebImageButton1.RoundedCorners.RenderingType = ButtonRoundedCornersType.FileImages;*/
            cell2Table4.Controls.Add(WebImageButton1);

            hdfNewRank.ID = "hdfNewRank";
            div2CellTable1.Controls.Add(hdfNewRank);

        }

        private void CreateStars(Panel div)
        {
            String[] tooltips = new String[5] {Resources.RankToolTip.Rank1, Resources.RankToolTip.Rank2, Resources.RankToolTip.Rank3, Resources.RankToolTip.Rank4, Resources.RankToolTip.Rank5};
            for (int index = 1; index != 6; index++)
            {
                Panel newPanel = new Panel();
                newPanel.ID = "star" + index;
                newPanel.Style.Value = "display:inline; background-repeat:no-repeat; width:16px; height:16px; position:relative; top:2px;left:1px";
                newPanel.Attributes.Add("onmouseover", "setbackground(" + index + ")");
                newPanel.Attributes.Add("onmouseout", "mouseout()");
                newPanel.Attributes.Add("onclick", "monclick(" + index + ")");
                newPanel.ToolTip = tooltips[index - 1];
                div.Controls.Add(newPanel);
                starClientID = newPanel.ClientID;
            }
            Literal lit = new Literal();
            lit.Text = "<br/>";
            div.Controls.Add(lit);
        }
        #endregion
    }
}
