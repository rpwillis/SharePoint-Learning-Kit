using System;
using System.Collections.Generic;
using System.Text;

namespace Axelerate.BusinessLayerUITools.Interfaces
{
    public interface ICustomValueEditor
    {
        Object DataSource
        {
            get;
            set;
        }

        string DataValueField
        {
            get;
            set;
        }

        string DataValueFormat
        {
            get;
            set;
        }
        void DataBind();

        object Value
        {
            get;
            set;
        }

        bool IsReadOnly
        {
            get;
            set;
        }
    }
}
