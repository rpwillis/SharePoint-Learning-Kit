using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.BusinessLogic.SharedBusinessLogic.Ranking;
using Axelerate.BusinessLogic.SharedBusinessLogic.Support;
using Axelerate.BusinessLogic.SharedBusinessLogic.Contacts;


namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable(), SecurityToken("clsADUser", "clsADUsers", "MIPCustom")]
    [SecurableClass( SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsADUser : GUIDTemplate<clsADUser>, IRankeable
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsADUser), "ADUsers", "_adu", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_UserName = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Domain = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "Contact", false)]
        private string m_ContactGUID = "";

        [CachedForeignObject("Contact", typeof(clsContact), "ContactGUID_adu", "GUID_ctc", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsContact m_Contact = null;

        [NonSerialized()] //TODO: Csla no longer supported , NotUndoable()]
        private clsRoles m_RolesCache = null;

        #endregion

        #region "Business Properties and Methods"

        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string UserName
        {
            get { return m_UserName; }
            set
            {
                m_UserName = value.ToUpper().Trim();
                PropertyHasChanged();
            }
        }

        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string Domain
        {
            get { return m_Domain; }
            set
            {
                m_Domain = value.ToUpper().Trim();
                PropertyHasChanged();
            }
        }
             
        [SecurableProperty( SecurablePropertyAttribute.SecurableTypes.Read)]
        public string ContactGUID
        {
            get { return m_ContactGUID; }
            set
            {
                m_ContactGUID = value;
                PropertyHasChanged();
            }
        }

        [SecurableProperty( SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsRecommendations Recommendations
        {
            get
            {
                return clsRecommendations.GetCollection(this);
            }
        }

        [SecurableProperty( SecurablePropertyAttribute.SecurableTypes.Read)]
        public clsContact Contact
        {
            get { return BLGUIDForeignPropertyCache<clsContact>.GetProperty(this, ref m_Contact, m_ContactGUID); }
            set
            {
                BLGUIDForeignPropertyCache<clsContact>.SetProperty(ref m_Contact, value, ref m_ContactGUID, true);
                PropertyHasChanged();
            }
        }
        public static clsADUser GetByName(string name)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsADUser));
            Criteria.AddBinaryExpression("UserName_adu", "UserName", "=", name.ToUpper().Trim(), BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            clsADUser ADUser = null;
            try
            {
                ADUser = GetObject(Criteria);
            }
            catch (Exception ex)
            {
                ADUser = null;
            }
            return ADUser;
        }
        /// <summary>
        /// Return the roles associated to this ADUser
        /// </summary>
        public clsRoles GetRoles()
        {
            if (m_RolesCache == null)
                    m_RolesCache = clsRoles.GetDetailsInMaster(this);
                return m_RolesCache;
        }

        /// <summary>
        /// If Associated is true get the roles associated to this ADUser
        /// Else get the roles not associated to this ADUser
        /// </summary>
        /// <param name="Associated"></param>
        /// <returns></returns>
        public clsRoles GetRoles(bool Associated)
        {
            if (Associated)
            {
                if (m_RolesCache == null)
                    m_RolesCache = clsRoles.GetDetailsInMaster(this);
            }
            else
            {
                if (m_RolesCache == null)
                    m_RolesCache = clsRoles.GetDetailsOutOfMaster(this);
            }
            return m_RolesCache;

        }

        /// <summary>
        /// Get the roles of respective Level associated to this ADUser 
        /// </summary>
        /// <param name="RoleLevelGUID"></param>
        /// <returns></returns>
        public clsRoles GetRoles(String RoleLevelGUID)
        {
            return null;
        }

        /// <summary>
        /// Check if the user has the Permission to specific RoleLevel and Object
        /// For example can check if the user has "Write" Permision for Role Level "Project" over the project with guid "1"
        /// If the role not has set this permission, this function check if the user has permission in the
        /// Master object (next role level up!) if has one, If not return false.
        /// </summary>
        /// <param name="Permission"></param>
        /// <param name="RoleLevelGUID"></param>
        /// <param name="Object"></param>
        /// <returns></returns>
        public bool HasPermission(clsPermission Permission, String RoleLevelGUID, BLBusinessBase Object)
        {
            return false;
        }

        /// <summary>
        /// Check if the user has the Permission to specific RoleLevel and Object
        /// For example can check if the user has "Write" Permision for Role Level "Project" over the project with guid "1"
        /// If the role not has set this permission, this function check if the user has permission in the
        /// Master object (next role level up!) if has one, If not return false.
        /// </summary>
        /// <param name="Permission"></param>
        /// <param name="RoleLevelGUID"></param>
        /// <param name="Object"></param>
        /// <returns></returns>
        public bool HasPermission(string  Permission, String RoleLevelGUID, string ObjectGUID)
        {
            return false;
        }

        /// <summary>
        /// Add this user to the Role for specific Object
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="Role"></param>
        public void AddToRole(BLBusinessBase Object, clsBaseRole Role)
        {
            Role.AddUser(this, Object);
        }

        /// <summary>
        /// Add this user to the Role for specific Object
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="Role"></param>
        public void AddToRole(string ObjectGUID, string RoleGUID)
        {
            clsBaseRole Role = clsBaseRole.GetObjectByGUID(RoleGUID, null);
            Role.AddUser(ObjectGUID, RoleGUID);
        }

        /// <summary>
        /// Remove the user from the Role for specific Object
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="Role"></param>
        public void RemoveFromRole(BLBusinessBase Object, clsBaseRole Role)
        {
        }

        /// <summary>
        /// Remove the user from the Role for specific Object
        /// </summary>
        /// <param name="ObjectGUID">GUID of the Portal, Project or Object that this role apply</param>
        /// <param name="RoleGUID">GUID of the Role object where the user must be added</param>
        public void RemoveFromRole(string  ObjectGUID, string RoleGUID)
        {
        }

        /// <summary>
        /// Check the name in the domain if found the identity then return the correct login name if not return ""
        /// </summary>
        /// <param name="LoginName"></param>
        /// <returns></returns>
        public string CheckLoginName(string LoginName)
        {
            return "";
        }

        public override string  ToString()
        {
 	        return m_UserName;
        }

        #region "Recommendations"       
        
        /// <summary>
        /// Makes a recommendation, if the recommendation already exists it gets updated
        /// </summary>
        /// <param name="user">the user who is submitting the recommendation</param>
        /// <param name="rank">the recommendation value</param>
        /// <param name="comment">an additional comment</param>
        public void Rank(clsADUser user, int rank, string comment)
        {
            clsRecommendations recommendations = null;
            String judge = user.GUID;
            if (!(this.GUID.Equals(judge)))      //checks if a user is ranking himself
            {
                //looks for all recomendations made by judge (the person that submitted the recommendation) to user (the person that is being recommended)
                BLCriteria Criteria = new BLCriteria(typeof(clsRecommendations));
                Criteria.AddBinaryExpression("ADUserJudgeGUID_rcm", "ADUserJudgeGUID", "=", judge, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                Criteria.AddBinaryExpression("ADUserRecommendedGUID_rcm", "ADUserJudgeGUID", "=", this.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                recommendations = clsRecommendations.GetCollection(Criteria);


                clsRecommendation recommendation = clsRecommendation.NewObject();
                if (recommendations.Count == 0)         //no recommendation found, create a new one
                {
                    recommendation.ADUserJudgeGUID = judge;
                    recommendation.ADUserRecommendedGUID = this.GUID;
                    //recommendation.MarkAsChild();
                    recommendations.Add(recommendation);
                }
                else
                {
                    recommendation = recommendations[0];    //a previous recommendation found, 
                }

                //fills  the common data and saves the object
                recommendation.Date = DateTime.Now;
                recommendation.Score = rank;
                recommendation.Comment = comment;
                recommendations.Save();
            }
            else
                throw new Exception(Axelerate.BusinessLogic.SharedBusinessLogic.Resources.ProjectResources.Exception_SelfRanking);
        }

        
       /// <summary>
       /// Gets the actual recommendation score submitted by user
       /// </summary>
       /// <param name="user">user who submitted a previous recommendation</param>
       /// <returns>the actual recommendation score</returns>
        public int ActualRank(clsADUser user)
        {
            clsRecommendations recommendations = null;
            String judge = user.GUID;
            if (!(this.GUID.Equals(judge)))      //checks if a user is ranking himself (this saves a DB Query)
            {
                //looks for all recomendations made by judge (the person that submitted the recommendation) to user (the person that is being recommended)
                BLCriteria Criteria = new BLCriteria(typeof(clsRecommendations));
                Criteria.AddBinaryExpression("ADUserJudgeGUID_rcm", "ADUserJudgeGUID", "=", judge, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                Criteria.AddBinaryExpression("ADUserRecommendedGUID_rcm", "ADUserJudgeGUID", "=", this.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                recommendations = clsRecommendations.GetCollection(Criteria);

                //if no previous recommendation was found, returns 0
                if (recommendations.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return recommendations[0].Score;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the average value of the recommendations received by THIS user
        /// </summary>
        /// <returns>the average value of the recommendations received by THIS user</returns>
        public float AverageRank()
        { 
            //Gets the recomendations received by THIS user
            clsRecommendations recommendations = null;
            BLCriteria Criteria = new BLCriteria(typeof(clsRecommendations));
            Criteria.AddBinaryExpression("ADUserRecommendedGUID_rcm", "ADUserRecommendedGUID", "=", this.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            recommendations = clsRecommendations.GetCollection(Criteria);

            //gets the total score received by THIS user
            int total = 0;
            foreach (clsRecommendation recom in recommendations)
            {
                total += recom.Score;
            }
            //returns the average score of the recomendations
            return total / recommendations.Count;
        }

        /// <summary>
        /// Gets the biography of THIS user
        /// </summary>
        public String Biography()
        {
            return this.Contact.Biography;
        }

        /// <summary>
        /// Gets the amount of recommendations received by THIS user
        /// </summary>
        /// <returns></returns>
        public int RankQuantity()
        {
            //Gets the recomendations received by THIS user
            BLCriteria Criteria = new BLCriteria(typeof(clsRecommendations));
            Criteria.AddBinaryExpression("ADUserRecommendedGUID_rcm", "ADUserRecommendedGUID", "=", this.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            //return the amount of recomendations received
            return clsRecommendations.GetCount(Criteria, null);            
        }

        /// <summary>
        /// Gets the amount of projects this user is works in
        /// </summary>
        /// <returns></returns>
        /*public int ProjectQuantity()
        {
            return clsProjects.GetCountByADUserGUID(this.GUID);
        }*/
        #endregion
        #endregion
    }
}
