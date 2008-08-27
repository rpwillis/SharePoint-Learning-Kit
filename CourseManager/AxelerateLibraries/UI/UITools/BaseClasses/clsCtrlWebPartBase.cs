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

using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties;
using Axelerate.BusinessLayerUITools.Interfaces;
using Axelerate.BusinessLayerUITools.Common;
using System.Reflection;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.BaseClasses
{
    /// <summary>
    /// This class is the base class for all the Web Parts of the project.
    /// It contains methods and properties that can be used by any Webpart.
    /// </summary>
    [XmlRoot(Namespace = "Axelerate.BusinessLayerUIToolsTest")]
    [ToolboxData("<{0}:wptBOFact runat=server />")]
    [ParseChildren(true)]
    public abstract class clsCtrlWebPartBase : System.Web.UI.WebControls.WebParts.WebPart, ICtrlWebPartBase
    {
        internal String ErrorText;
        #region enums
        public enum FactContentType
        {
            Property = 0,
            HTML = 1,
            ProgressBar = 2,
            Image = 5
        }
        public enum wptBehavior
        {
            EditableOnFocus = 0,
            NoEditable = 1,
            AlwaysEditable = 2
        }
        #endregion

        #region Constants
        //TODO: put it in a  setting
        public string imageLocation = "/_layouts/Shared/Images.asmx?"; //Location Url when value is a Bitmap type.

        //TODO: put it in a resource file
        public string strObjNotFound = Resources.ErrorMessages.errObjectNotFound;
        #endregion

        #region Constructor
        public clsCtrlWebPartBase()
        {

        }
        #endregion

#region utilie methods
        public static string UniqueIDWithDollars(Control ctrl)
        {

            string sId = ctrl.UniqueID;

            if (sId == null)
            {

                return null;

            }

            if (sId.IndexOf(':') >= 0)
            {

                return sId.Replace(':', '$');

            }

            return sId;

        }
#endregion

        #region WebPart Methods

        /// <summary> 
        /// This method parses the name of the BO property that comes
        /// enclosed by "{" and "}" and changes it for the value of the
        /// property
        /// </summary>
        /// <param name="rawString">String to be parsed</param>
        /// <returns>string with the values parsed</returns>
        /// <example>parseBOPropertiesString("{Name}'s Email: {Contact.Email}") will return "Niels' Email: niels@axeleratesolutions.com" in case we manage an ADUser Object</example>
        public string parseBOPropertiesString(string rawString)
        {
            try
            {
                BLBusinessBase item = (BLBusinessBase)GetBusinessClassInstance(); //(BLBusinessBase)ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", new object[] { getKeyFieldValue(), null });

                return clsSharedMethods.SharedMethods.parseBOPropertiesString(item, rawString, ClassName, item.DataKey.ToString());
            }
            catch (Exception exc)
            {
                return Axelerate.BusinessLayerUITools.Resources.LocalizationUIToolsResource.strErrorParsingParameters;
            }
        }

        /// <summary>
        /// Renders a control for the input property, the control that will be rendered can be an image or a label by using the
        /// PropertyType input parameter to choose the best option.
        /// </summary>
        /// <param name="bObject">The BusinessObject from which the property values will be parsed.</param>
        /// <param name="PropertyType">The type of the content that will be rendered</param>
        /// <param name="Content">The content that will be rendered as a control.</param>
        /// <returns>An image or label control.</returns>
        /// <example>renderPropertyControl((BLBusinessBase)item, clsBOBandColumn.FactContentType.HTML, "This a biography: {Contact.Biography}") returns a Label Control
        /// with the text: "This is a biography: Niels is a software developer with..."</example>
        public Control renderPropertyControl(BLBusinessBase bObject, string ClassName, string KeyFieldValue, FactContentType PropertyType, string Content, bool IsLink, string TargetURL)
        {
            return renderPropertyControl(bObject, ClassName, KeyFieldValue, PropertyType, Content, IsLink, TargetURL, -1);
        }

        public Control renderPropertyControl(BLBusinessBase bObject, string ClassName, string KeyFieldValue, FactContentType PropertyType, string Content, bool IsLink, string TargetURL, int maxchars)
        {
            String stringvalue = "";
            switch (PropertyType)
            {
                case FactContentType.Property:
                    Label lbl = new Label();
                    stringvalue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bObject, "{" + Content + "}", ClassName, KeyFieldValue);
                    if (maxchars != -1)
                    {
                        lbl.Text = stringvalue.Substring(0, maxchars) + "...";
                    }
                    else
                    {
                        lbl.Text = stringvalue;
                    }
                    return lbl;
                default:
                case FactContentType.HTML:
                    Label lblHTML = new Label();
                    stringvalue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bObject, Content, ClassName, KeyFieldValue);
                    if (maxchars != -1)
                    {
                        lblHTML.Text = stringvalue.Substring(0, maxchars) + "...";
                    }
                    else
                    {
                        lblHTML.Text = stringvalue;
                    }
                    return lblHTML;
                case FactContentType.ProgressBar:
                    Image imgPB = clsSharedMethods.SharedMethods.getImageProperty(bObject, "{" + Content + "}", true);
                    return imgPB;
                case FactContentType.Image:
                    Image img = clsSharedMethods.SharedMethods.getImageProperty(bObject, Content, false);
                    return img;
            }
        }

        /// <summary>
        /// Renders a control for the input property, the control that will be rendered can be an image or a label by using the
        /// PropertyType input parameter to choose the best option.
        /// </summary>
        /// <param name="bObject">The BusinessObject from which the property values will be parsed.</param>
        /// <param name="PropertyType">The type of the content that will be rendered</param>
        /// <param name="Content">The content that will be rendered as a control.</param>
        /// <returns>An image or label control.</returns>
        /// <example>renderPropertyControl((BLBusinessBase)item, clsBOBandColumn.FactContentType.HTML, "This a biography: {Contact.Biography}") returns a Label Control
        /// with the text: "This is a biography: Niels is a software developer with..."</example>
        public Control renderPropertyControl(BLBusinessBase bObject, string ClassName, string KeyFieldValue, string PropertyType, string Content, bool IsEditable, bool IsLink, string TargetURL)
        {
            if (!IsEditable)
            {
                switch (PropertyType.ToLower())
                {
                    case "fieldcontrol":
                        Label lbl = new Label();
                        lbl.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString(bObject, "{" + Content + "}", ClassName, KeyFieldValue);
                        return lbl;
                    case "imagecontrol":
                        Image img = clsSharedMethods.SharedMethods.getImageProperty(bObject, Content, false);
                        return img;
                    case "progressbarcontrol":
                        Image imgPB = clsSharedMethods.SharedMethods.getImageProperty(bObject, "{" + Content + "}", true);
                        return imgPB;
                    case "htmlfieldcontrol":
                        Label lblHTML = new Label();
                        lblHTML.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString(bObject, Content, ClassName, KeyFieldValue);
                        return lblHTML;
                    default:
                        Label lblError = new Label();
                        lblError.Text = Resources.ErrorMessages.errInvalidType;
                        return lblError;
                }
            }
            else
            {
                return new Label();
            }
        }

        protected string GetAppURL(string url)
        {
            string path = clsConfigurationProfile.Current.getPropertyValue("WebApplicationPath");
            if (path == null || path == "")
                throw new Exception(Resources.ErrorMessages.errCantFindWebApplicationPath);
            return path + url;
        }

        /// <summary>
        /// Returns an instance of a header container to use it on the WebPart.
        /// </summary>
        /// <returns>An instance of the container.</returns>
        public HeaderTemplateContainer InstantiateHeader()
        {
            HeaderTemplateContainer cont = new HeaderTemplateContainer();
            m_HeaderTemplate.InstantiateIn(cont);
            return cont;
        }

        /// <summary>
        /// Returns an instance of a footer container to use it on the WebPart.
        /// </summary>
        /// <returns>An instance of the container.</returns>
        public FooterTemplateContainer InstantiateFooter()
        {
            FooterTemplateContainer cont = new FooterTemplateContainer();
            m_FooterTemplate.InstantiateIn(cont);
            return cont;
        }

        /// <summary>
        /// Used to get the current value of the KeyField Property.
        /// </summary>
        /// <returns>The Current value of the KeyField Property</returns>
        protected string getKeyFieldValue(string KeyField)
        {
            if (this.Page != null)
            {
                if (Axelerate.BusinessLogic.SharedBusinessLogic.Support.clsBusinessGlobals.isGlobal(KeyField))
                {
                    return Axelerate.BusinessLogic.SharedBusinessLogic.Support.clsBusinessGlobals.GetGlobalValue(KeyField).ToString();
                }
                else if (this.Page.Request.Params[KeyField] != null)
                {
                    return (string)this.Page.Request.Params[KeyField];
                }
                else
                {
                    if (System.Web.HttpContext.Current.Session[KeyField] != null)
                    {
                        ViewState[KeyField] = System.Web.HttpContext.Current.Session[KeyField];
                    }
                    if (ViewState[KeyField] != null)
                    {
                        return (string)ViewState[KeyField];
                    }
                }
            }
            return "";
        }

        protected virtual bool CheckParameters()
        {
            Type DatasourceType = Type.GetType(ClassName);
            if (DatasourceType == null)
            {
                clsLog.Trace(Resources.ErrorMessages.errInvalidClassName, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Resources.ErrorMessages.errInvalidClassName + " " + ClassName);
                }
                ErrorText = Resources.ErrorMessages.errInvalidClassName + " " + ClassName;
                return false;
            }
            MemberInfo[] methods = DatasourceType.GetMember(FactoryMethod, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            if (methods.Length == 0)
            {
                clsLog.Trace(Resources.ErrorMessages.errInvalidFactoryMethod, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Resources.ErrorMessages.errInvalidFactoryMethod + " " + FactoryMethod);
                }
                ErrorText = Resources.ErrorMessages.errInvalidFactoryMethod + " " + FactoryMethod;
                return false;
            }
            else
            {
                object[] param = GetParams();
                Type[] types = new Type[param.Length];
                int index = 0;
                foreach (object obj in param)
                {
                    if (obj != null)
                    {
                        types[index] = obj.GetType();
                    }
                    else
                    {
                        types[index] = typeof(Object);
                        //clsLog.Trace(Resources.ErrorMessages.wrnNullParameter, LogLevel.LowPriority);
                    }
                    index++;
                }
                bool methodMatch = false;
                foreach (MemberInfo member in methods)
                {
                    if (member.MemberType == MemberTypes.Method)
                    {
                        MethodInfo method = (MethodInfo)member;
                        ParameterInfo[] parameters = method.GetParameters();
                        if (types.Length == parameters.Length)
                        {
                            index = 0;
                            methodMatch = true;
                            foreach (ParameterInfo parameter in parameters)
                            {
                                if (!types[index].IsAssignableFrom(parameter.ParameterType))
                                {
                                    methodMatch = false;
                                }
                                index++;
                            }
                            if (methodMatch)
                            {
                                break;
                            }
                        }
                    }
                }
               // MethodInfo info = DatasourceType.GetMethod(FactoryMethod, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, types, null);
                if (!methodMatch)
                {
                    String text = Resources.ErrorMessages.errInvalidFactoryParameters;
                    clsLog.Trace(text, LogLevel.LowPriority);
                    if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                    {
                        throw new Exception(text);
                    }
                    ErrorText = text;
                    return false;
                }

            }
            return true;
        }

        protected virtual Object[] GetParams()
        {
            object[] Params = new object[0];
            string[] ParamNames = new string[0];
            int i = 0;
            if (FactoryParameters != "")
            {
                ParamNames = FactoryParameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Params = new object[ParamNames.Length];
                foreach (string ParamName in ParamNames)
                {
                    object value = null;
                    switch (ParamName.Trim().ToUpper())
                    {
                        case "[CRITERIA]":
                            value = GetCriteria();
                            break;
                        case "NULL":
                            value = null;
                            break;
                        default:
                            String val = getKeyFieldValue(ParamName);
                            if (val == "")
                            {
                                int intval;
                                bool boolval;
                                if (ParamName.StartsWith("\"") || ParamName.StartsWith("\'"))
                                {
                                    value = ParamName.Substring(1, ParamName.Length - 2);
                                }
                                else if (int.TryParse(ParamName, out intval))
                                {
                                    value = intval;
                                }
                                else if (bool.TryParse(ParamName, out boolval))
                                {
                                    value = boolval;
                                }
                                else
                                {
                                    ErrorText = Resources.ErrorMessages.errInvalidFactoryParameters;
                                    clsLog.Trace(Resources.ErrorMessages.errInvalidFactoryParameters, LogLevel.LowPriority);
                                    if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                                    {
                                        throw new Exception(ErrorText);
                                    }
                                }
                            }
                            else
                            {
                                value = val;
                            }
                            break;
                    }
                    Params[i] = value;
                    i++;
                }
            }
            return Params;
        }

        protected virtual Object GetBusinessClassInstance()
        {
            return GetBusinessClassInstance(ClassName, FactoryMethod, FactoryParameters);
        }

        protected virtual Object GetBusinessClassInstance(string ClassN, string FactoryM, string FactoryParam)
        {
            string[] ParamNames = new string[0];
            object[] Params = new object[0];
            object Instance = null;
            int i = 0;
            if (FactoryParam != "")
            {
                ParamNames = FactoryParameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Params = new object[ParamNames.Length];
                foreach (string ParamName in ParamNames)
                {
                    object value = null;
                    switch (ParamName.Trim().ToUpper())
                    {
                        case "[CRITERIA]":
                            value = GetCriteria(ClassN);
                            break;
                        default:
                            value = getKeyFieldValue(ParamName);
                            break;
                    }
                    Params[i] = value;
                    i++;
                }
            }
            try
            {
                if (FactoryM.Trim() != "")
                {

                    Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassN, FactoryM, Params);
                }
                else
                {
                    if (Type.GetType(ClassN).IsSubclassOf(typeof(BLBusinessBase)))
                    {
                        if (Params.Length > 0)
                        {
                            Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassN, "TryGetObjectByGUID", Params);
                        }
                        else
                        {
                            //Axelerate.BusinessLogic.SharedBusinessLogic.Security.clsADUser a;
                            //a = Axelerate.BusinessLogic.SharedBusinessLogic.Security.clsADUser.NewObject() 
                            Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassN, "NewObject", new object[] { });
                        }
                    }
                    else if (typeof(IBLListBase).IsAssignableFrom(Type.GetType(ClassN)))
                    {
                        Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassN, "GetCollection", Params);
                    }

                }
            }
            catch
            {
                Instance = null;
            }
            return Instance;
        }

        protected virtual Object NewBusinessClass()
        {
            object instance = null;
            try
            {
                if (ClassName.Trim() != "")
                {
                    instance = System.Activator.CreateInstance(Type.GetType(ClassName));
                }
            }
            catch
            {
                return null;
            }
            return instance;
        }

        protected virtual BLCriteria GetCriteria()
        {
            return GetCriteria(ClassName);
        }

        protected virtual BLCriteria GetCriteria(string ClassN)
        {
            BLCriteria Criteria = new BLCriteria(Type.GetType(ClassN));
            return Criteria;
        }

        /// <summary>
        /// Patch to fix Ajax update panels in sharepoint.
        /// </summary>
        internal void EnsureUpdatePanelFixups()
        {
            if (this.Page != null && this.Page.Form != null)
            {
                string formOnSubmitAtt = this.Page.Form.Attributes["onsubmit"];
                if (formOnSubmitAtt == "return _spFormOnSubmitWrapper();")
                {
                    this.Page.Form.Attributes["onsubmit"] = "_spFormOnSubmitWrapper();";
                }
                ScriptManager.RegisterStartupScript(this, typeof(clsCtrlWebPartBase), "UpdatePanelFixup", "_spOriginalFormAction = document.forms[0].action; _spSuppressFormOnSubmitWrapper=true;", true);
            }

        }
        #endregion

        #region WebPart Members
        [Category("Miscellaneous")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "FactoryParameters")]
        [DefaultValue("")]
        [WebDisplayName("Factory Parameters:")]
        [WebDescription("Factory Parameters of the business object.")]
        public virtual string FactoryParameters
        {
            get
            {
                if (ViewState["FactoryParameters"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["FactoryParameters"];
                }
            }
            set
            {
                ViewState["FactoryParameters"] = value;
            }
        }

        [Category("Miscellaneous")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "FactoryMethod")]
        [DefaultValue("")]
        [WebDisplayName("Factory Method")]
        [WebDescription("Factory Method of the business object used to instanciate it.")]
        public virtual string FactoryMethod
        {
            get
            {
                if (ViewState["FactoryMethod"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["FactoryMethod"];
                }
            }
            set
            {
                ViewState["FactoryMethod"] = value;
            }
        }

        [Category("Miscellaneous")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ClassName")]
        [DefaultValue("")]
        [WebDisplayName("Class' Assembly Qualified Name")]
        [WebDescription("Class type of the object.")]
        public virtual string ClassName
        {
            get
            {
                if (ViewState["ClassName"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["ClassName"];
                }
            }
            set
            {
                ViewState["ClassName"] = value;
            }
        }

        /*[Category("Miscellaneous")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Title")]
        [DefaultValue("")]
        [WebDisplayName("Web Part Title")]
        [WebDescription("Title of this webpart.")]
        public override string Title
        {
            get
            {
                if (IsInEditMode || ClassName == "")
                {
                    return base.Title;
                }
                else
                {
                    try
                    {
                        Object item = GetBusinessClassInstance();//ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", new object[] { getKeyFieldValue(), null });
                        if (item != null)
                        {
                            if (typeof(BLBusinessBase).IsAssignableFrom(item.GetType()))
                            {
                                return clsSharedMethods.SharedMethods.parseBOPropertiesString((BLBusinessBase)item, base.Title, ClassName, ((BLBusinessBase)item).DataKey.ToString());
                            }
                            else
                            {
                                return base.Title;
                            }
                        }
                        else
                        {
                            return base.Title;
                        }
                    }
                    catch
                    {
                        return base.Title;
                    }
                }
            }
            set
            {
                base.Title = value;
            }
        }*/

        [Category("Miscellaneous")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "TitleUrl")]
        [DefaultValue("")]
        [WebDisplayName("Title URL")]
        [WebDescription("Title URL to use if the title is a link.")]
        public override string TitleUrl
        {
            get
            {
                if (IsInEditMode || ClassName == "")
                {
                    return base.TitleUrl;
                }
                else
                {
                    try
                    {
                        Object item = GetBusinessClassInstance();//ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", new object[] { getKeyFieldValue(), null });
                        if (item != null)
                        {
                            if (typeof(BLBusinessBase).IsAssignableFrom(item.GetType()))
                            {
                                return clsSharedMethods.SharedMethods.parseBOPropertiesString((BLBusinessBase)item, base.TitleUrl, ClassName, ((BLBusinessBase)item).DataKey.ToString());
                            }
                            else
                            {
                                return base.TitleUrl;
                            }
                        }
                        else
                        {
                            return base.TitleUrl;
                        }
                    }
                    catch
                    {
                        return base.TitleUrl;
                    }
                }
            }
            set
            {
                base.TitleUrl = value;
            }
        }

        public bool IsInEditMode
        {
            get
            {   //Can use this to check if this wepart is actually selected for editing. (base.WebPartManager.SelectedWebPart != null && base.WebPartManager.SelectedWebPart == this)
                if (base.DesignMode || (Page == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region WebPart Properties
        private ITemplate m_HeaderTemplate = null;
        private ITemplate m_FooterTemplate = null;

        /*[Category("Miscellaneous"), Browsable(false)]
        [WebBrowsable(false)]
        [XmlElement(ElementName = "HeaderTemplate")]
        [NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(Axelerate.BusinessLayerUITools.clsCtrlWebPartBase.HeaderTemplateContainer))]
        */
        public ITemplate HeaderTemplate
        {
            get
            {
                return m_HeaderTemplate;
            }
            set
            {
                m_HeaderTemplate = value;

            }
        }

        /*[Category("Miscellaneous"), Browsable(false)]
        [WebBrowsable(false)]
        [XmlElement(ElementName = "FooterTemplate")]
        [NotifyParentProperty(true)]
        [TemplateContainer(typeof(Axelerate.BusinessLayerUITools.clsCtrlWebPartBase.FooterTemplateContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]*/
        public ITemplate FooterTemplate
        {
            get
            {
                return m_FooterTemplate;
            }
            set
            {
                m_FooterTemplate = value;

            }
        }
        #endregion

        #region Webpart InnerClasses
        [XmlRoot(Namespace = "Axelerate.SharepointBusinessLayerUIToolsTest")]
        [Serializable]
        public class HeaderTemplateContainer : TemplateControl, INamingContainer
        {
            public HeaderTemplateContainer() { }

        }

        [XmlRoot(Namespace = "Axelerate.SharepointBusinessLayerUIToolsTest")]
        [Serializable]
        public class FooterTemplateContainer : TemplateControl, INamingContainer
        {
            public FooterTemplateContainer() { }

        }
        #endregion
    }
}

