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
        }

        public static void Error(string Message, string SystemException)
        {
            Error(Message + "\n\n" + SystemException);
        }
    }
}
