
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Locks
{
	/// <summary>
    ///  This class represents a Persistent Lock
    /// <threadsafety static="true" instance="false"/>
	/// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.Read), staReadOnly]
	public class clsPersistentLock : GUIDTemplate<clsPersistentLock>
	{
		#region "DataLayer Overrides"

		/// <summary>
        /// The Data layer is required to establish the connection
		/// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsPersistentLock), "PersistentLocks", "_loc", false, false, "Shared");
		
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
        /// Defines the Attribute PersistentLockTypeGUID
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "PersistentLockType", false)]
		private string m_PersistentLockTypeGUID = "";

        /// <summary>
        /// Defines a Class clsPersistentLockType, this class contains a Type of Lock which will be associated to the attribute PersistentLockTypeGUID
        /// </summary>
        [CachedForeignObject("PersistentLockType", typeof(clsPersistentLockType), "PersistentLockTypeGUID_loc", "GUID_plt", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsPersistentLockType m_PersistentLockType = null;
	
		/// <summary>
        /// Defines the Attribute LockCount
		/// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
		private int m_LockCount = -1;
		
		/// <summary>
        /// Defines the Attribute ExpirationTime
		/// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
		private int m_ExpirationTime = -1;

        /// <summary>
        /// GUID of the object that is locked
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ObjectGUID = "";

        /// <summary>
        /// ADUser that locked the object
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ADUser = "";

        /// <summary>
        /// Defines the Attribute CreationDate
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_CreationDate = new DateTime();

        #endregion

		#region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute PersistentLockTypeGUID
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
        public string PersistentLockTypeGUID
		{
			get{return m_PersistentLockTypeGUID;}
			set{
				m_PersistentLockTypeGUID = value;
				PropertyHasChanged();
			}
		}

        /// <summary>
        /// Gets or sets the correct Guid to the class PersistentLockType using the specific key PersistentLockTypeGUID
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
        public clsPersistentLockType PersistentLockType
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsPersistentLockType>.GetProperty(this, ref m_PersistentLockType, m_PersistentLockTypeGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsPersistentLockType>.SetProperty(ref m_PersistentLockType, value, ref m_PersistentLockTypeGUID, true);
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute LockCount
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
		public int LockCount
		{
			get{return m_LockCount;}
			set{
				m_LockCount = value;
				PropertyHasChanged();
			}
		}

        /// <summary>
        /// Gets or sets the value of the attribute ExpirationTime
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
		public int ExpirationTime
		{
			get{return m_ExpirationTime;}
			set{
				m_ExpirationTime = value;
				PropertyHasChanged();
			}
		}

        /// <summary>
        /// Gets or sets the locked object GUID
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
        public string ObjectGUID
        {
            get { return m_ObjectGUID; }
            set
            {
                m_ObjectGUID = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ADUser
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
        public string ADUser
        {
            get { return m_ADUser; }
            set
            {
                m_ADUser = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute CreationDate
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Read)]
        public DateTime CreationDate
        {
            get { return m_CreationDate.Date; }
            set
            {
                m_CreationDate = value;
                PropertyHasChanged();
            }
        }

        private static int m_ExpirationTimeDefault = 0;

        /// <summary>
        /// Get the Default Time
        /// </summary>
        public static int ExpirationTimeDefault
        {
            get
            {
                if (m_ExpirationTimeDefault == 0)
                {
                    m_ExpirationTimeDefault = -1;
                }
                return m_ExpirationTimeDefault;
            }
        }

        #endregion

        #region "Factory Methods"
        /// <summary>
        /// Return the Persistent Lock by Object GUID
        /// </summary>
        /// <returns>Return the Persistent Lock by Object GUID</returns>
        [staFactory("ObjectGUID", "1")]
        public static clsPersistentLock GetPersistentLockByObjectGUID(string ObjectGUID)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsPersistentLock));
            Criteria.AddBinaryExpression("ObjectGUID_loc", "ObjectGUID", "=", ObjectGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return TryGetObject (Criteria);
        }
        #endregion
    }
}
