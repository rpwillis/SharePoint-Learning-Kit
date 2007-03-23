/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Note: There are some issues in running the below Visual Studio Tests in x64 environments
// Functions that make a managed to unmanaged code transition fail in the CLR in x64 
// environments

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.LearningComponents
{
    namespace CompressionUnitTest
    {
        /// <summary>
        ///This is a test class for Microsoft.LearningComponents.Compression and is intended
        ///to contain all Microsoft.LearningComponents.Compression Unit Tests
        ///</summary>
        [TestClass()]
        public class CompressionTest
        {


            string unittestdir = null;
            string destdir = null;
            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }
            #region Additional test attributes
            // 
            //You can use the following additional attributes as you write your tests:
            //
            //Use ClassInitialize to run code before running the first test in the class
            //
            //[ClassInitialize()]
            //public static void MyClassInitialize(TestContext testContext)
            //{
            //}
            //
            //Use ClassCleanup to run code after all tests in a class have run
            //
            //[ClassCleanup()]
            //public static void MyClassCleanup()
            //{
            //}
            //
            //Use TestInitialize to run code before running each test
            //
            [TestInitialize()]
            public void MyTestInitialize()
            {
                
                DirectoryInfo currentDir = new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                unittestdir =  currentDir.Parent.Parent.Parent.FullName + "\\CompressionUnitTest\\UnitTestData";
                destdir = unittestdir + "\\UnbundleFolder";
            }
            //
            //Use TestCleanup to run code after each test has run
            //
            //[TestCleanup()]
            //public void MyTestCleanup()
            //{
            //}
            //
            #endregion


            /// <summary>
            ///A test for Unbundle (FileInfo, DirectoryInfo)
            ///</summary>
            [TestMethod()]
            public void UnbundleTest()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\MyFirstLR.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\MyFirstLR");

                Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);

            }

            [TestMethod()]
            public void UnbundleNonExistingFile()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "DoesNotExist.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\DoesNotExist");

                try
                {

                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Assert.AreEqual("System.IO.FileNotFoundException", e.GetType().ToString());
                }
            }

            [TestMethod()]
            public void BoundaryStringMissing()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\BoundaryStringMissing.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\BoundaryStringMissing");

                try
                {

                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: Corrupt File", e.Message);
                }

            }

            [TestMethod()]
            public void ContentLocationMissing()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\ContentLocationMissing.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\ContentLocationMissing");

                try
                {
                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: Corrupt File", e.Message);
                }

            }

            [TestMethod()]
            public void CorruptMessagePart()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\CorruptMessagePart.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\CorruptMessagePart");

                try
                {
                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: Corrupt File", e.Message);
                }
            }

            [TestMethod()]
            public void IncorrectEncodingLength()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\IncorrectEncodingLength.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\IncorrectEncodingLength");

                try
                {
                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: Corrupt File", e.Message);
                }

            }

            [TestMethod()]
            public void InvalidLRMVersion()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\InvalidLRMVersion.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\InvalidLRMVersion");

                try
                {
                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: Bad version info", e.Message);
                }


            }


            [TestMethod()]
            public void PrematureFileEnd()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\PrematureFileEnd.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\PrematureFileEnd");

                try
                {
                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: File has ended prematurely", e.Message);
                }

            }


            [TestMethod()]
            public void TerminatingBSMissing()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\TerminatingBSMissing.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\TerminatingBSMissing");

                try
                {
                    Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);
                }
                catch (CompressionException e)
                {
                    Assert.AreEqual("Bad LRM input: File has ended prematurely", e.Message);
                }
            }


            /// <summary>
            ///A test for Unzip (FileInfo, DirectoryInfo)
            ///</summary>
            [TestMethod()]
            public void UnzipTest()
            {
                FileInfo zipFile = new FileInfo(unittestdir + "\\Solitaire.zip");
                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\Solitaire");

                Microsoft.LearningComponents.Compression.Unzip(zipFile, destinationDirectory);

            }

            [TestMethod()]
            public void UnzipNonExistingFile()
            {
                FileInfo zipFile = new FileInfo(unittestdir + "DoesNotExist.zip");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\DoesNotExist");

                try
                {

                    Microsoft.LearningComponents.Compression.Unzip(zipFile, destinationDirectory);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Assert.AreEqual("System.IO.FileNotFoundException", e.GetType().ToString());
                }
            }

            [TestMethod()]
            public void UnzipCorruptFile()
            {
                FileInfo zipFile = new FileInfo(unittestdir + "\\CorruptZip.zip");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\CorruptZip");

                try
                {

                    Microsoft.LearningComponents.Compression.Unzip(zipFile, destinationDirectory);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("System.IO.FileFormatException", e.GetType().ToString());
                }
            }
        }
    }
}