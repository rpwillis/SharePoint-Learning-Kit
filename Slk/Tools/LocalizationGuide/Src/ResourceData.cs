/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Xml;

namespace Loc
{
    class ResourceData 
    {

        string _type;
        string _name;
        DataSet _ds;

        public ResourceData()
        {
            Init();
        }

        private void Init()
        {
            _ds = new DataSet();
            PrepareDataSet(_ds);
        }

        public ResourceData(string ResourceName, string ResourceType)
        {

            Init();

            this.ResourceName = ResourceName;
            this.ResourceType = ResourceType;
        }

        public DataSet DataSet
        {
            get
            {
                return _ds;
            }
            set
            {
                _ds = value;
            }
        }

        public string ResourceName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                AddToMetaData("Name", _name);
            }
        }

        private void AddToMetaData(string FieldName, string Value)
        {
            if (_ds.Tables["MetaData"].Rows.Count == 0)
            {
                DataRow dr = _ds.Tables["MetaData"].NewRow();
                _ds.Tables["MetaData"].Rows.Add(dr);
            }
            _ds.Tables["MetaData"].Rows[0][FieldName] = Value;
            
        }

        public string ResourceType
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                AddToMetaData("Type", _type.ToString());
            }
        }


        public void Add(string ID, string Value)
        {
            DataRow dr = _ds.Tables["Resources"].NewRow();
            dr["ID"] = ID;
            dr["Source"] = Value;
            dr["Translation"] = Value;
            _ds.Tables["Resources"].Rows.Add(dr);
        }


        public static void PrepareDataSet(DataSet ds)
        {
            ds.Tables.Add("MetaData");
            ds.Tables["MetaData"].Columns.Add("Name");
            ds.Tables["MetaData"].Columns.Add("Type");

            ds.Tables.Add("Resources");
            ds.Tables["Resources"].Columns.Add("ID");
            ds.Tables["Resources"].Columns.Add("Source");
            ds.Tables["Resources"].Columns.Add("Translation");
        }


    }

    class ResourceDataCollection : CollectionBase
    {

        public ResourceData this[int index]
        {
            get
            {
                return ((ResourceData)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(ResourceData value)
        {
            return (List.Add(value));
        }

        public void Add(ResourceDataCollection coll)
        {
            foreach (ResourceData it in coll)
            {
                List.Add(it);
            }
        }

        public int IndexOf(ResourceData value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, ResourceData value)
        {
            List.Insert(index, value);
        }

        public void Remove(ResourceData value)
        {
            List.Remove(value);
        }

        public bool Contains(ResourceData value)
        {
            return (List.Contains(value));
        }

        protected override void OnValidate(object value)
        {
            if (value.GetType() != Type.GetType("Loc.ResourceData"))
            {
                throw new ArgumentException("Argument must be of type 'ResourceData'.");
            }
        }
    }


    public enum ResourceTypes{StringTable = 0, XML}
}
