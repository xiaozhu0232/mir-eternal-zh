using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

internal class XElementWrapper : XContainerWrapper, IXmlElement, IXmlNode
{
	private List<IXmlNode>? _attributes;

	private XElement Element => (XElement)base.WrappedNode;

	public override List<IXmlNode> Attributes
	{
		get
		{
			if (_attributes == null)
			{
				if (!Element.HasAttributes && !HasImplicitNamespaceAttribute(NamespaceUri))
				{
					_attributes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_attributes = new List<IXmlNode>();
					foreach (XAttribute item in Element.Attributes())
					{
						_attributes!.Add(new XAttributeWrapper(item));
					}
					string namespaceUri = NamespaceUri;
					if (HasImplicitNamespaceAttribute(namespaceUri))
					{
						_attributes!.Insert(0, new XAttributeWrapper(new XAttribute("xmlns", namespaceUri)));
					}
				}
			}
			return _attributes;
		}
	}

	public override string? Value
	{
		get
		{
			return Element.Value;
		}
		set
		{
			Element.Value = value;
		}
	}

	public override string? LocalName => Element.Name.LocalName;

	public override string? NamespaceUri => Element.Name.NamespaceName;

	public bool IsEmpty => Element.IsEmpty;

	public XElementWrapper(XElement element)
		: base(element)
	{
	}

	public void SetAttributeNode(IXmlNode attribute)
	{
		XObjectWrapper xObjectWrapper = (XObjectWrapper)attribute;
		Element.Add(xObjectWrapper.WrappedNode);
		_attributes = null;
	}

	private bool HasImplicitNamespaceAttribute(string namespaceUri)
	{
		if (!StringUtils.IsNullOrEmpty(namespaceUri) && namespaceUri != ParentNode?.NamespaceUri && StringUtils.IsNullOrEmpty(GetPrefixOfNamespace(namespaceUri)))
		{
			bool flag = false;
			if (Element.HasAttributes)
			{
				foreach (XAttribute item in Element.Attributes())
				{
					if (item.Name.LocalName == "xmlns" && StringUtils.IsNullOrEmpty(item.Name.NamespaceName) && item.Value == namespaceUri)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public override IXmlNode AppendChild(IXmlNode newChild)
	{
		IXmlNode result = base.AppendChild(newChild);
		_attributes = null;
		return result;
	}

	public string GetPrefixOfNamespace(string namespaceUri)
	{
		return Element.GetPrefixOfNamespace(namespaceUri);
	}
}
