using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Xml;

namespace Axelerate.BusinessLayerUITools.Misc
{
    public class HyperColumn
    {

        public enum enuEnterEditMode
        {
            OnClick,
            Allways
        }


        internal int ColumnIndex = -1;

        /// <summary>
        /// Indicate if this column is part of a RepeatableColumn that has a column Datasource related to the row datasource
        /// </summary>
        internal bool RepeatableColumn = false;
        
        /// <summary>
        /// Property of the column datasource binded to this column
        /// </summary>
        internal string PropertyBound = null;
        
        /// <summary>
        /// Property of the row datasource binded to this column
        /// </summary>
        internal string Property = null;

        internal object RepeatableColumnKey = null;

        internal string PropertyFriendlyName = null;

        internal HiddenField ValueField = null;

        internal XmlNode ContentXml = null;
        internal int repeatIndex = 0;

        internal enuEnterEditMode m_EditMode = enuEnterEditMode.OnClick;
        

        public bool IsNewRowEditable
        {
            get
            {
                return ValueField != null;
            }
        }

        public enuEnterEditMode EditMode
        {
            get
            {
                return m_EditMode;
            }
            set
            {
                m_EditMode = value;
            }
        }

        private bool _IsEditable = false;
        public bool IsEditable
        {
            get
            {
                return _IsEditable;
            }
            set
            {
                _IsEditable = value;
            }
        }
       

        #region Constructors

        public HyperColumn(int index, bool RepeatableCol)
        {
            ColumnIndex = index;
            RepeatableColumn = RepeatableCol;
        }

        #endregion
    }
}
