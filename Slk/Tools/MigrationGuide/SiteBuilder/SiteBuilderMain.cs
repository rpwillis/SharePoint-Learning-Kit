/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MigrationHelper;

namespace SiteBuilder
{
    public partial class SiteBuilderMain : Form
    {
        private CS4UserCollection m_users;
        public SiteBuilderMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Displays settings form
        /// </summary>
        private void llGotoSettingsForm_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lblSettingsStatus.Text = String.Empty;
            SiteBuilderSettings settingsForm = new SiteBuilderSettings();
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                if (AllAppSettingsSet())
                {
                    lblSettingsStatus.Text = TextResources.Completed;
                }
                else
                {
                    lblSettingsStatus.Text = TextResources.AppSettingsNotSet;
                }
            }    
        }

        /// <summary>
        /// Checks if some settings missing in the config file
        /// </summary>
        /// <returns>true if all settings are set</returns>
        private bool AllAppSettingsSet()
        {
            bool result = true;
            if ((SiteBuilder.Default.ClassServerDBConnectionString.Trim().Length == 0) ||
                (SiteBuilder.Default.ActiveDirectoryPath.Trim().Length == 0) ||
                (SiteBuilder.Default.SLKSchoolWeb.Trim().Length == 0) ||
                (SiteBuilder.Default.SLKDocLibraryWeb.Trim().Length == 0) ||
                (SiteBuilder.Default.SLKDocLibraryName.Trim().Length == 0))
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Asyncronously calls VerifyAgainstActiveDirectory methos of ClassServerUsers class
        /// </summary>
        private void llCheckUsersInAD_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!AllAppSettingsSet())
            {
                lblCheckUsersInADStatus.Text = TextResources.AppSettingsNotSet;
                return;
            }
            m_users = new CS4UserCollection();
            System.ComponentModel.BackgroundWorker usersCheck = new BackgroundWorker();
            usersCheck.WorkerReportsProgress = true;
            usersCheck.DoWork += new DoWorkEventHandler(usersCheck_DoWork);
            usersCheck.RunWorkerCompleted += new RunWorkerCompletedEventHandler(usersCheck_RunWorkerCompleted);
            usersCheck.ProgressChanged += new ProgressChangedEventHandler(usersCheck_ProgressChanged);
            usersCheck.RunWorkerAsync(SiteBuilder.Default.ActiveDirectoryPath);
        }

        private void usersCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = m_users.VerifyAgainstActiveDirectory((string)e.Argument, SiteBuilder.Default.ADCheckLogFile, worker, e);
        }

        private void usersCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {       
            string status = e.Result.ToString();            
            if (m_users.UsersNotInActiveDirectory > 0)
            {
                status += String.Format(System.Environment.NewLine + TextResources.UsersNotFoundInAD,m_users.UsersNotInActiveDirectory.ToString());
            }
            status += String.Format(TextResources.SeeLogFileForMoreDetails,SiteBuilder.Default.ADCheckLogFile);
            lblCheckUsersInADStatus.Text = status;
        }

        private void usersCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblCheckUsersInADStatus.Text = e.UserState.ToString();
        }

        /// <summary>
        /// Calls CS4Database.ExportClassesStructure to export Class Server 4 classes, 
        /// groups and users into XML file. File name is taken from settings file.
        /// </summary>
        private void llExportClassesXML_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!AllAppSettingsSet())
            {
                lblExportClassesStatus.Text = TextResources.AppSettingsNotSet;
                return;
            }
            lblExportClassesStatus.Text = String.Empty;
            CS4Database database = new CS4Database(SiteBuilder.Default.ClassServerDBConnectionString);            
            try
            {
                bool result = database.ExportClassesStructure(System.IO.Directory.GetCurrentDirectory() + "\\" + SiteBuilder.Default.ClassStructureXML);
                if (result)
                {
                    lblExportClassesStatus.Text = String.Format(TextResources.SuccessExportingData, SiteBuilder.Default.ClassStructureXML);
                }
                else
                {
                    lblExportClassesStatus.Text = String.Format(TextResources.FailedToExportData, SiteBuilder.Default.ClassStructureXML);
                }
            }
            catch (System.Exception ex)
            {
                lblExportClassesStatus.Text = TextResources.DataExportError + ex.Message;
            }
        }

        private SLKSite m_site;

        /// <summary>
        /// Asyncronously calls SLKSite.CreateSiteFromXML to parse XML file (file name 
        /// is taken from application settings file) and create SLK sites system for 
        /// Class Server classes, groups and users
        /// </summary>
        private void llCreateSLKClasses_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!AllAppSettingsSet())
            {
                lblCreateSitesStatus.Text = TextResources.AppSettingsNotSet;
                return;
            }
            m_site = new SLKSite();
            System.ComponentModel.BackgroundWorker createSiteWorker = new BackgroundWorker();
            createSiteWorker.WorkerReportsProgress = true;
            createSiteWorker.DoWork += new DoWorkEventHandler(createSiteWorker_DoWork);
            createSiteWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(createSiteWorker_RunWorkerCompleted);
            createSiteWorker.ProgressChanged += new ProgressChangedEventHandler(createSiteWorker_ProgressChanged);
            createSiteWorker.RunWorkerAsync(System.IO.Directory.GetCurrentDirectory() + "\\" + SiteBuilder.Default.ClassStructureXML);
        }

        private void createSiteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = m_site.CreateSitesFromXML((string)e.Argument, SiteBuilder.Default.SLKSiteLogFile, worker, e);
        }

        private void createSiteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool status = (bool)e.Result;
            lblCreateSitesStatus.Text = String.Empty;
            if (!status)
            {
                lblCreateSitesStatus.Text = TextResources.ErrorsProcessingXMLFile + System.Environment.NewLine;
            }
            else
            {
                lblCreateSitesStatus.Text = TextResources.XMLFileProcessingSuccessful;
            }
            lblCreateSitesStatus.Text += String.Format(TextResources.SeeLogFileForMoreDetails,SiteBuilder.Default.SLKSiteLogFile);
        }

        private void createSiteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblCreateSitesStatus.Text = e.UserState.ToString();
        }

        private UserDataTransfer dataTransfer;

        /// <summary>
        /// Asyncroniously calls the UserDataTransfer.MoveUserAssignments method,
        /// reports progress events returned.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void llMoveUserData_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!AllAppSettingsSet())
            {
                lblMoveUserDataStatus.Text = TextResources.AppSettingsNotSet;
                return;
            }
            dataTransfer = new UserDataTransfer();
            System.ComponentModel.BackgroundWorker moveDataWorker = new BackgroundWorker();
            moveDataWorker.WorkerReportsProgress = true;
            moveDataWorker.DoWork += new DoWorkEventHandler(moveDataWorker_DoWork);
            moveDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(moveDataWorker_RunWorkerCompleted);
            moveDataWorker.ProgressChanged += new ProgressChangedEventHandler(moveDataWorker_ProgressChanged);
            moveDataWorker.RunWorkerAsync(System.IO.Directory.GetCurrentDirectory() + "\\" + SiteBuilder.Default.ClassStructureXML);
        }

        private void moveDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = dataTransfer.MoveUserAssignments((string)e.Argument, SiteBuilder.Default.UserDataTransferLogFile, worker, e);
        }

        private void moveDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool status = (bool)e.Result;
            lblMoveUserDataStatus.Text = String.Empty;
            if (!status)
            {
                lblMoveUserDataStatus.Text = TextResources.ErrorsMovingUserData + System.Environment.NewLine;
            }
            else
            {
                lblMoveUserDataStatus.Text = TextResources.UserDataTransferSuccessful;
            }
            lblMoveUserDataStatus.Text += String.Format(TextResources.SeeLogFileForMoreDetails, SiteBuilder.Default.UserDataTransferLogFile);
        }

        private void moveDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblMoveUserDataStatus.Text = e.UserState.ToString();
        }






    }
}