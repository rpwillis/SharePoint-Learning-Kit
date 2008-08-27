using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties
{
    [Serializable()]
    public class clsPropertyValues : DetailsGUIDCollectionTemplate<clsPropertyValues , clsProperty, clsPropertyValue>
    {
        #region "Factory Methods"

        /// <summary>
        /// Returns a collection containing the values for the properties for a specific object.
        /// </summary>
        /// <param name="ObjectType">System.Type's full name for the object</param>
        /// <param name="ObjectGUID">GUID of the speficic object for which we are looking the properties </param>
        /// <returns></returns>
        [staFactory()] 
        public static clsPropertyValues GetCollection(Type ObjectType, string ObjectGUID)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsPropertyValue));
            Criteria.Filter = new clsInstancePropertyValues(ObjectType, ObjectGUID);
            clsPropertyValues ToReturn = GetCollection(Criteria);
            clsProperties AllPropertiesForType = clsProperties.GetCollection(ObjectType);
            foreach (clsProperty Property in AllPropertiesForType)
            {
                clsPropertyValue PropertyValue = (clsPropertyValue) ToReturn.Find("MasterGUID", Property.GUID);
               if (PropertyValue == null)
               {
                   PropertyValue = new clsPropertyValue();
                   //PropertyValue.MarkAsChild();
                   PropertyValue.MasterGUID = Property.GUID;
                   PropertyValue.ObjectGUID = ObjectGUID;
                   PropertyValue.PropertyValue = Property.DefaultValue;
                   ToReturn.Add(PropertyValue);
               }
            }

            return ToReturn;



        }


        #endregion

        #region "ExtendedFilters"
        public class clsInstancePropertyValues: DataLayerFilterBase
        {
            string m_ObjectGUID = "";
            Type m_ObjectType = null;


            public clsInstancePropertyValues(Type ObjectType, string ObjectGUID)
            {
                m_ObjectType = ObjectType;
                m_ObjectGUID = ObjectGUID;
                
            }


            public override string SelectCommandText(DataLayerAbstraction pDataLayer, BLFieldMapList FieldMapList, string AditionalFilter, ref System.Collections.Generic.List<DataLayerParameter> Parameters)
            {

                SQLDataLayer TypedDataLayer = (SQLDataLayer)pDataLayer;
                string NSelectSQL = " SELECT " + TypedDataLayer.get_FieldListString(FieldMapList, "") +
                                    " FROM         Properties INNER JOIN " +
                                    " PropertyValues ON Properties.GUID_prp = PropertyValues.MasterGUID_prv " +
                                    " WHERE     (Properties.BusinessObjectType_prp = '" + m_ObjectType.FullName + "') AND (PropertyValues.ObjectGUID_prv = '" + m_ObjectGUID + "') ";
                if (AditionalFilter != "")
                {
                    AddAditionalFilter(ref NSelectSQL, AditionalFilter, "AND");
                }
                //return the filtered SQL to use in the Project.GetCollection functions that use this object as Extended Filter Criteria.
                return NSelectSQL;

            }
        }
        #endregion


    }
}
