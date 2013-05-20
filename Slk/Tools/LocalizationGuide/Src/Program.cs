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

        static void Main(string[] args)
        {

            try
            {
                Program program = new Program();
                program.Run(args);
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

        }

        public void Run(string[] args)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            Console.WriteLine(String.Format("{0} v{1}", entryAssembly.GetName().Name, entryAssembly.GetName().Version.ToString()));

            if (!ParseArgs(args))
            {
                Console.WriteLine();
                Console.WriteLine(entryAssembly.GetName().Name+@" <Extract|Generate|Replace> <source file> [<target file>]");
                Console.WriteLine();
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
                            Success();
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine(e.Message);
                            Fail();
                        }
                        break;

                    case Task.Generate:
                        Generator resourceGenerator = new Generator();
                        resourceGenerator.LoadXML(source);
                        resourceGenerator.Save();
                        Success();
                        break;

                    case Task.Replace:
                        Replacer replace = new Replacer(culture);
                        replace.Load(source);
                        replace.Save(target);
                        break;

                    default:
                        throw new InvalidOperationException("Invalid task type.");
                }
            }
        }

        private static void Fail()
        {
            Console.WriteLine();
            Console.WriteLine("FAILED.");
        }

        private static void Success()
        {
            Console.WriteLine();
            Console.WriteLine("Successful.");
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
