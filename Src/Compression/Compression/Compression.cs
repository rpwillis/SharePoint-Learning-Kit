/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Globalization;
[assembly: CLSCompliantAttribute(true)]

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Compression class has two static methods Unzip and Unbundle to decompress zip
    /// and lrm file formats respectively
    /// </summary>
    sealed public class Compression
    {
        // Removing constructors from the Class
        private Compression() { }

        /// <summary>
        /// Unzips the .zip file <zipFile>; the result is stored
        /// in a directory at location <destinationDirectory>.  The destination
        /// directory is created if it doesn't exist; files within that directory
        /// are overwriten if their names conflict with files in the .zip file.
        /// Throws IO exceptions for errors related to the input file; 
        /// CompressionException for errors pertaining to creation of output files;
        /// ReflectionException for errors dealing with the inability of the runtime 
        /// to call the internal classes
        /// </summary>
        /// <param name="zipFile">The .zip file to unzip</param>
        /// <param name="destinationDirectory">The directory that the contents are uncompressed to</param>
        public static void Unzip(FileInfo zipFile, DirectoryInfo destinationDirectory)
        {           

            Type typeZipArchive = null;
            object zipArchive = null;

            try
            {
                typeZipArchive = Type.GetType("MS.Internal.IO.Zip.ZipArchive, WindowsBase, Version=3.*.*.*, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                zipArchive = typeZipArchive.InvokeMember("OpenOnFile",
                                                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod,
                                                null,
                                                null,
                                                new object[] {
                                                zipFile.FullName, FileMode.Open, FileAccess.Read, FileShare.None, false},
                                                System.Globalization.CultureInfo.CurrentCulture
                                            );
                IEnumerable zipFileInfoCollection = (IEnumerable)typeZipArchive.InvokeMember("GetFiles",
                                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                                null,
                                                zipArchive,
                                                null,
                                                System.Globalization.CultureInfo.CurrentCulture
                                            );

                Type typeZipFileInfo = Type.GetType("MS.Internal.IO.Zip.ZipFileInfo, WindowsBase, Version=3.*.*.*, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

                foreach (object zipFileInfo in zipFileInfoCollection)
                {
                    string zipEntryName = (string)typeZipFileInfo.InvokeMember("get_Name",
                                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                                null,
                                                zipFileInfo,
                                                null,
                                                System.Globalization.CultureInfo.CurrentCulture
                                             );

                    FileInfo zipEntryFile = new FileInfo(Path.Combine(destinationDirectory.FullName, zipEntryName));

                    using (Stream zipEntryReadStream = (Stream)typeZipFileInfo.InvokeMember("GetStream",
                                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                                    null,
                                                    zipFileInfo,
                                                    new object[] {
                                                    FileMode.Open,FileAccess.Read },
                                                    System.Globalization.CultureInfo.CurrentCulture
                                                 ))
                   {

                        bool isFolder = (bool)typeZipFileInfo.InvokeMember("get_FolderFlag",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null,
                                zipFileInfo,
                                null,
                                System.Globalization.CultureInfo.CurrentCulture
                             );

                        // Due to what looks like a bug in the ZipArchive class, get_FolderFlag returns 'false' for 
                        // directories which are more than one level deep
                        // Hence adding two conditions to check for directory entries
                        if (isFolder || String.IsNullOrEmpty(zipEntryFile.Name))
                        {

                            Directory.CreateDirectory(zipEntryFile.FullName);
                            continue;
                        }

                        //Create directory, if needed
                        if (!zipEntryFile.Directory.Exists)
                            zipEntryFile.Directory.Create();

                        try
                        {
                            //Copy contents from the zipArchive's Stream into the corresponding file
                            using (FileStream zipWriteStream = new FileStream(zipEntryFile.FullName, FileMode.Create))
                            {
                                int bytesPerRead = 4000;
                                byte[] buffer = new byte[bytesPerRead];
                                int bytesRead = zipEntryReadStream.Read(buffer, 0, buffer.Length);
                                while (bytesRead > 0)
                                {
                                    zipWriteStream.Write(buffer, 0, bytesRead);
                                    bytesRead = zipEntryReadStream.Read(buffer, 0, buffer.Length);
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            // Wrap IO Exceptions with more decompression related information
                            throw new CompressionException(Properties.CompressionResources.ZipOutputCreationError, e);
                        }
                    }
                }

                typeZipArchive.InvokeMember("Close",
                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                    null,
                                    zipArchive,
                                    null,
                                    System.Globalization.CultureInfo.CurrentCulture
                                );
            }
            catch (System.Reflection.TargetInvocationException e) 
            {
                try
                {
                    if (typeZipArchive != null && zipArchive != null)
                    {
                        typeZipArchive.InvokeMember("Close",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                            null,
                            zipArchive,
                            null,
                            System.Globalization.CultureInfo.CurrentCulture
                        );
                    }
                }
                catch { } // Swallow any failures here

                throw e.InnerException;
            }

        }
         
        /// <summary>
        /// Uncompresses the .lrm file <lrmFile>; the result is stored
        /// in a directory at location <destinationDirectory>.  The destination
        /// directory is created if it doesn't exist; files within that directory
        /// are overwriten if their names conflict with files in the .lrm file.
        /// </summary>
        /// <param name="lrmFile">The .lrm file to uncompress</param>
        /// <param name="destinationDirectory">The directory that the contents are uncompressed to</param>
  
        public static void Unbundle(FileInfo lrmFile, DirectoryInfo destinationDirectory)
        {
            // The LRM stream is read in as Strings assuming a UTF-8 encoding
            // (Is UTF-8 a reasonable assumption to make?)
            StreamReader lrmFileCharReader = new StreamReader(lrmFile.FullName);

            // 'pos' keeps track of the current byte position in the file
            // This is to keep track of the bytes read and seek to the start of the message parts
            // The StreamReader uses buffering internally and hence the BaseStream's Position
            // cannot be used for this information
            long pos = 0;
                
            #region ParseMimeHeaders            

            // Parse MIME header
            // Parse Content-Type, MIME-Version, X-LRM-Version, X-Multi-LR strings from 
            // the header
            
            LrmMimeHeader header = new LrmMimeHeader();
            string line;
            while ((line = lrmFileCharReader.ReadLine()) != null)
            {
                pos += line.ToCharArray().Length + 2; //+2 for the /r/n
                //If the read line is a Boundary, the header parsing is done
                if (!String.IsNullOrEmpty(header.BoundaryString) && line.Equals("--" + header.BoundaryString))
                {
                    //Error if all required headers are not present

                    if (String.IsNullOrEmpty(header.ContentType) || String.IsNullOrEmpty(header.MimeVersion)
                        || String.IsNullOrEmpty(header.LrmVersion) || String.IsNullOrEmpty(header.BoundaryString))
                        throw new CompressionException(Properties.CompressionResources.LRMRequiredHeadersMissing);

                    break;
                }
                else // Parse the read-in line
                    header.ParseMimeHeader(line);

            } 

            #endregion

           
            // Parsing of the message parts
            bool prematureFileEnd = true;
            while ((line = lrmFileCharReader.ReadLine()) != null && !String.IsNullOrEmpty(line))
            {
                //Parsing message part header
                pos += line.ToCharArray().Length + 2;
                line = line.Trim();
                string relativeActivityPath = null;
                bool isEncoded = false;
                int messageLength = 0;
                if (line.StartsWith("Content-Location"))
                {
                    relativeActivityPath = line.Substring(17).Trim();
                    if ((line = lrmFileCharReader.ReadLine()) != null)
                    {
                        pos += line.ToCharArray().Length + 2;
                        if (line.StartsWith("X-Enc"))
                        {
                            string encodingInfo = line.Substring(6).Trim();
                            string[] encodingInfoParts = encodingInfo.Split('/');
                            if (encodingInfoParts[0].Equals("1"))
                                isEncoded = true;
                            //Assumption: The unencoded length should not be empty. Code should change if this is a wrong assumption
                            if (encodingInfoParts.Length < 2 || String.IsNullOrEmpty(encodingInfoParts[1]))
                                throw new CompressionException(Properties.CompressionResources.LRMImproperXencValue);
                            messageLength = Int32.Parse(encodingInfoParts[1], System.Globalization.NumberFormatInfo.CurrentInfo);

                            // Read the following empty line
                            if ((line = lrmFileCharReader.ReadLine()) != null)
                            {
                                pos += line.ToCharArray().Length + 2;
                                // If the line is non-empty error
                                if (line.Trim().Length > 0)
                                    throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);
                            }
                            else
                                break;
                        }
                        else if (line.Trim().Length > 0)
                            throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);
                    }
                    else
                        break;
                }
                else if (line.StartsWith("X-Enc"))
                {
                    string encodingInfo = line.Substring(6).Trim();
                    string[] encodingInfoParts = encodingInfo.Split('/');
                    if (encodingInfoParts[0].Equals("1"))
                        isEncoded = true;
                    else
                        throw new CompressionException(Properties.CompressionResources.LRMUnrecognizedEncoding);
                    //Assumption: The unencoded length should not be empty. Code should change if this is wrong
                    if (encodingInfoParts.Length < 2 || String.IsNullOrEmpty(encodingInfoParts[1]) == true)
                        throw new CompressionException(Properties.CompressionResources.LRMImproperXencValue);
                    messageLength = Int32.Parse(encodingInfoParts[1], System.Globalization.NumberFormatInfo.CurrentInfo);

                    if ((line = lrmFileCharReader.ReadLine()) != null)
                    {
                        pos += line.ToCharArray().Length + 2;
                        if (line.StartsWith("Content-Location"))
                        {
                            relativeActivityPath = line.Substring(17).Trim();

                            // Read the following empty line
                            line = lrmFileCharReader.ReadLine();
                            pos += line.ToCharArray().Length + 2;
                            // If the line is non-empty error
                            if (line.Trim().Length > 0)
                                throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);

                        }
                        else
                            throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);
                    }
                    else
                        break;
                }
                else
                    throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);

                //Message Part header parsing done
      
                //Create the filestream corresponding to the current Activity
                String fullActivityPath = Path.Combine(destinationDirectory.FullName, relativeActivityPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullActivityPath));
                FileStream activityFile = new FileStream(fullActivityPath, FileMode.Create, FileAccess.Write);
                

                #region ParseMimeMessagePartBody
                // Parsing Message Part Body
                // Assumption: We can hold the whole message part in memory

                //Read every 'CBytesPerRead' characters into buffer and search for the boundary string
                int CBytesPerRead = 4000;
                bool isBoundaryStringFound = false;

                // The Message Part body is raw binary and hence we use the underlying BaseStream
                lrmFileCharReader.DiscardBufferedData();
                Stream lrmReader = lrmFileCharReader.BaseStream;

                // Seek to the start of the message part body
                lrmReader.Seek(pos, SeekOrigin.Begin);

                long startpos = lrmReader.Position;
                long endpos = 0;
                while (isBoundaryStringFound == false)
                {
                    byte[] buffer = new byte[CBytesPerRead];
                    int bytesread = lrmReader.Read(buffer, 0, buffer.Length);
                    if (bytesread == header.BoundaryString.Length)
                        throw new CompressionException(Properties.CompressionResources.LRMPrematureFileEnd);

                    char[] cbuffer = new char[CBytesPerRead];
                    buffer.CopyTo(cbuffer, 0);

                    int curMesgBoundary = -1;
                    string cbufferStr = new string(cbuffer);

                    if ((curMesgBoundary = cbufferStr.IndexOf("--" + header.BoundaryString)) != -1)
                    {
                        isBoundaryStringFound = true;
                        endpos = lrmReader.Position - (bytesread - curMesgBoundary) - 2;
                    }
                    else
                    {
                        // Seek behind boundaryString characters to make sure it is not missed due to fragmentation
                        lrmReader.Seek(-header.BoundaryString.Length, SeekOrigin.Current);
                    }
                }

                // bytes between 'startpos' and 'endpos' hold the message part body
                lrmReader.Seek(startpos, SeekOrigin.Begin);
                byte[] ActivityData = new byte[endpos - startpos];
                lrmFileCharReader.BaseStream.Read(ActivityData, 0, ActivityData.Length);

                //Read the following boundary string
                lrmFileCharReader.BaseStream.Seek(header.BoundaryString.Length + 6, SeekOrigin.Current);
                        // + 6 includes - 2 CRLFs and 2 '-'
                pos = lrmFileCharReader.BaseStream.Position;
                prematureFileEnd = false;


                if (isEncoded) //the stream is encoded
                {
                    
                    // Call the internal MRCI decompression module to decode the stream
                    byte[] decodedOutput = new byte[messageLength];
                    
                    unsafe
                    {
                        fixed (byte* srcPtr = ActivityData) fixed (byte* desPtr = decodedOutput)
                        {
                            long retval = MRCI.MRCIDecompress((IntPtr)srcPtr, ActivityData.Length, (IntPtr)desPtr, messageLength); 
                            if (retval != 0)
                            {
                                if (retval == 0x8007000E)
                                    throw new CompressionException(Properties.CompressionResources.LRMExplodeOutOfMemory);

                            }
                        }
                    }
                    try
                    {
                        activityFile.Write(decodedOutput, 0, messageLength);
                        activityFile.Close();
                    }
                    catch (Exception e)
                    {
                        throw new CompressionException(Properties.CompressionResources.LRMExplodeCannotCreateOutput, e);
                    }
                }
                else // the stream is not encoded
                {
                    //Copy contents 
                    try
                    {
                        activityFile.Write(ActivityData, 0, (int)(endpos - startpos));
                        activityFile.Close();
                    }
                    catch (Exception e)
                    {
                        throw new CompressionException(Properties.CompressionResources.LRMExplodeCannotCreateOutput, e);
                    }
                } 
                #endregion
               
            }
            if (prematureFileEnd == true)
                throw new CompressionException(Properties.CompressionResources.LRMPrematureFileEnd);
            lrmFileCharReader.Close();
        }

    }

}
