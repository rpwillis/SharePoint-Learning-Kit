using System;
using System.Collections.Generic;
using System.Text;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Support
{
    public class clsDirectory
    {
        public static void Delete(string DirectoryPath)
        {
            if (System.IO.Directory.Exists(DirectoryPath))
            {
                string[] files = System.IO.Directory.GetFiles(DirectoryPath, "*", System.IO.SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    System.IO.FileAttributes attributes = System.IO.File.GetAttributes(file);
                    if ((attributes & System.IO.FileAttributes.ReadOnly) != 0)
                    {
                        System.IO.File.SetAttributes(file, ~System.IO.FileAttributes.ReadOnly);
                    }
                }
                System.IO.Directory.Delete(DirectoryPath, true);
            }
            
        }
    }
}
