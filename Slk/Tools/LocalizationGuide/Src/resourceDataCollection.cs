/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Xml;

namespace SharePointLearningKit.Localization
{

    class ResourceDataCollection : CollectionBase
    {
        Dictionary<string, ResourceData> keyedData = new Dictionary<string, ResourceData>();

        public ResourceData this[string key]
        {
            get
            {
                ResourceData data = null;
                keyedData.TryGetValue(key, out data);
                return data;
            }
        }

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
            keyedData.Add(value.ResourceName, value);
            return (List.Add(value));
        }

        public void Add(ResourceDataCollection coll)
        {
            foreach (ResourceData it in coll)
            {
                keyedData.Add(it.ResourceName, it);
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
            if (value.GetType() != Type.GetType("Loc.ResourceData") && value.GetType() != Type.GetType("SharePointLearningKit.Localization.ResourceData"))
            {
                throw new ArgumentException("Argument must be of type 'ResourceData'. Type is " + value.GetType().ToString());
            }
        }
    }
}
