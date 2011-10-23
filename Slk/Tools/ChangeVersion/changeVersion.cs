using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Changes the version of aspx files from 2007 to 2010.</summary>
    public class ChangeVersion
    {
        string destinationFolder;

#region constructor
        /// <summary>Initializes a new instance of <see cref="ChangeVersion"/>.</summary>
        /// <param name="destinationFolder">The destination folder for the changed files.</param>
        public ChangeVersion(string destinationFolder)
        {
            this.destinationFolder = destinationFolder;
        }
#endregion constructor

#region properties
        /// <summary>Indicates if an error has occurred.</summary>
        public bool HasError { get; private set; }
#endregion properties

#region methods
        /// <summary>Changes the files and saves to the destination directory.</summary>
        /// <param name="files">The files to change.</param>
        public void ChangeFiles(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                string contents;

                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        contents = reader.ReadToEnd();
                    }

                    contents = contents.Replace("12.0.0.0", "14.0.0.0");

                    string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                    using (StreamWriter writer = new StreamWriter(destinationFile))
                    {
                        writer.Write(contents);
                    }

                    Console.WriteLine("Changed version of {0} to {1}", file, destinationFile);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine("Failed to process {0}.", file);
                    HasError = true;
                }
            }
        }
#endregion methods

#region Main method
        /// <summary>Runs the program.</summary>
        /// <param name="arguments">The list of files to modify.</param>
        static int Main(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.Error.WriteLine("Destination folder not passed.");
                return 1;
            }
            else if (arguments.Length == 1)
            {
                Console.Error.WriteLine("No files passed.");
                return 1;
            }
            else
            {
                try
                {
                    string destinationFolder = arguments[0];
                    string[] files = new string[arguments.Length - 1];
                    Array.Copy(arguments, 1, files, 0, files.Length);
                    ChangeVersion changer = new ChangeVersion(destinationFolder);
                    changer.ChangeFiles(files);
                    if (changer.HasError)
                    {
                        return 1;
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    return 1;
                }
            }

            return 0;
        }
#endregion Main method
    }
}
