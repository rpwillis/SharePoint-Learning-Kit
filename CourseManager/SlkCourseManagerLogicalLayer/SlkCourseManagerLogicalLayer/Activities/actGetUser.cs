using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;


namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetUser : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsUser m_User = null; 
        
        #endregion

        #region "Public Properties and Methods"        

        public static DependencyProperty UserGUIDProperty = DependencyProperty.Register("UserGUID", typeof(string), typeof(actGetUser));

        [DescriptionAttribute("GUID of the User to recover")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string UserGUID
        {
            get
            {
                return ((string)(base.GetValue(actGetUser.UserGUIDProperty)));
            }
            set
            {
                base.SetValue(actGetUser.UserGUIDProperty, value);
            }
        }

        public clsUser User 
        {
            get 
            {
                return m_User;  
            }
        }

        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. This method recovers the User from SLK.
        /// </summary>
        public override void BLExecuteCommand()
        {
            m_User = clsUser.GetUser(UserGUID);
        }         

        /// <summary>
        /// This method verifies that User was correctly recovered from SLK.
        /// </summary>
        public override void Test()
        {
            if (User == null)
            {
                throw new Exception("The User with ID:" + UserGUID + " can't be retrieved from SLK.");
            }
        }
        #endregion

    }
}
