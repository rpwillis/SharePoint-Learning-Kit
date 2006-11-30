/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

/*
 * Main program file
 * Internal error numbers: 1000-1999
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Main class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main function
        /// </summary>
        /// <param name="args">Arguments from command line</param>
        internal static int Main(string[] args)
        {
	        // execute the application
	        int returnValue = 0;
	        try
	        {
		        Program.Run(args);
	        }
	        catch (Exception)
	        {
		        returnValue = 1;
	        }
	        finally
	        {
	        }
	        return returnValue;
        }

        /// <summary>
        /// Run the application
        /// </summary>
        /// <param name="args">Arguments from command line</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        internal static void Run(string[] args)
	    {
	        // Command-line: Input path, or null if not provided
	        string inputPath = null;

	        // Command-line: /OutputInit path, or null if not provided
	        string outputInitPath = null;
	        
	        // Command-line: /OutputUpgrade path, or null if not provided
	        string outputUpgradePath = null;
	        
	        // Command-line: /OutputHelper path, or null if not provided
	        string outputHelperPath = null;
	        
            // Command-line: /Namespace value, or null if not provided
            string outputNamespace = null;

            // Command-line: /SchemaNamespace value, or null if not provided
            string outputSchemaNamespace = null;

            // Command-line: /BaseSchema path, or null if not provided
            string baseSchemaPath = null;

            // Command-line: /OutputComponentsHelper value, or null if not provided
            string outputComponentsHelperPath = null;
            
            // Command-line: /OutputStorageHelper value, or null if not provided
            string outputStorageHelperPath = null;

            // Command-line: /OutputBaseInit path, or null if not provided
            string outputBaseInitPath = null;
                                   	        
	        try
	        {            
	            // parse the command line
                for (int iArg=0; iArg < args.Length; iArg++)
		        {
			        string arg = args[iArg];			    
			        if (Util.EqualsCI(arg, "/OutputInit"))
			        {
				        if (iArg + 1 >= args.Length)
				            throw new CommandLineParameterException(Resources.Usage);
				        if (outputInitPath != null)
				            throw new CommandLineParameterException(Resources.Usage);
				        outputInitPath = args[++iArg]; 
			        }
			        else if(Util.EqualsCI(arg, "/OutputUpgrade"))
			        {
                        if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputUpgradePath != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        outputUpgradePath = args[++iArg];
                    }
			        else if(Util.EqualsCI(arg, "/OutputHelper"))
			        {
			            if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputHelperPath != null)
                            throw new CommandLineParameterException(Resources.Usage);
			            outputHelperPath = args[++iArg];
			        }
			        else if(Util.EqualsCI(arg, "/Namespace"))
			        {
			            if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputNamespace != null)
                            throw new CommandLineParameterException(Resources.Usage);                            
			            outputNamespace = args[++iArg];
			        }
                    else if (Util.EqualsCI(arg, "/SchemaNamespace"))
                    {
                        if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputSchemaNamespace != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        outputSchemaNamespace = args[++iArg];
                    }
                    else if (Util.EqualsCI(arg, "/BaseSchema"))
                    {
                        if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (baseSchemaPath != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        baseSchemaPath = args[++iArg];
                    }
                    else if (Util.EqualsCI(arg, "/OutputComponentsHelper"))
                    {
                        if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputComponentsHelperPath != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        outputComponentsHelperPath = args[++iArg];
                    }
                    else if (Util.EqualsCI(arg, "/OutputStorageHelper"))
                    {
                        if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputStorageHelperPath != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        outputStorageHelperPath = args[++iArg];
                    }
                    else if (Util.EqualsCI(arg, "/OutputBaseInit"))
                    {
                        if (iArg + 1 >= args.Length)
                            throw new CommandLineParameterException(Resources.Usage);
                        if (outputBaseInitPath != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        outputBaseInitPath = args[++iArg];
                    }
                    else
                    {
                        if(inputPath != null)
                            throw new CommandLineParameterException(Resources.Usage);
                        inputPath = arg;
                    }
		        }

                // If we are asked to output information about a derived schema, then we'd better
                // have an input path for the derived schema
                if((outputInitPath != null) || (outputHelperPath != null) || (outputUpgradePath != null))
                {
                    if(inputPath == null)
                        throw new CommandLineParameterException(Resources.Usage);
                }

                // If we are asked to output a helper file, then we need namespaces
                if(outputHelperPath != null)
                {
                    if((outputNamespace == null) || (outputSchemaNamespace == null))
                        throw new CommandLineParameterException(Resources.Usage);
                }
                
                // If we aren't asked to output anything and not given an input file, then give the user usage information
                if((outputInitPath == null) && (outputUpgradePath == null) && (outputHelperPath == null) &&
                   (outputComponentsHelperPath == null) && (outputStorageHelperPath == null) &&
                   (outputBaseInitPath == null) && (inputPath == null))
                    throw new CommandLineParameterException(Resources.Usage);
                   
                // First output files only related to a base schema
                if((outputComponentsHelperPath != null) || (outputStorageHelperPath != null) ||
                   (outputBaseInitPath != null))
                {
                    // Load the schema
                    SchemaInfo schema = SchemaInfo.CreateFromBaseSchema(baseSchemaPath);

                    // Write the .SQL file if needed
                    if(outputBaseInitPath != null)
                        SqlInitFile.Write(schema, outputBaseInitPath);

                    // Write the helper files if needed
                    if(outputComponentsHelperPath != null)
                        HelperFile.WriteComponentsHelper(schema, outputComponentsHelperPath);
                    if(outputStorageHelperPath != null)
                        HelperFile.WriteStorageHelper(schema, outputStorageHelperPath);
                }

                // Now output derived schema files
                if((outputInitPath != null) || (outputHelperPath != null) || (outputUpgradePath != null))
                {
                    // Load the schema
                    SchemaInfo schema = SchemaInfo.CreateFromBaseSchemaAndFile(baseSchemaPath, inputPath);

                    // Write the .SQL file if needed
                    if (outputInitPath != null)
                        SqlInitFile.Write(schema, outputInitPath);

                    // Write the upgrade .SQL file if needed
                    if (outputUpgradePath != null)
                        SqlInitFile.WriteUpgrade(schema, outputUpgradePath);
                        
                    // Write the helper files if needed
                    if (outputHelperPath != null)
                        HelperFile.Write(schema, outputHelperPath, outputNamespace, outputSchemaNamespace);                
                }
            }
            catch(ValidationException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (CommandLineParameterException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
                    
    }
}
