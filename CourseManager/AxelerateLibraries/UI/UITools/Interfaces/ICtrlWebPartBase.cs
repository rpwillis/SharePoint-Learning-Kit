using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Axelerate.BusinessLayerUITools.Interfaces
{
    public interface ICtrlWebPartBase
    {
        String FactoryParameters
        {
            get;
            set;
        }

        String FactoryMethod
        {
            get;
            set;
        }

        String ClassName
        {
            get;
            set;
        }

        ITemplate HeaderTemplate
        {
            get;
            set;
        }

        ITemplate FooterTemplate
        {
            get;
            set;
        }
    }
}
