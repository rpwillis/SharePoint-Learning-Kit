using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Xml.Serialization;

using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using System.Reflection;


namespace Axelerate.BusinessLayerUITools.Common
{
    public class clsSharedMethods
    {
        #region Constants
        public string imageLocation = "Images.asmx?"; //Location Url when value is a Bitmap type.
        public string strObjNotFound = Resources.ErrorMessages.errObjectNotFound;
        #endregion

        #region Constructors
        public clsSharedMethods()
        {
        }
        #endregion

        #region Properties
        private static clsSharedMethods m_clsSharedMethods;
        public static clsSharedMethods SharedMethods
        {
            get
            {
                if (m_clsSharedMethods == null)
                {
                    m_clsSharedMethods = new clsSharedMethods();
                    return m_clsSharedMethods;
                }
                else return m_clsSharedMethods;
            }
        }
        #endregion

        #region Methods


        public string getStringFromResources(object BObject, string rawString)
        {
            const string strResourceRegularExpression = @"[%]([^,]+),([^%]+)[%]";
            Regex regExPattern = new Regex(strResourceRegularExpression);
            Match matPattern = regExPattern.Match(rawString);

            while (matPattern.Success)
            {
                if (matPattern.Value != "")
                {
                    string matchPatternContent = matPattern.Value.Substring(1, matPattern.Value.Length - 2);
                    string[] ResourceFileAndName = matchPatternContent.Split(new char[] { ',' });
                    if (ResourceFileAndName.Length == 2)
                    {

                        string valuetoReplace = matPattern.Value;
                        string newValue = "";

                        string ResourceType = ResourceFileAndName[0];

                        System.Type objType = BObject.GetType();
                        System.Resources.ResourceManager rm = new global::System.Resources.ResourceManager(ResourceType, objType.Assembly);

                        try
                        {
                            newValue = rm.GetString(ResourceFileAndName[1]);
                        }
                        catch (Exception e)
                        {
                            newValue = "";
                        }
                        rawString = rawString.Replace(valuetoReplace, newValue);
                    }
                    matPattern = matPattern.NextMatch();
                }
            }
            return rawString;
        }

        /// <summary> 
        /// This method parses the name of the BO property that comes
        /// enclosed by "{" and "}" and changes it for the value of the
        /// property of the business object
        /// </summary>
        /// <param name="bObject">BusinessObject from which the elements to be parsed on the rawString will be taken.</param>
        /// <param name="rawString">String to be parsed</param>
        /// <returns>string with the values parsed</returns>
        public string parseBOPropertiesString(BLBusinessBase bObject, string rawString, string ClassName, string KeyField)
        {
            const string strRegularExpression = "[\\{](\\w+)(\\.(\\w+))*(\\(\\))?(,[']([^']+)['])?[\\}]";
            Regex regExPattern = new Regex(strRegularExpression);
            Match matPattern = regExPattern.Match(rawString);

            try
            {
                BLBusinessBase item = bObject;
                if (item != null)
                {
                    while (matPattern.Success)
                    {
                        if (matPattern.Value != "")
                        {
                            string valuetoReplace = matPattern.Value;
                            string newValue = "";

                            if (valuetoReplace.Contains(","))
                            {
                                string[] valueAndFormat = valuetoReplace.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (valueAndFormat.Length == 2) //This code removes the "{" and "}" from the object and the format string and applies the format to the object.
                                {
                                    Object objProperty = null;
                                    string valueString = valueAndFormat[0].Remove(0, 1);

                                    if (IsFloatLiteral(valueString))
                                    {
                                        objProperty = System.Convert.ToDouble(valueString);
                                    }
                                    else if (IsMethod(valueString))
                                    {
                                        objProperty = getMethodProperty(bObject, valueString.Remove(valueString.Length - 2)); //Removes the "()" from the name of the method
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (clsSharedMethods.SharedMethods.CheckAccess(item, "READ", valueString))
                                            {
                                                objProperty = DataBinder.Eval(item, valueString);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            objProperty = null;
                                        }
                                    }

                                    if (objProperty != null)
                                    {
                                        string format = valueAndFormat[1].Remove(valueAndFormat[1].Length - 1);
                                        if (objProperty.GetType() == typeof(double) || objProperty.GetType() == typeof(float))
                                        {
                                            if (format.Contains("type:Progressbar"))
                                            {
                                                Image bmp = getImageProperty(valuetoReplace, true, ClassName, KeyField);
                                                newValue = "<img src=\"" + bmp.ImageUrl + "\" style=\"" + bmp.Style.Value + "\" />";
                                            }
                                            else
                                            {
                                                newValue = getPropertyValueToString(objProperty, format);
                                            }
                                        }
                                        else if (objProperty.GetType() == typeof(Image) || objProperty.GetType() == typeof(System.Drawing.Bitmap))
                                        {
                                            Image bmp = getImageProperty(bObject, valuetoReplace, false);
                                            newValue = "<img src=\"" + bmp.ImageUrl + "\" style=\"" + bmp.Style.Value + "\" />";
                                        }
                                    }
                                    else
                                    {
                                        newValue = "";
                                    }
                                }
                                else
                                {
                                    newValue = matPattern.Value;
                                }
                            }
                            else
                            {
                                Object objProperty = null;
                                string matchPatternContent = matPattern.Value.Substring(1, matPattern.Value.Length - 2);
                                if (IsMethod(matchPatternContent))
                                {
                                    objProperty = getMethodProperty(bObject, matchPatternContent.Remove(matchPatternContent.Length - 2)); //Removes the "()" from the name of the method
                                }
                                else
                                {
                                    try
                                    {
                                        if (clsSharedMethods.SharedMethods.CheckAccess(item, "READ", matchPatternContent))
                                        {
                                            objProperty = DataBinder.Eval(item, matchPatternContent);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        objProperty = null;
                                    }
                                }
                                if (objProperty != null)
                                {
                                    if (objProperty.GetType() == typeof(Image) || objProperty.GetType() == typeof(System.Drawing.Bitmap))
                                    {
                                        Image bmp = getImageProperty(bObject, valuetoReplace, false);
                                        newValue = "<img src=\"" + bmp.ImageUrl + "\" style=\"" + bmp.Style.Value + "\" />";
                                    }
                                    else
                                    {
                                        newValue = getPropertyValueToString(objProperty, "");
                                    }
                                }
                                else
                                {
                                    newValue = "";
                                }
                            }
                            rawString = rawString.Replace(valuetoReplace, newValue);
                            matPattern = matPattern.NextMatch();
                        }
                    }
                    return rawString;
                }
                else
                {
                    return matPattern.Value;
                }
            }
            catch (Exception exc)
            {
                return Axelerate.BusinessLayerUITools.Resources.GenericWebParts.strErrorParsingParameters;
            }
        }

        /// <summary>
        /// Converts the property value to a string with a format if it is provided.
        /// </summary>
        /// <param name="objProperty">Property that will be converted to string</param>
        /// <param name="strFormat">An optional format string that will be used to format the property</param>
        /// <returns>The property as a string</returns>
        public string getPropertyValueToString(object objProperty, string strFormat)
        {
            const int presicionDecimals = 2; //Number of decimals used to round a double when showing a double type value.
            const string dateSeparator = "/";

            Type propertyType = objProperty.GetType();

            if (propertyType.Equals(typeof(string)))
            {
                return (string)objProperty;
            }
            else if (strFormat != "") //The object is an int, double, decimal or a date and has a format, then use the Format property.
            {
                return string.Format(strFormat, objProperty);
            }
            else if (propertyType.Equals(typeof(int))) //The object is one of the following types without a specified format.
            {
                return objProperty.ToString();
            }
            else if (propertyType.Equals(typeof(double)))
            {
                double newValue = (double)objProperty;
                double roundedValue = System.Math.Round(newValue, presicionDecimals, MidpointRounding.AwayFromZero);
                return roundedValue.ToString();
            }
            else if (propertyType.Equals(typeof(float)))
            {
                float newValue = (float)objProperty;
                return newValue.ToString();
            }
            else if (propertyType.Equals(typeof(DateTime)))
            {
                DateTime date = (DateTime)objProperty;
                return date.ToShortDateString();
            }
            /* TODO: Csla no longer supported
            else if (propertyType.Equals(typeof(SmartDate)))
            {
                SmartDate date = (SmartDate)objProperty;
                return date.Date.ToShortDateString();
            }
            */
            else if (propertyType.Equals(typeof(Image)) || propertyType.Equals(typeof(System.Drawing.Bitmap)))
            {
                Image img = (Image)objProperty;
                return "<img src=\"" + img.ImageUrl + "/>";
            }
            else if (propertyType.Equals(typeof(bool)) || propertyType.Equals(typeof(Boolean)))
            {
                return ((bool)objProperty) ? "Yes" : "No";
            }
            else
            {
                return objProperty.ToString();
            }
        }

        /// <summary>
        /// Returns an Image control from a string object
        /// </summary>
        /// <param name="content">String {property} (default style) or {property,"style"} used to create the image</param>
        /// <param name="progressbar">Boolean used to determine if the image is a generated progress bar</param>
        /// <returns>An Image Object with the ImageUrl and Style set</returns>
        /// <example>getImageProperty("{Contact.Picture,"width:10"}",false) will return the Contact's picture</example>
        /// <example>getImageProperty("{Contact.Progress,"width:10"}",true) will return a progress bar Image with the current contact's progress</example>
        public Image getImageProperty(string content, bool progressbar, string ClassName, string KeyFieldValue)
        {
            //Remove the "{" and "}" characters
            content = content.Substring(1, content.Length - 2);

            string[] propertyAndstyle = content.Split(new char[] { ',' });

            Image img = new Image();
            string encodedClassName = System.Web.HttpContext.Current.Server.UrlEncode(ClassName);

            string property = "";
            string style = "";
            property = propertyAndstyle[0];

            if (propertyAndstyle.Length > 1)
            {
                style = propertyAndstyle[1];
            }
            if (progressbar)
            {
                //StringBuilder strBuilder = new StringBuilder(GetAppURL(imageLocation));
                StringBuilder strBuilder = new StringBuilder(imageLocation);
                //strBuilder.Append("ObjectGUID=" + KeyFieldValue);
                //TODO: Check this line, because this can be a complex datakey that not has a GUID property.
                strBuilder.Append(KeyFieldValue.Replace("GUID", "ObjectGUID"));
                strBuilder.Append("&ObjectClass=" + encodedClassName);
                strBuilder.Append("&ObjectProperty=" + property);
                strBuilder.Append("&IsProgressBar=" + true.ToString());

                if (style != "")
                {
                    const string strRegularExpression = "(\\w+)*:[^; ]*]";
                    Regex regExPattern = new Regex(strRegularExpression);
                    Match matPattern = regExPattern.Match(style);

                    while (matPattern.Success)
                    {
                        if (matPattern.Value != "")
                        {
                            string[] stylePropAndValue = matPattern.Value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (stylePropAndValue.Length == 2)
                            {
                                if (string.Equals("width", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarWidth=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("height", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarHeight=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("tubeColor", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarTubeColor=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("textColor", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarTextColor=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("fillColor", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarFillColor=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("horizontal:", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarHorizontal=" + stylePropAndValue[1]);
                                }
                            }
                        }
                    }
                }
                img.ImageUrl = strBuilder.ToString();
            }
            else
            {
                //StringBuilder strBuilder = new StringBuilder(GetAppURL(imageLocation));
                StringBuilder strBuilder = new StringBuilder(imageLocation);
                //strBuilder.Append("ObjectGUID=" + KeyFieldValue);
                //TODO: Check this line, to support complex datakeys.
                strBuilder.Append(KeyFieldValue.Replace("GUID", "ObjectGUID"));
                strBuilder.Append("&ObjectClass=" + encodedClassName);
                strBuilder.Append("&ObjectProperty=" + property);
                img.ImageUrl = strBuilder.ToString();
                if (style != null)
                {
                    if (style.Length > 3)
                    {
                        img.Style.Value = style.Substring(1, style.Length - 2);
                    }
                }
            }
            return img;
        }

        /// <summary>
        /// Returns an Image control from a string object
        /// </summary>
        /// <param name="bObject">BusinessObject from which the elements to be parsed on the rawString will be taken.</param>
        /// <param name="content">String {property} (default style) or {property,"style"} used to create the image</param>
        /// <param name="progressbar">Boolean used to determine if the image is a generated progress bar</param>
        /// <returns>An Image Object with the ImageUrl and Style set</returns>
        public Image getImageProperty(BLBusinessBase bObject, string content, bool progressbar)
        {
            //Remove the "{" and "}" characters
            content = content.Substring(1, content.Length - 2);

            string[] propertyAndstyle = content.Split(new char[] { ',' });

            Image img = new Image();
            string encodedClassName = System.Web.HttpContext.Current.Server.UrlEncode(bObject.GetType().AssemblyQualifiedName);

            string property = "";
            string style = "";
            property = propertyAndstyle[0];

            if (propertyAndstyle.Length > 1)
            {
                style = propertyAndstyle[1];
            }
            if (progressbar)
            {
                //StringBuilder strBuilder = new StringBuilder(GetAppURL(imageLocation));
                StringBuilder strBuilder = new StringBuilder(imageLocation);
                //strBuilder.Append("ObjectGUID=" + getKeyFieldValue());
                strBuilder.Append("ObjectGUID=" + bObject["GUID"]);
                strBuilder.Append("&ObjectClass=" + encodedClassName);
                strBuilder.Append("&ObjectProperty=" + property);
                strBuilder.Append("&IsProgressBar=" + true.ToString());

                if (style != "")
                {
                    const string strRegularExpression = "(\\w+)*:[^; ]*]";
                    Regex regExPattern = new Regex(strRegularExpression);
                    Match matPattern = regExPattern.Match(style);

                    while (matPattern.Success)
                    {
                        if (matPattern.Value != "")
                        {
                            string[] stylePropAndValue = matPattern.Value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (stylePropAndValue.Length == 2)
                            {
                                if (string.Equals("width", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarWidth=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("height", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarHeight=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("tubeColor", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarTubeColor=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("textColor", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarTextColor=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("fillColor", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarFillColor=" + stylePropAndValue[1]);
                                }
                                else if (string.Equals("horizontal:", stylePropAndValue[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    strBuilder.Append("&PBarHorizontal=" + stylePropAndValue[1]);
                                }
                            }
                        }
                    }
                }
                img.ImageUrl = strBuilder.ToString();
            }
            else
            {
                //StringBuilder strBuilder = new StringBuilder(GetAppURL(imageLocation));
                StringBuilder strBuilder = new StringBuilder(imageLocation);
                strBuilder.Append("ObjectGUID=" + bObject["GUID"]);
                strBuilder.Append("&ObjectClass=" + encodedClassName);
                strBuilder.Append("&ObjectProperty=" + property);
                img.ImageUrl = strBuilder.ToString();
                if (style != null)
                {
                    if (style.Length > 3)
                    {
                        img.Style.Value = style.Substring(1, style.Length - 2);
                    }
                }
            }
            return img;
        }

        public BLDataKey getDatakeyFromString(string ClassName, string strDatakey)
        {
            BLBusinessBase newObject = (BLBusinessBase)System.Activator.CreateInstance(Type.GetType(ClassName));

            BLDataKey objectDataKey = newObject.DataKey.NewDataKey;

            const string strRegularExpression = @"(\w+)=([^&])*";
            Regex regExPattern = new Regex(strRegularExpression);
            System.Text.RegularExpressions.Match matPattern = regExPattern.Match(strDatakey);

            while (matPattern.Success)
            {
                if (matPattern.Value.Length > 0)
                {
                    string[] propertyNameandValue = matPattern.Value.Split(new char[] { '=' });
                    string propertyName = propertyNameandValue[0];
                    string propertyValue = "";
                    if (propertyNameandValue.Length >= 2)
                    {
                        propertyValue = propertyNameandValue[1];
                    }
                    if (newObject.GetType().GetMember(propertyName).Length > 0)
                    {
                        try
                        {
                            PropertyInfo propInfo = newObject.GetType().GetProperty(propertyName);
                            propInfo.SetValue(newObject, System.Convert.ChangeType(propertyValue, propInfo.PropertyType), null);
                        }
                        catch (Exception e)
                        {
                            return null;
                        }
                    }
                }
                matPattern = matPattern.NextMatch();
            }
            return newObject.DataKey;
        }

        /// <summary>
        /// Gives the Image Url for an INode Object.
        /// </summary>
        /// <param name="iNodeObject"></param>
        /// <returns></returns>
        public string getImageUrl(INode iNodeObject)
        {
            string encodedClassName = System.Web.HttpContext.Current.Server.UrlEncode(iNodeObject.GetType().AssemblyQualifiedName);

            StringBuilder strBuilder = new StringBuilder(imageLocation);
            strBuilder.Append("ObjectGUID=" + ((BLBusinessBase)iNodeObject)["GUID"]);
            strBuilder.Append("&ObjectClass=" + encodedClassName);
            strBuilder.Append("&ObjectProperty=Icon");
            strBuilder.Append("&IsINode=True");
            return strBuilder.ToString();
        }

        public Object getMethodProperty(BLBusinessBase bObject, string methodName)
        {
            //BindingFlags.FlattenHierarchy | BindingFlags.GetProperty 
            //| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            //| BindingFlags.InvokeMethod | BindingFlags.Default

            return bObject.GetType().InvokeMember(methodName, BindingFlags.Public | BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Instance
               , null, bObject, new object[0]);
        }

        public bool IsFloatLiteral(string stringToEvaluate)
        {
            const string strDecimalExpression = "(\\d+)(\\.(\\d+))?";
            Regex regDecPattern = new Regex(strDecimalExpression);
            Match matDecPattern = regDecPattern.Match(stringToEvaluate);

            return matDecPattern.Success;
        }

        public bool IsMethod(string stringToEvaluate)
        {
            const string strMethodExpression = "(\\w+)(\\.(\\w+))*\\(\\)";
            Regex regMetPattern = new Regex(strMethodExpression);
            Match matMetPattern = regMetPattern.Match(stringToEvaluate);

            return matMetPattern.Success;
        }

        public bool CheckAccess(object Datasource, string operation, string fieldname)
        {
            if (typeof(BLBusinessBase).IsAssignableFrom(Datasource.GetType()))
            {

                BLBusinessBase BObject = (BLBusinessBase)Datasource;
                clsSecurityOperation Operation;
                try
                {
                    bool result = false;
                    int indexOfPoint = fieldname.IndexOf('.');
                    string subproperty;
                    if (indexOfPoint > 0)
                    {
                        subproperty = fieldname.Substring(0, indexOfPoint);
                    }
                    else
                    {
                        subproperty = fieldname;
                    }
                    switch (operation.ToUpper())
                    {
                        default:
                        case "READ":
                            Operation = BObject.get_ReadPropertySecurityOperation(subproperty);
                            break;
                        case "UPDATE":
                            Operation = BObject.get_UpdatePropertySecurityOperation(subproperty);
                            break;
                    }
                    bool hasAccess = true;
                    if (Operation != null)
                    {
                        try
                        {
                            if (!Operation.CheckAccess())
                            {
                                hasAccess = false;
                            }
                        }
                        catch (SecurityException ex)
                        {
                            hasAccess = false;
                        }
                    }
                    if (hasAccess && (indexOfPoint > 0))
                    {
                        object newDatasource = DataBinder.Eval(BObject, subproperty);
                        result = CheckAccess(newDatasource, operation, fieldname.Remove(0, indexOfPoint + 1));
                    }
                    else
                    {
                        result = hasAccess;
                    }
                    return result;
                }
                catch (SecurityException se)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
