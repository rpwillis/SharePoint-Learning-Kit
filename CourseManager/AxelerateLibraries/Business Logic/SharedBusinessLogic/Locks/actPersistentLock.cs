using System;
using System.Text;
using System.Transactions;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLogic.SharedBusinessLogic.Support;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Locks
{
    /// <summary>
    /// This class Locks Objects
    /// </summary>
    public class actPersistentLock : BLCommandBase<SQLDataLayer>
    {
        #region "Constructors"

        /// <summary>
        /// This is the constructor, define the constructor of the class BLCommandBase.
        /// Recive the name of the command, in this case is a Stored Procedure, the array of parameters,
        /// the Data Source Name and a boolean value indicating if is a stored procedure
        /// </summary>
        public actPersistentLock()
            : base("SP_UpdateLock", new DataLayerParameter[0], "IrazuCRM", true)
        {            
        }

        #endregion
        
        #region "Public Properties and Methods"
        
        /// <summary>
        /// Defines the attribute PersistentLockTypeGUID, this is the GUID of the Type of Lock.
        /// </summary>
        public static DependencyProperty PersistentLockTypeGUIDProperty = System.Workflow.ComponentModel.DependencyProperty.Register("PersistentLockTypeGUID", typeof(string), typeof(actPersistentLock));

        /// <summary>
        /// Defines the property that return or set the value of the attribute PersistentLockTypeGUID.
        /// </summary>
        [Description("This is the GUID of the Type of Lock")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PersistentLockTypeGUID
        {
            get
            {
                return ((string)(base.GetValue(actPersistentLock.PersistentLockTypeGUIDProperty)));
            }
            set
            {
                base.SetValue(actPersistentLock.PersistentLockTypeGUIDProperty, value);
            }
        }

        /// <summary>
        /// Defines the attribute LockCount, this is the number of Locks for the object.
        /// </summary>
        public static DependencyProperty LockCountProperty = System.Workflow.ComponentModel.DependencyProperty.Register("LockCount", typeof(int), typeof(actPersistentLock));

        /// <summary>
        /// Defines the property that return or set the value of the attribute LockCount.
        /// </summary>
        [Description("This is the number of Locks for the object")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int LockCount
        {
            get
            {
                return ((int)(base.GetValue(actPersistentLock.LockCountProperty)));
            }
            set
            {
                base.SetValue(actPersistentLock.LockCountProperty, value);
            }
        }

        /// <summary>
        /// Defines the attribute ExpirationTime, this is the time that the PersistentLock will be active.
        /// if the value is "-1" the PersistentLock never will be inactive.
        /// </summary>
        public static DependencyProperty ExpirationTimeProperty = System.Workflow.ComponentModel.DependencyProperty.Register("ExpirationTime", typeof(int), typeof(actPersistentLock));

        /// <summary>
        /// Defines the property that return or set the value of the attribute ExpirationTime.
        /// </summary>
        [Description("This is the time that the PersistentLock will be active")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ExpirationTime
        {
            get
            {
                return ((int)(base.GetValue(actPersistentLock.ExpirationTimeProperty)));
            }
            set
            {
                base.SetValue(actPersistentLock.ExpirationTimeProperty, value);
            }
        }

        /// <summary>
        /// Defines the attribute ObjectGUID, this is the GUID of the object that will be lock.
        /// </summary>
        public static DependencyProperty ObjectGUIDProperty = System.Workflow.ComponentModel.DependencyProperty.Register("ObjectGUID", typeof(string), typeof(actPersistentLock));
          
        /// <summary>
        /// Defines the property that return or set the value of the attribute ObjectGUID.
        /// </summary>
        [Description("This is the GUID of the object that will be lock")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ObjectGUID
        {
	      get 
          { 
            return ((string)(base.GetValue(actPersistentLock.ObjectGUIDProperty))); 
          }
          set 
          { 
            base.SetValue(actPersistentLock.ObjectGUIDProperty, value); 
          }
        }

        /// <summary>
        /// Defines the attribute TimeOut, this is the Total time that the process tries to lock the object.
        /// </summary>
        public static DependencyProperty TimeOutProperty = System.Workflow.ComponentModel.DependencyProperty.Register("TimeOut", typeof(Double), typeof(actPersistentLock));

        /// <summary>
        /// Defines the property that return or set the value of the attribute TimeOut.
        /// </summary>
        [Description("This is the Total time that the process tries to lock the object")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Double TimeOut
        {
            get
            {
                return ((Double)(base.GetValue(actPersistentLock.TimeOutProperty)));
            }
            set
            {
                base.SetValue(actPersistentLock.TimeOutProperty, value);
            }
        }

        /// <summary>
        /// Defines the attribute TryTime, this is the time that the process waits to try to lock the object.
        /// </summary>
        public static DependencyProperty TryTimeProperty = System.Workflow.ComponentModel.DependencyProperty.Register("TryTime", typeof(int), typeof(actPersistentLock));

        /// <summary>
        /// Defines the property that return or set the value of the attribute TryTime.
        /// </summary>
        [Description("This is the time that the process waits to try to lock the object")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int TryTime
        {
            get
            {
                return ((int)(base.GetValue(actPersistentLock.TryTimeProperty)));
            }
            set
            {
                base.SetValue(actPersistentLock.TryTimeProperty, value);
            }
        }

        #endregion

        #region "DataPortal Overrides"

        /// <summary>
        /// This method calls the Execute, this is overrides because is necessary to chance the option IsolationLevel.Serializable
        /// and this method has the control of the tries of every Lock.
        /// </summary>
        protected override void DataPortal_Execute()
        {
            // If the process doesn't define a TimeOut, the value of the property is obtained of the Data Base.
            if (TimeOut == 0)
            {
                TimeOut = Convert.ToInt32(clsConfigurationProfile.Current.getPropertyValue("TimeOut"));
            }

            // If the process doesn't define a TryTime, the value of the property is obtained of the Data Base.
            if (TryTime == 0)
            {
                TryTime = Convert.ToInt32(clsConfigurationProfile.Current.getPropertyValue("TryTime"));
            }

            TimeSpan TryOutTime = new TimeSpan(0, 0, 0, 0, (int) TimeOut);
            DateTime InitialTime = DateTime.Now;
            string StrError = "";
            TimeSpan MillisecondsElapsed = DateTime.Now-InitialTime;

            // The TryOutTime is the valid time that the process can to try Lock the Object.
            while (MillisecondsElapsed <= TryOutTime)
            {
               StrError = "";
               try
               {
                   TransactionOptions Options = new TransactionOptions();
                   Options.IsolationLevel = IsolationLevel.Serializable;
                   using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, Options))
                   {
                       BLExecuteCommand();
                       scope.Complete();
                   }                   
               }

               // catch the exception that the stored procedure return
               catch(Exception ex)
               {
                   StrError = ex.InnerException.Message;  
               }

               MillisecondsElapsed = DateTime.Now - InitialTime;
               if (StrError == "")
               {
                   TryOutTime = new TimeSpan(0);
               }
               else
               {
                   // If the process doesn't obtaint the Lock, must be sleep some time, this time is the variable TryTime
                   System.Threading.Thread.Sleep(TryTime);
               }
            }

            // If the process doesn't obtaint the Lock and the Total Time is over,
            // the process end and a new Exception is throw.
            if (StrError != "")
            {
                throw new Exception(StrError);
            }
        }

        #endregion

        #region "DataLayer Access"

        /// <summary>
        /// This is the execute method, execute a Lock.
        /// The Lock is in a PersistenLock with the Object-GUID specified.
        /// </summary>
        public override void BLExecuteCommand()
        {
            m_Parameters = new DataLayerParameter[7];
            m_Parameters[0] = new DataLayerParameter("GUID", System.Guid.NewGuid().ToString());
            if (PersistentLockTypeGUID == "" || PersistentLockTypeGUID == null)
            {
                PersistentLockTypeGUID = clsPersistentLockType.GetDefaultPersistentLockType().GUID;
            }
            m_Parameters[1] = new DataLayerParameter("PersistentLockTypeGUID",PersistentLockTypeGUID);
            m_Parameters[2] = new DataLayerParameter("LockCount",1 ); // LockCount); /// hay que desarrollarlo en el SP
            m_Parameters[3] = new DataLayerParameter("ExpirationTime", clsPersistentLock.ExpirationTimeDefault);
            m_Parameters[4] = new DataLayerParameter("ObjectGUID",ObjectGUID);
            m_Parameters[5] = new DataLayerParameter("ADUser", clsActiveDirectory.CurrentUserName);
            m_Parameters[6] = new DataLayerParameter("CreationDate", DateTime.Now);

            base.BLExecuteCommand();
        }

        /// <summary>
        /// This method checks that the PersistenLock with the Object-GUID specified in the test, was truly Lock. 
        /// </summary>
        public override void Test()
        {
            clsPersistentLock ObjectLoked = clsPersistentLock.GetPersistentLockByObjectGUID(ObjectGUID);
            if (ObjectLoked.ObjectGUID == "")
            {                                
                throw new Exception(Resources.ErrorMessages.lockWithObjectGUID + ObjectGUID + Resources.ErrorMessages.errShouldBeLocked);               
            }
        }    
        #endregion
    }
}

