using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class DisplayText : Asn1Encodable, IAsn1Choice
{
	public const int ContentTypeIA5String = 0;

	public const int ContentTypeBmpString = 1;

	public const int ContentTypeUtf8String = 2;

	public const int ContentTypeVisibleString = 3;

	public const int DisplayTextMaximumSize = 200;

	internal readonly int contentType;

	internal readonly IAsn1String contents;

	public DisplayText(int type, string text)
	{
		if (text.Length > 200)
		{
			text = text.Substring(0, 200);
		}
		contentType = type;
		switch (type)
		{
		case 0:
			contents = new DerIA5String(text);
			break;
		case 2:
			contents = new DerUtf8String(text);
			break;
		case 3:
			contents = new DerVisibleString(text);
			break;
		case 1:
			contents = new DerBmpString(text);
			break;
		default:
			contents = new DerUtf8String(text);
			break;
		}
	}

	public DisplayText(string text)
	{
		if (text.Length > 200)
		{
			text = text.Substring(0, 200);
		}
		contentType = 2;
		contents = new DerUtf8String(text);
	}

	public DisplayText(IAsn1String contents)
	{
		this.contents = contents;
	}

	public static DisplayText GetInstance(object obj)
	{
		if (obj is IAsn1String)
		{
			return new DisplayText((IAsn1String)obj);
		}
		if (obj is DisplayText)
		{
			return (DisplayText)obj;
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		return (Asn1Object)contents;
	}

	public string GetString()
	{
		return contents.GetString();
	}
}
