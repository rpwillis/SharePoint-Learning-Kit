/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace SiteBuilder
{

    public class CS4GroupCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Methods for supporting CollectionBase interface
        /// </summary>
        public void Add(CS4Group group)
        {
            List.Add(group);
        }

        public void Remove(int index)
        {
            List.RemoveAt(index);
        }

        public CS4Group Item(int index)
        {
            return (CS4Group)List[index];
        }

    }

    /// represents Class Server 4 group item 
    /// as it is stored in the configuration XML file
    /// exported from the Class Server database

    public class CS4Group
    {
        private string m_groupWebName;
        private bool m_overwrite;
        private bool m_transfer;
        private CS4UserCollection m_users;

        public CS4UserCollection GroupUsers
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

        public string WebName
        {
            get
            {
                return m_groupWebName;
            }
            set
            {
                m_groupWebName = value;
            }
        }

        public bool Transfer
        {
            get
            {
                return m_transfer;
            }
            set
            {
                m_transfer = value;
            }
        }

        public bool Overwrite
        {
            get
            {
                return m_overwrite;
            }
            set
            {
                m_overwrite = value;
            }
        }


    }
}
