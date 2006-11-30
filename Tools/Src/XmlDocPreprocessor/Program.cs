/* Copyright (c) Microsoft Corporation. All rights reserved. */

﻿// Program.cs
//
// Implements XmlDocProcessor.exe, which does some simple transformations
// of XML documentation (extracted from C# comments using csc.exe).
// See Readme.txt for more information.
//
// Tip: Search for "Processing the XML File" (including quotes) in MSDN
// for information about the format of the input XML file read by this tool.
//
#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

#endregion

class Program
{
	/// <summary>
	/// The path of the input XML documentation file currently being processed.
	/// </summary>
	string m_inputPath;

	/// <summary>
	/// The input XML documentation file currently being processed.
	/// </summary>
	XmlTextReader m_xmlReader;

	/// <summary>
	/// The output XML documentation currently being written to.
	/// </summary>
	XmlTextWriter m_xmlWriter;

	/// <summary>
	/// The "name" attribute of the "&lt;member&gt;" node currently being
	/// processed (representing a type (class, interface, struct, enum or
	/// delegate), field, property, method, or event) e.g.
	/// "T:Microsoft.Foo.Bar" (a class) or
	/// "M:Microsoft.Foo.Bar.op_Implicit(Microsoft.Foo.Bar)~System.String"
	/// (an operator method) or "M:Microsoft.Foo.Bar.#ctor(System.String)"
	/// (a constructor).
	/// </summary>
	string m_currentMemberDescriptor;

	/// <summary>
	/// The path portion of the <m_currentMemberDescriptor>, e.g.
	/// e.g. "Microsoft.Foo.Bar" or "Microsoft.Foo.Bar.op_Implicit" or
	/// "Microsoft.Foo.Bar.#ctor".  Note that the member path may not uniquely
	/// identify the member; for example, methods can have multipe overloads.
	/// Only <c>m_currentMemberDescriptor</c> can uniquely identify a member.
	/// </summary>
	string m_currentMemberPath;

	/// <summary>
	/// The type (class, interface, struct, enum, or delegate) currently being
	/// processed.  For example, if we're currently processing the method
	/// with descriptor "M:Microsoft.Foo.Bar.#ctor(System.String)",
	/// <c>m_currentType</c> will be "Microsoft.Abc.Def".  If we're currently
	/// processing "T:Microsoft.Foo.Bar" (a class), <c>m_currentType<c> will
	/// be "Microsoft.Foo.Bar".
	/// </summary>
	string m_currentType;

	/// <summary>
	/// The namespace currently being processed.  For example, if we're
	/// currently processing class "Microsoft.Foo.Bar" or one of its members,
	/// <c>m_currentNamespace</c> will be "Microsoft.Foo".  Note that for
	/// a nested class, <c>m_currentNamespace</c> will include the
	/// parent/ancestor class name(s).
	/// </summary>
	string m_currentNamespace;

	/// <summary>
	/// Program entry point.
	/// </summary>
	///
	/// <param name="args">Command-line arguments.</param>
	///
	static void Main(string[] args)
	{
        Program program = null;
		try
		{
			program = new Program();
			program.Run(args);
		}
		catch (Exception ex)
		{
            if ((program != null) && (program.m_xmlReader != null))
            {
                IXmlLineInfo lineInfo = program.m_xmlReader as IXmlLineInfo;
                if (lineInfo != null)
                {
                    Console.Error.WriteLine("Error in {0}({1})",
						program.m_inputPath ?? "input file", lineInfo.LineNumber);
                }
            }
			Console.Error.WriteLine("Error: " + ex.Message);
			System.Environment.ExitCode = 1;
		}
	}
	
	/// <summary>
	/// Implements program.
	/// </summary>
	///
	/// <param name="args">Command-line arguments.</param>
	///
	void Run(string[] args)
	{
		// if <args> specifies a response file (e.g. "@Foo.txt") containing
		// command-line arguments, process it
		if ((args.Length == 1) && (args[0][0] == '@'))
		{
			string responseFileName = args[0].Substring(1);
			string argString;
			using (StreamReader reader = new StreamReader(responseFileName))
				argString = reader.ReadToEnd();
            Run(argString.Split(new char[] { '\r', '\n', '\t', ' ' },
				StringSplitOptions.RemoveEmptyEntries));
			return;
		}

		// parse command line arguments <args>; set <outputDirPath> to the
		// output directory argument (or null if none); call ProcessFile()
		// for subsequent arguments
		string outputDirPath = null;
		foreach (string arg in args)
		{
			if (outputDirPath == null)
			{
				outputDirPath = arg;
				if (!Directory.Exists(outputDirPath))
					throw Error("Directory doesn't exist: {0}", outputDirPath);
			}
			else
			{
				foreach (string filePath in Directory.GetFiles(
					Path.GetDirectoryName(arg), Path.GetFileName(arg)))
				{
					if (!File.Exists(filePath))
						throw Error("File doesn't exist: {0}", filePath);
					Console.WriteLine("Processing: " + filePath);
					ProcessFile(filePath, Path.Combine(outputDirPath,
						Path.GetFileName(filePath)));
					m_xmlReader = null; // clear state
					m_xmlWriter = null; // clear state
				}
			}
		}

		// if no command-line arguments were provided, give simple usage
		// information to the user
		if (outputDirPath == null)
		{
			Console.Error.WriteLine(
				"Usage 1: XmlDocPreprocessor <output-dir> <input-xml-file>...");
			Console.Error.WriteLine(
				"Usage 2: XmlDocPreprocessor @<response-file>");
		}

		// we're done -- give user feedback
		Console.WriteLine("Done.");
	}

	/// <summary>
	/// Processes an input XML documentation file (generated by Visual Studio),
	/// and write the result to an output file.
	/// </summary>
	///
	/// <param name="inputPath">Path of input file.</param>
	/// 
	/// <param name="outputPath">Path of output file.</param>
	///
	void ProcessFile(string inputPath, string outputPath)
	{
		using (Disposer disposer = new Disposer()) {

		// reset state
		ResetMemberState();

		// save <inputPath> (for error messages)
		m_inputPath = inputPath;

		// set <streamReader> to read file <inputPath>
		StreamReader streamReader = new StreamReader(inputPath);
		disposer.Push(streamReader);

		// set <m_xmlReader> to parse XML from <streamReader>
        //m_xmlReader = XmlReader.Create(streamReader);
		m_xmlReader = new XmlTextReader(streamReader);
		//disposer.Push(m_xmlReader);

		// set <m_xmlWriter> to generate output XML, written to <outputPath>
		m_xmlWriter = new XmlTextWriter(outputPath,
			new NoPreambleUTF8Encoding());
		disposer.Push(m_xmlWriter);

		// loop once for each input XML node
        try
        {
            while (m_xmlReader.Read())
            {
                // process the input XML node
                switch (m_xmlReader.NodeType)
                {
                    case XmlNodeType.Attribute:
                        throw Error("Not implemented: XmlNodeType.Attribute");
                    case XmlNodeType.CDATA:
                        m_xmlWriter.WriteCData(m_xmlReader.Value);
                        break;
                    case XmlNodeType.Comment:
                        m_xmlWriter.WriteComment(m_xmlReader.Value);
                        break;
                    case XmlNodeType.Document:
                        throw Error("Not implemented: XmlNodeType.Document");
                    case XmlNodeType.DocumentFragment:
                        throw Error("Not implemented: XmlNodeType.DocumentFragment");
                    case XmlNodeType.DocumentType:
                        throw Error("Not implemented: XmlNodeType.DocumentType");
                    case XmlNodeType.Element:
                        if (!ProcessXmlElement())
                        {
                            m_xmlWriter.WriteStartElement(m_xmlReader.Name);
                            m_xmlWriter.WriteAttributes(m_xmlReader, false);
                            if (m_xmlReader.IsEmptyElement)
                                m_xmlWriter.WriteEndElement();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (!ProcessXmlEndElement())
                            m_xmlWriter.WriteFullEndElement();
                        break;
                    case XmlNodeType.EndEntity:
                        throw Error("Not implemented: XmlNodeType.EndEntity");
                    case XmlNodeType.Entity:
                        throw Error("Not implemented: XmlNodeType.Entity");
                    case XmlNodeType.EntityReference:
                        m_xmlWriter.WriteEntityRef(m_xmlReader.Value);
                        break;
                    case XmlNodeType.None:
                        throw Error("Not implemented: XmlNodeType.None");
                    case XmlNodeType.Notation:
                        throw Error("Not implemented: XmlNodeType.Notation");
                    case XmlNodeType.ProcessingInstruction:
                        m_xmlWriter.WriteProcessingInstruction(m_xmlReader.Name,
                            m_xmlReader.Value);
                        break;
                    case XmlNodeType.SignificantWhitespace:
                        m_xmlWriter.WriteWhitespace(m_xmlReader.Value);
                        break;
                    case XmlNodeType.Text:
                        m_xmlWriter.WriteString(m_xmlReader.Value);
                        break;
                    case XmlNodeType.Whitespace:
                        m_xmlWriter.WriteWhitespace(m_xmlReader.Value);
                        break;
                    case XmlNodeType.XmlDeclaration:
                        m_xmlWriter.WriteProcessingInstruction("xml",
                            m_xmlReader.Value);
                        break;
                    default:
                        throw Error("Not implemented: XmlNodeType.{0}",
                            m_xmlReader.NodeType);
                }
            }
        }
        catch (XmlException ex)
        {
            throw new Exception(String.Format("{0}(1): {2}",
                inputPath, ((IXmlLineInfo)m_xmlReader).LineNumber, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new Exception(String.Format("{0}(1): {2}",
                inputPath, ((IXmlLineInfo)m_xmlReader).LineNumber, ex.Message));
        }
	} }

	/// <summary>
	/// Processes an input XML element.  The current node of <c>m_xmlReader</c>
	/// is the input XML element (i.e. XmlNodeType.Element) to process.
	/// The transformed version of the input element, if any, is written to
	/// <c>m_xmlWriter</c>.</param>
	///
	/// <returns>
	/// If this method chooses to process the input XML node, true is returned.
	/// Otherwise, false is returned, in which case it's the caller's
	/// responsibility to process the input XML node.
	/// </returns>
	///
	bool ProcessXmlElement()
	{
		string nodeName = m_xmlReader.Name;
		switch (nodeName)
		{

		case "code":

			// remove common leading white space from each line of text
			// within "<code>...</code>" so that the text isn't indented
			m_xmlWriter.WriteStartElement("code");
			m_xmlWriter.WriteAttributes(m_xmlReader, false);
            string value = MyReadString(m_xmlReader);
			m_xmlWriter.WriteString(Unindent(value));
			m_xmlWriter.WriteEndElement();
			return true;

		case "member":

			// reset global state tracking which "<member>" we're in
			ResetMemberState();

			// set <currentMemberDescriptor> to the member descriptor, e.g.
			// "T:Microsoft.Abc.Def".
			string currentMemberDescriptor = m_xmlReader.GetAttribute("name");
			if (currentMemberDescriptor == null)
			{
				Warning("<member> missing \"name\" attribute");
				return false; // copy input element to output
			}

			// member descriptors starting with "!" are errors
			if (currentMemberDescriptor.StartsWith("!"))
			{
				Warning("<member> \"name\" attribute specified an error: {1}",
					currentMemberDescriptor);
				return false; // copy input element to output
			}

			// Parse <currentMemberDescription> into <currentMemberPath>
			// (the path portion), <currentType> (the path of the class,
			// interface, struct, enum, or delegate), and namespace.
			//
			// Example 1:
			// <currentMemberDescription> (a class):
			//   T:MS.Foo.Bar
			// <currentMemberPath>:
			//   MS.Foo.Bar
			// <currentType>:
			//   MS.Foo.Bar
			// <currentNamespace>:
			//   MS.Foo
			//
			// Example 2:
			// <currentMemberDescription> (an operator):
			//   M:MS.Foo.Bar.op_Implicit(MS.Foo.Bar)~System.String
			// <currentMemberPath>:
			// <currentMemberPath>:
			//   MS.Foo.Bar.op_Implicit
			// <currentType>:
			//   MS.Foo.Bar
			// <currentNamespace>:
			//   MS.Foo
			//
			// Example 3:
			// <currentMemberDescription> (a constructor):
			//   M:MS.Foo.Bar.#ctor(System.String)"
			// <currentMemberPath>:
			//   MS.Foo.Bar.#ctor
			// <currentType>:
			//   MS.Foo.Bar
			// <currentNamespace>:
			//   MS.Foo
			//

			// make sure that <currentMemberDescriptor> starts with
			// "<kind-char>:"
			if ((currentMemberDescriptor.Length < 3) ||
			    (currentMemberDescriptor[1] != ':'))
			{
				Warning("<member> \"name\" attribute incorrect syntax" +
					" (expecting \"<kind-char>:\" prefix): {1}",
					currentMemberDescriptor);
				return false; // copy input element to output
			}

			// parse <currentMemberPath> from <currentMemberDescriptor>
			string currentMemberPath = currentMemberDescriptor.Substring(2);
			int ich = currentMemberPath.IndexOf("(");
			if (ich >= 0)
				currentMemberPath = currentMemberPath.Substring(0, ich);

			// parse <currentType> and <currentNamespace> from
			// <currentMemberPath>
			string currentNamespace, currentType;
			string[] a = currentMemberPath.Split('.');
			if (currentMemberDescriptor.StartsWith("T:"))
			{
				// the current item is a type, i.e.
				// "<namespace-path>.<type-base-name>"
				if (a.Length <= 1)
					currentNamespace = ""; // no namespace
				else
					currentNamespace = String.Join(".", a, 0, a.Length - 1);
				currentType = currentMemberPath;
			}
			else
			{
				// the current item is presumably something scoped within
				// a type (i.e. a field, property, method, etc.), e.g.
				// "<namespace-path>.<type-base-name>.<something>"
				if (a.Length <= 1)
				{
					Warning("<member> \"name\" attribute incorrect syntax" +
						" (expecting \"<namespace-path>.<type-base-name>" +
						".<something>\"): {1}",
						currentMemberDescriptor);
					return false; // copy input element to output
				}
				else
				if (a.Length == 2)
				{
					// no namespace, i.e. "<type-base-name>.<something>"
					currentType = String.Join(".", a, 0, a.Length - 1);
					currentNamespace = "";
				}
				else
				{
					currentType = String.Join(".", a, 0, a.Length - 1);
					currentNamespace = String.Join(".", a, 0, a.Length - 2);
				}
			}

			// update global state
			m_currentMemberDescriptor = currentMemberDescriptor;
			m_currentMemberPath = currentMemberPath;
			m_currentType = currentType;
			m_currentNamespace = currentNamespace;

			// copy input element to output
			return false;

		case "Nsp": // namespace

			ProcessSee("see", "N", m_currentNamespace);
			return true; // don't copy input element to output

		case "Typ": // type: class, interface, struct, enum, delegate

			ProcessSee("see", "T", m_currentNamespace);
			return true; // don't copy input element to output

		case "Fld": // field

			ProcessSee("see", "F", m_currentType);
			return true; // don't copy input element to output

		case "Prp": // property (including indexers or other indexed properties)

			ProcessSee("see", "P", m_currentType);
			return true; // don't copy input element to output

		case "Mth": // method (including such special methods as constructors,
			        // operators, and so forth)

			ProcessSee("see", "M", m_currentType);
			return true; // don't copy input element to output

		case "Evt": // event

			ProcessSee("see", "E", m_currentType);
			return true; // don't copy input element to output

		case "SeeAlsoNsp": // namespace

			ProcessSee("seealso", "N", m_currentNamespace);
			return true; // don't copy input element to output

		case "SeeAlsoTyp": // type: class, interface, struct, enum, delegate

			ProcessSee("seealso", "T", m_currentNamespace);
			return true; // don't copy input element to output

		case "SeeAlsoFld": // field

			ProcessSee("seealso", "F", m_currentType);
			return true; // don't copy input element to output

		case "SeeAlsoPrp": // property (including indexers or other indexed
					// properties)

			ProcessSee("seealso", "P", m_currentType);
			return true; // don't copy input element to output

		case "SeeAlsoMth": // method (including such special methods as
					// constructors, operators, and so forth)

			ProcessSee("seealso", "M", m_currentType);
			return true; // don't copy input element to output

		case "SeeAlsoEvt": // event

			ProcessSee("seealso", "E", m_currentType);
			return true; // don't copy input element to output

		case "P": // parameter

			// write a corresponding "<paramref>" element
			string paramName = MyReadString(m_xmlReader);
			m_xmlWriter.WriteStartElement("paramref");
			m_xmlWriter.WriteAttributeString("name", paramName);
			m_xmlWriter.WriteEndElement();
			return true; // don't copy input element to output

		case "Img": // image

			// write corresponding "<img>" element and related HTML markup
			string imgFileName = MyReadString(m_xmlReader);
			m_xmlWriter.WriteStartElement("p");
			m_xmlWriter.WriteStartElement("img");
			m_xmlWriter.WriteAttributeString("src", imgFileName);
			m_xmlWriter.WriteEndElement();
			m_xmlWriter.WriteEndElement();
			return true; // don't copy input element to output

		default:

			// copy input element to output
			return false;
		}
	}

	/// <summary>
	/// Processes an input element that generates a "&lt;see&gt;" or
	/// "&lt;seealso&gt;" output element.  Reads from <c>m_xmlReader</c>;
	/// writes to <c>m_xmlWriter</c>.
	/// </summary>
	///
	/// <param name="outputNodeName">The name of the outut element, e.g.
	/// 	"see".</param>
	///
	/// <param name="itemKind">The code used to refer to this kind of item
	/// 	in a "cref" attribute, e.g. "M" for method.</param>
	///
	/// <param name="relativeTo">The path of the namespace or type that
	/// 	this item is relative to, e.g. "Microsoft.Foo.Bar".</param>
	///
	void ProcessSee(string outputNodeName, string itemKind, string relativeTo)
	{
		// set <qualifiedItemName> to the fully-qualified item name (e.g.
		// method name) required to create a valid link to the specified item
		string value = MyReadString(m_xmlReader);
		string qualifiedItemName = CombineItemPath(relativeTo, value);

		// write a "<see>" element that references <qualifiedItemName>
		m_xmlWriter.WriteStartElement(outputNodeName);
		m_xmlWriter.WriteAttributeString("cref",
			String.Format("{0}:{1}", itemKind, qualifiedItemName));
		m_xmlWriter.WriteEndElement();
	}

	/// <summary>
	/// Like <c>XmlReader.ReadString</c>, but throws an exception if the
	/// element contains nested XML.
	/// </summary>
	///
	static string MyReadString(XmlReader xmlReader)
	{
		string nodeName = xmlReader.Name;
		int initialDepth = xmlReader.Depth;
		string text = xmlReader.ReadString();
		if (xmlReader.Depth != initialDepth)
		{
			throw new Exception(
				String.Format("<{0}> is not allowed to contain nested XML", nodeName));
		}
        return text;
	}

	/// <summary>
	/// Processes an input XML end element (e.g. "&lt;/Foo&gt;").  Reads from
	/// <c>m_xmlReader</c>; writes to <c>m_xmlWriter</c>.
	/// </summary>
	///
	/// <returns>
	/// If this method chooses to process the input XML end element, true is
	/// returned.  Otherwise, false is returned, in which case it's the
	/// caller's responsibility to process the input XML end element.
	/// </returns>
	///
	bool ProcessXmlEndElement()
	{
		switch (m_xmlReader.Name)
		{

		case "member":

			// reset global state tracking which "<member>" we're in
			ResetMemberState();

			// copy input element to output
			return false;

		default:

			// copy input element to output
			return false;

		}
	}

	/// <summary>
	/// Reset the global state that tracks which &lt;member&gt; we're in.
	/// </summary>
	///
	void ResetMemberState()
	{
		m_currentMemberDescriptor = "???"; // indicates an unknown member
		m_currentMemberPath = "???"; // indicates an unknown member
		m_currentType = "???"; // indicates an unknown type
		m_currentNamespace = "???"; // indicates an unknown namespace
	}

	/// <summary>
	/// Given an absolute item path (e.g. "Microsoft.Foo.Bar") and a
	/// relative item path (e.g. "../Abc.#ctor(System.String)"
	/// the resulting absolute item path (e.g.
	/// "Microsoft.Foo.Abc.#ctor(System.String)").
	/// </summary>
	///
	/// <param name="absoluteItemPath">The absolute path.  Should not contain
	///     "/".</param>
	///
	/// <param name="relativeItemPath">The relative path.  May begin with one
	///		or more occurrences of "../".  However, the portion of the string
	///     following the "../" sequence prefix should not contain "/".</param>
	///     Alternatively, <paramref name="relativeItemPath"/> may begin with
	///     "/", in which case the entire <paramref name="relativeItemPath"/>
	/// 	following "/" is returned and <paramref name="absoluteItemPath"/>
	/// 	is ignored.
	///
	string CombineItemPath(string absoluteItemPath, string relativeItemPath)
	{
		// if <relativeItemPath> begins with "/", treat everything following
		// the "/" as an absolute path and ignore <absoluteItemPath>
		if (relativeItemPath.StartsWith("/"))
			return relativeItemPath.Substring(1);

		// set <parentCount> to the number of "../" at the beginning of
		// <relativeItemPath>, and set <itemSuffix> to everything after that
		int parentCount = 0;
		int ich = 0;
		const string parentDelim = "../";
		while ((relativeItemPath.Length >= (ich + parentDelim.Length)) &&
		       (relativeItemPath.Substring(ich, parentDelim.Length) ==
				   parentDelim))
		{
			parentCount++;
			ich += parentDelim.Length;
		}
		string itemSuffix = relativeItemPath.Substring(ich);

		// "/" shouldn't occur in either <relativeItemPath> or <itemSuffix>
		if (absoluteItemPath.IndexOf('/') >= 0)
			Warning("unexpected \"/\" in: {0}", absoluteItemPath);
		if (itemSuffix.IndexOf('/') >= 0)
			Warning("unexpected \"/\" in: {0}", itemSuffix);

		// combine <absoluteItemPath> and <relativeItemPath>, and return the
		// result
		string[] absoluteComponents = absoluteItemPath.Split('.');
		if (absoluteComponents.Length < parentCount)
		{
			Warning("too many \"..\" in \"{0}\" (relative to \"{1}\")",
				relativeItemPath, absoluteItemPath);
			return relativeItemPath;
		}
		string leftPart = String.Join(".", absoluteComponents, 0,
			absoluteComponents.Length - parentCount);
		if (leftPart.Length > 0)
			leftPart += ".";
		return leftPart + itemSuffix;
	}

	/// <summary>
	/// Remove left indentation from a block of text.
	/// </summary>
	///
	/// <param name="text">Text to unindent.</param>
	///
	/// <returns>
	/// Returns <paramref name="text"/>, with leading indentation removed.
	/// Indentation consists of one or more space characters.  The number of
	/// leading space characters that are removed is equal to the number of
	/// leading space characters on the first line of <paramref name="text"/>
	/// that's doesn't contain only blank characters.
	/// </returns>
	string Unindent(string text)
	{
		// processed text will be written to <output>
		StringBuilder output = new StringBuilder(text.Length);

		// loop once for each line in <text>
		int ichStartOfLine = 0; // index into the input string <text>
		int cchIndentToRemove = -1; // no. chars. indentation to remove
			// (-1 until known)
		while (true)
		{
			// set <ichEndOfLine> to the index of the end of the line that
			// begins with index <ichStartOfLine>
			int ichEndOfLine = text.IndexOf("\r\n", ichStartOfLine);
			if (ichEndOfLine < 0)
				ichEndOfLine = text.Length;

			// if we're still trying to figure out how much indentation
			// to remove from each line, see if this line will answer that
			// question (i.e. if it's not entirely blanks)
			if (cchIndentToRemove < 0)
			{
				int ich = FirstNonBlank(text, ichStartOfLine);
				if (ich < ichEndOfLine)
					cchIndentToRemove = ich - ichStartOfLine;
			}

			// special case: if there's no indentation to remove, return
			// <text> as-is
			if (cchIndentToRemove == 0)
				return text;

			// remove indentation from this line, if we know how much to
			// remove
			if (ichStartOfLine == ichEndOfLine)
			{
				// line is empty -- do nothing
			}
			else
			if (cchIndentToRemove > 0)
			{
				// remove up to <cchIndentToRemove> characters of indentation
				int cchThisLineIndent = FirstNonBlank(text, ichStartOfLine)
					- ichStartOfLine;
				int cchThisLineIndentToRemove =
					Math.Min(cchIndentToRemove, cchThisLineIndent);
				int ichCopy = ichStartOfLine + cchThisLineIndentToRemove;
				int cchCopy = ichEndOfLine - ichStartOfLine -
					cchThisLineIndentToRemove;
				output.Append(text.Substring(ichCopy, cchCopy));
			}
			else
			{
				// don't know how much indentation to remove yet, so copy
				// this line to <output> as-is
				output.Append(text.Substring(ichStartOfLine,
					ichEndOfLine - ichStartOfLine));
			}

			// copy the newline (if any), and continue on to the next line
			// (if any)
			if (ichEndOfLine == text.Length)
				break;
			else
			{
				output.Append("\r\n");
				ichStartOfLine = ichEndOfLine + 2/*"\r\n".Length*/;
			}
		}

		return output.ToString();
	}

	/// <summary>
	/// Returns the index of the first non-blank character in
	/// <paramref name="text"/>, starting at index <paramref name="ich"/>.
	/// Returns <c>text.Length</c> if <paramref name="text"/> contains only
	/// blank characters.
	/// </summary>
	///
	int FirstNonBlank(string text, int ich)
	{
		while ((ich < text.Length) && (text[ich] == ' '))
			ich++;
		return ich;
	}

	/// <summary>
	/// Formats and outputs a warning message that includes the current line
	/// number of <c>m_xmlReader</c> (if any).
	/// </summary>
	///
	/// <param name="format">String.Format format string.</param>
	/// 
	/// <param name="args">String.Format formatting arguments.</param>
	/// 
	/// <returns>
	/// Returns an exception containing the specified message.
	/// </returns>
	///
	void Warning(string format, params string[] args)
	{
		if (m_xmlReader == null)
		{
			Console.Error.WriteLine("Warning: {1}",
				String.Format(format, args));
		}
		else
		{
			Console.Error.WriteLine("Warning: {0} line {1}: {2}", m_inputPath,
				m_xmlReader.LineNumber, String.Format(format, args));
		}
	}

	/// <summary>
	/// Formats an error message and returns an exception with that message.
	/// </summary>
	///
	/// <param name="format">String.Format format string.</param>
	/// 
	/// <param name="args">String.Format formatting arguments.</param>
	/// 
	/// <returns>
	/// Returns an exception containing the specified message.
	/// </returns>
	///
	static Exception Error(string format, params object[] args)
	{
		return new Exception(String.Format(format, args));
	}
}

/// <summary>
/// Implements a stack of disposable objects.  Intended for use within a
/// "using" statement.  Implements scope-level object cleanup (as in C++):
/// </summary>
///
/// <remarks>
/// When Disposer's <c>IDisposable.Dispose</c> method is called (either
/// explicitly, or implicitly by a "using" statement), the object references
/// that were pushed onto the Disposer stack are disposed of in the reverse
/// order in which they were pushed on.
/// </remarks>
///
/// <example>
/// Example of using the Disposer object:
/// <code>
/// using (Disposer disposer = new Disposer())
/// {
///     ...
///     Font font = new Font("Arial", 10.0f);
///     disposer.Push(font);
///     ...
///     FileStream fs = new FileStream(...);
///     disposer.Push(fs);
///     ...
/// } // at this point, IDisposable.Dispose is called on &lt;fs&gt;, then &lt;font&gt;
/// </code>
/// </example>
///
internal class Disposer : IDisposable
{
	/// <summary>
	/// Stack of objects to dispose of.
	/// </summary>
	private Stack<IDisposable> m_stack = new Stack<IDisposable>();

	/// <summary>
	/// Pushes an object onto the stack of objects to dispose.
	/// </summary>
	/// 
	/// <param name="d">Object to dispose.</param>
	///
	internal void Push(IDisposable d)
    {
        // implement Push() instead of inheriting from Stack to check that
        // <d> implements IDisposable at the time Push() is called
        if (d != null)
            m_stack.Push(d);
    }

	/// <summary>
	/// Disposes of all objects on the stack.
	/// </summary>
	///
    void IDisposable.Dispose()
    {
        while (m_stack.Count > 0)
        {
            IDisposable d = (IDisposable) m_stack.Pop();
            d.Dispose();
        }
		GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A special case of <c>UTF8Encoding</c> encoder that doesn't write the
/// three-byte UTF-8 preamble.
/// </summary>
///
internal class NoPreambleUTF8Encoding : UTF8Encoding
{
	public override byte[] GetPreamble()
	{
		return new byte[] { };
	}
}

