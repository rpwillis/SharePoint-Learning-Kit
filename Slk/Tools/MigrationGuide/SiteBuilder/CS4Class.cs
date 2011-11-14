/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace SiteBuilder
{
    /// <summary>
    /// represents Class Server 4 class item 
    /// as it is stored in the configuration XML file
    /// exported from the Class Server database
    /// </summary>
    public class CS4Class
    {
        private int m_classId;
        private string m_classWeb;
        private string m_className;
        private uint m_classLCID;
        private bool m_classOverwrite; 
        private bool m_classTransfer;
        private CS4GroupCollection m_groups;
        private CS4UserCollection m_users;

        public CS4GroupCollection Groups
        {
            get
            {
                return m_groups;
            }
            set
            {
                m_groups = value;
            }
        }

        public int ClassId
        {
            get
            {
                return m_classId;
            }
            set
            {
                m_classId = value;
            }
        }

        public CS4UserCollection Users
        {
            get
            {
                return m_users;
            }
            set
            {
                m_users = value;
            }
        }

        public bool Transfer
        {
            get
            {
                return m_classTransfer;
            }
            set
            {
                m_classTransfer = value;
            }
        }

        public bool Overwrite
        {
            get
            {
                return m_classOverwrite;
            }
            set
            {
                m_classOverwrite = value;
            }
        }

        public uint ClassLCID
        {
            get
            {
                return m_classLCID;
            }
            set
            {
                m_classLCID = value;
            }
        }

        public string ClassWeb
        {
            get
            {
                return m_classWeb;
            }
            set
            {
                m_classWeb = value;
            }
        }

        public string ClassName
        {
            get
            {
                return m_className;
            }
            set
            {
                m_className = value;
            }
        }
    }
}
