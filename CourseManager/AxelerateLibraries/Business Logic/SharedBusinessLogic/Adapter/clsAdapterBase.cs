using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Adapters
{
    public class clsAdapterBase
    {
        public enum AdapterMethodType
        {
            AdapterFactory = 0,
            AdapterInsert = 1,
            AdapterUpdate = 2,
            AdapaterDelete = 3
        }

        public enum AdapterCapabilities
        {
            AdapterReadOnly = 0,
            AdapterUpdatable = 1,
            AdapterAddDelete = 2,
        }

        public virtual object DataTransform(int FieldIndex, DataRow Row)
        {
            int InvertedIndex = (Row.ItemArray.Length - 1) - (FieldIndex);
            return Row[InvertedIndex];
        }

        public virtual string GetMethodName(AdapterMethodType MethodType)
        {
            switch (MethodType)
            {
                case AdapterMethodType.AdapterFactory:
                    return "Get";
                case AdapterMethodType.AdapterInsert:
                    return "Insert";
                case AdapterMethodType.AdapterUpdate:
                    return "Update";
                case AdapterMethodType.AdapaterDelete:
                    return "Delete";
            }
            return "";
        }

        public virtual AdapterCapabilities AdapterType
        {
            get
            {
                return AdapterCapabilities.AdapterUpdatable;
            }
        }
    }
}
