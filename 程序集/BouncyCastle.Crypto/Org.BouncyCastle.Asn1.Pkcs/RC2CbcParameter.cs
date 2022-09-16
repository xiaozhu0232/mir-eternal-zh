using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class RC2CbcParameter : Asn1Encodable
{
	internal DerInteger version;

	internal Asn1OctetString iv;

	public BigInteger RC2ParameterVersion
	{
		get
		{
			if (version != null)
			{
				return version.Value;
			}
			return null;
		}
	}

	public static RC2CbcParameter GetInstance(object obj)
	{
		if (obj is Asn1Sequence)
		{
			return new RC2CbcParameter((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public RC2CbcParameter(byte[] iv)
	{
		this.iv = new DerOctetString(iv);
	}

	public RC2CbcParameter(int parameterVersion, byte[] iv)
	{
		version = new DerInteger(parameterVersion);
		this.iv = new DerOctetString(iv);
	}

	private RC2CbcParameter(Asn1Sequence seq)
	{
		if (seq.Count == 1)
		{
			iv = (Asn1OctetString)seq[0];
			return;
		}
		version = (DerInteger)seq[0];
		iv = (Asn1OctetString)seq[1];
	}

	public byte[] GetIV()
	{
		return Arrays.Clone(iv.GetOctets());
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(version);
		asn1EncodableVector.Add(iv);
		return new DerSequence(asn1EncodableVector);
	}
}
