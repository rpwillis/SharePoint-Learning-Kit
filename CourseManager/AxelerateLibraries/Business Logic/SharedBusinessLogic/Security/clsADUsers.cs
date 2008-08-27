using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{

    [Serializable()]
    public class clsADUsers : BLListBase<clsADUsers, clsADUser, clsADUser.BOGUIDDataKey>
    {

        #region "Shared Properties and Methods"

        /// <summary>
        /// Returns a collection containing the user with the User Name
        /// </summary>
        /// <param name="UserName">User's domain login name</param>
        /// <returns>Returns a collection containing the user with the User Name.  An empty collection if the user is not found </returns>
        [staFactory()]
        public static clsADUsers GetCollectionByUserName(string UserName)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsADUsers));
            Criteria.AddBinaryExpression("UserName_adu", "UserName", "=", UserName.ToUpper().Trim(), BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return GetCollection(Criteria);
        }
        [staFactory()]
        public static clsADUsers GetCollectionByProjectGUID(string ProjectGUID)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsADUsers));
            /* Gustavo --> Regis & Daniel: I think we might be better off comunicating by email or MSN, just a hunch.
             * Daniel --> Regis & Gustavo: 
             * I add the ExtendedFilter that is the correct mode to do this type of things, not is too complex. :D
             * The tecnic uses in the code in coments not work propertly because the AddBinaryExpression interpret the
             * type of the 3 parameter as string and add a single quote to the begining and the end of the strInnerSQL text.
             // Regis --> Gustavo
             // i tried to debug this GetCollection and it kind of works -- but then there is an issue somewhere
             // in the VB code. I dont think this is the right way, but i see that you know SQL :-)
             string strInnerSQL = 
                 "(SELECT [ADUsers_Roles].[MasterGUID_adr] as [GUID] from [ADUsers_Roles] INNER JOIN [Roles] ON " +
                  "[Roles].[GUID_rol]=[ADUsers_Roles].[DetailGUID_adr] WHERE [Roles].[RoleLevel_rol]='PROJECT' " +
                  "AND [ADUsers_Roles].[ObjectGUID_adr] = '" + ProjectGUID + "')";

             Criteria.AddBinaryExpression("GUID_adu", "GUID", " in ", strInnerSQL, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);*/
            Criteria.Filter = new clsADUserByProjectFilter(ProjectGUID);
            return GetCollection(Criteria);
        }

        #endregion

        #region "ExtendedFilters"

        /// <summary>
        /// This class filter the ADUsers by the selecting only that are member of a role that is in project
        /// with guid equal to ProjectGUID parameter.
        /// </summary>
        public class clsADUserByProjectFilter : DataLayerFilterBase
        {
            //here go the parameters of the SelectComandText generator must be use.
            private string m_ProjectGUID="";

            /// <summary>
            /// Contructor of the clsADUserByProjectFilter.
            /// </summary>
            /// <param name="ProjectGUID">GUID of the project</param>
            /// <param name="pOrderByField">Order by Expression</param>
            /// <param name="pConcatenationOperator">Operator to use as concatenator of the KeyWords filter.</param>
            public clsADUserByProjectFilter(string ProjectGUID)
            {
                m_ProjectGUID = ProjectGUID;
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
                 * SELECT [ADUsers_Roles].[MasterGUID_adr] as [GUID] from [ADUsers_Roles] INNER JOIN [Roles] ON " +
                 "[Roles].[GUID_rol]=[ADUsers_Roles].[DetailGUID_adr] WHERE [Roles].[RoleLevel_rol]='PROJECT' " +
                 "AND [ADUsers_Roles].[ObjectGUID_adr] = '" + ProjectGUID
                 */
                //Generate the Select command; the DISTINCT is used because this join add one project row for each keyword, and this collections only must have one unique row for project
                //The function TypedDataLayer.get_FieldListString retrive the list of fields to use in the select in the correct order for the business object; 
                //Use all time get_FieldListString for this and NEVER try to put manualy the fields or use will cards in the select beacuse this can produce unspected results.
                string NSelectSQL = " SELECT DISTINCT ";

                NSelectSQL += TypedDataLayer.get_FieldListString(FieldMapList, "") +
                                    " FROM (ADUsers INNER JOIN ADUsers_Roles ON GUID_adu = MasterGUID_adr) " +
                                    " INNER JOIN [Roles] ON [Roles].[GUID_rol]=[ADUsers_Roles].[DetailGUID_adr] ";
                //Now we add the where with the filters over the keywords using the FilterWords list.
                if (AditionalFilter != "" || m_ProjectGUID != "")
                {
                    NSelectSQL += " WHERE ";
                }
                if (m_ProjectGUID != "")
                {
                    NSelectSQL += "([Roles].[RoleLevel_rol]='PROJECT' " +
                                  "AND [ADUsers_Roles].[ObjectGUID_adr] = '" + m_ProjectGUID + "') ";
                    
                }
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
