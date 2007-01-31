/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Loc
{
    class Program
    {

        private static int _task;
        private static string _source;
        private static string _target;

        static void Main(string[] args)
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
                if (_task == (int)Tasks.Extract)
                {
                    Extractor resourceExtractor = new Extractor();
                    resourceExtractor.Extract(_source);
                    resourceExtractor.Save(_target);

                    Success();
                }
                else
                {
                    Generator resourceGenerator = new Generator();
                    resourceGenerator.LoadXML(_source);
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

        private static bool ParseArgs(string[] args)
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

                        _task = (int)Tasks.Extract;
                        break;
                    case "g":
                        _task = (int)Tasks.Generate;
                        break;
                    default:
                        return false;

                }
                if (_task == (int)Tasks.Extract)
                {
                    if (args.Length != 3)
                    {
                        return false;
                    }
                    else
                    {
                        _source = args[1];
                        _target = args[2];
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
                        _source = args[1];
                    }
                }
            }
            return true;
        }

        private enum Tasks { Extract = 0, Generate }

    }
}
