/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharePointLearningKit.Localization
{
    enum Task
    {
        Extract = 0,
        Generate,
        Replace
    }

    class Program
    {

        private Task task;
        private string source;
        private string target;
        private string culture;

        static int Main(string[] args)
        {

            try
            {
                Program program = new Program();
                return program.Run(args);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Failed:");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return 1;
        }

        public int Run(string[] args)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            Console.WriteLine(String.Format("{0} v{1}", entryAssembly.GetName().Name, entryAssembly.GetName().Version.ToString()));

            if (!ParseArgs(args))
            {
                Console.WriteLine();
                Console.WriteLine(entryAssembly.GetName().Name+@" <Extract|Generate|Replace> <source file> [<target file>]");
                Console.WriteLine();
                return 1;
            }
            else
            {
                switch (task)
                {
                    case Task.Extract:
                        Extractor resourceExtractor = new Extractor();
                        Console.WriteLine("Processing for output to {0}", target);
                        resourceExtractor.Extract(source);
                        try
                        {
                            resourceExtractor.Save(target);
                            return Success();
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine(e.Message);
                            return Fail();
                        }

                    case Task.Generate:
                        Generator resourceGenerator = new Generator();
                        resourceGenerator.LoadXML(source);
                        resourceGenerator.Save();
                        return Success();

                    case Task.Replace:
                        Replacer replace = new Replacer(culture);
                        replace.Load(source);
                        replace.Save(target);
                        return 0;

                    default:
                        throw new InvalidOperationException("Invalid task type.");
                }
            }
        }

        private static int Fail()
        {
            Console.WriteLine();
            Console.WriteLine("FAILED.");
            return 1;
        }

        private static int Success()
        {
            Console.WriteLine();
            Console.WriteLine("Successful.");
            return 0;
        }

        private bool ParseArgs(string[] args)
        {

            if (args.Length < 2)
            {
                return false;
            }
            else
            {
                string command = args[0];
                if (command[0] == '/')
                {
                    command = command.Substring(1,1);
                }
                else
                {
                    command = command.Substring(0,1);
                }
                command = command.ToLowerInvariant();

                switch (command)
                {
                    case "e":
                        task = Task.Extract;
                        if (args.Length != 3)
                        {
                            return false;
                        }
                        else
                        {
                            target = args[2];
                        }
                        break;

                    case "g":
                        task = Task.Generate;
                        if (args.Length != 2)
                        {
                            return false;
                        }
                        break;

                    case "r":
                        task = Task.Replace;
                        if (args.Length != 4)
                        {
                            return false;
                        }
                        else
                        {
                            target = args[2];
                            culture = args[3];
                        }
                        break;

                    default:
                        return false;

                }

                source = args[1];
                return true;
            }
        }


    }
}
