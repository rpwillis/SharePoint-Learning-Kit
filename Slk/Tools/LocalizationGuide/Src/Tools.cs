/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace SharePointLearningKit.Localization
{
    class Tools
    {

        public static void Error(string Message)
        {
            Console.WriteLine("Failed to extract resources");
            Console.WriteLine(Message);
            throw new Exception(Message);
        }

        public static void Error(string Message, string SystemException)
        {
            string message = Message + "\n\n" + SystemException;
            Error(message);
            throw new Exception(message);
        }
    }
}
