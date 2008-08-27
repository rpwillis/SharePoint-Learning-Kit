using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.PoolCore
{
    public class clsPoolParameters<T>
    {
        #region "Variables"

        public const int DefaultValue = -1;
        public const int DefaultGetRetrys = 3;

        #endregion

        #region "Properties and Methods"

        public static int SleepTime
        {
            get
            {
                string TTypeName = typeof(T).Name;
                TTypeName += "_SleepTime";
                try
                {
                    return Convert.ToInt32(clsConfigurationProfile.Current.getPropertyValue(TTypeName));
                }
                catch
                {
                    return DefaultValue;
                }
            }
        }

        public static int UsedAgingTime
        {
            get
            {
                string TTypeName = typeof(T).Name;
                TTypeName += "_UsedAgingTime";
                try
                {
                    return Convert.ToInt32(clsConfigurationProfile.Current.getPropertyValue(TTypeName));
                }
                catch
                {
                    return DefaultValue;
                }
            }
        }

        public static int FreeAgingTime
        {
            get
            {
                string TTypeName = typeof(T).Name;
                TTypeName += "_FreeAgingTime";
                try
                {
                    return Convert.ToInt32(clsConfigurationProfile.Current.getPropertyValue(TTypeName));
                }
                catch
                {
                    return DefaultValue;
                }
            }
        }

        public static int MaxPoolSize
        {
            get
            {
                string TTypeName = typeof(T).Name;
                TTypeName += "_MaxPoolSize";
                try
                {
                    return Convert.ToInt32(clsConfigurationProfile.Current.getPropertyValue(TTypeName));
                }
                catch 
                {
                    return DefaultValue;
                }
            }
        }

        #endregion
    }
}
