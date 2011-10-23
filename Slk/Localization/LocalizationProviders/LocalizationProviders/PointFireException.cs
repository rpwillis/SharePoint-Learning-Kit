using System;
using System.Collections.Generic;
using System.Text;

namespace Itworx.Localization.LocalizationProviders
{
    [Serializable()]
    public class PointFireException:ApplicationException
    {
        public PointFireException()
        {
        }

        public PointFireException(string message)
            : base(message)
        {
        }

        public PointFireException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
