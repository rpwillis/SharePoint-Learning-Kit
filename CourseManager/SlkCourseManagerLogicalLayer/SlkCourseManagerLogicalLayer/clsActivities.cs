using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;

namespace Axelerate.SlkCourseManagerLogicalLayer
{

    /// <summary>
    /// This class represents a collection of Activities
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivities : BLListBase<clsActivities, clsActivity, clsActivity.BOGUIDDataKey>
    {
        #region "Factory Methods"
        //private static clsActivity activity = null;
        internal string ActivityStatusGUID = "";

        /// <summary>
        /// Returns in ascending order the Collection of Activities for a specific ActivityGroup
        /// </summary>
        /// <param name="ActivityGroupGUID">The GUID of the Activity Group requested</param>
        /// <returns>Return the Collection of Activities for a specific ActivityGroup in ascending order</returns>
        public static clsActivities GetCollectionByActivityGroupGUID(string ActivityGroupGUID)
        {

            BLCriteria Criteria = new BLCriteria(typeof(clsActivities));

            Criteria.AddBinaryExpression("ActivityGroupGUID_act", "ActivityGroupGUID", "=", ActivityGroupGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            Criteria.AddOrderedField("GUID_act", true);
            clsActivities collection = GetCollection(Criteria);
            
            BLCriteria newCriteria = new BLCriteria(typeof(clsActivities));            
            bool search = false;
            int expCounter = 1;
            foreach (clsActivity act in collection)
            {
                if ((act.ActivityStatusGUID.CompareTo(clsActivityStatus.AssignedStatus.GUID) == 0 && act.SLKAssignment != null) || act.ActivityStatusGUID.CompareTo(clsActivityStatus.UnassignedStatus.GUID) == 0)
                {
                    search = true;
                    newCriteria.AddBinaryExpression("GUID_act:" + expCounter, "GUID", "=", act.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorOr);
                    expCounter++;
                }
            }
            newCriteria.AddOrderedField("DueDate_act", true);

            clsActivities newCollection;  

            if (search)
            {
                newCollection = clsActivities.GetCollection(newCriteria);
            }
            else
            {                
                newCollection = new clsActivities(); 
            }

            newCollection.ActivityStatusGUID = ActivityGroupGUID;

            return newCollection;
        }

        /// <summary>
        /// Returns in ascending order the Collection of Activities for a specific ActivityGroup
        /// </summary>
        /// <param name="ActivityGroupGUID"></param>
        /// <returns></returns>
        public static clsActivities GetFullCollectionByActivityGroupGUID(string ActivityGroupGUID) 
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsActivities));

            Criteria.AddBinaryExpression("ActivityGroupGUID_act", "ActivityGroupGUID", "=", ActivityGroupGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            Criteria.AddOrderedField("DueDate_act", true);
            return clsActivities.GetCollection(Criteria);        
        }

        /// <summary>
        /// Creates a new Business Object
        /// </summary>
        /// <returns></returns>
        public override BLBusinessBase NewBusinessObject()
        {
            clsActivity newActivity = (clsActivity)base.NewBusinessObject();
            newActivity.ActivityStatus = clsActivityStatus.UnassignedStatus;
            //newActivity.ActivityStatusGUID = newActivity.ActivityStatus.GUID;

            if (ActivityStatusGUID != "")
            {
                //newActivity.ActivityGroup = activity.ActivityGroup;
                newActivity.ActivityGroupGUID = ActivityStatusGUID;
            }

            return newActivity;
        }        

        #endregion

        #region "ExtendedFilters"
        #endregion
    }
}
