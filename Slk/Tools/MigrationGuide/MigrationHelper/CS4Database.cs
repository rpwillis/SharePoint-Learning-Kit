/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;


namespace MigrationHelper
{
    /// <summary>
    /// Performs requests to Class Server database
    /// </summary>
    public class CS4Database
    {
        private SqlConnection objConn = null;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="connectionString">Full connection string to class server database
        /// </param>
        public CS4Database(string connectionString)
        {
            objConn = new SqlConnection();
            objConn.ConnectionString = connectionString;
        }

        /// <summary>
        /// Retrieves Users list (domain name, user name) from Class Server Database
        /// </summary>
        /// <param name="UsersList">Reference on a datatable that will be filled with user records</param>
        /// <returns>Number of users</returns>
        public int GetUsersList(ref DataTable usersList)
        {
            int numUsersReturned;
            SqlCommand objComm = new SqlCommand("sp_GetUsersList");
            objComm.CommandType = CommandType.StoredProcedure;
            objComm.Connection = objConn;
            usersList = GetDataSet(objComm).Tables[0];
            numUsersReturned = usersList.Rows.Count;
            objComm.Dispose();
            return numUsersReturned;
        }

        /// <summary>
        /// Returns datatables with assignments list and user-to-assignment relations
        /// using sp_GetAssignments
        /// </summary>
        /// <param name="classId">Class Server 4 database classID key</param>
        /// <param name="assignmentItems">returned datatable with assignments</param>
        /// <param name="userAssignments">returned datatable with user-to-assignment relations, including grades and teacher comments</param>
        /// <returns>number of assignments</returns>
        public int GetAssignments(int classId, out DataTable assignmentItems, out DataTable userAssignments)
        {
            int numAssignmentsReturned;
            SqlCommand objComm = new SqlCommand("sp_GetAssignments");
            objComm.CommandType = CommandType.StoredProcedure;
            objComm.Connection = objConn;
            objComm.Parameters.Add(new SqlParameter("ClassID",classId));
            DataSet returnedData = GetDataSet(objComm);
            assignmentItems = returnedData.Tables[0];
            userAssignments = returnedData.Tables[1];
            numAssignmentsReturned = assignmentItems.Rows.Count;
            objComm.Dispose();
            return numAssignmentsReturned;
        }


        /// <summary>
        /// Saves hierarchical structure of Class Server classes - groups - users 
        /// to the specified file in Xml format
        /// </summary>
        /// <param name="XmlFileName">full path and name of the xml file</param>
        /// <returns>true if the export was successful</returns>
        public bool ExportClassesStructure(string xmlFileName)
        {
            SqlCommand objComm = new SqlCommand("sp_GetClassesStructureXML");
            objComm.CommandType = CommandType.StoredProcedure;
            objComm.Connection = objConn;
            return ExportXMLFromDB(objComm, xmlFileName);
        }

        /// <summary>
        /// opens connection
        /// </summary>
        private void OpenConnection()
        {
            objConn.Open();
        }

        /// <summary>
        /// attempts to open and close a connection
        /// </summary>
        public void TestConnection()
        {
            this.OpenConnection();
            objConn.Close();
        }

        /// <summary>
        /// Saves data to AssignmentPackageTransfer table using SqlBulkCopy
        /// </summary>
        /// <param name="assignmentsData">datatable in the same format as AssignmentPackageTransfer table</param>
        public void WriteAssignmentsTransferTable(DataTable assignmentsData)
        {
            SqlCommand objComm = new SqlCommand("sp_EnsureTransferTable");
            objComm.CommandType = CommandType.StoredProcedure;
            objComm.Connection = objConn;
            ExecNonQuery(objComm);
            SqlBulkCopy tableCopy = new SqlBulkCopy(objConn);
            tableCopy.DestinationTableName = "AssignmentPackageTransfer";
            bool openedByMe = false;
            if (!(objConn.State == ConnectionState.Open))
            {
                this.OpenConnection();
                openedByMe = true;
            }
            tableCopy.WriteToServer(assignmentsData);
            if (openedByMe)
            {
                objConn.Close();
            }
        }

        /// <summary>
        /// wraps SqlCommand.ExecuteNonQuery to include opening and closing connection
        /// </summary>
        /// <param name="objComm">SqlCommand to execute</param>
        private void ExecNonQuery(SqlCommand objComm)
        {
            bool bOpenedByMe = false;
            try
            {
                if (!(objConn.State == ConnectionState.Open))
                {
                    this.OpenConnection();
                    bOpenedByMe = true;
                }
                objComm.ExecuteNonQuery();
            }
            finally
            {
                if (bOpenedByMe)
                    objConn.Close();
            }
        }


        /// <summary>
        /// Gets dataset returned as result of executing of provided SqLComand object
        /// </summary>
        /// <param name="objComm">SqlCommand that will be used to fill the dataset</param>
        /// <returns>DataSet</returns>
        private DataSet GetDataSet(SqlCommand objComm)
        {
            DataSet retDataSet = new DataSet("Res");
            bool openedByMe = false;
            try
            {
                if (!(objConn.State == ConnectionState.Open))
                {
                    this.OpenConnection();
                    openedByMe = true;
                }
                SqlDataAdapter oAdapter = new SqlDataAdapter();
                oAdapter.SelectCommand = objComm;
                oAdapter.Fill(retDataSet);
            }
            finally
            {
                if (openedByMe)
                {
                    objConn.Close();
                }
            }
            return retDataSet;
        }

        /// <summary>
        /// Saves XML output of the provided SqlCommand into XML file
        /// </summary>
        /// <param name="ObjComm">SqlCommand that will be used to return XML output</param>
        /// <param name="FilePath">Full path and name of the XML file. If the file exists it will be overwritten.</param>
        /// <returns>true if the export was successful</returns>
        private bool ExportXMLFromDB(SqlCommand objComm, string filePath)
        {
            bool result = false;
            bool openedByMe = false;
            if (!(objConn.State == ConnectionState.Open))
            {
                this.OpenConnection();
                openedByMe = true;
            }
            XmlWriter writer = new XmlTextWriter(filePath, Encoding.UTF8);
            XmlReader reader = null;
            try
            {
                reader = objComm.ExecuteXmlReader();
                reader.MoveToContent();
                writer.WriteNode(reader,true);
                result = true;
            }
            finally
            {
                writer.Close();
                reader.Close();

                if (openedByMe)
                {
                    objConn.Close();
                }
            }
            return result;

        }


    }
}
