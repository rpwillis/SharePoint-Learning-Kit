using System;
using System.Text;
using System.Transactions;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLogic.SharedBusinessLogic.Support;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Locks
{
    /// <summary>
    /// This class UnLocks Objects, this class has all the rights,
    /// and can UnLock any Object Locked.
    /// </summary>
    class actMasterPersistentUnLock : BLCommandBase<SQLDataLayer>
    {
        #region "Constructors"
        /// <summary>
        /// This is the constructor, define the constructor of the class BLCommandBase.
        /// Recive the name of the command, in this case is a Stored Procedure, the array of parameters,
        /// the Data Source Name and a boolean value indicating if is a stored procedure
        /// </summary>
        public actMasterPersistentUnLock()
            : base("SP_UpdateUnLock", new DataLayerParameter[0], "IrazuCRM", true)
        {
        }
        #endregion

        #region "Public Properties and Methods"

        /// <summary>
        /// Defines the attribute ObjectGUID, this is the GUID of the object that will be Unlock.
        /// </summary>
        public static DependencyProperty ObjectGUIDProperty = System.Workflow.ComponentModel.DependencyProperty.Register("ObjectGUID", typeof(string), typeof(actMasterPersistentUnLock));

        /// <summary>
        /// Defines the property that return or set the value of the attribute ObjectGUID.
        /// </summary>
        [Description("This is the GUID of the object that will be Unlock")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ObjectGUID
        {
            get
            {
                return ((string)(base.GetValue(actMasterPersistentUnLock.ObjectGUIDProperty)));
            }
            set
            {
                base.SetValue(actMasterPersistentUnLock.ObjectGUIDProperty, value);
            }
        }

        #endregion

        #region "DataPortal Overrides"

        /// <summary>
        /// This method calls the Execute, is overrides because is necessary to chance the option IsolationLevel.Serializable.
        /// </summary>
        protected override void DataPortal_Execute()
        {
            TransactionOptions Options = new TransactionOptions();
            Options.IsolationLevel = IsolationLevel.Serializable;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, Options))
            {
                BLExecuteCommand();
                scope.Complete();
            }
        }
        #endregion

        #region "DataLayer Access"

        /// <summary>
        /// This is the execute method, execute the Unlock.
        /// The UnLock is in a PersistenLock with the Object-GUID specified.
        /// </summary>
        public override void BLExecuteCommand()
        {
            m_Parameters = new DataLayerParameter[2];
            m_Parameters[0] = new DataLayerParameter("ObjectGUID", ObjectGUID);
            m_Parameters[1] = new DataLayerParameter("ADUser", clsActiveDirectory.CurrentUserName);
            string StrError = "";

            try
            {
                base.BLExecuteCommand();
            }
            // catch the exception that the stored procedure return
            catch (Exception ex)
            {
                StrError = ex.InnerException.Message;
            }

            // If the PersistenLock with the Object-GUID specified isn't Lock, a new Exception is throw.
            if (StrError != "")
            {
                throw new Exception(StrError);
            }
        }

        /// <summary>
        /// This method checks that the PersistenLock with the Object-GUID specified in the test, was truly UnLock.
        /// </summary>
        public override void Test()
        {
            clsPersistentLock ObjectUnLoked = clsPersistentLock.GetPersistentLockByObjectGUID(ObjectGUID);

            // If the PersistenLock with the Object-GUID specified in the test until exist, a new Exception is throw. 
            if (ObjectUnLoked.ObjectGUID != "")
            {
                throw new Exception(Resources.ErrorMessages.lockWithObjectGUID + ObjectGUID + Resources.ErrorMessages.errShouldBeUnlocked);
            }
            // The correct operation is: the PersistenLock with the Object-GUID specified in the test was UnLock. 
        }
        #endregion
    }
}
