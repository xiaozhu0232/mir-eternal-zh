using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class AttCertIssuer : Asn1Encodable, IAsn1Choice
{
	internal readonly Asn1Encodable obj;

	internal readonly Asn1Object choiceObj;

	public Asn1Encodable Issuer => obj;

	public static AttCertIssuer GetInstance(object obj)
	{
		if (obj is AttCertIssuer)
		{
			return (AttCertIssuer)obj;
		}
		if (obj is V2Form)
		{
			return new AttCertIssuer(V2Form.GetInstance(obj));
		}
		if (obj is GeneralNames)
		{
			return new AttCertIssuer((GeneralNames)obj);
		}
		if (obj is Asn1TaggedObject)
		{
			return new AttCertIssuer(V2Form.GetInstance((Asn1TaggedObject)obj, explicitly: false));
		}
		if (obj is Asn1Sequence)
		{
			return new AttCertIssuer(GeneralNames.GetInstance(obj));
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public static AttCertIssuer GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(obj.GetObject());
	}

	public AttCertIssuer(GeneralNames names)
	{
		obj = names;
		choiceObj = obj.ToAsn1Object();
	}

	public AttCertIssuer(V2Form v2Form)
	{
		obj = v2Form;
		choiceObj = new DerTaggedObject(explicitly: false, 0, obj);
	}

	public override Asn1Object ToAsn1Object()
	{
		return choiceObj;
	}
}
