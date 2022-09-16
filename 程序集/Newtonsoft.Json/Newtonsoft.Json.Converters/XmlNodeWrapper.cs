using System.Collections.Generic;
using System.Xml;

namespace Newtonsoft.Json.Converters;

internal class XmlNodeWrapper : IXmlNode
{
	private readonly XmlNode _node;

	private List<IXmlNode>? _childNodes;

	private List<IXmlNode>? _attributes;

	public object? WrappedNode => _node;

	public XmlNodeType NodeType => _node.NodeType;

	public virtual string? LocalName => _node.LocalName;

	public List<IXmlNode> ChildNodes
	{
		get
		{
			if (_childNodes == null)
			{
				if (!_node.HasChildNodes)
				{
					_childNodes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_childNodes = new List<IXmlNode>(_node.ChildNodes.Count);
					foreach (XmlNode childNode in _node.ChildNodes)
					{
						_childNodes!.Add(WrapNode(childNode));
					}
				}
			}
			return _childNodes;
		}
	}

	protected virtual bool HasChildNodes => _node.HasChildNodes;

	public List<IXmlNode> Attributes
	{
		get
		{
			if (_attributes == null)
			{
				if (!HasAttributes)
				{
					_attributes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_attributes = new List<IXmlNode>(_node.Attributes.Count);
					foreach (XmlAttribute attribute in _node.Attributes)
					{
						_attributes!.Add(WrapNode(attribute));
					}
				}
			}
			return _attributes;
		}
	}

	private bool HasAttributes
	{
		get
		{
			if (_node is XmlElement xmlElement)
			{
				return xmlElement.HasAttributes;
			}
			XmlAttributeCollection attributes = _node.Attributes;
			if (attributes == null)
			{
				return false;
			}
			return attributes.Count > 0;
		}
	}

	public IXmlNode? ParentNode
	{
		get
		{
			XmlNode xmlNode = ((_node is XmlAttribute xmlAttribute) ? xmlAttribute.OwnerElement : _node.ParentNode);
			if (xmlNode == null)
			{
				return null;
			}
			return WrapNode(xmlNode);
		}
	}

	public string? Value
	{
		get
		{
			return _node.Value;
		}
		set
		{
			_node.Value = value;
		}
	}

	public string? NamespaceUri => _node.NamespaceURI;

	public XmlNodeWrapper(XmlNode node)
	{
		_node = node;
	}

	internal static IXmlNode WrapNode(XmlNode node)
	{
		return node.NodeType switch
		{
			XmlNodeType.Element => new XmlElementWrapper((XmlElement)node), 
			XmlNodeType.XmlDeclaration => new XmlDeclarationWrapper((XmlDeclaration)node), 
			XmlNodeType.DocumentType => new XmlDocumentTypeWrapper((XmlDocumentType)node), 
			_ => new XmlNodeWrapper(node), 
		};
	}

	public IXmlNode AppendChild(IXmlNode newChild)
	{
		XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)newChild;
		_node.AppendChild(xmlNodeWrapper._node);
		_childNodes = null;
		_attributes = null;
		return newChild;
	}
}
