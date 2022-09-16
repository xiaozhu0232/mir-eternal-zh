using System.Collections.Generic;
using System.Xml;

namespace Newtonsoft.Json.Converters;

internal interface IXmlNode
{
	XmlNodeType NodeType { get; }

	string? LocalName { get; }

	List<IXmlNode> ChildNodes { get; }

	List<IXmlNode> Attributes { get; }

	IXmlNode? ParentNode { get; }

	string? Value { get; set; }

	string? NamespaceUri { get; }

	object? WrappedNode { get; }

	IXmlNode AppendChild(IXmlNode newChild);
}
