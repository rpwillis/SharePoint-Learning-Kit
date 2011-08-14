/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using MigrationHelper;

namespace SiteBuilder
{
    /// <summary>
    /// represents settings form, allows user to set CS4 to SLK transfer parameters,
    /// and saves them into settings file.
    /// </summary>
    public partial class SiteBuilderSettings : Form
    {
        public SiteBuilderSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// switches user name and password text boxes on and off
        /// depending on Windows Authentication check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbWindowsAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            if (chbWindowsAuthentication.Checked)
            {
                this.tbPassword.Enabled = false;
                this.tbUserName.Enabled = false;
            }
            else
            {
                this.tbPassword.Enabled = true;
                this.tbUserName.Enabled = true;
            }
        }

        /// <summary>
        /// Builds CS4 database connection string using text input from user
        /// </summary>
        /// <returns>connection string</returns>
        private string BuildConnectionString()
        {
            if (tbServerName.Text.Length == 0)
                tbServerName.Text = "(local)";
            if (tbDatabaseName.Text.Length == 0)
                tbDatabaseName.Text = "master";
            string connectionString = "server=" + this.tbServerName.Text + ";database=" + this.tbDatabaseName.Text + ";";

            if (this.chbWindowsAuthentication.Checked)
            {
                connectionString += "Trusted_Connection=Yes;";
            }
            else
            {
                connectionString += "uid=" + this.tbUserName.Text + ";pwd=" + this.tbPassword.Text + ";";
            }
            return connectionString;

        }

        /// <summary>
        /// tries to establish connection with the CS4 database and displays the status
        /// </summary>
        private void btnTestDBConnection_Click(object sender, EventArgs e)
        {
            this.btnTestDBConnection.Enabled = false;
            this.lblConnectionStatus.Text = String.Empty;
            CS4Database database = new CS4Database(this.BuildConnectionString());            
            try
            {
                database.TestConnection();
                this.lblConnectionStatus.Text = TextResources.ConnectionSuccessful;
            }
            catch (SystemException ex)
            {
                this.lblConnectionStatus.Text = ex.Message;
            }
            this.btnTestDBConnection.Enabled = true;
        }

        /// <summary>
        /// Attempts to bind to the LDAP path provided by user and displays the status
        /// </summary>
        private void btnBindAD_Click(object sender, EventArgs e)
        {
            btnBindAD.Enabled = false;
            lblADBindingStatus.Text = String.Empty;
            ADHelper helper = new ADHelper();
            try
            {
                helper.DoADBinding(this.tbADPath.Text);
                this.lblADBindingStatus.Text = TextResources.BindingSuccessful;
            }
            catch (SystemException ex)
            {
                this.lblADBindingStatus.Text = TextResources.BindingError + ex.Message;
            }
            btnBindAD.Enabled = true;

        }

        /// <summary>
        /// Pre-populates UI textboxes from configuration file and 
        /// tries to guess the Active Directory LDAP path if its empty in the config file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SiteBuilderUI_Shown(object sender, EventArgs e)
        {
            //Class Server DB pre sets
            tbServerName.Text = SiteBuilder.Default.ClassServerDBServerName;
            tbDatabaseName.Text = SiteBuilder.Default.ClassServerDBName;
            if (SiteBuilder.Default.ClassServerDBUserName.Length == 0)
            {
                tbUserName.Text = String.Empty;
                tbPassword.Text = String.Empty;
                chbWindowsAuthentication.Checked = true;
            }
            else
            {
                tbUserName.Text = SiteBuilder.Default.ClassServerDBUserName;
                tbPassword.Text = SiteBuilder.Default.ClassServerDBPassword;
                chbWindowsAuthentication.Checked = false;
            }
            //SLK Web 
            tbSLKWeb.Text = SiteBuilder.Default.SLKSchoolWeb;
            tbDocLibWeb.Text = SiteBuilder.Default.SLKDocLibraryWeb;
            tbDocLibName.Text = SiteBuilder.Default.SLKDocLibraryName;
            //Active Directory Path
            tbADPath.Text = SiteBuilder.Default.ActiveDirectoryPath;
            if (tbADPath.Text.Length == 0)
            {
                try
                {
                    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface adapter in adapters)
                    {
                        IPInterfaceProperties properties = adapter.GetIPProperties();
                        if ((properties.IsDnsEnabled) || (properties.IsDynamicDnsEnabled))
                        {
                            tbADPath.Text = "GC://DC=" + properties.DnsSuffix.Replace(".", ",DC=");
                            break;
                        }
                    }
                }
                catch
                {
                    //failed to populate Active Directory Path text box automatically
                    //not too important, the user will need to enter the Active Directory path
                }
            }

        }

        /// <summary>
        /// Saving parameters to the config file and closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            //Class Server DB
            SiteBuilder.Default.ClassServerDBServerName = this.tbServerName.Text;
            SiteBuilder.Default.ClassServerDBName = this.tbDatabaseName.Text;
            if (chbWindowsAuthentication.Checked)
            {
                SiteBuilder.Default.ClassServerDBUserName = System.String.Empty;
                SiteBuilder.Default.ClassServerDBPassword = System.String.Empty;
            }
            else
            {
                SiteBuilder.Default.ClassServerDBUserName = tbUserName.Text;
                SiteBuilder.Default.ClassServerDBPassword = tbPassword.Text;
            }
            SiteBuilder.Default.ClassServerDBConnectionString = this.BuildConnectionString();
            SiteBuilder.Default.ActiveDirectoryPath = this.tbADPath.Text;
            SiteBuilder.Default.SLKSchoolWeb = this.tbSLKWeb.Text;
            SiteBuilder.Default.SLKDocLibraryWeb = this.tbDocLibWeb.Text;
            SiteBuilder.Default.SLKDocLibraryName = this.tbDocLibName.Text;
            SiteBuilder.Default.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Attempts to detect if there is a SharePoint v3 site at the URL provided by user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCheckSharepointV3Web_Click(object sender, EventArgs e)
        {
            lblSLKSiteInitStatus.Text = String.Empty;
            btnCheckSharepointV3Web.Enabled = false;
            SharePointV3 site = new SharePointV3();            
            string result;
            if (site.TestSharePointV3Site(tbSLKWeb.Text, out result))
            {
                lblSLKSiteInitStatus.Text = TextResources.WebConnectionSuccessful;
            }
            else
            {
                lblSLKSiteInitStatus.Text = TextResources.WebConnectionError + result;
            }
            btnCheckSharepointV3Web.Enabled = true;
        }

        /// <summary>
        /// attempts to verify validity of the SharePoint V3 library path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCheckSLKDocLib_Click(object sender, EventArgs e)
        {
            lblSLKDocLibStatus.Text = String.Empty;
            btnCheckSLKDocLib.Enabled = false;
            SharePointV3 site = new SharePointV3();
            string result;
            if (site.TestSharePointV3Site(tbDocLibWeb.Text, out result))
            {                
                if (site.TestSharePointDocLibrary(tbDocLibWeb.Text, tbDocLibName.Text, out result))
                {
                    lblSLKDocLibStatus.Text += TextResources.WebConnectionSuccessful;
                }
                else
                {
                    lblSLKDocLibStatus.Text += TextResources.DocLibraryTestError + result;
                }
            }
            else
            {
                lblSLKDocLibStatus.Text += TextResources.WebConnectionError + result;
            }
            btnCheckSLKDocLib.Enabled = true;
        }


 

 

    }
}