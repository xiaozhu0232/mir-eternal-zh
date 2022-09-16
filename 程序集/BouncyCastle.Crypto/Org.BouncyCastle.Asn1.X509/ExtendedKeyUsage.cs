using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class ExtendedKeyUsage : Asn1Encodable
{
	internal readonly IDictionary usageTable = Platform.CreateHashtable();

	internal readonly Asn1Sequence seq;

	public int Count => usageTable.Count;

	public static ExtendedKeyUsage GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static ExtendedKeyUsage GetInstance(object obj)
	{
		if (obj is ExtendedKeyUsage)
		{
			return (ExtendedKeyUsage)obj;
		}
		if (obj is X509Extension)
		{
			return GetInstance(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		if (obj == null)
		{
			return null;
		}
		return new ExtendedKeyUsage(Asn1Sequence.GetInstance(obj));
	}

	public static ExtendedKeyUsage FromExtensions(X509Extensions extensions)
	{
		return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, X509Extensions.ExtendedKeyUsage));
	}

	private ExtendedKeyUsage(Asn1Sequence seq)
	{
		this.seq = seq;
		foreach (Asn1Encodable item in seq)
		{
			DerObjectIdentifier instance = DerObjectIdentifier.GetInstance(item);
			usageTable[instance] = instance;
		}
	}

	public ExtendedKeyUsage(params KeyPurposeID[] usages)
	{
		seq = new DerSequence(usages);
		foreach (KeyPurposeID keyPurposeID in usages)
		{
			usageTable[keyPurposeID] = keyPurposeID;
		}
	}

	[Obsolete]
	public ExtendedKeyUsage(ArrayList usages)
		: this((IEnumerable)usages)
	{
	}

	public ExtendedKeyUsage(IEnumerable usages)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (object usage in usages)
		{
			DerObjectIdentifier instance = DerObjectIdentifier.GetInstance(usage);
			asn1EncodableVector.Add(instance);
			usageTable[instance] = instance;
		}
		seq = new DerSequence(asn1EncodableVector);
	}

	public bool HasKeyPurposeId(KeyPurposeID keyPurposeId)
	{
		return usageTable.Contains(keyPurposeId);
	}

	[Obsolete("Use 'GetAllUsages'")]
	public ArrayList GetUsages()
	{
		return new ArrayList(usageTable.Values);
	}

	public IList GetAllUsages()
	{
		return Platform.CreateArrayList(usageTable.Values);
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}
}
