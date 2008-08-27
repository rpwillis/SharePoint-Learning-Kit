using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLayerUITools.Misc
{
    public class HyperRow
    {
        internal Axelerate.BusinessLayerUITools.WebParts.wptHyperGrid Parent = null;
        internal int RowIndex = -1;
        internal TableRow row = null;
        internal TableRow childbandRow = null;
        internal HiddenField childbandStatus = null;
        internal Table scrollPanel = null;
        internal Image expandImage = null;
        internal object DataItem = null;
       

        #region Constructors

        public HyperRow(int index, TableRow baserow, TableRow basechildband)
        {
            RowIndex = index;
            row = baserow;
            childbandRow = basechildband;
        }

        public HyperRow(int index, TableRow baserow)
            : this(index, baserow, null)
        {
        }

        #endregion


        public string ClientID
        {
            get
            {
                return row.ClientID;
            }
        }

        public string PanelClientID
        {
            get
            {
                return scrollPanel.ClientID;
            }
        }

        internal virtual void OnPrerender()
        {
            if (Parent != null)
            {
                if (childbandRow != null && expandImage != null)
                {
                    expandImage.Attributes["onclick"] = "javascript:OnChildBandCollapseExpand(event,'"+ childbandStatus.ClientID +"', '" + childbandRow.ClientID + "', '" + expandImage.ClientID + "', '" + Parent.CollapseImageUrl + "', '" + Parent.ExpandImageUrl + "')";
                }
                else
                {
 //                   expandImage.Visible = false;
                }
            }
            if (childbandRow != null)
            {
                childbandRow.Style.Add(HtmlTextWriterStyle.Display, childbandStatus.Value);
            }
        }

        internal virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Axelerate.BusinessLayerUITools.WebParts.HyperRow", this.row.ClientID);
            if (childbandRow != null)
            {
                descriptor.AddElementProperty("childbandRow", childbandRow.ClientID);
            }
            if (expandImage != null)
            {
                descriptor.AddElementProperty("Image", expandImage.ClientID);
            }
            if (scrollPanel != null)
            {
                descriptor.AddElementProperty("Panel", scrollPanel.ClientID);
            }

            yield return descriptor;
        }

    }
}
