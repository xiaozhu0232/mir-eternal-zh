using System;
using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509;

public class Restriction : Asn1Encodable
{
	private readonly DirectoryString restriction;

	public virtual DirectoryString RestrictionString => restriction;

	public static Restriction GetInstance(object obj)
	{
		if (obj is Restriction)
		{
			return (Restriction)obj;
		}
		if (obj is IAsn1String)
		{
			return new Restriction(DirectoryString.GetInstance(obj));
		}
		throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	private Restriction(DirectoryString restriction)
	{
		this.restriction = restriction;
	}

	public Restriction(string restriction)
	{
		this.restriction = new DirectoryString(restriction);
	}

	public override Asn1Object ToAsn1Object()
	{
		return restriction.ToAsn1Object();
	}
}
