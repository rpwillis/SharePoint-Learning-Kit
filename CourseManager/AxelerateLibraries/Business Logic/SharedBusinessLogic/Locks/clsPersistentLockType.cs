
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Locks
{
	/// <summary>
    /// This class represents a Type of Lock supported by the documents 
    /// <threadsafety static="true" instance="false"/>
	/// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
	public class clsPersistentLockType : GUIDNameBusinessTemplate<clsPersistentLockType>
	{
        #region "DataLayer Overrides"

		/// <summary>
        /// The Data layer is required to establish the connection
		/// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsPersistentLockType), "PersistentLockTypes", "_plt", false, false, "Shared");
		
		/// <summary>
        /// Gets or sets the DataLayer value
		/// </summary>
        public override DataLayerAbstraction DataLayer
		{
			get { return m_DataLayer; }
			set { }
		}
			
        #endregion
		
		#region "Business Object Data"
		
		/// <summary>
        /// Defines the Attribute LifeTime
		/// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
		private int m_LifeTime = -1;
		
		/// <summary>
        /// Defines the Attribute LockReads
		/// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_LockReads = false;
		
		/// <summary>
        /// Defines the Attribute LockWrites
		/// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_LockWrites = false;
			
        #endregion

		#region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute LifeTime
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
		public int LifeTime
		{
			get{return m_LifeTime;}
			set{
				m_LifeTime = value;
				PropertyHasChanged();
			}
		}

        /// <summary>
        /// Gets or sets the value of the attribute LockReads
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
		public bool LockReads
		{
			get{return m_LockReads;}
			set{
				m_LockReads = value;
				PropertyHasChanged();
			}
		}

        /// <summary>
        /// Gets or sets the value of the attribute LockWrites
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public bool LockWrites
		{
			get{return m_LockWrites;}
			set{
				m_LockWrites = value;
				PropertyHasChanged();
			}
		}


        private static clsPersistentLockType m_CheckOutPersistentLockType = null;

        /// <summary>
        /// Get the SQL Storage Type
        /// </summary>
        public static clsPersistentLockType CheckOutPersistentLockType
        {
            get
            {
                if (m_CheckOutPersistentLockType == null)
                {
                    m_CheckOutPersistentLockType = GetObjectByGUID("2", null);
                }
                return m_CheckOutPersistentLockType;
            }
        }


        #endregion

        #region "Factory Methods"

        private static clsPersistentLockType m_DefaultPersistentLockType = null;

        [staFactory()]
        public static clsPersistentLockType GetDefaultPersistentLockType()
        {
            if (m_DefaultPersistentLockType == null)
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsPersistentLockType));
                Criteria.AddBinaryExpression("Name_plt", "Name", "=", clsConfigurationProfile.Current.getPropertyValue("PersistentLockType"), BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                m_DefaultPersistentLockType = GetObject(Criteria);
            }
            return m_DefaultPersistentLockType;
        }
        #endregion
    }
}
