using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    public class TwoStateImageButton : System.Web.UI.WebControls.ImageButton
    {
        #region public properties
        public bool IsPressed
        {
            get
            {
                object value = ViewState["IsPressed"];
                if (value == null)
                {
                    ViewState["IsPressed"] = false;
                    return false;
                }
                else
                {
                    return (bool)value;
                }
            }
            set
            {
                ViewState["IsPressed"] = value;
            }
        }
        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [Description("Use special rendering for add the text to asociate images.")]
        public bool UseImageButtonComposer
        {
            get
            {
                object value = ViewState["UseImageButtonCmp"];
                if (value == null)
                {
                    return true;
                }
                else
                {
                    return (bool)value;
                }
            }
            set
            {
                ViewState["UseImageButtonCmp"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(8)]
        [Description("Used width special rendering for set the size of the text associate.")]
        public int FontSize
        {
            get
            {
                object value = ViewState["FontSize"];
                if (value == null)
                {
                    if (this.Style["font-size"] != null)
                    {
                        System.Web.UI.WebControls.Unit FSize = new System.Web.UI.WebControls.Unit(this.Style["font-size"]);
                        return (int)FSize.Value;
                    }
                    else
                    {
                        return 8;
                    }
                }
                else
                {
                    return (int)value;
                }
            }
            set
            {
                ViewState["FontSize"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(false)]
        [Description("Used width special rendering for put a shadow on the text asociate.")]
        public bool TextDropShadow
        {
            get
            {
                object value = ViewState["TextDropShadow"];
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
                ViewState["TextDropShadow"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(false)]
        [Description("Used width special rendering for set to bold the text asociate.")]
        public bool TextBold
        {
            get
            {
                object value = ViewState["TextBold"];
                if (value == null)
                {
                    if (this.Style["font-weight"] != null)
                    {
                        if (this.Style["font-weight"].ToUpper() == "BOLD")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return (bool)value;
                }
            }
            set
            {
                ViewState["TextBold"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(false)]
        [Description("Used width special rendering for set font to render the text asociate.")]
        public string FontFamilly
        {
            get
            {
                object value = ViewState["FontFamilly"];
                if (value == null)
                {
                    if (this.Style["font-family"] != null)
                    {
                        return this.Style["font-family"];
                    }
                    else
                    {
                        return "Arial";
                    }
                }
                else
                {
                    return (string)value;
                }
            }
            set
            {
                ViewState["FontFamilly"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(typeof(System.Drawing.Color), "Black")]
        [Description("Used width special rendering for set the color of the font used to render the text asociate.")]
        public System.Drawing.Color FontColor
        {
            get
            {
                object value = ViewState["FontColor"];
                if (value == null)
                {
                    if (this.Style["color"] != null)
                    {
                        return System.Drawing.ColorTranslator.FromHtml(this.Style["color"]);
                    }
                    else
                    {
                        return System.Drawing.Color.Black;
                    }
                }
                else
                {
                    return (System.Drawing.Color)value;
                }
            }
            set
            {
                ViewState["FontColor"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(typeof(System.Drawing.Color), "Black")]
        [Description("Used width special rendering for set the color of the font used to render the text asociate, when the button is pressed.")]
        public System.Drawing.Color PressedFontColor
        {
            get
            {
                object value = ViewState["PressedFontColor"];
                if (value == null)
                {
                    return FontColor;
                }
                else
                {
                    return (System.Drawing.Color)value;
                }
            }
            set
            {
                ViewState["PressedFontColor"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [Description("Set path to on pressed image file.")]
        [Editor(typeof(System.Web.UI.Design.UrlEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        public string PressedImageUrl
        {
            get
            {
                object value = ViewState["PressedImg"];
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)value;
                }
            }
            set
            {
                ViewState["PressedImg"] = value;
            }
        }


        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(typeof(System.Drawing.Color), "Black")]
        [Description("Used width special rendering for set the color of the font used to render the text asociate, when the button roll over.")]
        public System.Drawing.Color MouseOverFontColor
        {
            get
            {
                object value = ViewState["MouseOverFontColor"];
                if (value == null)
                {
                    return FontColor;
                }
                else
                {
                    return (System.Drawing.Color)value;
                }
            }
            set
            {
                ViewState["MouseOverFontColor"] = value;
            }
        }
        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue(typeof(System.Drawing.Color), "Gray")]
        [Description("Used width special rendering for set the color of the font used to render the text asociate, when the button roll over.")]
        public System.Drawing.Color DisableFontColor
        {
            get
            {
                object value = ViewState["DisableFontColor"];
                if (value == null)
                {
                    return System.Drawing.Color.Gray;
                }
                else
                {
                    return (System.Drawing.Color)value;
                }
            }
            set
            {
                ViewState["DisableFontColor"] = value;
            }
        }
        #endregion

        #region Image Properties
        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [Description("Set path to mouse over image file.")]
        [Editor(typeof(System.Web.UI.Design.UrlEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        public string MouseOverImageUrl
        {
            get
            {
                object value = ViewState["MouseOverImg"];
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)value;
                }
            }
            set
            {
                ViewState["MouseOverImg"] = value;
            }
        }
        
        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue("Arial")]
        [Description("Set text of the button, to apply if UseImageButtonComposer is true, else this property are ignored")]
        [Editor(typeof(System.Web.UI.Design.UrlEditor),
         typeof(System.Drawing.Design.UITypeEditor))]
        public string MouseOutImageUrl
        {
            get
            {
                object value = ViewState["MouseOutImg"];
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)value;
                }
            }
            set
            {
                ViewState["MouseOutImg"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [Description("Set path to mouse over image file for a pressed button.")]
        [Editor(typeof(System.Web.UI.Design.UrlEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        public string PressedMouseOverImageUrl
        {
            get
            {
                object value = ViewState["PressedMouseOverImg"];
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)value;
                }
            }
            set
            {
                ViewState["PressedMouseOverImg"] = value;
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue("")]
        [Description("Set the Image that should be displayed when the button status is pressed")]
        [Editor(typeof(System.Web.UI.Design.UrlEditor),
         typeof(System.Drawing.Design.UITypeEditor))]
        public string PressedMouseOutImageUrl
        {
            get
            {
                object value = ViewState["PressedMouseOutImg"];
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)value;
                }
            }
            set
            {
                ViewState["PressedMouseOutImg"] = value;
            }
        }
        #endregion

        #region overrides

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (MouseOutImageUrl != "")
            {
                this.ImageUrl = FormatImageUrl((IsPressed) ? this.PressedMouseOutImageUrl : this.MouseOutImageUrl);
            }
            else
            {
                this.MouseOutImageUrl = this.ImageUrl;
                this.PressedMouseOutImageUrl = this.PressedImageUrl;

                if (UseImageButtonComposer)
                {
                    this.ImageUrl = FormatImageUrl((IsPressed) ? this.PressedMouseOutImageUrl : this.MouseOutImageUrl);
                }
            }
            if (MouseOverImageUrl != "")
            {
                this.Attributes.Add("onmouseover", "this.src='" + FormatImageUrl((IsPressed) ? PressedMouseOverImageUrl : MouseOverImageUrl) + "';");
            }
            if (MouseOutImageUrl != "")
            {
                this.Attributes.Add("onmouseout", "this.src='" + FormatImageUrl((IsPressed) ? PressedMouseOutImageUrl : MouseOutImageUrl) + "';");
            }
            if (PressedImageUrl != "")
            {
                this.Attributes.Add("onmousedown", "this.src='" + FormatImageUrl(PressedImageUrl) + "';");
            }
            if (MouseOutImageUrl != "")
            {
                this.Attributes.Add("onmouseup", "this.src='" + FormatImageUrl((IsPressed) ? PressedMouseOverImageUrl : MouseOverImageUrl) + "';");
            }
        }
        protected string FormatImageUrl(string ImgUrl)
        {
            string ImgComposerURL = "";
            try
            {
                object value = Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties.clsConfigurationProfile.Current.getPropertyValue("ImageComposerUrl");
                if (value != null)
                {
                    ImgComposerURL = this.ResolveClientUrl((string)value);
                }
                else
                {
                    ImgComposerURL = "";
                }
            }
            catch
            {
                ImgComposerURL = "";
            }
            if (ImgComposerURL == null || ImgComposerURL == "")
            {
                ImgComposerURL = this.ResolveClientUrl("~/ImgCmp.asmx");
            }
            if (UseImageButtonComposer)
            {
                if (this.Enabled)
                {
                    return string.Format(ImgComposerURL + "?btWidth={0}&btHeight={1}&Text={2}&ImagePath={3}&FontSize={4}&FontFamilyName={5}&DropShadow={6}&Bold={7}&TextColor={8}&Enabled={9}", this.Width.Value.ToString(), this.Height.Value.ToString(), this.AlternateText.Replace("'", "\'"), this.ResolveClientUrl(ImgUrl).Replace("&", "$and;"), FontSize, FontFamilly, TextDropShadow, TextBold, System.Drawing.ColorTranslator.ToHtml(FontColor), this.Enabled);
                }
                else
                {
                    return string.Format(ImgComposerURL + "?btWidth={0}&btHeight={1}&Text={2}&ImagePath={3}&FontSize={4}&FontFamilyName={5}&DropShadow={6}&Bold={7}&TextColor={8}&Enabled={9}", this.Width.Value.ToString(), this.Height.Value.ToString(), this.AlternateText.Replace("'", "\'"), this.ResolveClientUrl(ImgUrl).Replace("&", "$and;"), FontSize, FontFamilly, TextDropShadow, TextBold, System.Drawing.ColorTranslator.ToHtml(DisableFontColor), this.Enabled);
                }
            }
            else
            {
                return this.ResolveClientUrl(ImgUrl);
            }
        }
        protected override void OnClick(System.Web.UI.ImageClickEventArgs e)
        {
            this.IsPressed = !IsPressed;
            base.OnClick(e);
        }
        #endregion        
    }
}

