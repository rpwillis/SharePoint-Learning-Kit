/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// ValidatePackage.cs
//
// This is Microsoft Learning Components (MLC) sample code that compiles into a Windows Forms
// application.  (MLC is a set of components distributed with SharePoint Learning Kit.)  You can
// compile this application using Visual Studio 2005, or you can compile and run this application
// without Visual Studio installed by double-clicking CompileAndRun.bat.
//
// This sample code is located in Samples\ValidatePackage within SLK-SDK-n.n.nnn-ENU.zip.
//
// This application validates the contents of a SCORM 1.2, SCORM 2004 or Class Server LRM package.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.LearningComponents;

namespace ValidatePackage
{
    public partial class ValidatePackage : Form
    {
        /// <summary>
        /// Called when this class is created.
        /// </summary>
        ///
        /// <param name="initialFileOrFolder">If a file or folder was specified on the command
		/// 	line, this is that file or folder, otherwise this parameter is null.</param>
        ///
        public ValidatePackage(string initialFileOrFolder)
        {
            InitializeComponent();
            if (initialFileOrFolder != null)
            {
				if (Directory.Exists(initialFileOrFolder))
                {
                    folderTextBox.Text = initialFileOrFolder;
                    PopulatePackageList(null);
                }
                else
                if (File.Exists(initialFileOrFolder))
                {
                    folderTextBox.Text = Path.GetDirectoryName(initialFileOrFolder);
                    PopulatePackageList(Path.GetFileName(initialFileOrFolder));
                }
            }
        }

        /// <summary>
        /// Called when the user clicks View, to view the folder they typed in.
        /// </summary>
        ///
        private void viewButton_Click(object sender, EventArgs e)
        {
            // fill in <packageListBox> with the packages in the selected folder
            PopulatePackageList(null);
        }

        /// <summary>
        /// Called when the user clicks Browse to view a folder browser dialog.
        /// </summary>
        ///
        private void browseButton_Click(object sender, EventArgs e)
        {
            // prompt the user for a folder to view
            folderBrowserDialog.SelectedPath = folderTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
                return;

            // copy the selected folder path into <folderTextBox>
            folderTextBox.Text = folderBrowserDialog.SelectedPath;

            // fill in <packageListBox> with the packages in the selected folder
            PopulatePackageList(null);
        }

        /// <summary>
        /// Fills in the listbox with the names of .zip/.lrm/.ims files in the specified folder.
        /// </summary>
        /// 
        /// <param name="fileNameToSelect">If not null, then the file with this name is selected
        ///     if it's found.</param>
        ///
        void PopulatePackageList(string fileNameToSelect)
        {
            // enumerate the contents of the folder into <packageListBox>; set <selectedIndex> to
            // the index of the item corresponding to <fileNameToSelect>, or -1 if none
            int selectedIndex = -1;
            try
            {
                packageListBox.Items.Clear();
                foreach (string filePath in Directory.GetFiles(folderTextBox.Text))
                {
                    string extension = Path.GetExtension(filePath);
                    if (!String.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase) &&
                        !String.Equals(extension, ".lrm", StringComparison.OrdinalIgnoreCase) &&
                        !String.Equals(extension, ".ims", StringComparison.OrdinalIgnoreCase))
                        continue;
                    int index = packageListBox.Items.Add(new FileListItem(filePath));
                    if (String.Equals(fileNameToSelect, Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase))
                        selectedIndex = index;
                }
            }
            catch (DirectoryNotFoundException)
            {
                folderTextBox.SelectAll();
                MessageBox.Show("Invalid folder");
            }

            // empty <logTextBox>
            logTextBox.Clear();

            // if <fileNameToSelect> was found, select it now
            packageListBox.SelectedIndex = selectedIndex;
        }

        /// <summary>
        /// Called when the user clicks on the name of a package.
        /// </summary>
        ///
        private void packageListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateSelectedPackage();
        }

		/// <summary>
		/// Validate the package selected by the user.
		/// </summary>
        ///
		void ValidateSelectedPackage()
		{
            // do nothing if no package is selected
            if (packageListBox.SelectedIndex < 0)
                return;

            // execute the remainder of this method with a wait cursor displayed
            Cursor = Cursors.WaitCursor;
            try
            {
               // set <filePath> to the path to the package the user selected
                FileListItem fileListItem = (FileListItem)packageListBox.SelectedItem;
                string filePath = fileListItem.FilePath;

                // empty <logTextBox>, and add the file name
                logTextBox.Clear();
                logTextBox.AppendText(String.Format("Validating: {0}\r\n\r\n", filePath));

                // validate the package
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool anyMessages = false;
                    using (PackageReader packageReader = PackageReader.Create(stream))
                    {
                        // set package validator settings based on the setting of the <additionalScormWarningsCheckBox> control
                        PackageValidatorSettings settings;
						if (additionalScormWarningsCheckBox.Checked)
						{
                            // perform the same validation as SLK, plus look for additional SCORM issues
							settings = new PackageValidatorSettings(
								ValidationBehavior.LogWarning, ValidationBehavior.LogWarning,
                                ValidationBehavior.LogError, ValidationBehavior.LogWarning);
						}
						else
						{
                            // perform the same validation as SLK
                            settings = new PackageValidatorSettings(
                                ValidationBehavior.LogWarning, ValidationBehavior.None,
                                ValidationBehavior.LogError, ValidationBehavior.LogWarning);
						}

                        // validate the package; set <results> to the list of results
                        ValidationResults results;
                        try
                        {
                            results = PackageValidator.Validate(packageReader, settings);
                        }
                        catch (InvalidPackageException ex)
                        {
                            logTextBox.AppendText(String.Format("Package is invalid.\r\n\r\n{0}", ex.Message));
                            return;
                        }

                        // display <results>
                        foreach (ValidationResult result in results.Results)
                        {
                            string message;
                            if (result.IsError)
                                message = String.Format("MLC Error: {0}\r\n\r\n", result.Message);
                            else
                                message = String.Format("SCORM Warning: {0}\r\n\r\n", result.Message);
                            logTextBox.AppendText(message);
                            anyMessages = true;
                        }
                    }

                    // if there were no messages, give feedback to that effect to the user
                    if (!anyMessages)
                        logTextBox.AppendText("Package is valid.");
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// The user checked or unchecked the "additional SCORM warnings" checkbox.
        /// </summary>
        ///
        private void additionalScormWarningsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ValidateSelectedPackage();
        }
    }

    // <summary>
    // An item in the packageListBox control.
    // </summary>
    //
    class FileListItem
    {
        public string FilePath;
        public FileListItem(string filePath)
        {
            FilePath = filePath;
        }
        public override string ToString()
        {
            return Path.GetFileName(FilePath);
        }
    }
}

