/* Copyright (c) Microsoft Corporation. All rights reserved. */

﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Provides a class that does performance efficient comparisons between atomized strings
    /// from the same NameTable.
    /// </summary>
    internal class AtomizedString
    {
        private string m_String;
        private NameTable m_NameTable;

        /// <summary>
        /// Creates a new AtomizedString object.
        /// </summary>
        /// <param name="str">The string.  May not be null.</param>
        /// <param name="nameTable">The <Typ>NameTable</Typ> that should contain <P>str</P>,
        /// although this is not enforced (see remarks.)</param>
        /// <remarks>
        /// In order for <Typ>AtomizedString</Typ> comparisons to work correctly, the <P>str</P>
        /// parameter must be contained in the <P>nameTable</P>.  For performance reasons, this
        /// is not checked at runtime, except in debug builds where it throws ArgumentException.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <P>str</P> or <P>nameTable</P> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// (debug build only) <P>nameTable</P> does not contain <P>str</P>.
        /// </exception>
        internal AtomizedString(string str, NameTable nameTable)
        {
            if (str == null) throw new ArgumentNullException("str");
            if (nameTable == null) throw new ArgumentNullException("nameTable");
            m_NameTable = nameTable;
            // although there is no real guarantee that str is in nameTable without
            // specifically checking, it's not worth it to do so (except in debug builds.)
#if DEBUG
            if (null == nameTable.Get(str)) throw new ArgumentException(Resources.WrongNameTable);
#endif
            m_String = str;
        }

        /// <summary>
        /// Returns the string value.
        /// </summary>
        internal string Value
        {
            get
            {
                return m_String;
            }
        }

        /// <summary>
        /// Returns the NameTable.
        /// </summary>
        internal NameTable NameTable
        {
            get
            {
                return m_NameTable;
            }
        }

        /// <summary>
        /// Returns true if the Value of the two objects are the same atomized string from
        /// the same NameTable.
        /// </summary>
        /// <param name="obj">AtomizedString object to compare with.</param>
        /// <returns>True if equal, false if not.</returns>
        /// <exception cref="ArgumentException">
        /// If comparing <Typ>AtomizedString</Typ> objects with different <Typ>NameTable</Typ>'s.
        /// </exception>
        public override bool Equals(object obj)
        {
            AtomizedString atom = obj as AtomizedString;
            if (atom != null)
            {
                if (!m_NameTable.Equals(atom.NameTable)) throw new ArgumentException(Resources.WrongNameTable);
                return Object.ReferenceEquals(m_String, atom.Value);
            }
            return false;
        }

        /// <summary>
        /// Compare two atomized strings for equality.  See <Mth>AtomizedString.Equals</Mth>.
        /// </summary>
        /// <param name="atom1">AtomizedString to the left of the ==.</param>
        /// <param name="atom2">AtomizedString to the right of the ==.</param>
        /// <returns>True if equal, false if not.</returns>
        public static bool operator ==(AtomizedString atom1, AtomizedString atom2)
        {
            if (null == (object)atom1)
            {
                if (null == (object)atom2) return true;
                else return false;
            }
            return atom1.Equals(atom2);
        }

        /// <summary>
        /// Compare two atomized strings for inequality.  See <Mth>AtomizedString.Equals</Mth>.
        /// </summary>
        /// <param name="atom1">AtomizedString to the left of the !=.</param>
        /// <param name="atom2">AtomizedString to the right of the !=.</param>
        /// <returns>True if inequal, false if equal.</returns>
        public static bool operator !=(AtomizedString atom1, AtomizedString atom2)
        {
            if (null == (object)atom1)
            {
                if (null == (object)atom2) return false;
                else return true;
            }
            return !atom1.Equals(atom2);
        }

        /// <summary>
        /// Overrides GetHashCode (necessary to override <Mth>AtomizedString.Equals</Mth>.
        /// </summary>
        /// <returns>The underlying string's <Mth>String.GetHashCode</Mth> return value.</returns>
        public override int GetHashCode()
        {
            // the following is not the most efficient algorithm, but it is expediant to code
            // and is sufficient.
            return m_String.GetHashCode();
        }
    }

    /// <summary>
    /// Specifies the type of node.  When the HtmlTextReader parses an HTML file, it reads the file
    /// character by character.  Once it has read enough characters to know what the syntax represents,
    /// it assigns a "node type" - e.g. Element, EndElement, Text, etc.
    /// </summary>
    internal enum HtmlNodeType
    {
        /// <summary>
        /// There is no current node.
        /// </summary>
        None,
        /// <summary>
        /// Comment. E.g. anything that starts with a less than sign followed by an exclamation point
        /// and two dashes.
        /// </summary>
        Comment,
        /// <summary>
        /// Identifier. E.g. anything that starts with a less than sign and an exclamation point but
        /// isn't a comment. Not an "official" HTML type.
        /// </summary>
        Identifier,
        /// <summary>
        /// Element.  E.g. tag.
        /// </summary>
        Element,
        /// <summary>
        /// End element.  E.g. end tag.
        /// </summary>
        EndElement,
        /// <summary>
        /// Text.
        /// </summary>
        Text,
    }

    // state machine variables
    internal enum ParseMode
    {
        Skip, // Parse to next node only, don't store info from the current node.
        Normal, // Parse to next node while storing info from current node into appropriate variables.
        Write, // Parse to next node while writing the current node to a stream.
    }

    /// <summary>
    /// Holds information about a single attribute (name / value pair, quote character, line number
    /// and position.)
    /// </summary>
    internal class AttributeNode
    {
        private HtmlString m_Name; // name of the attribute
        private HtmlString m_Value; // value of the attribute
        
        /// <summary>
        /// Quote character used around the attribute value. '\0' means no quote character is used.
        /// Otherwise will be either '\'' or '"'.
        /// </summary>
        internal char QuoteChar; // will be '\0', '\'', or '"'.  '\0' means no quote char.
        
        /// <summary>
        /// Line number of the attribute.
        /// </summary>
        internal int LineNumber;

        /// <summary>
        /// Line position of the attribute.
        /// </summary>
        internal int LinePosition;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the attribute, as it is encoded in the HTML.</param>
        /// <param name="value">Value of the attribute, as it is encoded in the HTML.</param>
        /// <param name="quoteChar">The quote character, will be '\0', '\'', or '"'.  '\0' means no quote char.</param>
        /// <param name="lineNumber">The line number of the attribute.</param>
        /// <param name="linePosition">The line position of the attribute.</param>
        internal AttributeNode(HtmlString name, HtmlString value, char quoteChar, int lineNumber, int linePosition)
        {
            m_Name = name;
            m_Value = value;
            QuoteChar = quoteChar;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        internal PlainTextString Name
        {
            get
            {
                return new PlainTextString(m_Name);
            }
        }

        internal PlainTextString Value
        {
            get
            {
                return new PlainTextString(m_Value);
            }
        }

    }

    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to HTML data which
    /// may also contain embedded XML.
    /// </summary>
    internal class HtmlTextReader : IDisposable
    {
        /// <summary>
        /// State-machine class that parses a StreamReader, character-by-character, for HTML and XML.
        /// From a macro view, it is always in one of two states: between nodes or parsing a node.
        /// The main parsing routine is <Mth>HtmlNodeParser.Parse</Mth>.  If the parser's state is "between nodes"
        /// upon entering this method, upon exiting the method (assuming there is another node to parse)
        /// the state will usually  be parsing a node.  This is because it exits as soon as it knows the type of node
        /// being parsed, which usually won't take until the end of the node.  If the parser's state is "parsing a 
        /// node" upon entry, upon exit the state will be "between nodes."
        /// </summary>
        /// <remarks>
        /// Call <Mth>Close</Mth> when done to dispose of the <Typ>StreamWriter</Typ>'s.
        /// </remarks>
        private class HtmlNodeParser
        {
            private enum HtmlParseState
            {
                None, // between nodes
                BeginTag, // just read a '<' character and that's all
                Text, // parsing a text node
                Tag, // parsing an element node
                EndTag, // parsing an end element node
                BetweenAttributes, // between attributes inside an element or Identifier
                AttributeName, // parsing the name of an attribute
                AttributeBeforeEquals, // after the attribute name but before the equal sign
                AttributeAfterEquals, // after the equal sign but before the attribute value
                AttributeValue, // parsing the attribute value
                Comment, // parsing a comment
                QuotedAttributeValue, // parsing a quoted attribute value
                Identifier, // parsing anything besides a comment that starts with <!
            }

            /// <summary>
            /// holds the stream being parsed
            /// </summary>
            private StreamReader m_Reader;

            /// <summary>
            /// current parse state
            /// </summary>
            private HtmlParseState m_ParseState;

            /// <summary>
            /// collection of attribute nodes on currently parsed element or Identifier
            /// </summary>
            private List<AttributeNode> m_Attributes;

            /// <summary>
            /// name of the currently parsing attribute.
            /// </summary>
            private StringBuilder m_AttributeName;

            /// <summary>
            /// value of the currently parsing attribute.
            /// </summary>
            private StringBuilder m_AttributeValue;

            /// <summary>
            /// name of the currently parsing node.
            /// </summary>
            private StringBuilder m_Name;
            /// <summary>
            /// Name of the currently parsing node, or String.Empty if none.
            /// </summary>
            internal PlainTextString Name
            {
                get
                {
                    if (m_Name == null) return String.Empty;
                    return HttpUtility.HtmlDecode(m_Name.ToString());
                }
            }

            /// <summary>
            /// The current Parse mode of the HtmlNodeParser.  See <Typ>ParseMode</Typ>.
            /// </summary>
            private ParseMode m_ParseMode;

            /// <summary>
            /// value of the currently parsing node.
            /// </summary>
            private StringBuilder m_Value;
            /// <summary>
            /// value of the currently parsing node, or String.Empty if none.
            /// </summary>
            internal PlainTextString Value
            {
                get
                {
                    // if the HtmlNodeParser parse state is not None, it hasn't parsed the entire value yet,
                    // so parse it.
                    try
                    {
                        // set the Parse mode for the following calls to Parse() to Normal so the
                        // value is stored in m_Value.
                        m_ParseMode = ParseMode.Normal;
                        while (m_ParseState != HtmlParseState.None)
                        {
                            Parse();
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        // do nothing
                    }
                    if (m_Value == null) return String.Empty;
                    return HttpUtility.HtmlDecode(m_Value.ToString());
                }
            }

            /// <summary>
            /// the line position of the currently parsing character
            /// </summary>
            private int m_LinePosition;

            /// <summary>
            /// the line number of the currently parsing character
            /// </summary>
            private int m_LineNumber;

            /// <summary>
            /// quote char around the currently parsing attribute.
            /// </summary>
            private char m_QuoteChar;

            /// <summary>
            /// same as m_LineNumber but for the beginning of the currently parsing attribute name
            /// </summary>
            private int m_AttributeLineNumber;

            /// <summary>
            /// same as m_LinePosition but for the beginning of the currently parsing attribute name
            /// </summary>
            private int m_AttributeLinePosition;

            /// <summary>
            /// On a per-node basis, contains the contents of the currently parsing node, up to the
            /// point at which it has been parsed so far.
            /// </summary>
            private StreamWriter m_ParseWriter;

            /// <summary>
            /// This StreamWriter is used by GetOuterHtml and is only non-null during the lifetime of that call.
            /// </summary>
            private StreamWriter m_GetOuterHtmlWriter;

            /// <summary>
            /// Close the <Typ>StreamWriter</Typ>'s used by this object.
            /// </summary>
            internal void Close()
            {
                if (m_ParseWriter != null)
                {
                    m_ParseWriter.Dispose();
                    m_ParseWriter = null;
                }
                if (m_GetOuterHtmlWriter != null)
                {
                    m_GetOuterHtmlWriter.Dispose();
                    m_GetOuterHtmlWriter = null;
                }
            }

            /// <summary>
            /// This TextWriter is provided to the CopyNode call,
            /// and is only non-null during the lifetime of that call.
            /// </summary>
            private TextWriter m_CopyNode;

            /// <summary>
            /// Identifies the Element as an empty element (e.g. one that ends with /> instead of just >).
            /// This is only valid when the current node is an Element and after the entire Element has been 
            /// parsed (e.g. m_ParseState is None.)
            /// </summary>
            private bool m_IsEmptyElement;
            /// <summary>
            /// Get accessor for m_IsEmptyElement, which ensures the correct validations.
            /// </summary>
            /// <exception cref="InvalidOperationException">If the current NodeType is not Element
            /// or m_ParseState is not None.</exception>
            private bool IsEmptyElement
            {
                get
                {
                    if (NodeType != HtmlNodeType.Element || m_ParseState != HtmlParseState.None)
                        throw new InvalidOperationException();
                    return m_IsEmptyElement;
                }
                set
                {
                    m_IsEmptyElement = value;
                }
            }

            /// <summary>
            /// The type of node being parsed.
            /// </summary>
            private HtmlNodeType m_NodeType;
            /// <summary>
            /// The type of node being parsed.
            /// </summary>
            internal HtmlNodeType NodeType
            {
                get
                {
                    return m_NodeType;
                }
            }

            /// <summary>
            /// Creates a new HtmlNodeParser using the provided reader.
            /// </summary>
            /// <param name="reader">StreamReader that reads from the Html stream.</param>
            internal HtmlNodeParser(StreamReader reader)
            {
                m_Reader = reader;
                m_Attributes = new List<AttributeNode>();
                m_NodeType = HtmlNodeType.None;
            }

            /// <summary>
            /// Copies the current node to the supplied TextWriter at the TextWriter's current position.
            /// </summary>
            /// <remarks>Does not call <Mth>Flush</Mth> on the supplied <Typ>TextWriter</Typ>.</remarks>
            /// <exception cref="ArgumentNullException"><P>textWriter</P> is null.</exception>
            /// <exception cref="InvalidOperationException">The state of the reader is incorrect for this call.</exception>
            internal void CopyNode(TextWriter textWriter)
            {
                if (textWriter == null) throw new ArgumentNullException();
                // if m_ParseWriter is null, something is wrong - e.g. the user called GetOuterXml() on this node already.
                if (m_ParseWriter == null) throw new InvalidOperationException();
                // Copy the current contents of m_ParseWriter into textWriter.  m_ParseWriter contains the current node's content
                // up to the point it has been read so far.
                m_ParseWriter.Flush();
                m_ParseWriter.BaseStream.Position = 0;
                // We'll close the m_ParseWriter after this operation, so don't worry about the StreamReader
                // closing the underlying stream.
                using (StreamReader reader = new StreamReader(m_ParseWriter.BaseStream, m_ParseWriter.Encoding))
                {
                    int i = reader.Read();
                    while (i != -1)
                    {
                        textWriter.Write((char)(ushort)i);
                        i = reader.Read();
                    }
                }
                // Set m_ParseWriter to null since ReadNextChar() should no longer write into it, but rather
                // create a new one.
                m_ParseWriter.Dispose();
                m_ParseWriter = null;
                // if parse state is not None, the current node is partially parsed.
                // Set m_CopyNode to textWriter. This will cause further calls to Parse() to stream into m_CopyNode
                // instead of m_ParseWriter.
                if (m_ParseState != HtmlParseState.None)
                {
                    m_CopyNode = textWriter;
                    m_ParseMode = ParseMode.Skip;
                    try
                    {
                        while (m_ParseState != HtmlParseState.None)
                        {
                            Parse();
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        // do nothing
                    }
                    // now set m_CopyNode to null since we're done having Parse() copy into the textWriter.
                    // Next call to Parse() will create a new m_ParseWriter.
                    m_CopyNode = null;
                }
            }

            /// <summary>
            /// Returns the Html/Xml of the current Element up to its corresponding EndElement, unless the
            /// current Element is an empty element (e.g. ends with a /> and not just a >).  If it is an
            /// empty element, returns only the current Element.
            /// </summary>
            /// <returns>
            /// Returns a <Typ>TextReader</Typ> that gives access to the HTML (or XML as the case may be) from
            /// the current node (which must be an Element node) to the corresponding EndElement
            /// node (or the end of the file if the EndElement doesn't exist.)
            /// </returns>
            /// <remarks>
            /// After calling this method, the state of the parser will be that the current note type is "none."
            /// </remarks>
            /// <exception cref="InvalidOperationException">If the node type isn't an Element node,
            /// or the node's name is blank.</exception>
            internal TextReader GetOuterHtml()
            {
                PlainTextString name = Name;
                // the current node type must be an Element node and have a name.
                if (m_NodeType != HtmlNodeType.Element || String.IsNullOrEmpty(name)) throw new InvalidOperationException();

                // Move m_ParseWriter over to m_GetOuterHtmlWriter so everything gets copied into it, and it never
                // gets replaced (see ReadNextChar() - if m_GetOuterHtmlWriter is non-null, characters get
                // copied into it instead of m_ParseWriter.)
                m_GetOuterHtmlWriter = m_ParseWriter;
                m_ParseWriter = null;

                // Capture the rest of the current Element.  Set m_ParseMode to Skip to avoid saving
                // attribute values.
                m_ParseMode = ParseMode.Skip;
                try
                {
                    while (m_ParseState != HtmlParseState.None) Parse();
                }
                catch (EndOfStreamException)
                {
                    // do nothing
                }

                // If this isn't an empty element, find the corresponding EndElement
                if (!IsEmptyElement)
                {
                    // keep a count of Element node names equivalent to the current node name, to
                    // account for Elements of the same name that are inside the current Element,
                    // in order to stop parsing at the corrent EndElement.
                    int count = 1; // count the current Element
                    while (count > 0 && GetNextNode())
                    {
                        if (m_NodeType == HtmlNodeType.Element && 0 == String.Compare(Name, name, StringComparison.OrdinalIgnoreCase)) count++;
                        else if (m_NodeType == HtmlNodeType.EndElement && 0 == String.Compare(Name, name, StringComparison.OrdinalIgnoreCase)) count--;
                    }
                    // If there is still a count, it means GetNextNode returned false, meaning end of stream.
                    if (count == 0)
                    {
                        // make sure to finish parsing the current node
                        try
                        {
                            while (m_ParseState != HtmlParseState.None)
                            {
                                Parse();
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            // do nothing
                        }
                    }
                }
                // transfer the stream writer's stream into a text reader and return it.
                m_GetOuterHtmlWriter.Flush();
                // the stream is a DetachableStream from the ReadNextChar() method
                DetachableStream detachableStream = (DetachableStream)m_GetOuterHtmlWriter.BaseStream;
                Stream stream = detachableStream.Stream; // the underlying stream
                // detach the stream from the m_GetOuterHtmlWriter
                detachableStream.Detach();
                // position the underlying stream at position 0 and hand it off to the StreamReader
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream, m_GetOuterHtmlWriter.Encoding);
                m_GetOuterHtmlWriter.Dispose();
                m_GetOuterHtmlWriter = null;

                // set the current node type to "none"
                m_NodeType = HtmlNodeType.None;

                // return the reader
                return reader;
            }

            /// <summary>
            /// Parses until a new node is found.  Skips over the remainder of the currently parsing node.
            /// </summary>
            /// <returns>true if a new node is found. false if end of stream is reached.</returns>
            internal bool GetNextNode()
            {
                try
                {
                    // set Parse mode to Skip for the following calls to Parse, to parse over the
                    // current node.
                    m_ParseMode = ParseMode.Skip;
                    while (m_ParseState != HtmlParseState.None) Parse();

                    // Now read the next node.  Once Parse has parsed enough of the next node to
                    // know what it is, it will set the m_NodeType, m_Name, etc. and return.
                    // Set Parse mode to normal so the name of the node is stored in m_Name.
                    m_ParseMode = ParseMode.Normal;
                    Parse();
                }
                catch (EndOfStreamException)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Parses the next section of the stream.
            /// </summary>
            /// <remarks>
            /// Throws EndOfStreamException when it hits the end of the stream without finishing
            /// parsing a full node.
            /// 
            /// On entry to this method:
            /// m_ParseMode must be set.
            /// 
            /// On exit from this method:
            /// m_AttributeLineNumber is set
            /// m_AttributeLinePosition is set
            /// m_ParseState is set
            /// m_NodeType is set if a new node is parsed.
            /// m_Reader is advanced to either the end of the currently parsing node, or just enough of the
            /// newly parsing node to recognize the type of node it is.
            /// </remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")] // Most of the complexity is due to simple switch statements.  Not sure if it would truly be less complex if it was split into different methods...
            private void Parse()
            {
                if (m_ParseState == HtmlParseState.BetweenAttributes)
                {
                    m_AttributeLineNumber = m_LineNumber;
                    m_AttributeLinePosition = m_LinePosition;
                }

                // keep track of the previous character to find empty element nodes.
                char? previousChar = null;
                bool setPreviousChar = false; // only set it after the first loop
                char currentChar = '\0'; // only setting this here to shut up the compiler.
                while (true)
                {
                    // All node types end at the '>' character with the exception of the Text node.
                    // So, for the Text parse state, peek at the next character rather than read it.
                    // If it is the '<' character, end the node.
                    if (m_ParseState == HtmlParseState.Text && (char)((ushort)m_Reader.Peek()) == '<')
                    {
                        EndNode(previousChar);
                        break; // break out of the while loop - done parsing the Text node
                    }
                    if (setPreviousChar) previousChar = currentChar;
                    currentChar = NextChar();
                    setPreviousChar = true;
                    switch (m_ParseState)
                    {
                        case HtmlParseState.AttributeAfterEquals:
                            if (currentChar == '>')
                            {
                                AddAttribute();
                                EndNode(previousChar);
                                break;
                            }
                            else if (!Char.IsWhiteSpace(currentChar))
                            {
                                if (currentChar == '\'' || currentChar == '"')
                                {
                                    m_QuoteChar = currentChar;
                                    m_ParseState = HtmlParseState.QuotedAttributeValue;
                                    // continue parsing
                                }
                                else // non-quoted attribute value
                                {
                                    AppendToAttributeValue(currentChar);
                                    m_ParseState = HtmlParseState.AttributeValue;
                                    // continue parsing
                                }
                            }
                            continue;
                        case HtmlParseState.AttributeBeforeEquals:
                            if (!Char.IsWhiteSpace(currentChar))
                            {
                                if (currentChar == '>')
                                {
                                    AddAttribute();
                                    EndNode(previousChar);
                                    break;
                                }
                                else if (currentChar == '=')
                                {
                                    m_ParseState = HtmlParseState.AttributeAfterEquals;
                                    // continue parsing
                                }
                                else // start of a new attribute name
                                {
                                    AddAttribute();
                                    StartNewAttribute();
                                    AppendToAttributeName(currentChar);
                                    m_ParseState = HtmlParseState.AttributeName;
                                    // continue parsing
                                }
                            }
                            continue;
                        case HtmlParseState.AttributeName:
                            if (Char.IsWhiteSpace(currentChar))
                            {
                                m_ParseState = HtmlParseState.AttributeBeforeEquals;
                                // continue parsing
                            }
                            else if (currentChar == '=')
                            {
                                m_ParseState = HtmlParseState.AttributeAfterEquals;
                                // continue parsing
                            }
                            else if (currentChar == '>')
                            {
                                AddAttribute();
                                EndNode(previousChar);
                                break;
                            }
                            else
                            {
                                AppendToAttributeName(currentChar);
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.AttributeValue:
                            if (Char.IsWhiteSpace(currentChar))
                            {
                                AddAttribute();
                                m_ParseState = HtmlParseState.BetweenAttributes;
                                // continue parsing
                            }
                            else if (currentChar == '>')
                            {
                                AddAttribute();
                                EndNode(previousChar);
                                break;
                            }
                            else
                            {
                                AppendToAttributeValue(currentChar);
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.BetweenAttributes:
                            if (!Char.IsWhiteSpace(currentChar))
                            {
                                if (currentChar == '>')
                                {
                                    EndNode(previousChar);
                                    break;
                                }
                                else
                                {
                                    StartNewAttribute();
                                    AppendToAttributeName(currentChar);
                                    m_ParseState = HtmlParseState.AttributeName;
                                    // continue parsing
                                }
                            }
                            continue;
                        case HtmlParseState.Comment:
                            if (currentChar == '-')
                            {
                                char secondChar = NextChar();
                                if (secondChar == '-')
                                {
                                    char thirdChar = NextChar();
                                    int count = 0; // keep track of the number of '-' signs to append to the node value
                                    while (thirdChar == '-')
                                    {
                                        thirdChar = NextChar();
                                        count++;
                                    }
                                    // check if the end of the comment has been reached
                                    if (thirdChar == '>')
                                    {
                                        while (count-- > 0)
                                        {
                                            AppendToValue('-');
                                        }
                                        EndNode(previousChar);
                                        break;
                                    }
                                    else
                                    {
                                        // If there were extra dashes before the final -->, append them to the
                                        // value.
                                        count += 2;
                                        while (count-- > 0)
                                        {
                                            AppendToValue('-');
                                        }
                                        AppendToValue(thirdChar);
                                        // continue parsing
                                    }
                                }
                                else
                                {
                                    AppendToValue(currentChar);
                                    AppendToValue(secondChar);
                                    // continue parsing
                                }
                            }
                            else
                            {
                                AppendToValue(currentChar);
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.Identifier:
                            if (currentChar == '>')
                            {
                                EndNode(previousChar);
                                break;
                            }
                            else if (Char.IsWhiteSpace(currentChar))
                            {
                                m_ParseState = HtmlParseState.BetweenAttributes;
                                break;
                            }
                            else
                            {
                                m_Name.Append(currentChar);
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.QuotedAttributeValue:
                            if (currentChar == m_QuoteChar && currentChar != '\0')
                            {
                                AddAttribute();
                                m_ParseState = HtmlParseState.BetweenAttributes;
                                // continue parsing
                            }
                            else
                            {
                                AppendToAttributeValue(currentChar);
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.Tag:
                        case HtmlParseState.EndTag:
                            if (currentChar == '>')
                            {
                                EndNode(previousChar);
                                break;
                            }
                            else if (Char.IsWhiteSpace(currentChar))
                            {
                                m_ParseState = HtmlParseState.BetweenAttributes;
                                break;
                            }
                            else
                            {
                                m_Name.Append(currentChar);
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.Text:
                            // This node type ending was checked above as a special case.
                            AppendToValue(currentChar);
                            // continue parsing
                            continue;
                        case HtmlParseState.None:
                            if (currentChar != '<')
                            {
                                m_ParseState = HtmlParseState.Text;
                                StartNode(HtmlNodeType.Text);
                                AppendToValue(currentChar);
                                break;
                            }
                            else
                            {
                                m_ParseState = HtmlParseState.BeginTag;
                                // continue parsing
                            }
                            continue;
                        case HtmlParseState.BeginTag:
                            if (currentChar == '!')
                            {
                                char secondChar = NextChar();
                                if (secondChar == '-')
                                {
                                    char thirdChar = NextChar();
                                    if (thirdChar == '-')
                                    {
                                        m_ParseState = HtmlParseState.Comment;
                                        StartNode(HtmlNodeType.Comment);
                                        break;
                                    }
                                    else if (thirdChar == '>')
                                    {
                                        StartNode(HtmlNodeType.Identifier);
                                        AppendToValue(secondChar);
                                        EndNode(previousChar);
                                        break;
                                    }
                                }
                                else if (secondChar == '>')
                                {
                                    StartNode(HtmlNodeType.Identifier);
                                    EndNode(previousChar);
                                    break;
                                }
                                else
                                {
                                    StartNode(HtmlNodeType.Identifier);
                                    m_Name.Append(secondChar);
                                    m_ParseState = HtmlParseState.Identifier;
                                    // continue parsing
                                }
                            }
                            else if (Char.IsLetter(currentChar))
                            {
                                StartNode(HtmlNodeType.Element);
                                m_ParseState = HtmlParseState.Tag;
                                m_Name.Append(currentChar);
                                // continue parsing
                            }
                            else if (currentChar == '/')
                            {
                                StartNode(HtmlNodeType.EndElement);
                                m_ParseState = HtmlParseState.EndTag;
                                // continue parsing
                            }
                            else
                            {
                                // if a number or symbol appears after a '<', IE treats it
                                // as text instead of an element.
                                m_ParseState = HtmlParseState.Text;
                                StartNode(HtmlNodeType.Text);
                                AppendToValue('<');
                                AppendToValue(currentChar);
                                break;
                            }
                            continue;
                    }
                    break;
                }
            }

            internal List<AttributeNode> Attributes
            {
                get
                {
                    // compute m_Attributes first if needed.
                    try
                    {
                        ParseAttributes();
                    }
                    catch (EndOfStreamException)
                    {
                        // do nothing
                    }
                    return m_Attributes;
                }
            }

            private int ReadNextChar()
            {
                if (m_ParseState == HtmlParseState.None && m_GetOuterHtmlWriter == null && m_CopyNode == null)
                {
                    // it's time to parse a new node. Create a new StreamWriter to write the new node into.
                    if (m_ParseWriter != null)
                    {
                        m_ParseWriter.Dispose();
                    }
                    // this must be a detachable stream to support the operations in the GetOuterHtml() method
                    m_ParseWriter = new StreamWriter(new DetachableStream(1024), m_Reader.CurrentEncoding);
                }
                int i = m_Reader.Read();
                if (i != -1)
                {
                    // If m_GetOuterHtmlWriter is non-null, it means we are currently copying all read characters into it.  
                    // It is unneccessary to copy into m_ParseWriter in this case.  Same thing for m_CopyNode.
                    // The three are all mutually exclusive.
                    TextWriter writer;
                    if (m_GetOuterHtmlWriter != null)
                    {
                        writer = m_GetOuterHtmlWriter;
                    }
                    else if (m_CopyNode != null)
                    {
                        writer = m_CopyNode;
                    }
                    else
                    {
                        writer = m_ParseWriter;
                    }
                    writer.Write((char)(ushort)i);
                }
                return i;
            }

            private char NextChar()
            {
                // Read a character.  If it is a CR, check if the next one is a LF.  If it is a LF, don't
                // increment the line number yet.  It will increment when the LF is read.  Otherwise, 
                // Increment the line position when a non-carriage return or line feed is read.
                // A single whitespace character (even a tab) increments position by one.
                // If i is -1, it is the end of stream
                int i = ReadNextChar();
                if (i == -1)
                {
                    // i is -1, so the end of the stream has been reached
                    throw new EndOfStreamException(Resources.EndOfStream);
                }
                else
                {
                    // if i is a carriage return, peek at the next character.  If it is not a line feed, increment
                    // the line number.  Also increment the line number for line feed.  When incrementing the
                    // line number, reset the line position.
                    if ((i == 0x000D && m_Reader.Peek() != 0x000A) || i == 0x000A)
                    {
                        m_LineNumber++;
                        m_LinePosition = 0;
                    }
                    return (char)((ushort)i);
                }
            }

            /// <summary>
            /// Begins parsing a new node.
            /// </summary>
            /// <param name="nodeType">The type of node being parsed.</param>
            private void StartNode(HtmlNodeType nodeType)
            {
                m_NodeType = nodeType;
                m_Attributes.Clear();
                m_Name = new StringBuilder();
                m_Value = new StringBuilder();
            }

            /// <summary>
            /// Finishes parsing the current node.
            /// </summary>
            private void EndNode(char? previousChar)
            {
                // if previousChar is "/" and this is an element node, set IsEmptyElement.
                if (previousChar == '/' && NodeType == HtmlNodeType.Element)
                {
                    IsEmptyElement = true;
                }
                else
                {
                    IsEmptyElement = false;
                }
                m_ParseState = HtmlParseState.None;
            }

            /// <summary>
            /// Begins parsing a new attribute.
            /// </summary>
            /// <remarks>
            /// If <Fld>m_ParseMode</Fld> is <Typ>ParseMode.Skip</Typ> this method does nothing.
            /// Otherwise, it clears the current attribute name, value, and quote char.
            /// </remarks>
            private void StartNewAttribute()
            {
                if (m_ParseMode != ParseMode.Skip)
                {
                    m_AttributeName = new StringBuilder();
                    m_AttributeValue = new StringBuilder();
                    m_QuoteChar = '\0';
                }
            }

            /// <summary>
            /// Add an attribute to the list of attributes.
            /// </summary>
            /// <remarks>
            /// If <Fld>m_ParseMode</Fld> is <Typ>ParseMode.Skip</Typ> this method does nothing.
            /// Otherwise, it creates a new attribute node using the parsed attribute name, value, quote char,
            /// line number, and line position.
            /// </remarks>
            private void AddAttribute()
            {
                if (m_ParseMode != ParseMode.Skip)
                {
                    m_Attributes.Add(new AttributeNode(
                        new HtmlString(m_AttributeName.ToString()),
                        new HtmlString(m_AttributeValue.ToString()),
                        m_QuoteChar, m_AttributeLineNumber, m_AttributeLinePosition));
                }
            }

            /// <summary>
            /// Append the character to m_Value.
            /// </summary>
            /// <param name="c">Character to append.</param>
            private void AppendToValue(char c)
            {
                if (m_ParseMode == ParseMode.Normal)
                {
                    m_Value.Append(c);
                }
            }

            /// <summary>
            /// Append the character to m_AttributeName.
            /// </summary>
            /// <param name="c">Character to append.</param>
            private void AppendToAttributeName(char c)
            {
                if (m_ParseMode == ParseMode.Normal)
                {
                    m_AttributeName.Append(c);
                }
            }

            /// <summary>
            /// Append the character to m_AttributeValue.
            /// </summary>
            /// <param name="c">Character to append.</param>
            private void AppendToAttributeValue(char c)
            {
                if (m_ParseMode == ParseMode.Normal)
                {
                    m_AttributeValue.Append(c);
                }
            }

            private void ParseAttributes()
            {
                // the node must be an element or Identifier for this to be a valid call
                if (m_NodeType != HtmlNodeType.Element && m_NodeType != HtmlNodeType.Identifier) return;
                // if parse state isn't None, the full node hasn't yet been parsed.
                if (m_ParseState != HtmlParseState.None)
                {
                    // the call to Parse can throw if the end of stream is reached.
                    // set m_ParseMode to ParseMode.Normal to capture attributes
                    m_ParseMode = ParseMode.Normal;
                    try
                    {
                        while (m_ParseState != HtmlParseState.None)
                        {
                            Parse();
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        // do nothing
                    }
                }
            }
        }

        private XmlNamespaceManager m_xmlNamespaceManager; // namespace manager parsed from the <html> tag
        private NameTable m_nameTable; // name table used by the m_XmlNamespaceManager, AddNamespaceURI(), and NamespaceURI.
        private StreamReader m_streamReader; // the stream reader containing the data being parsed
        private HtmlNodeParser m_htmlNodeParser; // HtmlNodeParser that does the actual parsing
        private ReadState m_readState; // The System.Xml.ReadState of the reader.

        /// <summary>
        /// Calls Close().
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Throws ObjectDisposedException if this object has been disposed or closed.
        /// </summary>
        private void CheckDispose()
        {
            if (m_readState == ReadState.Closed) throw new ObjectDisposedException("HtmlTextReader");
        }

        #region Initialization
        /// <summary>
        /// Initializes a new instance of <Typ>HtmlTextReader</Typ>.
        /// </summary>
        /// <remarks>
        /// The caller should ensure the <paramref name="stream"/> has an efficient, high performance, ReadByte()
        /// method.  For instance, use <Typ>BufferedStream</Typ>.  The <paramref name="stream"/> must be readable 
        /// and seekable.
        /// </remarks>
        /// <exception cref="ArgumentException">The <paramref name="stream"/>is invalid (not readable or not seekable.)</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is null.</exception>
        /// <param name="stream">Stream containing the data to parse.</param>
        internal HtmlTextReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!stream.CanRead || !stream.CanSeek) throw new ArgumentException(Resources.StreamMustReadAndSeek, "stream");
            m_streamReader = new StreamReader(stream);
            m_htmlNodeParser = new HtmlNodeParser(m_streamReader);
            m_nameTable = new NameTable();
            m_xmlNamespaceManager = new XmlNamespaceManager(m_nameTable);
        }
        #endregion // Initialization

        #region Properties

        /// <summary>
        /// Gets the number of attributes on the current node.
        /// </summary>
        /// <remarks>
        /// Only valid when NodeType is Element or Identifier.  Otherwise, returns 0.
        /// <p>Causes attributes to be parsed if they haven’t been already.</p>
        /// </remarks>
        internal int AttributeCount
        {
            get
            {
                CheckDispose();
                HtmlNodeType nodeType = NodeType;
                if (nodeType == HtmlNodeType.Identifier
                    || nodeType == HtmlNodeType.Element
                    )
                {
                    return m_htmlNodeParser.Attributes.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Skips the current node up to the current node's end tag if the current node is an
        /// element.
        /// </summary>
        /// <remarks>
        /// If the current node is an element and there is no associated end element, skips all
        /// the way to the end of the file.
        /// If the current node is not an element, throws <Typ>NotSupportedException</Typ>.
        /// </remarks>
        internal void Skip()
        {
            CheckDispose();
            if (NodeType == HtmlNodeType.Element)
            {
                // just calling GetOuterHtml() has the effect of skipping the current node
                using (m_htmlNodeParser.GetOuterHtml()) { /*no operations needed*/ };
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Parses the current node as Xml, and creates an XmlReader containing the current node 
        /// up to the current node’s end tag.  The returned XmlReader is conforms to
        /// <Mth>ConformanceLevel.Fragment</Mth>.
        /// </summary>
        /// <returns><Typ>XmlReader</Typ> limited to reading the current stream from the current tag to
        /// the corresponding end tag.</returns>
        /// <remarks>
        /// After calling this function, the state of the reader will be "between nodes".  E.g. the
        /// NodeType will be "None".  The next node read by <Mth>Read</Mth> will be the node
        /// following the end tag parsed of the Xml island.
        /// <para>
        /// The caller is responsible for closing the <Typ>XmlReader</Typ> returned by this method.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If the node type isn't an Element node,
        /// or the node's name is blank.</exception>
        internal XmlReader GetOuterXml()
        {
            CheckDispose();
            // Create an xmlReader with a "fragment" conformance level, since this will be
            // an xml fragment.
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlParserContext parserContext = new XmlParserContext(null, m_xmlNamespaceManager, null, XmlSpace.None);
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.CloseInput = true;
            // Use the version of Create that takes XmlParserContext
            return XmlReader.Create(m_htmlNodeParser.GetOuterHtml(), settings, parserContext);
        }

        /// <summary>
        /// Gets a value indicating whether the current node has any attributes.
        /// </summary>
        /// <remarks>
        /// Only true when NodeType is Element or Identifier and contains attributes.  Otherwise, returns false.
        /// </remarks>
        internal bool HasAttributes
        {
            get
            {
                CheckDispose();
                HtmlNodeType nodeType = NodeType;
                // only element and Identifier node types can have attributes
                if (nodeType == HtmlNodeType.Identifier || nodeType == HtmlNodeType.Element)
                {
                    if (m_htmlNodeParser.Attributes.Count > 0) return true;
                    else return false;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the local name of the current node.
        /// </summary>
        /// <remarks>
        /// The local name is everything after the first ":" character in the Name, or the full
        /// Name if there is no ":" character.
        /// </remarks>
        internal PlainTextString LocalName
        {
            get
            {
                CheckDispose();
                string[] split = Name.ToString().Split(new Char[] { ':' }, 2);
                if (split.Length > 1)
                {
                    return split[split.Length - 1];
                }
                else
                {
                    return Name;
                }
            }
        }

        /// <summary>
        /// Gets the full name of the current node.
        /// </summary>
        /// <remarks>
        /// Since this represents the name of an HTML element, string comparisons should generally be done
        /// without case sensitivity.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The reader is in an error state.</exception>
        internal PlainTextString Name
        {
            get
            {
                CheckDispose();
                if (m_readState == ReadState.Error) throw new InvalidOperationException();
                PlainTextString name = String.Empty;
                // the NodeType property will throw InvalidOperationException if the reader is in an error state.
                switch (NodeType)
                {
                    case HtmlNodeType.Element:
                    case HtmlNodeType.EndElement:
                    case HtmlNodeType.Identifier:
                        name = m_htmlNodeParser.Name;
                        break;
                    default:
                        // leave name String.Empty.
                        break;
                }
                return name;
            }
        }

        /// <summary>
        /// Returns the namespace of the current node, treating the current node as Xml for the moment.
        /// </summary>
        /// <remarks>
        /// Unlike XmlTextReader, this is only valid if the current node is an Element.
        /// Returns null if there is no namespace or the current node is not an Element.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Current node is not an Element.</exception>
        internal AtomizedString NamespaceURI
        {
            get
            {
                CheckDispose();
                string ns = null;
                if (NodeType != HtmlNodeType.Element) return null;

                // temporarily add the current Element's xmlns attributes to m_XmlNamespaceManager
                // by pushing the scope, adding them, and then popping the scope when done.
                if (HasAttributes)
                {
                    m_xmlNamespaceManager.PushScope();
                    AddXmlns();
                    ns = m_xmlNamespaceManager.LookupNamespace(Prefix.ToString());
                    m_xmlNamespaceManager.PopScope();
                }
                else
                {
                    // repeating this code here instead of separating it out, to optimize the
                    // call to HasAttributes (e.g. avoid calling it more than once.)
                    ns = m_xmlNamespaceManager.LookupNamespace(Prefix.ToString());
                }

                // an empty string should return null
                if (ns == null || ns.Length == 0) return null;
                else return new AtomizedString(ns, m_nameTable);
            }
        }

        /// <summary>
        /// Gets the type of the current node.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is in an error state.</exception>
        internal HtmlNodeType NodeType
        {
            get
            {
                CheckDispose();
                if (ReadState == ReadState.Error) throw new InvalidOperationException(Resources.ReadError);
                return m_htmlNodeParser.NodeType;
            }
        }

        /// <summary>
        /// Gets the namespace prefix associated with the node.
        /// </summary>
        /// <remarks>
        /// If the node's name has a ":" character in it, everything to the left of the first instance of
        /// the ":" character is considered to be the namespace prefix.  If there is no ":" character,
        /// returns String.Empty.
        /// </remarks>
        private PlainTextString Prefix
        {
            get
            {
                CheckDispose();
                string[] split = Name.ToString().Split(new Char[] { ':' }, 2);
                if (split.Length > 1)
                {
                    return split[0];
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the state of the reader (Closed, EndOfFile, Error, Initial, Interactive)
        /// </summary>
        internal ReadState ReadState
        {
            get
            {
                return m_readState;
            }
        }

        /// <summary>
        /// Gets the text value of the current node.  Standard Html character entities are replaced with the corresponding 
        /// Unicode characters.
        /// </summary>
        internal PlainTextString Value
        {
            get
            {
                CheckDispose();
                PlainTextString value = String.Empty;
                // the NodeType property will throw InvalidOperationException if the reader is in an error state.
                switch (NodeType)
                {
                    case HtmlNodeType.Comment:
                    case HtmlNodeType.Identifier:
                    case HtmlNodeType.Text:
                        value = m_htmlNodeParser.Value;
                        break;
                    case HtmlNodeType.Element:
                    case HtmlNodeType.EndElement:
                    default:
                        // leave value String.Empty.
                        break;
                }
                return value;
            }
        }
        #endregion // Properties

        #region Methods

        /// <summary>
        /// Adds a namespace URI to the HtmlTextReader and returns an atomized object for efficient
        /// reference comparison with the object returned by NamespaceURI.
        /// </summary>
        /// <param name="uri">The string representing the URI.</param>
        /// <returns>Atomized object for efficient reference comparison with the object
        /// returned by NamespaceURI.</returns>
        /// <remarks>
        /// If the string <P>uri</P> has already been added, this method returns the atomized object
        /// representing that string.
        /// </remarks>
        internal AtomizedString AddNamespaceURI(string uri)
        {
            CheckDispose();
            if (uri == null) throw new ArgumentNullException();
            return new AtomizedString(m_nameTable.Add(uri), m_nameTable);
        }

        /// <summary>
        /// Changes the <Fld>ReadState</Fld> to Closed.
        /// </summary>
        /// <remarks>
        /// This method changes ReadState to Closed and also releases any resources held while reading 
        /// including calling Close on the underlying stream.  If Close has already been called, 
        /// no action is performed.
        /// <para>
        /// The caller should no longer call methods on this object after calling <Mth>Close</Mth> or
        /// <Mth>Dispose</Mth>.
        /// </para>
        /// </remarks>
        internal void Close()
        {
            m_readState = ReadState.Closed;
            if (m_streamReader != null)
            {
                m_streamReader.BaseStream.Close();
                m_streamReader.Close();
                m_streamReader = null;
            }
            if (m_htmlNodeParser != null)
            {
                m_htmlNodeParser.Close();
                m_htmlNodeParser = null;
            }
        }

        /// <summary>
        /// Copies the current node to the supplied TextWriter.  This may only be called once per node.
        /// <Typ>InvalidOperationException</Typ> is thrown if this is called more than once per node.
        /// </summary>
        /// <remarks>Does not call <Mth>Flush</Mth> on the supplied <Typ>TextWriter</Typ>.</remarks>
        /// <exception cref="ArgumentNullException"><P>textWriter</P> is null.</exception>
        /// <exception cref="InvalidOperationException">The state of the reader is incorrect for this call.</exception>
        internal void CopyNode(TextWriter textWriter)
        {
            CheckDispose();
            m_htmlNodeParser.CopyNode(textWriter);
        }

        /// <summary>
        /// Gets the value of the requested attribute name from the named Element or Identifier node.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ignoreCase"></param>
        /// <returns>The attribute value of the requested attribute name, or String.Empty if it doesn't exist.</returns>
        /// <remarks>
        /// If the node isn’t Element or Identifier, throws InvalidOperationException.  
        /// This method does not move the reader unless attributes haven’t been parsed yet.
        /// <p>If there is more than one attribute with the same name, only retrieves 
        /// the first.</p>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If <c>NodeType</c> is not
        /// <c>HtmlNodeType.Element</c> or <c>HtmlNodeType.Identifier</c>.</exception>
        internal PlainTextString GetAttributeValue(PlainTextString name, bool ignoreCase)
        {
            CheckDispose();
            if (NodeType != HtmlNodeType.Element && NodeType != HtmlNodeType.Identifier)
                throw new InvalidOperationException();
            for (int i = 0; i < m_htmlNodeParser.Attributes.Count; i++ )
            {
                if (0 == String.Compare(name, m_htmlNodeParser.Attributes[i].Name, 
                    ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return m_htmlNodeParser.Attributes[i].Value;
            }
            return String.Empty;
       }

        /// <summary>
        /// Gets the value of the requested attribute name from the case-sensitively named Element or Identifier node.  
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The attribute value of the requested attribute name, or String.Empty if it doesn't exist.</returns>
        /// <remarks>
        /// If the node isn’t Element or Identifier throws InvalidOperationException.  
        /// If the name doesn’t exist as an attribute returns String.Empty. 
        /// This method does not move the reader unless attributes haven’t been parsed yet.
        /// <p>If there is more than one attribute with the same name, only retrieves 
        /// the first.</p>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If <c>NodeType</c> is not
        /// <c>HtmlNodeType.Element</c> or <c>HtmlNodeType.Identifier</c>.</exception>
        internal PlainTextString GetAttributeValue(PlainTextString name)
        {
            CheckDispose();
            // calling the following can throw
            return GetAttributeValue(name, false);
        }

        /// <summary>
        /// Gets the value of an attribute on the current node, by index.
        /// </summary>
        /// <param name="index">Index of the attribute value to get.</param>
        /// <returns>Value of the attribute.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <P>index</P> is out of range. This can also throw if the node type is such that
        /// attributes aren't allowed. (E.g. text nodes can't have attributes.)
        /// </exception>
        internal PlainTextString GetAttributeValue(int index)
        {
            CheckDispose();
            if (index > m_htmlNodeParser.Attributes.Count - 1) throw new ArgumentException(Resources.IndexOutOfRange, "index");
            return m_htmlNodeParser.Attributes[index].Value;
        }

        /// <summary>
        /// Gets the name of an attribute on the current node, by index.
        /// </summary>
        /// <param name="index">Index of the attribute name to get.</param>
        /// <returns>Name of the attribute.</returns>
        /// <remarks>Since this represents the name of an HTML attribute, string comparisons should generally
        /// be done without case sensitivity.</remarks>
        /// <exception cref="IndexOutOfRangeException">
        /// <P>index</P> is out of range. This can also throw if the node type is such that
        /// attributes aren't allowed. (E.g. text nodes can't have attributes.)
        /// </exception>
        internal PlainTextString GetAttributeName(int index)
        {
            CheckDispose();
            if (index > m_htmlNodeParser.Attributes.Count - 1) throw new ArgumentException(Resources.IndexOutOfRange, "index");
            return m_htmlNodeParser.Attributes[index].Name;
        }

        /// <summary>
        /// Adds the current Element's xmlns attributes to m_XmlNamespaceManager.
        /// </summary>
        private void AddXmlns()
        {
            // Check for any xmlns attributes and add the namespaces to the XmlNamespaceManager
            for (int attributeIndex = 0; attributeIndex < AttributeCount; attributeIndex++)
            {
                // Note that the string returned from GetAttributeName is a PlainTextString.
                string[] split = GetAttributeName(attributeIndex).ToString().Split(new Char[] { ':' }, 2);
                if (split.Length > 1)
                {
                    // check for non-default namespace
                    if (0 == String.Compare("xmlns", split[0], StringComparison.OrdinalIgnoreCase))
                    {
                        // note that adding namespaces to m_XmlNamespaceManager also has the
                        // effect of adding the prefix and NamespaceURI to the m_NameTable.
                        m_xmlNamespaceManager.AddNamespace(split[1], GetAttributeValue(attributeIndex));
                    }
                }
                else
                {
                    // check for default namespace
                    if (0 == String.Compare("xmlns", split[0], StringComparison.OrdinalIgnoreCase))
                    {
                        m_xmlNamespaceManager.AddNamespace("", GetAttributeValue(attributeIndex));
                    }
                }
            }
        }

        /// <summary>
        /// Reads the next node from the stream.
        /// </summary>
        /// <remarks>
        /// If the encoding doesn’t exist on the system, an exception is thrown on the next Read.
        /// <para> Note: the following statement is currently unsupported:
        /// If the node read is a META node containing a charset encoding, the encoding changes.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If an error occurs while parsing.</exception>
        /// <returns><c>true</c> if the next node was read successfully; <c>false</c> if there are
        /// no more nodes to read.</returns>
        internal bool Read()
        {
            if (m_readState == ReadState.Closed || m_readState == ReadState.EndOfFile) return false;
            if (m_readState == ReadState.Error) throw new InvalidOperationException(Resources.ReadError);

            // A call to Read is a command to read to the next node.  Get the next node from the HtmlNodeParser.
            if( m_htmlNodeParser.GetNextNode() )
            {
                // If the node is an element we need to check a few more things.
                if (m_htmlNodeParser.NodeType == HtmlNodeType.Element)
                {
                    // If it is an <html> tag, get namespaces from it.
                    if (0 == String.Compare("html", Name, StringComparison.OrdinalIgnoreCase)) // non-case sensitive
                    {
                        // PushScope so the </html> can PopScope and remove the xmlns added by this <html> tag.
                        m_xmlNamespaceManager.PushScope();
                        // add the current element's xmlns (if any) to m_XmlNamespaceManager
                        if (HasAttributes) AddXmlns();
                    }
                    // Issue: the following is currently unsupported (this is reflected in the docs for this
                    // method).
                    // If it is a meta tag, we need to check it for a charset and change encoding if needed,
                    // for the next node read.  Finish reading this node and change the StreamReader to
                    // a new StreamReader object with the new encoding.
                }
                else if (m_htmlNodeParser.NodeType == HtmlNodeType.EndElement)
                {
                    // If it is an </html> tag, remove namespaces previously added.
                    if (0 == String.Compare("html", Name, StringComparison.OrdinalIgnoreCase)) // non-case sensitive
                    {
                        // PopScope to remove xmlns added by <html> tag previously.
                        m_xmlNamespaceManager.PopScope();
                    }
                }
            }
            else
            {
                // we've reached the end of the file.
                m_readState = ReadState.EndOfFile;
                return false;
            }
            m_readState = ReadState.Interactive;
            return true;
        }
        #endregion // Methods
    }
}

