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
    /// This class building dictionaries (XML's) that represents a business objects
    /// and create business objects from dictionaries
    /// </summary>
    public class clsBusinessObjectDictionary
    {
        #region "Fields"

        private string m_XMLPath = "";

        private string m_UnitTestType = "";

        private Type m_BusinessObjectType = null;

        private string m_Behavior = "";
        
        private XmlTextReader m_reader = null;
        private System.Collections.Generic.Dictionary<string,string> m_ObjectPropertyCollection;

        private bool m_CheckStructure = false;

        private bool m_IsXMLBusinessObject = false;

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
        /// The type of the unit test
        /// </summary>
        public string UnitTestType
        {
            get { return m_UnitTestType; }
            set
            {
                m_UnitTestType = value;

            }
        }
        /// <summary>
        /// The type of the Business Object
        /// </summary>
        public Type BusinessObjectType
        {
            get { return m_BusinessObjectType; }
            set
            {
                m_BusinessObjectType = value;

            }
        }
        /// <summary>
        /// the behavior of the business object in the unit test
        /// </summary>
        public string Behavior
        {
            get { return m_Behavior; }
            set
            {
                m_Behavior = value;

            }
        }
        /// <summary>
        /// The reader with the information of the dictionary
        /// </summary>
        public XmlTextReader  reader
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
        /// The path of the dictionary
        /// </summary>
        private bool CheckStructure
        {
            get { return m_CheckStructure; }
            set
            {
                m_CheckStructure = value;

            }
        }
        /// <summary>
        /// Boolean that represent if exists the XML that represent an Object Data
        /// </summary>
        public bool IsXMLBusinessObject
        {
            get { return m_IsXMLBusinessObject; }
            set { m_IsXMLBusinessObject = value; }
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
        ~clsBusinessObjectDictionary()
        {
            DeleteReader();
        }
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="unitTestType">The type of the unit test</param>
        /// <param name="behavior">The behavior of the object in the test</param>
        /// <param name="BusinessObjectType">The type of the object</param>
        /// <param name="xmlPath">the path of the dictionary</param>
        public clsBusinessObjectDictionary(string unitTestType, string behavior,Type businessObjectType,string xmlPath)
        {
            CheckStructure = true;
            BusinessObjectType = businessObjectType;
            UnitTestType = unitTestType;
            Behavior = behavior;
            ObjectPropertyCollection = new Dictionary<string, string>();
            XMLPath = xmlPath;
            if (XMLPath == "")
            {
                string DictionaryPath = clsConfigurationProfile.Current.getPropertyValue("CIDictionaryPath");
                XMLPath = DictionaryPath + "\\BusinessObject\\"
                    + UnitTestType.ToUpper() + "\\" + BusinessObjectType.Name
                    + "_" + Behavior.ToLower() + "_Dictionary.xml";
            }       

            if(ExistDictionary())
                reader = new XmlTextReader(XMLPath);
        }
        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="xmlPath">The path of Dictionary</param>
        public clsBusinessObjectDictionary(string xmlPath)
        {
            CheckStructure = false;
            BusinessObjectType = null;
            UnitTestType = "";
            Behavior = "";
            ObjectPropertyCollection = new Dictionary<string, string>();
            XMLPath = xmlPath;
            if (ExistDictionary())
                reader = new XmlTextReader(XMLPath);
        }
        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="ResourceType">Type of the object</param>
        /// <param name="XMLObjectData">The string that represent a XML that contain a Business Object</param>
        public clsBusinessObjectDictionary(string ResourceType, string XMLObjectData)
        {
            IsXMLBusinessObject = true;
            BusinessObjectType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(ResourceType);
            CheckStructure = false;
            ObjectPropertyCollection = new Dictionary<string, string>();
            MemoryStream mstream = new MemoryStream(Encoding.UTF8.GetBytes(XMLObjectData));
            reader = new XmlTextReader(mstream);
        }

        /// <summary>
        /// Create a XML dictionary with the information of a business object
        /// </summary>
        /// <param name="BusinessBase">The business base object</param>
        /// <returns>A string that contain the business object in a XML</returns>
        public string CreateXMLDictionary(BLBusinessBase BusinessBase)
        {
            try
            {
                XmlNode ChildNode = null;
                XmlDocument XMLFromBO = new XmlDocument();
                XMLFromBO.LoadXml("<dictionary></dictionary>");

                Type BusinessObjectType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(BusinessBase.GetType().AssemblyQualifiedName);
                List<BLFieldMap> ObjectFieldMaps = BusinessBase.DataLayer.FieldMapList.DataFetchFields;

                XmlAttribute Att = XMLFromBO.CreateAttribute("version");
                Att.Value = "1.0.0.0";
                XMLFromBO.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBO.CreateAttribute("type");
                Att.Value = "Business Object";
                XMLFromBO.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBO.CreateAttribute("name");
                Att.Value = BusinessBase.GetType().Name + " " + "Dictionary";
                XMLFromBO.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBO.CreateAttribute("order");
                Att.Value = "1";
                XMLFromBO.ChildNodes[0].Attributes.Append(Att);

                XmlNode Node = XMLFromBO.CreateNode(XmlNodeType.Element, "object", "");
                XMLFromBO.ChildNodes[0].AppendChild(Node);

                Att = XMLFromBO.CreateAttribute("name");
                Att.Value = BusinessBase.GetType().FullName;
                Node.Attributes.Append(Att);

                Att = XMLFromBO.CreateAttribute("behavior");
                Att.Value = "CREATE";
                Node.Attributes.Append(Att);

                foreach (BLFieldMap FieldMap in ObjectFieldMaps)
                {
                    ChildNode = XMLFromBO.CreateNode(XmlNodeType.Element, "property", "");
                    Node.AppendChild(ChildNode);

                    Att = XMLFromBO.CreateAttribute("name");
                    Att.Value = FieldMap.PropertyName;
                    ChildNode.Attributes.Append(Att);
                    Att = XMLFromBO.CreateAttribute("value");
                    Att.Value = FieldMap.Field.GetValue(BusinessBase).ToString();
                    ChildNode.Attributes.Append(Att);
                }
                return XMLFromBO.InnerXml;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
                
        /// <summary>
        /// Create a dictionary with the information of a business object collection
        /// </summary>
        /// <param name="BusinessBaseCollection">The collection of business base objects</param>
        /// <returns>A string that contain the business object collection in a XML</returns>
        public string CreateXMLDictionary(IBLListBase BusinessBaseCollection)
        {
            try
            {
                XmlNode Node = null;
                XmlNode ChildNode = null;
                XmlDocument XMLFromBC = new XmlDocument();
                XMLFromBC.LoadXml("<dictionary></dictionary>");

                Type BusinessObjectType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(BusinessBaseCollection[0].GetType().AssemblyQualifiedName);  //O es asi, BusinessBaseCollection.DataLayer.BusinessLogicType
                List<BLFieldMap> ObjectFieldMaps = ((BLBusinessBase)BusinessBaseCollection[0]).DataLayer.FieldMapList.DataFetchFields;

                XmlAttribute Att = XMLFromBC.CreateAttribute("version");
                Att.Value = "1.0.0.0";
                XMLFromBC.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBC.CreateAttribute("type");
                Att.Value = "Business Object";
                XMLFromBC.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBC.CreateAttribute("name");
                Att.Value = BusinessBaseCollection[0].GetType().Name + " " + "Dictionary";
                XMLFromBC.ChildNodes[0].Attributes.Append(Att);

                Att = XMLFromBC.CreateAttribute("order");
                Att.Value = "1";
                XMLFromBC.ChildNodes[0].Attributes.Append(Att);

                foreach (BLBusinessBase BusinessBase in BusinessBaseCollection)
                {
                    Node = XMLFromBC.CreateNode(XmlNodeType.Element, "object", "");
                    XMLFromBC.ChildNodes[0].AppendChild(Node);

                    Att = XMLFromBC.CreateAttribute("name");
                    Att.Value = BusinessBase.GetType().FullName;
                    Node.Attributes.Append(Att);

                    Att = XMLFromBC.CreateAttribute("behavior");
                    Att.Value = "CREATE";
                    Node.Attributes.Append(Att);

                    foreach (BLFieldMap FieldMap in ObjectFieldMaps)
                    {
                        ChildNode = XMLFromBC.CreateNode(XmlNodeType.Element, "property", "");
                        Node.AppendChild(ChildNode);

                        Att = XMLFromBC.CreateAttribute("name");
                        Att.Value = FieldMap.PropertyName;
                        ChildNode.Attributes.Append(Att);
                        Att = XMLFromBC.CreateAttribute("value");
                        Att.Value = FieldMap.Field.GetValue(BusinessBase).ToString();
                        ChildNode.Attributes.Append(Att);
                    }
                }
                return XMLFromBC.InnerXml;
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
        /// Reads the properties of the next object in the dictionary
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
                        string XMLBehavior = reader["behavior"];
                        string ObjectName = reader["name"];
                        //if is the Constructor that only has xmlpath,takes the first behavior and type of the dictionary
                        if (BusinessObjectType == null)
                        {
                            BusinessObjectType = Axelerate.BusinessLayerFrameWork.BLCore.ReflectionHelper.CreateBusinessType(ObjectName);
                            Behavior = XMLBehavior;
                        }
                        if (((XMLBehavior.ToLower().CompareTo(Behavior.ToLower()) == 0) &&
                            (ObjectName.ToLower().CompareTo(BusinessObjectType.Name.ToLower())==0))||(CheckStructure ==false))
                        {
                            ///////
                            while ((reader.Read())&&((reader.NodeType != XmlNodeType.EndElement) || (reader.Name != "object")))
                            {                               
                                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "property"))
                                {
                                    string PropertyName = reader["name"];
                                    string PropertyValue = reader["value"];
                                    ObjectPropertyCollection.Add(PropertyName, PropertyValue);                                   
                                    reader.Read();
                                }
                            }
                            if(reader.EOF)
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
                        }
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
        public  bool ExistDictionary()
        {
            if (IsXMLBusinessObject)
                return true;
            if (File.Exists(XMLPath) == false)
                return false;
            return true;
        }
        /// <summary>
        /// Sets the current properties of the dictionary in the business object.
        /// </summary>
        /// <param name="BusinessObject">The business object to be modified</param>
        /// <returns>Return true if the object is modified</returns>
        public bool ModifyBusinessObject(ref BLBusinessBase BusinessObject)
        {  
            foreach (KeyValuePair<string,string>  MyProperty in ObjectPropertyCollection)
            {
                SetProperty(MyProperty.Key, MyProperty.Value, ref BusinessObject);
            }
            return true;
        }
        /// <summary>
        /// Sets a property in the selected business object.
        /// </summary>
        /// <param name="PropertyName">The name of the property</param>
        /// <param name="PropertyValue">The value of the property</param>
        /// <param name="BusinessObject">The business object to be modified </param>
        /// <returns>Returns true if the object is modified</returns>
        private bool SetProperty(string PropertyName, string PropertyValue, ref BLBusinessBase BusinessObject)
         {
            PropertyInfo MyPropertyInfo = BusinessObject.DataLayer.GetProperty(PropertyName);
            Type PropertyType = MyPropertyInfo.PropertyType;

            object MyValue = null;
            if (PropertyType.Name == "Byte[]")
            {
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                MyValue = encoding.GetBytes(PropertyValue);
            }
            else
            {
                TypeConverter MyTypeConverter = TypeDescriptor.GetConverter(PropertyType);
                MyValue = MyTypeConverter.ConvertFrom(PropertyValue);
            }

            MyPropertyInfo.SetValue(BusinessObject, MyValue, null);
            return true;
        }
        /// <summary>
        /// Takes the Dictionaries in the folder, creates and saves the business objects.
        /// The name of type needs to have the format of the following example:
        /// Axelerate.MIPLogicalLayer.clsProject, Axelerate.MIPLogicalLayer
        /// </summary>
        /// <param name="FolderPath">The path with the dictionaries</param>
        static public void SaveAllInStore(string FolderPath)
        {

            List<string> InvalidTypes = new List<string>();
            List<string> InvalidDictionaries = new List<string>();
            //TODO: a new list if the object  exist in  the store:  List<string> AlreadyExist = new List<string>();
            SortedList<int, string> MySortedList= GetSortedDictionaryPathList(FolderPath);
            for (int i = 0; i < MySortedList.Count; i++)
            {
                string FilePath = MySortedList.Values[i];
                clsBusinessObjectDictionary BOCreateDictionary = new clsBusinessObjectDictionary(FilePath);
                bool IsInvalidDictionary = true;
                while (BOCreateDictionary.ReadObject())
                {
                    IsInvalidDictionary = false;
                   
                    object obj = Activator.CreateInstance(BOCreateDictionary.BusinessObjectType);
                    BLBusinessBase BusinessBase = (BLBusinessBase)obj;
                    BOCreateDictionary.ModifyBusinessObject(ref BusinessBase);
                    try
                    {
                        BusinessBase = BusinessBase.Save();                        

                    }
                    catch (Exception ex)
                    {
                        InvalidTypes.Add(BOCreateDictionary.BusinessObjectType.Name);
                    }

                }
                if (IsInvalidDictionary)
                {
                    InvalidDictionaries.Add(Path.GetFileName(FilePath));
                }
                BOCreateDictionary.DeleteReader();
            }
            //Message of error if exist
            bool IsError = false;
            string TotalTypeMessage = "";
            string TotalDictionaryMessage = "";
            if (InvalidTypes.Count!=0)
            {
                IsError = true;
                string TotalTypes = "";
                foreach (string MyType in InvalidTypes )
                {
                    if (TotalTypes!="")
                    {
                        TotalTypes = TotalTypes + ", ";
                    }
                    TotalTypes = TotalTypes + MyType;
                }
                TotalTypeMessage = Resources.ErrorMessages.errCantCreateTypes + TotalTypes;
            }
            if (InvalidDictionaries.Count  != 0)
            {
                IsError = true;
                string TotalDictionaries = "";
                foreach (string MyDictionary in InvalidDictionaries)
                {
                    if (TotalDictionaries != "")
                    {
                        TotalDictionaries = TotalDictionaries + ", ";
                    }
                    TotalDictionaries = TotalDictionaries + MyDictionary;
                }
                TotalDictionaryMessage = Resources.ErrorMessages.errCantCreateDict + TotalDictionaries;
            }
            if ((TotalTypeMessage != "") || (TotalDictionaryMessage != ""))
            {
                throw new Exception(TotalDictionaryMessage + " " + TotalTypeMessage);
            }
        }
        /// <summary>
        /// Gets a sorted list with the paths of dictionaries in the selected folder.
        /// </summary>
        /// <param name="FolderPath">The path of the folder</param>
        /// <returns>A sorted list with the paths  of dictionaries</returns>
        private static SortedList<int, string> GetSortedDictionaryPathList(string FolderPath)
        {

            SortedList<int, string> MySortedList = new SortedList<int, string>();
            foreach (string FileName in Directory.GetFiles(FolderPath))
            {
                XmlTextReader reader = new XmlTextReader(FileName);
                bool ExistOrder = false;
                while ((reader.Read()) && (ExistOrder == false))
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "dictionary"))
                    {
                        string strDictionaryOrder = reader["order"];
                        int DictionaryOrder = int.Parse(strDictionaryOrder);
                        MySortedList.Add(DictionaryOrder, FileName);
                        ExistOrder = true;

                    }
                }
                reader.Close();
            }
            return MySortedList;
        }        
        #endregion
    }
}
