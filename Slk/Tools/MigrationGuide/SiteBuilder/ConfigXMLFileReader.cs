/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Text;
using System.Xml;

namespace SiteBuilder
{
    /// <summary>
    /// Logic for processing the class server 4 configuration file (Classes.xml)
    /// </summary>
    class ConfigXMLFileReader : IDisposable 
    {
        private XmlReader m_reader;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="filePath">Path to the Classes.xml file</param>
        public ConfigXMLFileReader(string filePath)
        {
            m_reader = XmlReader.Create(filePath);
        }

        /// <summary>
        /// returns next class from xml file passed in constructor
        /// returns null if reached end of the file
        /// </summary>
        /// <returns>class object</returns>
        public CS4Class GetNextClass()
        {
            CS4Class nextClass = null;
            while (!((m_reader.Name == "Class") && (m_reader.NodeType != XmlNodeType.EndElement)))
            {
                if(!m_reader.Read())
                {
                    //reached end of the reader, no class nodes found
                    return null;
                }
            }
            XmlDocument classXML = new XmlDocument();
            classXML.LoadXml("<xml>" + m_reader.ReadOuterXml() + "</xml>");
            nextClass = GetClassFromXml(classXML);
            return nextClass;
        }

        /// <summary>
        /// Parses XML document into CS4Class
        /// </summary>
        /// <param name="classXML">Xml document with the class node</param>
        /// <returns>CS4Class with data from the xml document</returns>
        private CS4Class GetClassFromXml(XmlDocument classXML)
        {
            CS4Class classData = new CS4Class();
            XmlNode classElement = classXML.GetElementsByTagName("Class")[0];
            XmlNodeList groupsElements = classElement.SelectNodes("child::Group");
            XmlNodeList classUsersElements = classElement.SelectNodes("child::Person");
            classData.ClassId = System.Convert.ToInt32(classElement.Attributes["Id"].InnerText);
            classData.ClassWeb = classElement.Attributes["Web"].InnerText;
            classData.ClassName = classElement.Attributes["Name"].InnerText;
            classData.ClassLCID = System.Convert.ToUInt32(classElement.Attributes["LCID"].InnerText);
            classData.Overwrite = (classElement.Attributes["Overwrite"].InnerText == "1" ? true : false);
            classData.Transfer = (classElement.Attributes["Transfer"].InnerText == "1" ? true : false);
            classData.Groups = GetGroupsFromXml(groupsElements);
            classData.Users = GetUsersFromXml(classUsersElements);
            return classData;
        }

        /// <summary>
        /// Parses group nodes into CS4Group objects and returns them in a collection
        /// </summary>
        /// <param name="groupsElements">xml group nodes</param>
        /// <returns>CS4GroupCollection with the data from xml</returns>
        private CS4GroupCollection GetGroupsFromXml(XmlNodeList groupsElements)
        {
            CS4GroupCollection groups = new CS4GroupCollection();
            for (int elementIndex = 0; elementIndex < groupsElements.Count; elementIndex++)
            {
                CS4Group group = new CS4Group();
                group.WebName = groupsElements[elementIndex].Attributes["WebName"].InnerText;
                group.Overwrite = (groupsElements[elementIndex].Attributes["Overwrite"].InnerText == "1" ? true : false);
                group.Transfer = (groupsElements[elementIndex].Attributes["Transfer"].InnerText == "1" ? true : false);
                XmlNodeList groupUsersElements = groupsElements[elementIndex].SelectNodes("child::Person");
                group.GroupUsers = GetUsersFromXml(groupUsersElements);
                groups.Add(group);
            }
            return groups;
        }

        /// <summary>
        /// Parses xml nodes with user info into CS4User objects and returns them ina collection
        /// </summary>
        /// <param name="usersElements">user xml nodes</param>
        /// <returns>CS4UserCollection with the data from the xml</returns>
        private CS4UserCollection GetUsersFromXml(XmlNodeList usersElements)
        {
            CS4UserCollection users = new CS4UserCollection();
            for (int curElementIndex = 0; curElementIndex < usersElements.Count; curElementIndex++)
            {
                string userLogin = usersElements[curElementIndex].Attributes["LoginName"].InnerText;
                string userDomain = usersElements[curElementIndex].Attributes["LoginNameDomain"].InnerText;
                string userName = usersElements[curElementIndex].Attributes["UserName"].InnerText;
                string userEmail = usersElements[curElementIndex].Attributes["Email"].InnerText;
                string userRole = usersElements[curElementIndex].Attributes["Role"].InnerText;
                bool userTransfer = (usersElements[curElementIndex].Attributes["Transfer"].InnerText == "1" ? true : false);
                int userId = 0;
                Int32.TryParse(usersElements[curElementIndex].Attributes["Id"].InnerText, out userId);
                CS4User user = new CS4User(
                                userId,
                                userDomain,
                                userLogin,
                                userName,
                                userEmail,
                                userTransfer,
                                IsUserInTeacherRole(userRole));
                users.Add(user);
            }
            return users;
        }

        /// <summary>
        /// Wraps necessity to compare to exact string value every time we need to know
        /// if the user is a teacher 
        /// </summary>
        /// <param name="userRole">string to compare</param>
        /// <returns>true if userRole contains "teacher"</returns>
        private bool IsUserInTeacherRole(string userRole)
        {
            if (userRole.ToLower() == "teacher")
                return true;
            else
                return false;
        }
   
        /// <summary>
        /// Closes the internally used XmlReader
        /// </summary>
        public void Dispose()
        {
            m_reader.Close();
        }
    }


}
