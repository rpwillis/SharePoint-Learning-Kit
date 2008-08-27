using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLayerUITools.Interfaces
{
    /// <summary>
    /// ISelectorProvider is an interface for controls that allows the user select a business object from a 
    /// List
    /// </summary>
    public interface ISelectorProvider
    {
        /// <summary>
        /// Collection type that will be the datasource of the consumer.
        /// </summary>
        String ConsumerType
        {
            get;
            set;
        }

        /// <summary>
        /// Items currently selected in this selector
        /// </summary>
        /// <returns>An IBLListBase Collection with the selected items of all the Selectors</returns>
        IList SelectedItems
        {
            get;
        }

        /// <summary>
        /// Items currently not selected
        /// </summary>
        IList NotSelectedItems
        {
            get;
        }

        /// <summary>
        /// Property that will be returned.
        /// </summary>
        String ReturnedProperty
        {
            get;
            set;
        }

        /// <summary>
        /// Add an item to the selected items.
        /// </summary>
        /// <param name="ObjectDataKey">Datakey of the object that is selected.</param>
        void SelectItem(string ObjectDataKey);

        /// <summary>
        /// Removes an item from the selected items.
        /// </summary>
        /// <param name="ObjectDatakey"></param>
        void DeselectItem(string ObjectDatakey);
    }
}