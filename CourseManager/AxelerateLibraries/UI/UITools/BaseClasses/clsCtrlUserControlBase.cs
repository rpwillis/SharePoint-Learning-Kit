using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using Axelerate.BusinessLayerFrameWork;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Text.RegularExpressions;

using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLayerUITools.Interfaces;
using Axelerate.BusinessLayerUITools.Common;

namespace Axelerate.BusinessLayerUITools.BaseClasses
{
    /// <summary>
    /// This class is the base class for all the Web Parts of the project.
    /// It contains methods and properties that can be used by any Webpart.
    /// </summary>
    public class clsCtrlUserControlBase : System.Web.UI.UserControl, IWebPart, ICtrlWebPartBase
    {
        #region Constants
        public string imageLocation = "/_layouts/Shared/Images.asmx?"; //Location Url when value is a Bitmap type.
        public string strObjNotFound = Resources.ErrorMessages.errObjectNotFound;
        #endregion

        #region Constructor
        public clsCtrlUserControlBase()
        {
            //
            // TODO: Add constructor logic here
            //
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
                BLBusinessBase item = (BLBusinessBase)GetBusinessClassInstance();  //(BLBusinessBase)ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", new object[] { getKeyFieldValue(), null });
                return clsSharedMethods.SharedMethods.parseBOPropertiesString(item, rawString, ClassName, item.DataKey.ToString());
            }
            catch (Exception exc)
            {
                return Axelerate.BusinessLayerUITools.Resources.LocalizationUIToolsResource.strErrorParsingParameters;
            }
        }

        /// <summary>
        /// Get the Application's current Url.
        /// </summary>
        /// <param name="url">an url to be attached to the application url</param>
        /// <returns>the application's URL with the url attached.</returns>
        protected string GetAppURL(string url)
        {
            string path = clsConfigurationProfile.Current.getPropertyValue("WebApplicationPath");
            if (path == null || path == "")
                throw new Exception(Resources.ErrorMessages.errCantFindWebApplicationPath);
            return path + url;
        }

        /// <summary>
        /// Used to get the current value of the KeyField Property.
        /// </summary>
        /// <returns>The Current value of the KeyField Property</returns>
        /// <summary>
        /// Used to get the current value of the KeyField Property.
        /// </summary>
        /// <returns>The Current value of the KeyField Property</returns>
        protected string getKeyFieldValue(string KeyField)
        {
            try
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
            catch (Exception ex)
            {
                return "";
            }
        }

        protected virtual Object GetBusinessClassInstance()
        {
            string[] ParamNames = new string[0];
            object[] Params = new object[0];
            object Instance = null;
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
                if (FactoryMethod.Trim() != "")
                {

                    Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, FactoryMethod, Params);
                }
                else
                {
                    if (Type.GetType(ClassName).IsSubclassOf(typeof(BLBusinessBase)))
                    {
                        if (Params.Length > 0)
                        {
                            Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "TryGetObjectByGUID", Params);
                        }
                        else
                        {
                            //Axelerate.BusinessLogic.SharedBusinessLogic.Security.clsADUser a;
                            //a = Axelerate.BusinessLogic.SharedBusinessLogic.Security.clsADUser.NewObject() 
                            Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "NewObject", new object[] { });
                        }
                    }
                    else if (typeof(IBLListBase).IsAssignableFrom(Type.GetType(ClassName)))
                    {
                        Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", Params);
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
            BLCriteria Criteria = new BLCriteria(Type.GetType(ClassName));
            return Criteria;
        }

        #endregion

        #region Template

        [TemplateContainer(typeof(HeaderContainer))]
        public ITemplate HeaderTemplate
        {
            get
            {
                return _headerTemplate;
            }
            set
            {
                _headerTemplate = value;
            }
        }
        private ITemplate _headerTemplate;

        [TemplateContainer(typeof(FooterContainer))]
        public ITemplate FooterTemplate
        {
            get
            {
                return _footerTemplate;
            }
            set
            {
                _footerTemplate = value;
            }
        }
        private ITemplate _footerTemplate;


        public class HeaderContainer : Control, INamingContainer
        {
            public HeaderContainer()
            {
            }
        }
        public class FooterContainer : Control, INamingContainer
        {
            public FooterContainer()
            {
            }
        }


        #endregion

        #region IWebPart Members
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
        public virtual string CatalogIconImageUrl
        {
            get
            {
                object IconUrl = ViewState["IconURL"];
                if (IconUrl == null)
                {
                    return "";
                }
                else
                {
                    return (string)IconUrl;
                }
            }
            set
            {
                ViewState["IconURL"] = value;
            }
        }
        public virtual string Description
        {
            get
            {
                object VALUE = ViewState["Description"];
                if (VALUE == null)
                {
                    return "";
                }
                else
                {
                    return (string)VALUE;
                }
            }
            set
            {
                ViewState["Description"] = value;
            }
        }
        public virtual string Subtitle
        {
            get { return ""; }
        }
        public virtual string Title
        {
            get
            {
                object VALUE = ViewState["Title"];
                if (VALUE == null)
                {
                    return "";
                }
                else
                {
                    return (string)VALUE;
                }
            }
            set
            {
                ViewState["Title"] = value;
            }
        }
        public virtual string TitleIconImageUrl
        {
            get
            {
                object VALUE = ViewState["TitleIconImageUrl"];
                if (VALUE == null)
                {
                    return "";
                }
                else
                {
                    return (string)VALUE;
                }
            }
            set
            {
                ViewState["TitleIconImageUrl"] = value;
            }
        }
        public virtual string TitleUrl
        {
            get
            {
                object VALUE = ViewState["TitleUrl"];
                if (VALUE == null)
                {
                    return "";
                }
                else
                {
                    return (string)VALUE;
                }
            }
            set
            {
                ViewState["TitleUrl"] = value;
            }
        }
        #endregion
    }

}
