/* Copyright (c) Microsoft Corporation. All rights reserved. */

// AssignmentListWebPart.cs
//
// Implements the SLK Assignment List Web Part.
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Security;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System.Xml.Serialization;
using Microsoft.SharePoint.WebPartPages.Communication;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit.ApplicationPages;
using Microsoft.SharePointLearningKit.WebControls;
using Microsoft.SharePointLearningKit;
using Resources.Properties;



namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>
    /// Error Banner For ALWP. 
    /// </summary>
    public static class ErrorBanner
    {
        #region RenderImage
        /// <summary>
        /// Render Error Message Image Tag
        /// </summary>
        /// <param name="errorType">ErrorType decides Image Src Attribute</param> 
        private static Image RenderImage(ErrorType errorType)
        {
            //Controls to Render Error
            Image imgErrorType = new Image();
            SlkCulture culture = new SlkCulture();

            switch (errorType)
            {
                case ErrorType.Error:
                    {
                        //Error Image Tag
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.ErrorIcon;
                        imgErrorType.ToolTip = culture.Resources.SlkErrorTypeErrorToolTip;
                        break;
                    }
                case ErrorType.Info:
                    {
                        //Info Image Tag
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.InfoIcon;
                        imgErrorType.ToolTip = culture.Resources.SlkErrorTypeInfoToolTip;
                        break;
                    }
                case ErrorType.Warning:
                    {
                        //ErrorType Image Tag
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.WarningIcon;
                        imgErrorType.ToolTip = culture.Resources.SlkErrorTypeWarningToolTip;
                        break;
                    }

                default:
                    {
                        //Error Image Tag                       
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.ErrorIcon;
                        imgErrorType.ToolTip = culture.Resources.SlkErrorTypeErrorToolTip;
                        break;
                    }
            }

            return imgErrorType;
        }
        #endregion

        #region RenderErrorItems
        /// <summary>
        /// Render Error Message Literal Controls 
        /// Error Type and Error Text
        /// </summary>
        /// <param name="htmlTextWriter">HtmlTextWriter to Add the Items</param> 
        /// <param name="slkError">Error Items</param> 
        internal static void RenderErrorItems(HtmlTextWriter htmlTextWriter, SlkError slkError)
        {
            //Controls to Render Error

            Literal lcErrorText = new Literal();

            lcErrorText.ID = "lcErrorText";

            lcErrorText.Text = slkError.ErrorText;
           
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            using (new HtmlBlock(HtmlTextWriterTag.Div, 1, htmlTextWriter))
            {
                // render the "<table>" element and its contents

                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-summarycustombody");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 2px 4px 4px;");
                htmlTextWriter.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0px");

                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                    {
                        if (!(slkError.ErrorType == ErrorType.Info))
                        {
                            //Add Attributes for the <TD> tag
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 22px;");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                            {
                                Image imgError = RenderImage(slkError.ErrorType);
                                imgError.RenderControl(htmlTextWriter);
                            }
                        }

                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        {
                            lcErrorText.RenderControl(htmlTextWriter);
                        }

                    }
                }
            }
        }

        #endregion

        #region WriteException
        /// <summary>
        /// Checks for deadlock and writes the SqlExeception to the event Log and outs the SlkError Object. 
        /// </summary>
        /// <param name="store">The ISlkStore to log exceptions.</param>
        /// <param name="sqlEx">SqlException</param>
        /// <returns></returns>
        internal static SlkError WriteException(ISlkStore store, SqlException sqlEx)
        {
            //Set the Standard Error text 
            SlkCulture culture = new SlkCulture();
            string errorText = culture.Resources.SlkGenericError;
            
            //check whether deadlock occured
            if (sqlEx.Number == 1205)
            {
                errorText = culture.Resources.SlkExAlwpSqlDeadLockError;
            }

            //Slk Error with Generic or dead lock error message.
            errorText = Constants.Space + SlkUtilities.GetHtmlEncodedText(errorText);

            //log the exception
            store.LogException(sqlEx);

            //Add the Error to Error Collection.
            return new SlkError(ErrorType.Error, errorText);
        }
        #endregion
    }
    /// <summary>
    /// Defines the string value to use as a ToolTip for a property of a ALWP. 
    /// This allows the descriptions in ALWP's tool pane to be localized string resources.
    /// </summary>
    internal sealed class AlwpWebDescriptionAttribute : WebDescriptionAttribute
    {
        private bool loadedResource;

        public AlwpWebDescriptionAttribute(string resourceId)
            : base(resourceId)
        {
        }
        /// <summary>
        /// Gets the Description to display as a ToolTip
        /// </summary>
        public override string Description
        {
            get
            {
                if (!loadedResource)
                {
                    SlkCulture culture = new SlkCulture();
                    DescriptionValue = culture.Resources.ResourceManager.GetString(base.Description); 
                    loadedResource = true;
                }
                return base.Description;
            }
        }
    }

    /// <summary>
    /// Defines the friendly name for a property of a ALWP.
    /// This allows the labels in ALWP's tool pane to be localized string resources.
    /// </summary>
    internal sealed class AlwpWebDisplayNameAttribute : WebDisplayNameAttribute
    {
        private bool loadedResource;

        public AlwpWebDisplayNameAttribute(string resourceId)
            : base(resourceId)
        {
        }
        /// <summary>
        /// Gets the name of a property to display 
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (!loadedResource)
                {
                    SlkCulture culture = new SlkCulture();
                    DisplayNameValue = culture.Resources.ResourceManager.GetString(base.DisplayName);
                    loadedResource = true;
                }
                return base.DisplayName;
            }
        }
    }

    /// <summary>
    /// Defines the friendly name for a property of a SLK web part.
    /// This allows the labels in web part's tool pane to be localized string resources.
    /// </summary>
    internal sealed class SlkCategoryAttribute : CategoryAttribute
    {
        /// <summary>Initializes a new instance of <see cref="SlkCategoryAttribute"/>.</summary>
        public SlkCategoryAttribute() : base ("WebPartCategory")
        {
        }

        /// <summary>Initializes a new instance of <see cref="SlkCategoryAttribute"/>.</summary>
        public SlkCategoryAttribute(string category) : base (category)
        {
        }

        /// <summary>Gets the name of a property to display.</summary>
        protected override string GetLocalizedString(string value)
        {
            SlkCulture culture = new SlkCulture();
            return culture.Resources.ResourceManager.GetString(value);
        }
    }
}
