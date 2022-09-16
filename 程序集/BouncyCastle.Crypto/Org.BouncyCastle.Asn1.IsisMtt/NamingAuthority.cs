using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509;

public class NamingAuthority : Asn1Encodable
{
	public static readonly DerObjectIdentifier IdIsisMttATNamingAuthoritiesRechtWirtschaftSteuern = new DerObjectIdentifier(string.Concat(IsisMttObjectIdentifiers.IdIsisMttATNamingAuthorities, ".1"));

	private readonly DerObjectIdentifier namingAuthorityID;

	private readonly string namingAuthorityUrl;

	private readonly DirectoryString namingAuthorityText;

	public virtual DerObjectIdentifier NamingAuthorityID => namingAuthorityID;

	public virtual DirectoryString NamingAuthorityText => namingAuthorityText;

	public virtual string NamingAuthorityUrl => namingAuthorityUrl;

	public static NamingAuthority GetInstance(object obj)
	{
		if (obj == null || obj is NamingAuthority)
		{
			return (NamingAuthority)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new NamingAuthority((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public static NamingAuthority GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	private NamingAuthority(Asn1Sequence seq)
	{
		if (seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		IEnumerator enumerator = seq.GetEnumerator();
		if (enumerator.MoveNext())
		{
			Asn1Encodable asn1Encodable = (Asn1Encodable)enumerator.Current;
			if (asn1Encodable is DerObjectIdentifier)
			{
				namingAuthorityID = (DerObjectIdentifier)asn1Encodable;
			}
			else if (asn1Encodable is DerIA5String)
			{
				namingAuthorityUrl = DerIA5String.GetInstance(asn1Encodable).GetString();
			}
			else
			{
				if (!(asn1Encodable is IAsn1String))
				{
					throw new ArgumentException("Bad object encountered: " + Platform.GetTypeName(asn1Encodable));
				}
				namingAuthorityText = DirectoryString.GetInstance(asn1Encodable);
			}
		}
		if (enumerator.MoveNext())
		{
			Asn1Encodable asn1Encodable2 = (Asn1Encodable)enumerator.Current;
			if (asn1Encodable2 is DerIA5String)
			{
				namingAuthorityUrl = DerIA5String.GetInstance(asn1Encodable2).GetString();
			}
			else
			{
				if (!(asn1Encodable2 is IAsn1String))
				{
					throw new ArgumentException("Bad object encountered: " + Platform.GetTypeName(asn1Encodable2));
				}
				namingAuthorityText = DirectoryString.GetInstance(asn1Encodable2);
			}
		}
		if (enumerator.MoveNext())
		{
			Asn1Encodable asn1Encodable3 = (Asn1Encodable)enumerator.Current;
			if (!(asn1Encodable3 is IAsn1String))
			{
				throw new ArgumentException("Bad object encountered: " + Platform.GetTypeName(asn1Encodable3));
			}
			namingAuthorityText = DirectoryString.GetInstance(asn1Encodable3);
		}
	}

	public NamingAuthority(DerObjectIdentifier namingAuthorityID, string namingAuthorityUrl, DirectoryString namingAuthorityText)
	{
		this.namingAuthorityID = namingAuthorityID;
		this.namingAuthorityUrl = namingAuthorityUrl;
		this.namingAuthorityText = namingAuthorityText;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(namingAuthorityID);
		if (namingAuthorityUrl != null)
		{
			asn1EncodableVector.Add(new DerIA5String(namingAuthorityUrl, validate: true));
		}
		asn1EncodableVector.AddOptional(namingAuthorityText);
		return new DerSequence(asn1EncodableVector);
	}
}
