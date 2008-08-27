using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    /// <summary>
    /// Delegate to handle selected page events on the Pager.
    /// </summary>
    public delegate void SelectedPageChangeEventHandler(object sender, CommandEventArgs e);

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:Pager runat=server></{0}:Pager>")]
    public class Pager : ScriptControl, IPostBackEventHandler, INamingContainer 
    {
        internal System.Collections.ArrayList _eventList = null;

        public Pager():base()
        {
            _eventList = new System.Collections.ArrayList();
        }

        #region Rendering and create child controls
        
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Table tPager = new Table();
            tPager.BorderStyle = BorderStyle.None;
            tPager.BorderWidth= new Unit(0, UnitType.Pixel);
            tPager.Width = new Unit(100, UnitType.Percentage);
            tPager.Height = new Unit(16, UnitType.Pixel);
            tPager.CellSpacing = 0;
            tPager.CellPadding = 4;
            Controls.Add(tPager);

            TableRow rPager = new TableRow();
            tPager.Rows.Add(rPager);
            
            TableCell LeftSpace = new TableCell();
            LeftSpace.Width = new Unit(16, UnitType.Pixel);
            rPager.Cells.Add(LeftSpace);

            TableCell PaginControlCell = new TableCell();
            PaginControlCell.Width = new Unit(60, UnitType.Percentage);
            PaginControlCell.VerticalAlign = VerticalAlign.Middle;
            PaginControlCell.HorizontalAlign = HorizontalAlign.Center;
            rPager.Cells.Add(PaginControlCell);

            TableCell TextCell1 = new TableCell();
            TextCell1.Width = new Unit(20, UnitType.Percentage);
            TextCell1.VerticalAlign = VerticalAlign.Middle;
            TextCell1.HorizontalAlign = HorizontalAlign.Center;
            rPager.Cells.Add(TextCell1);

            TableCell TextCell2 = new TableCell();
            TextCell2.Width = new Unit(20, UnitType.Percentage);
            TextCell2.VerticalAlign = VerticalAlign.Middle;
            TextCell2.HorizontalAlign = HorizontalAlign.Center;
            rPager.Cells.Add(TextCell2);

            string dText = SelectedPageText;
            if (SelectedPageText != "")
            {
                dText = dText.Replace("[#Page]", (PageNumber + 1).ToString());
                dText = dText.Replace("[#PageCount]", PageCount.ToString());
            }

            Label lblText = new Label();
            lblText.Text = dText;
            TextCell1.Controls.Add(lblText);

            dText = Text;
            if (Text != "")
            {
                dText = dText.Replace("[#Page]", (PageNumber + 1).ToString());
                dText = dText.Replace("[#PageCount]", PageCount.ToString());
            }
            lblText = new Label();
            lblText.Text = dText;
            TextCell2.Controls.Add(lblText);

            string href = "";
            

            Table tPagerControl = new Table();
            tPagerControl.CellPadding = 0;
            tPagerControl.CellSpacing = 0;
            tPagerControl.BorderStyle = BorderStyle.None;
            tPagerControl.BorderWidth = new Unit(0, UnitType.Pixel);
            tPagerControl.Width = new Unit(100, UnitType.Percentage);
            tPagerControl.Height = new Unit(14, UnitType.Pixel);
            PaginControlCell.Controls.Add(tPagerControl);

            TableRow rPagerControl = new TableRow();
            rPagerControl.Style[HtmlTextWriterStyle.VerticalAlign] = "top"; 
            tPagerControl.Rows.Add(rPagerControl);
            


            // Left buttons
            TableCell cFirstPageButton = new TableCell();
            cFirstPageButton.Width = new Unit(14, UnitType.Pixel);
            cFirstPageButton.Height = new Unit(14, UnitType.Pixel);
            rPagerControl.Cells.Add(cFirstPageButton);

            Image FirstPageButton = new Image();
            FirstPageButton.ImageUrl = "Images/GotoFirst.gif";
            if (this.Page != null)
            {
                href = "javascript:" + this.Page.ClientScript.GetPostBackEventReference(this, "onselectedpagechange,0");
            }
            FirstPageButton.Attributes["onclick"] = href;
            cFirstPageButton.Controls.Add(FirstPageButton);

            TableCell cPreviewsPageButton = new TableCell();
            cPreviewsPageButton.Width = new Unit(14, UnitType.Pixel);
            cPreviewsPageButton.Height = new Unit(14, UnitType.Pixel);
            rPagerControl.Cells.Add(cPreviewsPageButton);

            Image PreviewsPageButton = new Image();
            PreviewsPageButton.ImageUrl = "Images/GoPreviews.gif";
            if (this.Page != null)
            {
                href = "javascript:" + this.Page.ClientScript.GetPostBackEventReference(this, "onselectedpagechange," + ((PageNumber > 0)?PageNumber - 1:0).ToString());
            }
            PreviewsPageButton.Attributes["onclick"] = href;
            cPreviewsPageButton.Controls.Add(PreviewsPageButton);

            //slider
            TableCell cLeftSlider = new TableCell();
            cLeftSlider.Width = new Unit(14, UnitType.Pixel);
            cLeftSlider.Height = new Unit(14, UnitType.Pixel);
            cLeftSlider.Style[HtmlTextWriterStyle.BackgroundImage] = "Images/leftslider.gif";
            cLeftSlider.Style["background-repeat"] = "no-repeat";
            rPagerControl.Cells.Add(cLeftSlider);

            TableCell cMidSlider = new TableCell();
            cMidSlider.ID = "SliderBar";
            cMidSlider.Height = new Unit(14, UnitType.Pixel);
            cMidSlider.Style[HtmlTextWriterStyle.BackgroundImage] = "Images/midslider.gif";
            cMidSlider.Style["background-repeat"] = "repeat-x";
            cMidSlider.Style["width"] = "auto";
            cMidSlider.HorizontalAlign = HorizontalAlign.Left;
            cMidSlider.VerticalAlign = VerticalAlign.Middle;
            rPagerControl.Cells.Add(cMidSlider);

            Panel SliderButton = new Panel();
            SliderButton.ID = "SliderButton";
            SliderButton.Style[HtmlTextWriterStyle.BackgroundImage] = "Images/slider.gif";
            SliderButton.Style["background-repeat"] = "no-repeat";
            SliderButton.Style["position"] = "relative";
            SliderButton.Style["top"] = "0px";
            SliderButton.Style["left"] = "0px";
            SliderButton.Width = new Unit(19, UnitType.Pixel);
            SliderButton.Height = new Unit(14, UnitType.Pixel);
            
            cMidSlider.Controls.Add(SliderButton);

            TableCell cRightSlider = new TableCell();
            cRightSlider.Width = new Unit(14, UnitType.Pixel);
            cRightSlider.Height = new Unit(14, UnitType.Pixel);
            cRightSlider.Style[HtmlTextWriterStyle.BackgroundImage] = "Images/rigthslider.gif";
            cRightSlider.Style["background-repeat"] = "no-repeat";
            rPagerControl.Cells.Add(cRightSlider);


            // Right buttons
            TableCell cNextPageButton = new TableCell();
            cNextPageButton.Width = new Unit(14, UnitType.Pixel);
            cNextPageButton.Height = new Unit(14, UnitType.Pixel);
            rPagerControl.Cells.Add(cNextPageButton);

            Image NextPageButton = new Image();
            NextPageButton.ImageUrl = "Images/GoNext.gif";
            if (this.Page != null)
            {
                href = "javascript:" + this.Page.ClientScript.GetPostBackEventReference(this, "onselectedpagechange," + ((PageNumber < (PageCount-1)) ? PageNumber + 1 : (PageCount - 1)).ToString());
            }
            NextPageButton.Attributes["onclick"] = href;
            cNextPageButton.Controls.Add(NextPageButton);

            TableCell cLastPageButton = new TableCell();
            cLastPageButton.Width = new Unit(14, UnitType.Pixel);
            cLastPageButton.Height = new Unit(14, UnitType.Pixel);
            rPagerControl.Cells.Add(cLastPageButton);

            Image LastPageButton = new Image();
            LastPageButton.ImageUrl = "Images/GotoLast.gif";
            if (this.Page != null)
            {
                href = "javascript:" + this.Page.ClientScript.GetPostBackEventReference(this, "onselectedpagechange," + (PageCount - 1).ToString());
            }
            LastPageButton.Attributes["onclick"] = href;
            cLastPageButton.Controls.Add(LastPageButton);

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

           
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when the selected page changes.
        /// </summary>
        [Description("Event fired when the selected TreeNode changes.")]
        public event SelectedPageChangeEventHandler SelectedPageChange;

        /// <summary>
        /// Event handler for selection changes.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected internal virtual void OnSelectedPageChange(CommandEventArgs e)
        {
            if (SelectedPageChange != null)
                SelectedPageChange(this, e);
        }

        #endregion

        #region ScriptControl Abstract overrides

        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Axelerate.BusinessLayerUITools.WebControls.Pager", this.ClientID);

            descriptor.AddProperty("pagenumber",PageNumber);
            descriptor.AddProperty("pagecount", PageCount);
            descriptor.AddProperty("postbackevent", this.Page.ClientScript.GetPostBackEventReference(this, "onselectedpagechange,[SelectedPage]"));

            yield return descriptor;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("Axelerate.BusinessLayerUITools.WebControls.Pager.js", this.GetType().Assembly.FullName);
        }

        #endregion

        #region Properties

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("Page [#Page] of [#PageCount]")]
        [Localizable(true)]
        public string SelectedPageText
        {
            get
            {
                String s = (String)ViewState["SelectedPageText"];
                return ((s == null) ? "Page [#Page] of [#PageCount]" : s);
            }

            set
            {
                ViewState["SelectedPageText"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public int PageNumber
        {
            get
            {
                return ((ViewState["PageNumber"] == null) ? 0 : (int)ViewState["PageNumber"]);
            }

            set
            {
                ViewState["PageNumber"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public int PageCount
        {
            get
            {
                return ((ViewState["PageCount"] == null) ? 0 : (int)ViewState["PageCount"]);
            }

            set
            {
                ViewState["PageCount"] = value;
            }
        }

        #endregion


        #region Methods



        #endregion



        #region IPostBackEventHandler Members

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        /// <summary>
        /// Called when a downlevel browser submits the form
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        protected void RaisePostBackEvent(string eventArg)
        {
            ProcessEvents(eventArg);
            RaisePostDataChangedEvent();
        }

        /// <summary>
        /// Called when the TreeView on the client-side submitted the form.
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        protected bool ProcessEvents(string eventArg)
        {
            if (eventArg == null || eventArg == String.Empty || eventArg == " ") // Don't know why, but the framework is giving a " " eventArg instead of null
                return false;

            TreeNode tn = null;
            String[] events = eventArg.Split(new Char[] { ';' });
            foreach (string strWholeEvent in events)
            {
                String[] parms = strWholeEvent.Split(new Char[] { ',' });
                if (parms[0].Length > 0)
                {
                    if (parms[0].Equals("onselectedpagechange") && parms.GetLength(0) == 2)
                    {
                        CommandEventArgs e = new CommandEventArgs("selectedpagechange", int.Parse(parms[1]));

                        PageNumber = (int)e.CommandArgument;
                        _eventList.Add("p");
                        _eventList.Add(e);
                    }
                }
            }
            if (_eventList.Count > 0)
                return true;
            else
                return false;
        }
        
        

        /// <summary>
        /// Fires pending events from a postback
        /// </summary>
        protected void RaisePostDataChangedEvent()
        {
            for (int i = 0; i < _eventList.Count; i += 2)
            {
                String str = (String)_eventList[i];
                if (str == "p")
                    OnSelectedPageChange((CommandEventArgs)_eventList[i + 1]);
            }
            _eventList.Clear();
        }

        #endregion
        
        #region overrides

        

        #endregion
    }
}
