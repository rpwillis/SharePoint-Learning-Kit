using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Dictionaries
{
    /// <summary>
    /// This class manages the activities of the unit test process
    /// in the continuos integration.
    /// </summary>
    public class clsActivityDictionary
    {
        #region "Fields"

        private string m_XMLPath = "";

        private XmlTextReader m_reader = null;

        private System.Collections.Generic.Dictionary<string, string> m_ObjectPropertyCollection;

        private bool m_IsXMLBusinessActivity = false;

        private Type m_BusinessActivityType = null;

        #endregion

        #region "Properties"

        /// <summary>
        /// The path of the dictionary
        /// </summary>
        public string XMLPath
        {
            get { return m_XMLPath; }
            set
            {
                m_XMLPath = value;

            }
        }

        /// <summary>
        /// The reader with the information of the dictionary
        /// </summary>
        public XmlTextReader reader
        {
            get { return m_reader; }
            set
            {
                m_reader = value;

            }
        }

        /// <summary>
        /// A collection with the current properties in the dictionary.
        /// </summary>
        public Dictionary<string, string> ObjectPropertyCollection
        {
            get { return m_ObjectPropertyCollection; }
            set
            {
                m_ObjectPropertyCollection = value;
            }
        }

        /// <summary>
        /// Boolean that represent if exists the XML that represent an Business Activity
        /// </summary>
        public bool IsXMLBusinessActivity
        {
            get { return m_IsXMLBusinessActivity; }
            set { m_IsXMLBusinessActivity = value; }
        }

        /// <summary>
        /// The type of the Business Activity
        /// </summary>
        public Type BusinessActivityType
        {
            get { return m_BusinessActivityType; }
            set
            {
                m_BusinessActivityType = value;
            }
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Deletes the xml text reader
        /// </summary>
        public void DeleteReader()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }

        /// <summary>
        /// Destructor of the class
        /// </summary>
        ~clsActivityDictionary()
        {
            DeleteReader();
        }

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="xmlPath">The path of the Dictionary</param>
        public clsActivityDictionary(string xmlPath)
        {
            ObjectPropertyCollection = new Dictionary<string, string>();
            XMLPath = xmlPath;
            if (ExistDictionary())
                reader = new XmlTextReader(XMLPath);
        }

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="ResourceType">Type of the activity</param>
        /// <param name="XMLActivityData">The string that represent a XML that contain a Business Activity</param>
        public clsActivityDictionary(string ResourceType, string XMLActivityData)
        {
            IsXMLBusinessActivity = true;
            BusinessActivityType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(ResourceType);
            ObjectPropertyCollection = new Dictionary<string, string>();
            MemoryStream mstream = new MemoryStream(Encoding.UTF8.GetBytes(XMLActivityData));
            reader = new XmlTextReader(mstream);
        }

        /// <summary>
        /// Create a XML dictionary with the information of a business activity
        /// </summary>
        /// <param name="BusinessActivity">The business activity</param>
        /// <returns>A string that contain the business activity in a XML</returns>
        public string CreateXMLDictionary(BLBusinessActivity BusinessActivity)
        {
            try
            {
                XmlNode ChildNode = null;
                XmlDocument XMLFromBA = new XmlDocument();
                XMLFromBA.LoadXml("<dictionary></dictionary>");

                Type BusinessActivityType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(BusinessActivity.GetType().AssemblyQualifiedName);
                PropertyInfo[] ActivityProperties = BusinessActivityType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                XmlAttribute Att = XMLFromBA.CreateAttribute("version");
                Att.Value = "1.0.0.0";
                XMLFromBA.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBA.CreateAttribute("type");
                Att.Value = "Business Activity";
                XMLFromBA.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBA.CreateAttribute("name");
                Att.Value = BusinessActivity.GetType().Name + " " + "Dictionary";
                XMLFromBA.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBA.CreateAttribute("order");
                Att.Value = "1";
                XMLFromBA.ChildNodes[0].Attributes.Append(Att);

                XmlNode Node = XMLFromBA.CreateNode(XmlNodeType.Element, "object", "");
                XMLFromBA.ChildNodes[0].AppendChild(Node);

                Att = XMLFromBA.CreateAttribute("name");
                Att.Value = BusinessActivity.GetType().FullName;
                Node.Attributes.Append(Att);

                Att = XMLFromBA.CreateAttribute("behavior");
                Att.Value = "CREATE";
                Node.Attributes.Append(Att);

                foreach (PropertyInfo Property in ActivityProperties)
                {
                    ChildNode = XMLFromBA.CreateNode(XmlNodeType.Element, "property", "");
                    Node.AppendChild(ChildNode);

                    Att = XMLFromBA.CreateAttribute("name");
                    Att.Value = Property.Name;
                    ChildNode.Attributes.Append(Att);
                    Att = XMLFromBA.CreateAttribute("value");
                    Att.Value = Property.GetValue(BusinessActivity, null).ToString();
                    ChildNode.Attributes.Append(Att);
                }
                return XMLFromBA.InnerXml;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Resets the dictionary reader.
        /// Opens a new reader with the path of the dictionary
        /// </summary>
        public void ResetReader()
        {
            if (reader != null)
            {
                reader.Close();
                reader = new XmlTextReader(XMLPath);
            }

        }

        /// <summary>
        /// Reads the properties of the next activity in the dictionary
        /// </summary>
        /// <returns>Returns true if can read the properties</returns>
        public bool ReadObject()
        {
            ObjectPropertyCollection.Clear();

            if (ExistDictionary())
            {
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "object"))
                    {
                        /*string XMLBehavior = reader["behavior"];
                        string ObjectName = reader["name"];
                        //if is the Constructor that only has xmlpath,takes the first behavior and type of the dictionary
                        if (BusinessObjectType == null)
                        {
                            BusinessObjectType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(ObjectName);
                            Behavior = XMLBehavior;
                        }*/
                        /*if (((XMLBehavior.ToLower().CompareTo(Behavior.ToLower()) == 0) &&
                            (ObjectName.ToLower().CompareTo(BusinessObjectType.Name.ToLower()) == 0)) || (CheckStructure == false))
                        {*/
                            ///////
                            while ((reader.Read()) && ((reader.NodeType != XmlNodeType.EndElement) || (reader.Name != "object")))
                            {
                                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "property"))
                                {
                                    string PropertyName = reader["name"];
                                    string PropertyValue = reader["value"];
                                    ObjectPropertyCollection.Add(PropertyName, PropertyValue);
                                    reader.Read();
                                }
                            }
                            if (reader.EOF)
                            {
                                //return false because the structure of the xml has problems
                                ObjectPropertyCollection.Clear();
                                return false;
                            }
                            else
                            {
                                //returns true if exists a object with the expected behavior
                                return true;
                            }
                            ///////
                        //}
                    }
                }
            }
            //if the reader is done this collection is null
            ObjectPropertyCollection.Clear();
            return false;
        }
        
        /// <summary>
        /// Verifies if the dictionary exists.
        /// </summary>
        /// <returns>Returs true if the dictionary exists</returns>
        public bool ExistDictionary()
        {
            if (IsXMLBusinessActivity)
                return true;
            if (File.Exists(XMLPath) == false)
                return false;
            return true;
        }

        /// <summary>
        /// Sets the current properties of the dictionary in the activity.
        /// </summary>
        /// <param name="BusinessObject">The activity to be modified</param>
        /// <returns>Return true if the object is modified</returns>
        public bool ModifyBusinessActivity(ref BLBusinessActivity BusinessActivity)
        {
            foreach (KeyValuePair<string, string> MyProperty in ObjectPropertyCollection)
            {
                SetProperty(MyProperty.Key, MyProperty.Value, ref BusinessActivity);
            }
            return true;
        }

        /// <summary>
        /// Sets a property in the selected Activity.
        /// </summary>
        /// <param name="PropertyName">The name of the property</param>
        /// <param name="PropertyValue">The value of the property</param>
        /// <param name="BusinessObject">The business object to be modified </param>
        /// <returns>Returns true if the object is modified</returns>
        private bool SetProperty(string PropertyName, string PropertyValue, ref BLBusinessActivity BusinessActivity)
        {
            Type BusinessActivityType =BusinessActivity.GetType();            
            PropertyInfo MyPropertyInfo = BusinessActivityType.GetProperty(PropertyName);
            Type PropertyType = MyPropertyInfo.PropertyType;

            object MyValue = null;
            if (PropertyType.Name=="Byte[]")
            {
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                MyValue = encoding.GetBytes(PropertyValue);
            }
            else
            {
                TypeConverter MyTypeConverter = TypeDescriptor.GetConverter(PropertyType);
                MyValue = MyTypeConverter.ConvertFrom(PropertyValue);
            }

            MyPropertyInfo.SetValue(BusinessActivity, MyValue, null);
            return true;
        }
       
        #endregion
    }
}