using System;
using System.Collections.Generic;
using System.Text;

namespace Axelerate.BusinessLayerUITools.Interfaces
{
    public interface ISearchConsumer
    {
        void RegisterProvider(ISearchProvider Provider);
    }
}
