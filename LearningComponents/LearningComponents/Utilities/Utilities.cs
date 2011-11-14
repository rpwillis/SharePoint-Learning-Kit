/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;

namespace Microsoft.LearningComponents
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    internal class LearningComponentsInternalException : Exception
    {
        private string m_uniqueId;

        public string UniqueId
        {
            get
            {
                return m_uniqueId;
            }
        }

        internal LearningComponentsInternalException(string uniqueId) : base()
        {
            m_uniqueId = uniqueId;
        } 

        internal LearningComponentsInternalException(string uniqueId, string message) 
            : base(message)
        {
            m_uniqueId = uniqueId;
        }

        internal LearningComponentsInternalException(string uniqueId, string message, Exception innerException)
            : base(message, innerException)
        {
            m_uniqueId = uniqueId;
        }
    }

    internal static class Utilities
    {
        /// <summary>
        /// Throws an exception if the condition is false.
        /// </summary>
        /// <param name="condition">A boolean representing a condition that should be true.</param>
		[Conditional("DEBUG")]
        internal static void Assert(bool condition)
        {
            if(!condition)
            {
                throw new LearningComponentsInternalException("Assert failed. Expected: true, Actual: false");
            }
        }

        /// <summary>
        /// Throws an exception if the condition is false.
        /// </summary>
        /// <param name="condition">A boolean representing a condition that should be true.</param>
        /// <param name="uniqueId">Unique string identifier to display for this internal error.</param>
		[Conditional("DEBUG")]
        internal static void Assert(bool condition, string uniqueId)
        {
            if(!condition)
            {
                throw new LearningComponentsInternalException(uniqueId);
            }
        }

        /// <summary>
        /// Converts a SCORM 2004 Timespan string into a <Typ>TimeSpan</Typ>.
        /// </summary>
        /// <param name="value">A length of time in hours, minutes and seconds shown in the following 
        /// format: P[yY][mM][dD][T[hH][mM][s[.s]S]] with a precision of 0.01 seconds.</param>
        /// <returns>The corresponding <Typ>TimeSpan</Typ>.  If <paramref name="value"/> is null or
        /// <c>String.Empty</c>, this returns <c>TimeSpan.MaxValue</c>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> is in an incorrect format.  No
        /// message is included in the exception.</exception>
        /// <remarks><para>
        /// XmlConvert.ToTimeSpan() will not work for converting SCORM 2004 values (which are also ISO 
        /// 8601 values) since XmlConvert uses an incorrect value for days per year and days per month.
        /// XmlConvert.ToString(TimeSpan) will produce a correct equivalent, however.</para>
        /// <para>
        /// 1 year ~ (365*4+1)/4*60*60*24*100 = 3155760000 centiseconds
        /// 1 month ~ (365*4+1)/48*60*60*24*100 = 262980000 centiseconds
        /// 1 day = 8640000 centiseconds
        /// 1 hour = 360000 centiseconds
        /// 1 minute = 6000 centiseconds
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static TimeSpan StringToTimeSpanScormV1p3(string value)
        {
            bool gotYears = false;
            bool gotMonths = false;
            bool gotDays = false;
            bool gotHours = false;
            bool gotMinutes = false;
            bool gotSeconds = false;
            bool gotCentiseconds = false;
            long num;
            int i;
            long totalCentiseconds = 0;

            if(String.IsNullOrEmpty(value) || !value.StartsWith("P", StringComparison.Ordinal))
            {
                throw new FormatException();
            }
            if(value.Length == 1)  // just have a "P" string, oddly valid
            {
                return TimeSpan.Zero;
            }
            value = value.Substring(1);
            while(Char.IsDigit(value[0]))
            {
                for(i = 0 ; Char.IsDigit(value[i]) ; ++i)
                    ;
                num = long.Parse(value.Substring(0, i), NumberStyles.None, CultureInfo.InvariantCulture);
                switch(value[i])
                {
                case 'Y':
                    if(gotYears)
                    {
                        throw new FormatException();
                    }
                    totalCentiseconds = num * 3155760000;
                    gotYears = true;
                    break;
                case 'M':
                    if(gotMonths)
                    {
                        throw new FormatException();
                    }
                    totalCentiseconds += (num * 262980000);
                    gotYears = true;
                    gotMonths = true;
                    break;
                case 'D':
                    if(gotDays)
                    {
                        throw new FormatException();
                    }
                    totalCentiseconds += (num * 8640000);
                    gotYears = true;
                    gotMonths = true;
                    gotDays = true;
                    break;
                default:
                    // unexpected character
                    throw new FormatException();
                }
                value = value.Substring(i + 1);
                if(value.Length == 0)
                {
                    return new TimeSpan(totalCentiseconds * 100000);
                }
            }
            if(!value.StartsWith("T", StringComparison.Ordinal))
            {
                throw new FormatException();
            }
            value = value.Substring(1);
            if(value.Length == 0) // ending with a "T" is valid also
            {
                return new TimeSpan(totalCentiseconds * 100000);
            }
            while(Char.IsDigit(value[0]))
            {
                for(i = 0 ; Char.IsDigit(value[i]) ; ++i)
                    ;
                num = long.Parse(value.Substring(0, i), NumberStyles.None, CultureInfo.InvariantCulture);
                switch(value[i])
                {
                case 'H':
                    if(gotHours)
                    {
                        throw new FormatException();
                    }
                    totalCentiseconds += (num * 360000);
                    gotHours = true;
                    break;
                case 'M':
                    if(gotMinutes)
                    {
                        throw new FormatException();
                    }
                    totalCentiseconds += (num * 6000);
                    gotHours = true;
                    gotMinutes = true;
                    break;
                case '.':
                    if(gotSeconds)
                    {
                        throw new FormatException();
                    }
                    totalCentiseconds += (num * 100);
                    gotHours = true;
                    gotMinutes = true;
                    gotSeconds = true;
                    break;
                case 'S':
                    if(gotCentiseconds)
                    {
                        throw new FormatException();
                    }
                    if(gotSeconds)
                    {
                        if(i > 2) // don't allow more than two digits after decimal point
                        {
                            throw new FormatException();
                        }
                        if(i == 1)
                        {
                            num *= 10;
                        }
                        totalCentiseconds += num;
                    }
                    else
                    {
                        totalCentiseconds += (num * 100);
                    }
                    gotHours = true;
                    gotMinutes = true;
                    gotSeconds = true;
                    gotCentiseconds = true;
                    break;
                default:
                    // unexpected character
                    throw new FormatException();
                }
                value = value.Substring(i + 1);
                if(value.Length == 0)
                {
                    return new TimeSpan(totalCentiseconds * 100000);
                }
            }
            // got extra chars at end of string
            throw new FormatException();
        }

        private static char[] s_colon = new char[] { ':' };
        private static char[] s_dot = new char[] { '.' };
        /// <summary>
        /// Converts a SCORM 1.2 Timespan string into a <Typ>TimeSpan</Typ>.
        /// </summary>
        /// <param name="value">A length of time in hours, minutes and seconds shown in the following 
        /// numerical format: HHHH:MM:SS.SS.  Hours has a minimum of 2 digits and a maximum of 4 digits.  
        /// Minutes shall consist of exactly 2 digits.  Seconds shall contain 2 digits, with an optional 
        /// decimal point and 1 or 2 additional digits.</param>
        /// <returns>The corresponding <Typ>TimeSpan</Typ>.  If <paramref name="value"/> is null or
        /// <c>String.Empty</c>, this returns <c>TimeSpan.MaxValue</c>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> is in an incorrect format.  No
        /// message is included in the exception.</exception>
        internal static TimeSpan StringToTimeSpanScormV1p2(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return TimeSpan.MaxValue;
            }
            // Since the SCORM 1.2 definition of Timespan is so specific, use simple tests and a string splitting
            // algorithm.
            string[] componentStrings = value.Split(s_colon);
            // There must be 3 component strings when split by ':'.
            // componentStrings[0] = hours, and must be either 2 or 4 characters in length.
            // componentStrings[1] = minutes, and must be 2 characters in length.
            if (componentStrings.Length != 3
                || (componentStrings[0].Length < 2 || componentStrings[0].Length > 4)
                || (componentStrings[1].Length != 2))
            {
                throw new FormatException();
            }
            string[] secondStrings = componentStrings[2].Split(s_dot);
            // There must be 1 or 2 second strings: either a single 2 digit string, or a 2 digit string and a 0-2 digit string.
            if (secondStrings.Length > 2
                || (secondStrings.Length == 2 && secondStrings[1].Length > 2)
                || secondStrings[0].Length != 2)
            {
                throw new FormatException();
            }
            // convert into int32.  The converter will throw FormatException if there are non-
            // digits in the strings.
            int hours = Int32.Parse(componentStrings[0], CultureInfo.InvariantCulture);
            int minutes = Int32.Parse(componentStrings[1], CultureInfo.InvariantCulture);
            int seconds = Int32.Parse(secondStrings[0], CultureInfo.InvariantCulture);
            int milliseconds;
            if (secondStrings.Length == 2 && secondStrings[1].Length > 0)
            {
                // if secondStrings[1] is 1 character long, it is 10ths of a second.
                // if secondStrings[1] is 2 characters long, it is 100ths of a second.
                milliseconds = Int32.Parse(secondStrings[1], CultureInfo.InvariantCulture) * 10;
                if (secondStrings[1].Length == 1) milliseconds *= 10;
            }
            else
            {
                milliseconds = 0;
            }
            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        /// Converts a string to an XPath literal.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For example, the string <c>"Abc'123'Def"</c> converts to <c>"\"Abc'123'Def\""</c>.
        /// </para>
        /// <para>
        /// XPath defines a literal string as a string between two double quotes that contains no double quotes
        /// or a string between two single quotes that contains no single quotes.
        /// </para>
        /// <para>
        /// This works well for most cases, but if the string in question contains both single and double quotes,
        /// we have some work to do.  We get around this by using the built-in concat() function to concatenate
        /// the string fragments without double-quotes along with double-quotes in single-quoted strings.
        /// </para>
        /// </remarks>
        /// <param name="value">The string to verify</param>
        /// <returns>A properly quoted string or equivalent expression.</returns>
        internal static string StringToXPathLiteral(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                return "''";
            }

            if(value.IndexOf('"') >= 0)
            {
                if(value.IndexOf('\'') >= 0)
                {
                    StringBuilder sb = new StringBuilder(value.Length * 2);

                    sb.Append("concat(");
                    string[] splitstr = value.Split(new char[] { '"' }, StringSplitOptions.None);
                    int i;
                    for(i = 0 ; i < splitstr.Length - 1 ; ++i)
                    {
                        if(i > 0)
                        {
                            sb.Append(',');
                        }
                        if(splitstr[i].Length == 0)
                        {
                            sb.Append("'\"'");
                        }
                        else
                        {
                            sb.Append('"');
                            sb.Append(splitstr[i]);
                            sb.Append("\",'\"'");
                        }
                    }
                    if(splitstr[i].Length != 0)
                    {
                        sb.Append(',');
                        sb.Append('"');
                        sb.Append(splitstr[i]);
                        sb.Append('"');
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
                return "'" + value + "'";
            }
            return "\"" + value + "\"";
        }

        /// <summary>
        /// Helper function to check if a parameter is null. If not, an ArgumentNullException
        /// is thrown with <paramref name="paramName"/> as the parameter name.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="parameter"></param>
        internal static void ValidateParameterNonNull(string paramName, object parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Helper function to verify a parameter is not an empty string. If the 
        /// <P>parameter</P> is null, empty or contains only blanks, an ArgumentException is thrown.
        /// </summary>
        /// <param name="paramName">The name of the parameter, provided in case of error.</param>
        /// <param name="parameter">The parameter value to validate.</param>
        internal static void ValidateParameterNotEmpty(string paramName, string parameter)
        {
            if (String.IsNullOrEmpty(parameter) || String.IsNullOrEmpty(parameter.Trim()))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.UTIL_ParamIsEmptyString, paramName), paramName);
        }

        /// <summary>
        /// Copy from one stream to another, using potentially different identities for reading and writing.
        /// </summary>
        /// <param name="fromStream">The stream to copy from.</param>
        /// <param name="readImpersonationBehavior">Determines whether to impersonate when reading from <paramref name="fromStream"/>. </param>
        /// <param name="toStream">The stream to write to.</param>
        /// <param name="writeImpersonationBehavior">Determines whether to impersonate when writing to <paramref name="toStream"/>. 
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]    // the writer is disposed for all code paths
        internal static void CopyStream(Stream fromStream, ImpersonationBehavior readImpersonationBehavior, Stream toStream, ImpersonationBehavior writeImpersonationBehavior)
        {
            using (Disposer disposer = new Disposer())
            {
                DetachableStream dsToStream = new DetachableStream(toStream);
                disposer.Push(dsToStream);

                BinaryWriter writer = new BinaryWriter(dsToStream);
                disposer.Push(writer);

                // An addition revert during write operations is required if the setting for reading and writing
                // is not the same
                bool requiresWriteRevert = (readImpersonationBehavior != writeImpersonationBehavior);

                using (ImpersonateIdentity readId = new ImpersonateIdentity(readImpersonationBehavior))
                {
                    byte[] bytesIn = new byte[65536];
                    int bytesRead;
                    while ((bytesRead = fromStream.Read(bytesIn, 0, bytesIn.Length)) != 0)
                    {
                        // If we have to impersonate to write, then do it. Otherwise skip it.
                        if (requiresWriteRevert)
                        {
                            using (ImpersonateIdentity id = new ImpersonateIdentity(writeImpersonationBehavior))
                            {
                                writer.Write(bytesIn, 0, bytesRead);
                            }
                        }
                        else
                        {
                            writer.Write(bytesIn, 0, bytesRead);
                        }
                    }
                }
                dsToStream.Detach();
            }
        }
    }

    // An internal class so that the StreamWriter that writes to this stream may be 
    // disposed, while returning the stream itself to the caller. 
    // NOTE: This is not a general-purpose class. It intentionally does not dispose 
    // its resources (Stream) when this object is disposed.
    internal class DetachableStream : Stream
    {
        private Stream m_stream;
        private bool m_isAttached;

        internal DetachableStream(int capacity)
        {
            m_stream = new MemoryStream(capacity);
            m_isAttached = true;
        }

        internal DetachableStream(Stream stream)
        {
            m_stream = stream;
            m_isAttached = true;
        }

        internal Stream Stream { get { return m_stream; } }

        // Remove the actual stream from the object in order to allow the StreamWriter writing to this 
        // stream to be disposed. Actual stream is flushed before being detached.
        internal void Detach()
        {
            Flush();
            m_isAttached = false;
        }

        public override bool CanRead
        {
            get
            {
                if (m_isAttached)
                    return m_stream.CanRead;
                return false;
            }
        }

        public override void Close()
        {
            if (m_isAttached)
                m_stream.Close();
        }

        public override bool CanSeek
        {
            get
            {
                if (m_isAttached)
                    return m_stream.CanSeek;
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (m_isAttached)
                    return m_stream.CanWrite;
                return false;
            }
        }

        public override void Flush()
        {
            if (m_isAttached)
                m_stream.Flush();
        }

        public override long Length
        {
            get
            {
                if (m_isAttached)
                    return m_stream.Length;
                return 0;
            }
        }

        public override long Position
        {
            get
            {
                if (m_isAttached)
                    return m_stream.Position;
                return 0;
            }
            set
            {
                if (m_isAttached)
                    m_stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_isAttached)
                return m_stream.Read(buffer, offset, count);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (m_isAttached)
                return m_stream.Seek(offset, origin);
            return 0;
        }

        public override void SetLength(long value)
        {
            if (m_isAttached)
                m_stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_isAttached)
                m_stream.Write(buffer, offset, count);
        }
    }

    /// <summary>
    /// The ImpersonateIdentity class manages the impersonation of the user
    /// to enable accessing resources.  Instances _must_ be disposed of -- best
    /// practice is to use within a using () {} construct.
    /// </summary>
    internal class ImpersonateIdentity : IDisposable
    {
        private WindowsImpersonationContext m_context;

        /// <summary>
        /// This method throws an exception of the user does not have the right
        /// to switch to the identity.  This must be Disposed of
        /// when we are finished with it. If the identity is null, this method has no effect.
        /// </summary>
        public ImpersonateIdentity(ImpersonationBehavior impersonationBehavior)
        {
            if (impersonationBehavior == ImpersonationBehavior.UseOriginalIdentity)
                m_context = WindowsIdentity.Impersonate(IntPtr.Zero);
        }

        /// <summary>
        /// This method Impersonates to the current user account and notifies
        /// the web part that it's be done. 
        /// </summary>
        public void Dispose()
        {
            if (m_context != null)
                m_context.Dispose();
        }

    }   // END: ImpersonateIdentity class
}
