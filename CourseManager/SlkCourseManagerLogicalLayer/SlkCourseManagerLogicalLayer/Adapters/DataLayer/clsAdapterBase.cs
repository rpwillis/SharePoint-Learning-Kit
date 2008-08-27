using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{
    /// <summary>
    /// Base class for the Adapters
    /// </summary>
    public class clsAdapterBase
    {
        /// <summary>
        /// Adapter's Method Types
        /// </summary>
        public enum AdapterMethodType
        {
            AdapterFactory = 0,
            AdapterInsert = 1,
            AdapterUpdate = 2,
            AdapaterDelete = 3
        }

        /// <summary>
        /// Adapter's Capabilities
        /// </summary>
        public enum AdapterCapabilities
        {
            AdapterReadOnly = 0,
            AdapterUpdatable = 1,
            AdapterAddDelete = 2,
        }

        /// <summary>
        /// Transforms data from the Datatable.
        /// </summary>
        /// <param name="FieldIndex">Field's Index</param>
        /// <param name="Row">DataRow of the DataTable</param>
        /// <returns></returns>
        public virtual object DataTransform(int FieldIndex, DataRow Row)
        {
            int InvertedIndex = (Row.ItemArray.Length - 1) - (FieldIndex);
            return Row[InvertedIndex];
        }

        /// <summary>
        /// Retrieves the Method Name for an specific AdapterMethodType object.
        /// </summary>
        /// <param name="MethodType">AdapterMethodType object.</param>
        /// <returns>An string with the Method's Name</returns>
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

        /// <summary>
        /// Gets the Adapter's Update Capabilities.
        /// </summary>
        public virtual AdapterCapabilities AdapterType
        {
            get
            {
                return AdapterCapabilities.AdapterUpdatable;
            }
        }
    }
}
