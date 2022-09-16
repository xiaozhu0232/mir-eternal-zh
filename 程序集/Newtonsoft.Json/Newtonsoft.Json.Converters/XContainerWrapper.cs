using System.Collections.Generic;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XContainerWrapper : XObjectWrapper
{
	private List<IXmlNode>? _childNodes;

	private XContainer Container => (XContainer)base.WrappedNode;

	public override List<IXmlNode> ChildNodes
	{
		get
		{
			if (_childNodes == null)
			{
				if (!HasChildNodes)
				{
					_childNodes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_childNodes = new List<IXmlNode>();
					foreach (XNode item in Container.Nodes())
					{
						_childNodes!.Add(WrapNode(item));
					}
				}
			}
			return _childNodes;
		}
	}

	protected virtual bool HasChildNodes => Container.LastNode != null;

	public override IXmlNode? ParentNode
	{
		get
		{
			if (Container.Parent == null)
			{
				return null;
			}
			return WrapNode(Container.Parent);
		}
	}

	public XContainerWrapper(XContainer container)
		: base(container)
	{
	}

	internal static IXmlNode WrapNode(XObject node)
	{
		if (node is XDocument document)
		{
			return new XDocumentWrapper(document);
		}
		if (node is XElement element)
		{
			return new XElementWrapper(element);
		}
		if (node is XContainer container)
		{
			return new XContainerWrapper(container);
		}
		if (node is XProcessingInstruction processingInstruction)
		{
			return new XProcessingInstructionWrapper(processingInstruction);
		}
		if (node is XText text)
		{
			return new XTextWrapper(text);
		}
		if (node is XComment text2)
		{
			return new XCommentWrapper(text2);
		}
		if (node is XAttribute attribute)
		{
			return new XAttributeWrapper(attribute);
		}
		if (node is XDocumentType documentType)
		{
			return new XDocumentTypeWrapper(documentType);
		}
		return new XObjectWrapper(node);
	}

	public override IXmlNode AppendChild(IXmlNode newChild)
	{
		Container.Add(newChild.WrappedNode);
		_childNodes = null;
		return newChild;
	}
}
