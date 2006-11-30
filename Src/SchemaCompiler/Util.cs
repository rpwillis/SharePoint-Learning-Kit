/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

/*
 * Utilities
 * Internal error numbers: 2000-2999
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// General-purpose utility functions
    /// </summary>
    internal static class Util
    {
	    /// <summary>
	    /// Returns true if <paramref name="str1"/> is the same as
	    ///		<paramref name="str2"/>, ignoring case.
	    /// </summary>
	    public static bool EqualsCI(string str1, string str2)
	    {
		    return String.Compare(str1, str2, true, CultureInfo.CurrentCulture) == 0;
	    }
    }

    /// <summary>
    /// Exception thrown when the command-line parameters are incorrect
    /// </summary>
    internal class CommandLineParameterException: Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public CommandLineParameterException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when some part of validation fails
    /// </summary>
    internal class ValidationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public ValidationException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when the program discovers an error
    /// </summary>
    internal class InternalException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uniqueId"></param>
        public InternalException(string uniqueId)
            : base(uniqueId)
        {
        }
    }
}
