/* Copyright (c) Microsoft Corporation. All rights reserved. */

//ErrorBanner.cs
//
//Implementation of ServerControl Classes ErrorBanner and ErrorTemplates
//
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design;
using Resources.Properties;
using Microsoft.LearningComponents;
using Microsoft.SharePointLearningKit;

namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  ErrorBanner Control used to display All Types of Error Messages in the Page
    /// </summary>
    /// <remarks>
    ///  usage: &lt;{0}:ErrorBanner id= "errorBanner" runat="server"&gt;&lt;/{0}:ErrorBanner&gt;
    /// </remarks>
    [DesignerAttribute(typeof(ControlDesigner)),
    ToolboxData("<{0}:ErrorBanner runat=\"server\"></{0}:ErrorBanner>")]
    public class ErrorBanner : WebControl, INamingContainer
    {
        #region Private Variables
        /// <summary>
        ///  Collection of SlkError Items
        /// </summary>
        private List<SlkError> m_slkErrorCollection;
        #endregion

        #region Private and Protected Methods

        #region Constructor
        /// <summary>
        /// Default Constructor for ErrorBanner 
        /// </summary> 
        public ErrorBanner()
        {
            //Initialize the Error Collection
            this.m_slkErrorCollection = new List<SlkError>();

            //Hide the Error Banner By default.
            this.Visible = false;
        }
        #endregion

        #region LoadViewState
        /// <summary>
        /// Loads View State from saved State
        /// </summary>
        /// <param name="savedState">savedState</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Pair p = (Pair)savedState;

                //get the Error Items in the Collection.
                ErrorType[] arrErrorType = (ErrorType[])p.First;
                string[] arrErrorText = (string[])p.Second;

                //Add the Error Items To the banner.
                for (int i = 0; i < arrErrorType.Length; i++)
                {
                    AddError(new SlkError(arrErrorType[i], arrErrorText[i]));
                }

            }  
        }
        #endregion

        #region AddHtmlErrorText
        /// <summary>
        /// Adds the SlkError to the Error Collection 
        /// and makes the Error Banner visible in the Page. 
        /// ErrorText Will not be HtmlEncoded.
        /// </summary>
        /// <param name="errorType">ErrorType</param> 
        /// <param name="errorText">
        ///     Html Error Text to be displayed.
        /// </param> 
        internal void AddHtmlErrorText(ErrorType errorType, String errorText)
        {
            //adds the Error to the Error Collection
            AddError(new SlkError(errorType, errorText));
        }
        #endregion

        #region AddError
        /// <summary>
        /// Adds the SlkError to the Error Collection 
        /// and makes the Error Banner visible in the Page.                 
        /// </summary> 
        /// <param name="slkError">Error Item to be displayed</param> 
        internal void AddError(SlkError slkError)
        {
            //adds the Error to the Error Collection
            m_slkErrorCollection.Add(slkError);
            //Show the Error Banner 
            this.Visible = true;
        }
        #endregion

        #region AddError
        /// <summary>
        /// Adds the SlkError to the Error Collection 
        /// and makes the Error Banner visible in the Page.
        /// ErrorText Will be HtmlEncoded.
        /// </summary>
        /// <param name="errorType">ErrorType</param> 
        /// <param name="errorText">Plain Error Text to be displayed</param> 
        internal void AddError(ErrorType errorType, String errorText)
        {
            //adds the Error to the Error Collection
           AddHtmlErrorText(errorType,
                            SlkUtilities.GetHtmlEncodedText(errorText));
        }
        #endregion

        #region Clear
        /// <summary>
        /// Removes all the SlkError in the Error Collection 
        /// and makes the Error Banner invisible in the Page.
        /// </summary>      
        public void Clear()
        {
            //Removes the Errors in the Error Collection
            this.m_slkErrorCollection.Clear();
            //Hide the Error Banner By default.
            this.Visible = false;
            
        }
        #endregion

        #region AddException
        /// <summary>
        /// Adds the Exeception to the Error Collection and log        
        /// the Exeception details in event Log
		/// The Exception message will be HtmlEncoded.
		/// </summary>
        /// <param name="ex">Exception</param> 
        public void AddException(Exception ex)
        {
            //adds the Error to the Error Collection
           
            SlkError slkError;
            
            //Call the WriteException to Add the Error Message 
            //to collection and log the exception in EventLog as needed.                    
            SlkError.WriteException(ex, out slkError);

            //Add the Error object to collection
            AddError(slkError);

            // if <ex> contains validation results, add them to the error banner
            SafeToDisplayException safeEx = ex as SafeToDisplayException;
            if ((safeEx != null) && (safeEx.ValidationResults != null) &&
                (safeEx.ValidationResults.HasErrors || safeEx.ValidationResults.HasErrors))
            {
                foreach (ValidationResult result in safeEx.ValidationResults.Results)
                {
                    if (result.IsWarning)
                        AddError(ErrorType.Warning, result.Message);
                    else
                        if (result.IsError)
                            AddError(ErrorType.Error, result.Message);
                }
            }
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the Error Item Collection
        /// </summary>
        /// <param name="writer">htmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (m_slkErrorCollection != null && m_slkErrorCollection.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-formbody");
                writer.AddAttribute(HtmlTextWriterAttribute.Style,
                                            "border: black 1px solid;" + 
                                            "padding-left: 4px; padding-right: 2px;");
                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, writer))
                {
                    foreach (SlkError item in m_slkErrorCollection)
                    {
                        //Renders Formatted Error Items.
                        RenderErrorItem(item, writer);
                    }
                }

            }
        }
        #endregion

        #region RenderErrorItem
        /// <summary>
        /// Render the Error Items
        /// </summary>       
        /// <param name="item">SlkError Item</param>
        /// <param name="htmlTextWriter">htmlTextWriter</param>
        private static void RenderErrorItem(SlkError item, HtmlTextWriter htmlTextWriter)
        {
            //Controls to Render Error
            Image imgErrorType = new Image();
            Literal lcErrorText = new Literal();
            imgErrorType.ID = "imgErrorType";
            lcErrorText.ID = "lcErrorText";

            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
            {
                //Add Attributes for the <TD> tag 
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 22px;");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                {
                    switch (item.ErrorType)
                    {
                        case ErrorType.Error:
                            {
                                //Error Image Tag
                                imgErrorType.ImageUrl = Constants.ImagePath + Constants.ErrorIcon;
                                imgErrorType.ToolTip = AppResources.SlkErrorTypeErrorToolTip;
                                break;
                            }
                        case ErrorType.Info:
                            {
                                //Info Image Tag
                                imgErrorType.ImageUrl = Constants.ImagePath + Constants.InfoIcon;
                                imgErrorType.ToolTip = AppResources.SlkErrorTypeInfoToolTip;
                                break;
                            }
                        case ErrorType.Warning:
                            {
                                //ErrorType Image Tag
                                imgErrorType.ImageUrl = Constants.ImagePath + Constants.WarningIcon;
                                imgErrorType.ToolTip = AppResources.SlkErrorTypeWarningToolTip;
                                break;
                            }
                        default:
                            {
                                //Error Image Tag                       
                                imgErrorType.ImageUrl = Constants.ImagePath + Constants.ErrorIcon;
                                imgErrorType.ToolTip = AppResources.SlkErrorTypeErrorToolTip;
                                break;
                            }
                    }
                    
                    imgErrorType.RenderControl(htmlTextWriter);
                }

                //Render Error Text
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-formvalidation");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                {
                    lcErrorText.Text = item.ErrorText;
                    lcErrorText.RenderControl(htmlTextWriter);
                }
            }
        }
        #endregion

        #region SaveViewState
        /// <summary>
        /// Saves the ViewState and returns the saved object
        /// </summary>
        /// <returns>savedObject</returns>
        protected override object SaveViewState()
        {
            if (m_slkErrorCollection != null && 
                m_slkErrorCollection.Count > 0)
            {
                //Saves the ErrorCollection in Control State
                int numOfItems = m_slkErrorCollection.Count;
                string[] arrErrorText = new string[numOfItems];
                ErrorType[] arrErrorType = new ErrorType[numOfItems];
                int count =0;
                foreach (SlkError item in m_slkErrorCollection)
                {
                    arrErrorText[count] = item.ErrorText;
                    arrErrorType[count] = item.ErrorType;
                    count++;
                }
                return new Pair(arrErrorType,arrErrorText);
            }
            return null;
        }
        #endregion

        #endregion
    }
}
