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
        Generate
    }

    class Program
    {

        private Task task;
        private string source;
        private string target;

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
                Console.WriteLine(entryAssembly.GetName().Name+@" <Extract|Generate> <source file> [<target file>]");
                Console.WriteLine();
            }
            else
            {
                if (task == Task.Extract)
                {
                    Extractor resourceExtractor = new Extractor();
                    resourceExtractor.Extract(source);
                    resourceExtractor.Save(target);

                    Success();
                }
                else
                {
                    Generator resourceGenerator = new Generator();
                    resourceGenerator.LoadXML(source);
                    resourceGenerator.Save();

                    Success();
                }
            }
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
                string Command = args[0].Substring(0, 1).ToLower();
                switch (Command)
                {
                    case "e":
                        task = Task.Extract;
                        break;
                    case "g":
                        task = Task.Generate;
                        break;
                    default:
                        return false;

                }

                if (task == Task.Extract)
                {
                    if (args.Length != 3)
                    {
                        return false;
                    }
                    else
                    {
                        source = args[1];
                        target = args[2];
                    }
                }
                else
                {
                    if (args.Length != 2)
                    {
                        return false;
                    }
                    else
                    {
                        source = args[1];
                    }
                }
            }
            return true;
        }


    }
}
