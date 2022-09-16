using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class X509ExtensionsGenerator
{
	private IDictionary extensions = Platform.CreateHashtable();

	private IList extOrdering = Platform.CreateArrayList();

	public bool IsEmpty => extOrdering.Count < 1;

	public void Reset()
	{
		extensions = Platform.CreateHashtable();
		extOrdering = Platform.CreateArrayList();
	}

	public void AddExtension(DerObjectIdentifier oid, bool critical, Asn1Encodable extValue)
	{
		byte[] derEncoded;
		try
		{
			derEncoded = extValue.GetDerEncoded();
		}
		catch (Exception ex)
		{
			throw new ArgumentException("error encoding value: " + ex);
		}
		AddExtension(oid, critical, derEncoded);
	}

	public void AddExtension(DerObjectIdentifier oid, bool critical, byte[] extValue)
	{
		if (extensions.Contains(oid))
		{
			throw new ArgumentException(string.Concat("extension ", oid, " already added"));
		}
		extOrdering.Add(oid);
		extensions.Add(oid, new X509Extension(critical, new DerOctetString(extValue)));
	}

	public X509Extensions Generate()
	{
		return new X509Extensions(extOrdering, extensions);
	}

	internal void AddExtension(DerObjectIdentifier oid, X509Extension x509Extension)
	{
		if (extensions.Contains(oid))
		{
			throw new ArgumentException(string.Concat("extension ", oid, " already added"));
		}
		extOrdering.Add(oid);
		extensions.Add(oid, x509Extension);
	}
}
