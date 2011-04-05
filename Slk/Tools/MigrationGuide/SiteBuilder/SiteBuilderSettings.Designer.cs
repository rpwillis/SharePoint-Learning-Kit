namespace SiteBuilder
{
    partial class SiteBuilderSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SiteBuilderSettings));
            this.lblServerName = new System.Windows.Forms.Label();
            this.tableLayoutPanelDBDetails = new System.Windows.Forms.TableLayoutPanel();
            this.tbServerName = new System.Windows.Forms.TextBox();
            this.chbWindowsAuthentication = new System.Windows.Forms.CheckBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.tbUserName = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.lblDBName = new System.Windows.Forms.Label();
            this.tbDatabaseName = new System.Windows.Forms.TextBox();
            this.tableLayoutPanelDBConnectionStatus = new System.Windows.Forms.TableLayoutPanel();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.btnTestDBConnection = new System.Windows.Forms.Button();
            this.lblCS4DB = new System.Windows.Forms.Label();
            this.lblADPath = new System.Windows.Forms.Label();
            this.lblADPathComment = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanelSLKWeb = new System.Windows.Forms.TableLayoutPanel();
            this.tbSLKWeb = new System.Windows.Forms.TextBox();
            this.lblSLKSiteInitStatus = new System.Windows.Forms.Label();
            this.btnCheckSharepointV3Web = new System.Windows.Forms.Button();
            this.lblSharepointWebComment = new System.Windows.Forms.Label();
            this.lblSharepointWeb = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanelADPath = new System.Windows.Forms.TableLayoutPanel();
            this.lblADBindingStatus = new System.Windows.Forms.Label();
            this.btnBindAD = new System.Windows.Forms.Button();
            this.tbADPath = new System.Windows.Forms.TextBox();
            this.tableLayoutPanelPage = new System.Windows.Forms.TableLayoutPanel();
            this.tabcSettings = new System.Windows.Forms.TabControl();
            this.tabPageClassServer4 = new System.Windows.Forms.TabPage();
            this.tabPageActiveDirectory = new System.Windows.Forms.TabPage();
            this.tabPageSLK = new System.Windows.Forms.TabPage();
            this.tableLayoutPanelDocLibrary = new System.Windows.Forms.TableLayoutPanel();
            this.lblSLKDocLibStatus = new System.Windows.Forms.Label();
            this.pnlSLKDocLib = new System.Windows.Forms.Panel();
            this.tbDocLibName = new System.Windows.Forms.TextBox();
            this.tbDocLibWeb = new System.Windows.Forms.TextBox();
            this.lblSLKDocLibName = new System.Windows.Forms.Label();
            this.lblSLKDocLibWeb = new System.Windows.Forms.Label();
            this.btnCheckSLKDocLib = new System.Windows.Forms.Button();
            this.lblDocLibraryComment = new System.Windows.Forms.Label();
            this.lblDocLibraryText = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.tableLayoutPanelDBDetails.SuspendLayout();
            this.tableLayoutPanelDBConnectionStatus.SuspendLayout();
            this.tableLayoutPanelSLKWeb.SuspendLayout();
            this.tableLayoutPanelADPath.SuspendLayout();
            this.tableLayoutPanelPage.SuspendLayout();
            this.tabcSettings.SuspendLayout();
            this.tabPageClassServer4.SuspendLayout();
            this.tabPageActiveDirectory.SuspendLayout();
            this.tabPageSLK.SuspendLayout();
            this.tableLayoutPanelDocLibrary.SuspendLayout();
            this.pnlSLKDocLib.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblServerName
            // 
            resources.ApplyResources(this.lblServerName, "lblServerName");
            this.lblServerName.Name = "lblServerName";
            // 
            // tableLayoutPanelDBDetails
            // 
            resources.ApplyResources(this.tableLayoutPanelDBDetails, "tableLayoutPanelDBDetails");
            this.tableLayoutPanelDBDetails.Controls.Add(this.tbServerName, 1, 0);
            this.tableLayoutPanelDBDetails.Controls.Add(this.lblServerName, 0, 0);
            this.tableLayoutPanelDBDetails.Controls.Add(this.chbWindowsAuthentication, 1, 1);
            this.tableLayoutPanelDBDetails.Controls.Add(this.lblUserName, 0, 2);
            this.tableLayoutPanelDBDetails.Controls.Add(this.tbUserName, 1, 2);
            this.tableLayoutPanelDBDetails.Controls.Add(this.lblPassword, 0, 3);
            this.tableLayoutPanelDBDetails.Controls.Add(this.tbPassword, 1, 3);
            this.tableLayoutPanelDBDetails.Controls.Add(this.lblDBName, 0, 4);
            this.tableLayoutPanelDBDetails.Controls.Add(this.tbDatabaseName, 1, 4);
            this.tableLayoutPanelDBDetails.Controls.Add(this.tableLayoutPanelDBConnectionStatus, 1, 5);
            this.tableLayoutPanelDBDetails.Name = "tableLayoutPanelDBDetails";
            // 
            // tbServerName
            // 
            resources.ApplyResources(this.tbServerName, "tbServerName");
            this.tbServerName.Name = "tbServerName";
            // 
            // chbWindowsAuthentication
            // 
            resources.ApplyResources(this.chbWindowsAuthentication, "chbWindowsAuthentication");
            this.chbWindowsAuthentication.Checked = true;
            this.chbWindowsAuthentication.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbWindowsAuthentication.Name = "chbWindowsAuthentication";
            this.chbWindowsAuthentication.UseVisualStyleBackColor = true;
            this.chbWindowsAuthentication.CheckedChanged += new System.EventHandler(this.chbWindowsAuthentication_CheckedChanged);
            // 
            // lblUserName
            // 
            resources.ApplyResources(this.lblUserName, "lblUserName");
            this.lblUserName.Name = "lblUserName";
            // 
            // tbUserName
            // 
            resources.ApplyResources(this.tbUserName, "tbUserName");
            this.tbUserName.Name = "tbUserName";
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // tbPassword
            // 
            resources.ApplyResources(this.tbPassword, "tbPassword");
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.UseSystemPasswordChar = true;
            // 
            // lblDBName
            // 
            resources.ApplyResources(this.lblDBName, "lblDBName");
            this.lblDBName.Name = "lblDBName";
            // 
            // tbDatabaseName
            // 
            resources.ApplyResources(this.tbDatabaseName, "tbDatabaseName");
            this.tbDatabaseName.Name = "tbDatabaseName";
            // 
            // tableLayoutPanelDBConnectionStatus
            // 
            resources.ApplyResources(this.tableLayoutPanelDBConnectionStatus, "tableLayoutPanelDBConnectionStatus");
            this.tableLayoutPanelDBConnectionStatus.Controls.Add(this.lblConnectionStatus, 0, 0);
            this.tableLayoutPanelDBConnectionStatus.Controls.Add(this.btnTestDBConnection, 1, 0);
            this.tableLayoutPanelDBConnectionStatus.Name = "tableLayoutPanelDBConnectionStatus";
            // 
            // lblConnectionStatus
            // 
            resources.ApplyResources(this.lblConnectionStatus, "lblConnectionStatus");
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            // 
            // btnTestDBConnection
            // 
            resources.ApplyResources(this.btnTestDBConnection, "btnTestDBConnection");
            this.btnTestDBConnection.Name = "btnTestDBConnection";
            this.btnTestDBConnection.UseVisualStyleBackColor = true;
            this.btnTestDBConnection.Click += new System.EventHandler(this.btnTestDBConnection_Click);
            // 
            // lblCS4DB
            // 
            resources.ApplyResources(this.lblCS4DB, "lblCS4DB");
            this.lblCS4DB.Name = "lblCS4DB";
            // 
            // lblADPath
            // 
            resources.ApplyResources(this.lblADPath, "lblADPath");
            this.lblADPath.Name = "lblADPath";
            // 
            // lblADPathComment
            // 
            resources.ApplyResources(this.lblADPathComment, "lblADPathComment");
            this.lblADPathComment.Name = "lblADPathComment";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tableLayoutPanelSLKWeb
            // 
            resources.ApplyResources(this.tableLayoutPanelSLKWeb, "tableLayoutPanelSLKWeb");
            this.tableLayoutPanelSLKWeb.Controls.Add(this.tbSLKWeb, 0, 0);
            this.tableLayoutPanelSLKWeb.Controls.Add(this.lblSLKSiteInitStatus, 0, 1);
            this.tableLayoutPanelSLKWeb.Controls.Add(this.btnCheckSharepointV3Web, 1, 0);
            this.tableLayoutPanelSLKWeb.Name = "tableLayoutPanelSLKWeb";
            // 
            // tbSLKWeb
            // 
            resources.ApplyResources(this.tbSLKWeb, "tbSLKWeb");
            this.tbSLKWeb.Name = "tbSLKWeb";
            // 
            // lblSLKSiteInitStatus
            // 
            resources.ApplyResources(this.lblSLKSiteInitStatus, "lblSLKSiteInitStatus");
            this.lblSLKSiteInitStatus.MaximumSize = new System.Drawing.Size(350, 70);
            this.lblSLKSiteInitStatus.Name = "lblSLKSiteInitStatus";
            // 
            // btnCheckSharepointV3Web
            // 
            resources.ApplyResources(this.btnCheckSharepointV3Web, "btnCheckSharepointV3Web");
            this.btnCheckSharepointV3Web.Name = "btnCheckSharepointV3Web";
            this.btnCheckSharepointV3Web.UseVisualStyleBackColor = true;
            this.btnCheckSharepointV3Web.Click += new System.EventHandler(this.btnCheckSharepointV3Web_Click);
            // 
            // lblSharepointWebComment
            // 
            resources.ApplyResources(this.lblSharepointWebComment, "lblSharepointWebComment");
            this.lblSharepointWebComment.Name = "lblSharepointWebComment";
            // 
            // lblSharepointWeb
            // 
            resources.ApplyResources(this.lblSharepointWeb, "lblSharepointWeb");
            this.lblSharepointWeb.Name = "lblSharepointWeb";
            // 
            // flowLayoutPanel2
            // 
            resources.ApplyResources(this.flowLayoutPanel2, "flowLayoutPanel2");
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            // 
            // tableLayoutPanelADPath
            // 
            resources.ApplyResources(this.tableLayoutPanelADPath, "tableLayoutPanelADPath");
            this.tableLayoutPanelADPath.Controls.Add(this.lblADBindingStatus, 0, 1);
            this.tableLayoutPanelADPath.Controls.Add(this.btnBindAD, 1, 0);
            this.tableLayoutPanelADPath.Controls.Add(this.tbADPath, 0, 0);
            this.tableLayoutPanelADPath.Name = "tableLayoutPanelADPath";
            // 
            // lblADBindingStatus
            // 
            resources.ApplyResources(this.lblADBindingStatus, "lblADBindingStatus");
            this.lblADBindingStatus.Name = "lblADBindingStatus";
            // 
            // btnBindAD
            // 
            resources.ApplyResources(this.btnBindAD, "btnBindAD");
            this.btnBindAD.Name = "btnBindAD";
            this.btnBindAD.UseVisualStyleBackColor = true;
            this.btnBindAD.Click += new System.EventHandler(this.btnBindAD_Click);
            // 
            // tbADPath
            // 
            resources.ApplyResources(this.tbADPath, "tbADPath");
            this.tbADPath.Name = "tbADPath";
            // 
            // tableLayoutPanelPage
            // 
            resources.ApplyResources(this.tableLayoutPanelPage, "tableLayoutPanelPage");
            this.tableLayoutPanelPage.Controls.Add(this.tabcSettings, 0, 0);
            this.tableLayoutPanelPage.Controls.Add(this.pnlButtons, 0, 1);
            this.tableLayoutPanelPage.Name = "tableLayoutPanelPage";
            // 
            // tabcSettings
            // 
            this.tabcSettings.Controls.Add(this.tabPageClassServer4);
            this.tabcSettings.Controls.Add(this.tabPageActiveDirectory);
            this.tabcSettings.Controls.Add(this.tabPageSLK);
            resources.ApplyResources(this.tabcSettings, "tabcSettings");
            this.tabcSettings.Name = "tabcSettings";
            this.tabcSettings.SelectedIndex = 0;
            // 
            // tabPageClassServer4
            // 
            this.tabPageClassServer4.Controls.Add(this.lblCS4DB);
            this.tabPageClassServer4.Controls.Add(this.tableLayoutPanelDBDetails);
            resources.ApplyResources(this.tabPageClassServer4, "tabPageClassServer4");
            this.tabPageClassServer4.Name = "tabPageClassServer4";
            this.tabPageClassServer4.UseVisualStyleBackColor = true;
            // 
            // tabPageActiveDirectory
            // 
            this.tabPageActiveDirectory.Controls.Add(this.lblADPathComment);
            this.tabPageActiveDirectory.Controls.Add(this.lblADPath);
            this.tabPageActiveDirectory.Controls.Add(this.tableLayoutPanelADPath);
            resources.ApplyResources(this.tabPageActiveDirectory, "tabPageActiveDirectory");
            this.tabPageActiveDirectory.Name = "tabPageActiveDirectory";
            this.tabPageActiveDirectory.UseVisualStyleBackColor = true;
            // 
            // tabPageSLK
            // 
            this.tabPageSLK.Controls.Add(this.tableLayoutPanelDocLibrary);
            this.tabPageSLK.Controls.Add(this.lblDocLibraryComment);
            this.tabPageSLK.Controls.Add(this.lblDocLibraryText);
            this.tabPageSLK.Controls.Add(this.lblSharepointWeb);
            this.tabPageSLK.Controls.Add(this.lblSharepointWebComment);
            this.tabPageSLK.Controls.Add(this.tableLayoutPanelSLKWeb);
            resources.ApplyResources(this.tabPageSLK, "tabPageSLK");
            this.tabPageSLK.Name = "tabPageSLK";
            this.tabPageSLK.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelDocLibrary
            // 
            resources.ApplyResources(this.tableLayoutPanelDocLibrary, "tableLayoutPanelDocLibrary");
            this.tableLayoutPanelDocLibrary.Controls.Add(this.lblSLKDocLibStatus, 0, 1);
            this.tableLayoutPanelDocLibrary.Controls.Add(this.pnlSLKDocLib, 0, 0);
            this.tableLayoutPanelDocLibrary.Controls.Add(this.btnCheckSLKDocLib, 1, 0);
            this.tableLayoutPanelDocLibrary.Name = "tableLayoutPanelDocLibrary";
            // 
            // lblSLKDocLibStatus
            // 
            resources.ApplyResources(this.lblSLKDocLibStatus, "lblSLKDocLibStatus");
            this.lblSLKDocLibStatus.MaximumSize = new System.Drawing.Size(350, 70);
            this.lblSLKDocLibStatus.Name = "lblSLKDocLibStatus";
            // 
            // pnlSLKDocLib
            // 
            this.pnlSLKDocLib.Controls.Add(this.tbDocLibName);
            this.pnlSLKDocLib.Controls.Add(this.tbDocLibWeb);
            this.pnlSLKDocLib.Controls.Add(this.lblSLKDocLibName);
            this.pnlSLKDocLib.Controls.Add(this.lblSLKDocLibWeb);
            resources.ApplyResources(this.pnlSLKDocLib, "pnlSLKDocLib");
            this.pnlSLKDocLib.Name = "pnlSLKDocLib";
            // 
            // tbDocLibName
            // 
            resources.ApplyResources(this.tbDocLibName, "tbDocLibName");
            this.tbDocLibName.Name = "tbDocLibName";
            // 
            // tbDocLibWeb
            // 
            resources.ApplyResources(this.tbDocLibWeb, "tbDocLibWeb");
            this.tbDocLibWeb.Name = "tbDocLibWeb";
            // 
            // lblSLKDocLibName
            // 
            resources.ApplyResources(this.lblSLKDocLibName, "lblSLKDocLibName");
            this.lblSLKDocLibName.Name = "lblSLKDocLibName";
            // 
            // lblSLKDocLibWeb
            // 
            resources.ApplyResources(this.lblSLKDocLibWeb, "lblSLKDocLibWeb");
            this.lblSLKDocLibWeb.Name = "lblSLKDocLibWeb";
            // 
            // btnCheckSLKDocLib
            // 
            resources.ApplyResources(this.btnCheckSLKDocLib, "btnCheckSLKDocLib");
            this.btnCheckSLKDocLib.Name = "btnCheckSLKDocLib";
            this.btnCheckSLKDocLib.UseVisualStyleBackColor = true;
            this.btnCheckSLKDocLib.Click += new System.EventHandler(this.btnCheckSLKDocLib_Click);
            // 
            // lblDocLibraryComment
            // 
            resources.ApplyResources(this.lblDocLibraryComment, "lblDocLibraryComment");
            this.lblDocLibraryComment.Name = "lblDocLibraryComment";
            // 
            // lblDocLibraryText
            // 
            resources.ApplyResources(this.lblDocLibraryText, "lblDocLibraryText");
            this.lblDocLibraryText.Name = "lblDocLibraryText";
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnOK);
            this.pnlButtons.Controls.Add(this.btnCancel);
            resources.ApplyResources(this.pnlButtons, "pnlButtons");
            this.pnlButtons.Name = "pnlButtons";
            // 
            // SiteBuilderSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelPage);
            this.Name = "SiteBuilderSettings";
            this.Shown += new System.EventHandler(this.SiteBuilderUI_Shown);
            this.tableLayoutPanelDBDetails.ResumeLayout(false);
            this.tableLayoutPanelDBDetails.PerformLayout();
            this.tableLayoutPanelDBConnectionStatus.ResumeLayout(false);
            this.tableLayoutPanelDBConnectionStatus.PerformLayout();
            this.tableLayoutPanelSLKWeb.ResumeLayout(false);
            this.tableLayoutPanelSLKWeb.PerformLayout();
            this.tableLayoutPanelADPath.ResumeLayout(false);
            this.tableLayoutPanelADPath.PerformLayout();
            this.tableLayoutPanelPage.ResumeLayout(false);
            this.tabcSettings.ResumeLayout(false);
            this.tabPageClassServer4.ResumeLayout(false);
            this.tabPageClassServer4.PerformLayout();
            this.tabPageActiveDirectory.ResumeLayout(false);
            this.tabPageActiveDirectory.PerformLayout();
            this.tabPageSLK.ResumeLayout(false);
            this.tabPageSLK.PerformLayout();
            this.tableLayoutPanelDocLibrary.ResumeLayout(false);
            this.tableLayoutPanelDocLibrary.PerformLayout();
            this.pnlSLKDocLib.ResumeLayout(false);
            this.pnlSLKDocLib.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDBDetails;
        private System.Windows.Forms.TextBox tbServerName;
        private System.Windows.Forms.CheckBox chbWindowsAuthentication;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox tbUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label lblCS4DB;
        private System.Windows.Forms.Label lblDBName;
        private System.Windows.Forms.TextBox tbDatabaseName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDBConnectionStatus;
        private System.Windows.Forms.Button btnTestDBConnection;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label lblADPath;
        private System.Windows.Forms.Label lblADPathComment;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSLKWeb;
        private System.Windows.Forms.TextBox tbSLKWeb;
        private System.Windows.Forms.Label lblSLKSiteInitStatus;
        private System.Windows.Forms.Label lblSharepointWebComment;
        private System.Windows.Forms.Label lblSharepointWeb;
        private System.Windows.Forms.Button btnCheckSharepointV3Web;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelADPath;
        private System.Windows.Forms.Label lblADBindingStatus;
        private System.Windows.Forms.Button btnBindAD;
        private System.Windows.Forms.TextBox tbADPath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPage;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.TabControl tabcSettings;
        private System.Windows.Forms.TabPage tabPageClassServer4;
        private System.Windows.Forms.TabPage tabPageActiveDirectory;
        private System.Windows.Forms.TabPage tabPageSLK;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDocLibrary;
        private System.Windows.Forms.TextBox tbDocLibWeb;
        private System.Windows.Forms.Label lblSLKDocLibStatus;
        private System.Windows.Forms.Button btnCheckSLKDocLib;
        private System.Windows.Forms.Label lblDocLibraryComment;
        private System.Windows.Forms.Label lblDocLibraryText;
        private System.Windows.Forms.Panel pnlSLKDocLib;
        private System.Windows.Forms.Label lblSLKDocLibWeb;
        private System.Windows.Forms.TextBox tbDocLibName;
        private System.Windows.Forms.Label lblSLKDocLibName;
    }
}

