using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLayerUITools.Interfaces
{
    /// <summary>
    /// ISearchProvider is an interface for controls that can be provider of search criterias.
    /// </summary>
    public interface ISearchProvider
    {
        /// <summary>
        /// The business object's property that represents the title of each search result. Its interpreted
        /// </summary>
        String title
        {
            get;
            set;
        }

        /// <summary>
        /// The business object's property that represents the description of each search result. Its interpreted
        /// </summary>
        String description
        {
            get;
            set;
        }

        /// <summary>
        /// The business object's property that represents the details of each search result. Its interpreted
        /// </summary>
        String details
        {
            get;
            set;
        }

        /// <summary>
        /// Page path where the search results are going to be diplayed
        /// </summary>
        String redirect
        {
            get;
            set;
        }

        /// <summary>
        /// The business object's properties involved in the search, used by the criteria property.
        /// </summary>
        String[] columns
        {
            get;
            set;
        }

        /// <summary>
        /// bussiness object name. Specifies where the search is going to be done
        /// </summary>
        String businessobj
        {
            get;
            set;
        }

        /// <summary>
        /// search criteria
        /// </summary>
        BLCriteria Criteria
        {
            get;
        }

        bool NewSearch
        {
            get;
        }
    }
}
