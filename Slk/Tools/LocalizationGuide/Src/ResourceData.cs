/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Xml;

namespace SharePointLearningKit.Localization
{
    class ResourceData 
    {

        string _type;
        string _name;
        DataSet _ds;
        Dictionary<string, DataRow> keyedRows = new Dictionary<string, DataRow>();

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

        public void SetTranslation(string id, string translation)
        {
            DataRow row = null;
            if (keyedRows.TryGetValue(id, out row))
            {
                row["Translation"] = translation;
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


        public void Add(string id, string value, bool crucial)
        {
            DataRow dr = _ds.Tables["Resources"].NewRow();
            dr["ID"] = id;
            dr["Source"] = value;
            dr["Translation"] = value;
            dr["Crucial"] = crucial;
            keyedRows.Add(id, dr);
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
            ds.Tables["Resources"].Columns.Add("Crucial");
        }


    }

    /// <summary>The type of resource.</summary>
    public enum ResourceTypes
    {
        /// <summary>A string table.</summary>
        StringTable = 0, 
        /// <summary>Xml data.</summary>
        XML
    }
}
