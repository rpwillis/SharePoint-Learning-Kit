/* Copyright (c) Microsoft Corporation. All rights reserved. */

// CustomRenderer.cs
//
// Custom CodeDoc topic renderer.
//

using System;
using System.IO;
using System.Xml;
using System.Web.UI;
using DwellNet.CodeDoc;

class CustomRenderer : DefaultTopicRenderer
{
    public override TopicWriter CreateTopicWriter(Topic topic,
		TextWriter textWriter, DefaultTopicRenderer topicRenderer)
    {
        return new CustomTopicWriter(topic, textWriter, topicRenderer);
    }
}

class CustomTopicWriter : TopicWriter
{
    protected override bool TransformCustomInsideXml(XmlCommentSection section,
        XmlReader xmlReader)
    {
        string element = xmlReader.Name;
        string text;
        switch (element)
        {

		case "Nsp": // namespace
		case "Typ": // type: class, interface, struct, enum, delegate
		case "Fld": // field
		case "Prp": // property (including indexers or other indexed properties)
		case "Mth": // method (including such special methods as constructors,
		case "Evt": // event

            if ((text = xmlReader.ReadElementContentAsString()) != null)
            {
                int index = text.LastIndexOf('/');
                if (index >= 0)
                    text = text.Substring(index + 1);
                GenerateInlineReferenceHtml(section,
                    ((IXmlLineInfo) xmlReader).LineNumber,
                    text, null, false, 0);
            }
            return true;

#if true
		case "SeeAlsoNsp": // namespace
		case "SeeAlsoTyp": // type: class, interface, struct, enum, delegate
		case "SeeAlsoFld": // field
		case "SeeAlsoPrp": // property (including indexers or other indexed
						   // properties)
		case "SeeAlsoMth": // method (including such special methods as
						   // constructors, operators, and so forth)
		case "SeeAlsoEvt": // event

            if ((text = xmlReader.ReadElementContentAsString()) != null)
            {
				GenerateInlineReferenceHtml(section,
					((IXmlLineInfo) xmlReader).LineNumber,
					text, null, false, 0);
			}
			return true;
#endif

        case "P": // Parameter

            if ((text = xmlReader.ReadElementContentAsString()) != null)
            {
				GenerateInlineParameterReferenceHtml(section,
					((IXmlLineInfo) xmlReader).LineNumber, text);
            }
            return true;

        default:

            // we don't know this element
            return false;
        }
    }

	/// <summary>
	/// Initializes an instance of this class.
	/// </summary>
	///
	/// <param name="topic">The topic to render.</param>
	///
	/// <param name="textWriter">The <n>TextWriter</n> that renders the markup
	/// 	content.</param>
    ///
    /// <param name="topicRenderer">The <n>TopicRenderer</n> that created this
    ///     <r>TextWriter</r>.</param>
	///
	public CustomTopicWriter(Topic topic, TextWriter textWriter,
            DefaultTopicRenderer topicRenderer) :
		base(topic, textWriter, topicRenderer)
	{
	}
}

