/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkSettings.cs
//
// Defines classes <c>SlkSettings</c>, <c>QueryDefinition</c>, <c>QuerySetDefinition</c>, and
// related types.
//
// See "SLK Settings Schema.doc" for more information.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    sealed class QuerySetLocalization
    {
        public static string LocalizedValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            try
            {
                if (value.Length > 11 && value.Substring(0, 11) == "$Resources:")
                {
                    SlkCulture culture;
                    string cultureValue = System.Web.HttpContext.Current.Request.QueryString["culture"];
                    if (string.IsNullOrEmpty(cultureValue))
                    {
                        culture = new SlkCulture();
                    }
                    else
                    {
                        culture = new SlkCulture(new CultureInfo(cultureValue));
                    }

                    if (value.Length > 18 && value.Substring(0, 18) == "$Resources:SlkDll,")
                    {
                        // Resource string from Slk dll resource
                        string key = value.Substring(18);
                        return culture.Resources.ResourceManager.GetString(key, culture.Culture);
                    }
                    else
                    {
                        // Generic resource string
                        int index = value.IndexOf(",");
                        if (index > -1)
                        {
                            string source = value.Substring(11, index - 11);
                            string key = value.Substring(index + 1);
                            return Microsoft.SharePoint.Utilities.SPUtility.GetLocalizedString("$Resources:" + key, source, (uint)culture.Culture.LCID);
                        }
                    }
                }

                // Not a valid resource string
                return value;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }

}

