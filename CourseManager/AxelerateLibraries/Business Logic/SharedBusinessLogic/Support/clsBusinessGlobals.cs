using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Support
{
    [Serializable]
    public class clsBusinessGlobals
    {
        private static Hashtable m_Globals = null;

        static clsBusinessGlobals()
        {
            m_Globals = new Hashtable();
            SetGlobal(new clsUserGUIDGlobal());
        }

        public static bool isGlobal(string GlobalName)
        {
            return m_Globals.ContainsKey(GlobalName);
        }

        public static object GetGlobalValue(string GlobalName)
        {
            clsBusinessGlobal Global = (clsBusinessGlobal)m_Globals[GlobalName];
            if (Global != null)
            {
                return Global.Value;
            }
            return null;
        }

        public static void SetGlobal(clsBusinessGlobal Global)
        {
            m_Globals[Global.Name] = Global;

        }
        #region "clsBusinessGlobal"
        public class clsBusinessGlobal
        {
            protected string m_Name;

            public clsBusinessGlobal(string pName)
            {
                m_Name = pName;
            }


            public string Name
            {
                get
                {
                    return m_Name;
                }
            }

            public virtual object Value
            {
                get
                {
                    return null;
                }
            }
        }
        #endregion

        #region "Default BusinessGlobal classes"
        public class clsUserGUIDGlobal : clsBusinessGlobal
        {
            public clsUserGUIDGlobal()
                : base("CurrentUser")
            {
            }

            public override object Value
            {
                get
                {
                    return clsActiveDirectory.CurrentUser.GUID;
                }
            }
        }
        #endregion
        
    }
}
