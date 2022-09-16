using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class AccessDescription : Asn1Encodable
{
	public static readonly DerObjectIdentifier IdADCAIssuers = new DerObjectIdentifier("1.3.6.1.5.5.7.48.2");

	public static readonly DerObjectIdentifier IdADOcsp = new DerObjectIdentifier("1.3.6.1.5.5.7.48.1");

	private readonly DerObjectIdentifier accessMethod;

	private readonly GeneralName accessLocation;

	public DerObjectIdentifier AccessMethod => accessMethod;

	public GeneralName AccessLocation => accessLocation;

	public static AccessDescription GetInstance(object obj)
	{
		if (obj is AccessDescription)
		{
			return (AccessDescription)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new AccessDescription((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private AccessDescription(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("wrong number of elements in sequence");
		}
		accessMethod = DerObjectIdentifier.GetInstance(seq[0]);
		accessLocation = GeneralName.GetInstance(seq[1]);
	}

	public AccessDescription(DerObjectIdentifier oid, GeneralName location)
	{
		accessMethod = oid;
		accessLocation = location;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(accessMethod, accessLocation);
	}

	public override string ToString()
	{
		return "AccessDescription: Oid(" + accessMethod.Id + ")";
	}
}
