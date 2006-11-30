/* Copyright (c) Microsoft Corporation. All rights reserved. */

﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.Reflection;

#endregion

namespace RunCS
{
    /// <summary>
    /// Main class for the program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// This is the entry point for the program.  It performs the following
        /// steps:
        ///   1) Reads all the arguments
        ///   2) Compiles the source code provided by the user, creating a DLL and PDB
        ///      in %TEMP%.
        ///   3) Creates a new application domain and uses the Executer class to
        ///      execute the user's code in that application domain.  This is executed
        ///      in a different app domain the DLL and PDB files can be deleted after
        ///      the app domain is unloaded.
        ///   4) Unload the app domain and delete the DLL and PDB files.  Output any
        ///      results to the user.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            // Initialize the arguments
            List<string> references = new List<string>();
            List<string> referencesByPath = new List<string>();
            List<string> sourceFiles = new List<string>();
            string mainSourceFile = null;
            string[] userArgs = null;
        
            // Start at the first argument
            int argIndex = 0;
            
            // Handle the parameters beginning with "/"
            while(args.Length > argIndex)
            {
                string currentArg = args[argIndex];
                if(currentArg.StartsWith("/reference:", StringComparison.CurrentCultureIgnoreCase))
                    references.Add(currentArg.Remove(0,11));
                else if(currentArg.StartsWith("/referencebypath:", StringComparison.CurrentCultureIgnoreCase))
                    referencesByPath.Add(currentArg.Remove(0,17));
                else if(currentArg.StartsWith("/include:", StringComparison.CurrentCultureIgnoreCase))
                    sourceFiles.Add(currentArg.Remove(0,9));
                else
                    break;
                argIndex++;
            }

            // Handle the filename argument
            if(args.Length > argIndex)
            {
                sourceFiles.Add(args[argIndex]);
                mainSourceFile = args[argIndex];
                argIndex++;
            }

            // Save the rest of the arguments so they can easily be passed
            // to the user's code
            userArgs = new string[args.Length-argIndex];
            for(int i=argIndex; i<args.Length; i++)
                userArgs[i-argIndex] = args[i];
                
            // Return an error if the parameters are incorrect
            if(mainSourceFile == null)
            {
                Console.WriteLine("RunCS: Runs a .CS file from the command line");
                Console.WriteLine();
                Console.WriteLine("Usage: RunCS [/reference:<assembly>] [/referencebypath:<pathtoassembly>] [/include:<additionalsourcefile>] <sourcefile> <arg1> <arg2> ... <argn>");
                Console.WriteLine();
                Console.WriteLine("<assembly> contains a name of an assembly that is referenced by the code.");
                Console.WriteLine("Example: System.Xml.dll.  Use this when the assembly is in the GAC.");
                Console.WriteLine();
                Console.WriteLine("<pathtoassembly> contains a path to an assembly that is referenced by the");
                Console.WriteLine("code.  Example: ..\\Foo.dll.  Use this when the assembly is not in the GAC.");
                Console.WriteLine(); 
                Console.WriteLine("<sourcefile> contains a name of a .cs file to compile.");
                Console.WriteLine();
                Console.WriteLine("<additionalsourcefile> contains a name of an additional .cs file to compile");
                Console.WriteLine();
                Console.WriteLine("Either <sourcefile> or one of the <additionalsourcefile> files must contain a");
                Console.WriteLine("class with a public static method named 'Main' that accepts an array of");
                Console.WriteLine("string arguments e.g.,:");
                Console.WriteLine();
                Console.WriteLine("    namespace MyApp");
                Console.WriteLine("    {");
                Console.WriteLine("        class MyClass");
                Console.WriteLine("        {");
                Console.WriteLine("            public static int Main(string[] args)");
                Console.WriteLine("            {");
                Console.WriteLine("                ...");
                Console.WriteLine("            }");
                Console.WriteLine("        }");
                Console.WriteLine("    }");
                return -1;
            }

            // Create a name for the output assembly based on a Guid.  The output
            // assembly is created in the Temp directory, and we'll try to delete
            // it (and the corresponding .PDB file) before exiting.
            string assemblyPath = Path.Combine(Path.GetTempPath(),
                "RunCS-" + Guid.NewGuid().ToString() + ".dll");

            // App domain that we'll be creating
            AppDomain executionAppDomain = null;

            // Now start the real work.
            // Put everything within a try/catch so we can delete the DLL,
            // delete the app domain, etc.
            try
            {            
                // Create the compiler
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();

                // Set up the compiler parameters
                System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateExecutable = true;                
                parameters.GenerateInMemory = false;
                parameters.IncludeDebugInformation = true;
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                parameters.OutputAssembly = assemblyPath;
                foreach(string s in references)
                    parameters.ReferencedAssemblies.Add(s);
                foreach(string s in referencesByPath)
                    parameters.CompilerOptions += "/r:" + Path.GetFullPath(s) + " ";
                
                // Compile
                CompilerResults results;
                try
                {
                    results = codeProvider.CompileAssemblyFromFile(parameters,
                        sourceFiles.ToArray());
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("Error: File '" + e.FileName + "' not found");
                    return -1;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Compile error: " + e.ToString());
                    return -1;
                }

                // If there was an error, display it and exit                                                                
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        Console.WriteLine(
                            "Error " + error.ErrorNumber + " at line " + error.Line +
                            " of " + error.FileName + ": " + error.ErrorText);
                    }
                    return -1;
                }

                // Set the current directory so it matches the input file
                string newDirectory = Path.GetDirectoryName(mainSourceFile);
                if (newDirectory != "")
                    Directory.SetCurrentDirectory(newDirectory);

                // Create the app domain
                executionAppDomain = AppDomain.CreateDomain("Execution");

                // Create a "Executer" class in the new app domain.
                Executer executer = (Executer)executionAppDomain.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().Location, "RunCS.Executer");
                
                // Call the Executer class so it can run the code
                string resultString;
                int result = executer.Execute(assemblyPath, referencesByPath.ToArray(), userArgs, out resultString);
                
                // Print any results returned from the Executer class
                if(resultString != "")
                    Console.WriteLine(resultString);
                    
                return result;
            }
            finally
            {
                // Unload the temporary app domain
                if(executionAppDomain != null)
                    AppDomain.Unload(executionAppDomain);
                    
                // Need to do garbage collection to clean up whatever keeps the
                // PDB file open
                System.GC.Collect();
                    
                // Delete the output files if they exist
                if(File.Exists(assemblyPath))
                    File.Delete(assemblyPath);
                string pdbPath = assemblyPath.Replace(".dll",".pdb");
                if(File.Exists(pdbPath))
                    File.Delete(pdbPath);
            }
        }
    }

    /// <summary>
    /// Executer class
    /// An instance of this class is created in a new app domain.
    /// </summary>
    public class Executer : MarshalByRefObject
    {
        /// <summary>
        /// Run the code in the passed-in assembly.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly</param>
        /// <param name="assemblyReferences">Other assembly references that won't be
        ///     found in the GAC</param>
        /// <param name="args">Arguments to pass to the method in the assembly</param>
        /// <param name="result">Result string to be displayed to the user</param>
        /// <returns>0 if succeeded, -1 if failed</returns>
        public int Execute(string assemblyPath, string[] assemblyReferences,
            string[] args, out string result)
        {
            // Initialize output
            result = "";

            // Load the assemblies that won't be found in the GAC
            foreach(string assemblyReference in assemblyReferences)
            {
                Assembly.LoadFrom(assemblyReference);
            }
                
            // Load the main assembly
            Assembly assembly = Assembly.LoadFrom(assemblyPath);

            // Get the entry point
            MethodInfo entryPoint = assembly.EntryPoint;
            if (entryPoint == null)
            {
                result = "Error: Entry point not found";
                return -1;
            }

            // Call the method
            try
            {
                entryPoint.Invoke(null, new Object[1] { args });
            }
            catch (Exception e)
            {
                result = "Unhandled Exception: " + e.InnerException.ToString();
                return -1;
            }

            // Return
            return 0;
        }
    }

}
