using System;
using System.Collections.Generic;
using System.Configuration;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.ComponentModel;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    /// <summary>
    /// Summary description for Checkbox
    /// </summary>
    public class Checkbox : ScriptControl
    {
        public enum CheckboxState
        {
            Checked = 1,
            Unchecked = 2,
            Undefine = 3
        }
        
        private Image _imgCheck;
        private Label _lblCheck;

        private HiddenField _State;
 
        public Checkbox()
        {
            
            //_State = CheckboxState.Undefine;
           
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            _imgCheck = new Image();
            _imgCheck.ID = "_imgCheck";
            _imgCheck.ImageUrl = "Images/undefine.gif";
            _lblCheck = new Label();
            _lblCheck.ID = "_lblCheck";
            _lblCheck.Text = "";
            _State = new HiddenField();
            _State.ID = "_State";
            _State.EnableViewState = true;
            _lblCheck.EnableViewState = true;
            _State.Value = CheckboxState.Undefine.ToString();
            Literal Sp = new Literal();
            Sp.Text = "&nbsp;";

            
            _imgCheck.Height = this.Height;
            _imgCheck.Width = _imgCheck.Height;

            Controls.Add(_State);
            //Controls.Add(_imgCheck);
            //Controls.Add(Sp);
            Table tb = new Table();
            tb.Width = new Unit("100%");
            tb.Height = new Unit("100%");
            tb.BorderWidth = new Unit("0px");
            tb.BorderStyle = BorderStyle.None;
            tb.CellPadding = 0;
            tb.CellSpacing = 0;
            TableRow tr = new TableRow();
            tb.Rows.Add(tr);

            TableCell tcCheckImg = new TableCell();
            tcCheckImg.VerticalAlign = VerticalAlign.Middle;
            tcCheckImg.Controls.Add(_imgCheck);
            tcCheckImg.Style.Add("width", "1px");
            tr.Cells.Add(tcCheckImg);

            TableCell tcLabel = new TableCell();
            tcLabel.HorizontalAlign = HorizontalAlign.Left;
            tcLabel.Controls.Add(Sp);
            tcLabel.Controls.Add(_lblCheck);
            tcLabel.VerticalAlign = VerticalAlign.Middle;
            tcLabel.Style.Add("width", "auto");
            tr.Cells.Add(tcLabel);


            Controls.Add(tb);
        }

        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            _imgCheck.Height = this.Height;
            _imgCheck.Width = _imgCheck.Height;
        }

        protected override IEnumerable<ScriptDescriptor>
                GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Axelerate.BusinessLayerUITools.WebControls.Checkbox", this.ClientID);
            //descriptor.AddProperty("state", State);
            descriptor.AddElementProperty("state", _State.ClientID);
            if (CheckedImageUrl == "")
            {
                descriptor.AddProperty("CheckedImageUrl", "Images/checked.gif");
            }
            else
            {
                descriptor.AddProperty("CheckedImageUrl", CheckedImageUrl);
            }
            if (UncheckedImageUrl == "")
            {
                descriptor.AddProperty("UncheckedImageUrl", "Images/unchecked.gif");
            }
            else
            {
                descriptor.AddProperty("UncheckedImageUrl", UncheckedImageUrl);
            }
            if (UndefineImageUrl == "")
            {
                descriptor.AddProperty("UndefineImageUrl", "Images/undefine.gif");
            }
            else
            {
                descriptor.AddProperty("UndefineImageUrl", UndefineImageUrl);
            }

            yield return descriptor;
        }

        // Generate the script reference
        protected override IEnumerable<ScriptReference>
                GetScriptReferences()
        {
            yield return new ScriptReference("Axelerate.BusinessLayerUITools.WebControls.Checkbox.js", this.GetType().Assembly.FullName);
        }

        [Bindable(true), Browsable(true), Category("Behavior"), DefaultValue(CheckboxState.Undefine)]
        public CheckboxState State
        {
            get
            {
                EnsureChildControls();
                if (_State.Value  == null)
                {
                   _State.Value  = CheckboxState.Undefine.ToString();
                }
                return (CheckboxState)Enum.Parse(typeof(CheckboxState), _State.Value);
                
            }
            set
            {
                EnsureChildControls();
                _State.Value = value.ToString();
                switch (value)
                {
                    case CheckboxState.Undefine:
                        if (CheckedImageUrl == "")
                        {
                            _imgCheck.ImageUrl = "Images/undefine.gif";
                        }
                        else
                        {
                            _imgCheck.ImageUrl = UndefineImageUrl;
                        }
                        break;
                    case CheckboxState.Checked:
                        if (CheckedImageUrl == "")
                        {
                            _imgCheck.ImageUrl = "Images/checked.gif";
                        }
                        else
                        {
                            _imgCheck.ImageUrl = CheckedImageUrl;
                        }
                        break;
                    case CheckboxState.Unchecked:
                        if (CheckedImageUrl == "")
                        {
                            _imgCheck.ImageUrl = "Images/unchecked.gif";
                        }
                        else
                        {
                            _imgCheck.ImageUrl = UncheckedImageUrl;
                        }
                        break;
                }
            }
        }
        
        [DefaultValue("")]
        public String Text
        {
            get
            {
                EnsureChildControls();
                return _lblCheck.Text;
            }
            set
            {
                EnsureChildControls();
                _lblCheck.Text = value;
            }
        }

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        public String CheckedImageUrl
        {
            get
            {
                if (ViewState["_CheckedImageUrl"] == null)
                {
                    return "";
                }
                else
                {
                    return (String)ViewState["_CheckedImageUrl"];
                }
            }
            set
            {
                ViewState["_CheckedImageUrl"] = value;
            }
        }

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        public String UncheckedImageUrl
        {
            get
            {
                if (ViewState["_UncheckedImageUrl"] == null)
                {
                    return "";
                }
                else
                {
                    return (String)ViewState["_UncheckedImageUrl"];
                }
            }
            set
            {
                ViewState["_UncheckedImageUrl"] = value;
            }
        }

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        public String UndefineImageUrl
        {
            get
            {
                if (ViewState["_UncheckedImageUrl"] == null)
                {
                    return "";
                }
                else
                {
                    return (String)ViewState["_UndefineImageUrl"];
                }
            }
            set
            {
                ViewState["_UndefineImageUrl"] = value;
            }
        }



    }
}