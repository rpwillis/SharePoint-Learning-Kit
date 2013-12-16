/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Note: There are some issues in running the below Visual Studio Tests in x64 environments
// Functions that make a managed to unmanaged code transition fail in the CLR in x64 
// environments

using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Microsoft.LearningComponents
{
    namespace CompressionUnitTest
    {
        /// <summary>
        ///This is a test class for Microsoft.LearningComponents.Compression and is intended
        ///to contain all Microsoft.LearningComponents.Compression Unit Tests
        ///</summary>
        public class CompressionTest : IDisposable
        {


            string unittestdir = null;
            string destdir = null;

#region intitialization
            /// <summary>Initializes a new instance of <see cref="CompressionTest"/>.</summary>
            public CompressionTest()
            {
                unittestdir =  "..\\CompressionUnitTest\\UnitTestData";
                destdir = unittestdir + "\\UnbundleFolder";
            }

            /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
            public void Dispose()
            {
                if (Directory.Exists(destdir))
                {
                    Directory.Delete(destdir, true);
                }
            }
#endregion intitialization

#region tests
            /// <summary>
            ///A test for Unbundle (FileInfo, DirectoryInfo)
            ///</summary>
            [Fact]
            public void UnbundleTest()
            {
                FileInfo lrmFile = new FileInfo(unittestdir + "\\MyFirstLR.lrm");

                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\MyFirstLR");

                Microsoft.LearningComponents.Compression.Unbundle(lrmFile, destinationDirectory);

            }

            /// <summary>Test for unbundling a non-existant file.</summary>
            [Fact]
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
                    Assert.Equal("System.IO.FileNotFoundException", e.GetType().ToString());
                }
            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: Corrupt File", e.Message);
                }

            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: Corrupt File", e.Message);
                }

            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: Corrupt File", e.Message);
                }
            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: Corrupt File", e.Message);
                }

            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: Bad version info", e.Message);
                }


            }


            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: File has ended prematurely", e.Message);
                }

            }


            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("Bad LRM input: File has ended prematurely", e.Message);
                }
            }


            /// <summary>
            ///A test for Unzip (FileInfo, DirectoryInfo)
            ///</summary>
            [Fact]
            public void UnzipTest()
            {
                FileInfo zipFile = new FileInfo(unittestdir + "\\Solitaire.zip");
                DirectoryInfo destinationDirectory = new DirectoryInfo(destdir + "\\Solitaire");

                Microsoft.LearningComponents.Compression.Unzip(zipFile, destinationDirectory);

            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("System.IO.FileNotFoundException", e.GetType().ToString());
                }
            }

            /// <summary></summary>
            [Fact]
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
                    Assert.Equal("System.IO.FileFormatException", e.GetType().ToString());
                }
            }
#endregion tests
        }
    }
}
