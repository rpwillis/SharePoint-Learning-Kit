using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties
{
    [Serializable()]
    public class clsProfile_Properties : BLListBase<clsProfile_Properties, clsProfile_Property, clsProfile_Property.BOGUIDDataKey>
    {
        #region "Shared Properties and Methods"
        [staFactory()]
        public static clsProfile_Properties GetPropertybyProfileName(string ProfileName, string PropertyName)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsProfile_Properties));
            Criteria.Filter = new clsConfigurationPropertyByProfileName(ProfileName, PropertyName);
            return GetCollection(Criteria);
        }
        #endregion

        #region "ExtendedFilters"
        public class clsConfigurationPropertyByProfileName : DataLayerFilterBase
        {
            //here go the parameters of the SelectComandText generator must be use.
            private string m_ProfileName = "";
            private string m_PropertyName = "";

            /// <summary>
            /// Contructor of the clsConfigurationPropertiyByProfileFilter.
            /// </summary>
            /// <param name="ProjectGUID">GUID of the profile</param>
            public clsConfigurationPropertyByProfileName(string ProfileName, string PropertyName)
            {
                m_ProfileName = ProfileName;
                m_PropertyName = PropertyName;
            }


            /// <summary>
            /// This if the function that do the work. They is called by de Datalayer when create the SQL to perform a GetCollection
            /// and this object is setted in the criteria.
            /// </summary>
            /// <param name="pDataLayer">This is the datalayer calling this function</param>
            /// <param name="FieldMapList">This is the fields that must be in the select</param>
            /// <param name="AditionalFilter">Aditional Filters that could be added to the criteria</param>
            /// <param name="Parameters">Parameters that could be added for the sql generator</param>
            /// <returns></returns>
            public override string SelectCommandText(DataLayerAbstraction pDataLayer, BLFieldMapList FieldMapList, string AditionalFilter, ref System.Collections.Generic.List<DataLayerParameter> Parameters)
            {
                //the first thing that we need is get a typed datalayer
                SQLDataLayer TypedDataLayer = (SQLDataLayer)pDataLayer;
                /*
                 * SELECT * FROM ConfigurationProfiles inner join
                 * (ConfigurationProperties inner join Profiles_Properties on ConfigurationProperties.GUID_cpr = Profiles_Properties.DetailGUID_ppr)
                 * ON ConfigurationProfiles.GUID_cfp = Profiles_Properties.MasterGUID_ppr
                 * WHERE ConfigurationProfiles.GUID_cfp = '1';
                 */
                //The function TypedDataLayer.get_FieldListString retrive the list of fields to use in the select in the correct order for the business object; 
                //Use all time get_FieldListString for this and NEVER try to put manualy the fields or use will cards in the select beacuse this can produce unspected results.
                string NSelectSQL = " SELECT ";

                NSelectSQL += TypedDataLayer.get_FieldListString(FieldMapList, "") +
                                    " FROM ConfigurationProfiles inner join " +
                                    " (ConfigurationProperties inner join Profiles_Properties on ConfigurationProperties.GUID_cpr = Profiles_Properties.DetailGUID_ppr) " +
                                    " ON ConfigurationProfiles.GUID_cfp = Profiles_Properties.MasterGUID_ppr ";
                //Now we add the where with the filters over the keywords using the FilterWords list.
                    NSelectSQL += " WHERE ";

                    NSelectSQL += "ConfigurationProfiles.Name_cfp = '" + m_ProfileName + "' AND ";
                    NSelectSQL += "ConfigurationProperties.Name_cpr = '" + m_PropertyName + "' " +
                                  " or ConfigurationProfiles.Name_cfp is null ";
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
