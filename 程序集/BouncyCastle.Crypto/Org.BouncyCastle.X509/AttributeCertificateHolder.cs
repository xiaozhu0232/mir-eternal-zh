using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.X509;

public class AttributeCertificateHolder : IX509Selector, ICloneable
{
	internal readonly Holder holder;

	public int DigestedObjectType => holder.ObjectDigestInfo?.DigestedObjectType.IntValueExact ?? (-1);

	public string DigestAlgorithm => holder.ObjectDigestInfo?.DigestAlgorithm.Algorithm.Id;

	public string OtherObjectTypeID => holder.ObjectDigestInfo?.OtherObjectTypeID.Id;

	public BigInteger SerialNumber
	{
		get
		{
			if (holder.BaseCertificateID != null)
			{
				return holder.BaseCertificateID.Serial.Value;
			}
			return null;
		}
	}

	internal AttributeCertificateHolder(Asn1Sequence seq)
	{
		holder = Holder.GetInstance(seq);
	}

	public AttributeCertificateHolder(X509Name issuerName, BigInteger serialNumber)
	{
		holder = new Holder(new IssuerSerial(GenerateGeneralNames(issuerName), new DerInteger(serialNumber)));
	}

	public AttributeCertificateHolder(X509Certificate cert)
	{
		X509Name issuerX509Principal;
		try
		{
			issuerX509Principal = PrincipalUtilities.GetIssuerX509Principal(cert);
		}
		catch (Exception ex)
		{
			throw new CertificateParsingException(ex.Message);
		}
		holder = new Holder(new IssuerSerial(GenerateGeneralNames(issuerX509Principal), new DerInteger(cert.SerialNumber)));
	}

	public AttributeCertificateHolder(X509Name principal)
	{
		holder = new Holder(GenerateGeneralNames(principal));
	}

	public AttributeCertificateHolder(int digestedObjectType, string digestAlgorithm, string otherObjectTypeID, byte[] objectDigest)
	{
		holder = new Holder(new ObjectDigestInfo(digestedObjectType, otherObjectTypeID, new AlgorithmIdentifier(new DerObjectIdentifier(digestAlgorithm)), Arrays.Clone(objectDigest)));
	}

	public byte[] GetObjectDigest()
	{
		return holder.ObjectDigestInfo?.ObjectDigest.GetBytes();
	}

	private GeneralNames GenerateGeneralNames(X509Name principal)
	{
		return new GeneralNames(new GeneralName(principal));
	}

	private bool MatchesDN(X509Name subject, GeneralNames targets)
	{
		GeneralName[] names = targets.GetNames();
		for (int i = 0; i != names.Length; i++)
		{
			GeneralName generalName = names[i];
			if (generalName.TagNo != 4)
			{
				continue;
			}
			try
			{
				if (X509Name.GetInstance(generalName.Name).Equivalent(subject))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
		}
		return false;
	}

	private object[] GetNames(GeneralName[] names)
	{
		int num = 0;
		for (int i = 0; i != names.Length; i++)
		{
			if (names[i].TagNo == 4)
			{
				num++;
			}
		}
		object[] array = new object[num];
		int num2 = 0;
		for (int j = 0; j != names.Length; j++)
		{
			if (names[j].TagNo == 4)
			{
				array[num2++] = X509Name.GetInstance(names[j].Name);
			}
		}
		return array;
	}

	private X509Name[] GetPrincipals(GeneralNames names)
	{
		object[] names2 = GetNames(names.GetNames());
		int num = 0;
		for (int i = 0; i != names2.Length; i++)
		{
			if (names2[i] is X509Name)
			{
				num++;
			}
		}
		X509Name[] array = new X509Name[num];
		int num2 = 0;
		for (int j = 0; j != names2.Length; j++)
		{
			if (names2[j] is X509Name)
			{
				array[num2++] = (X509Name)names2[j];
			}
		}
		return array;
	}

	public X509Name[] GetEntityNames()
	{
		if (holder.EntityName != null)
		{
			return GetPrincipals(holder.EntityName);
		}
		return null;
	}

	public X509Name[] GetIssuer()
	{
		if (holder.BaseCertificateID != null)
		{
			return GetPrincipals(holder.BaseCertificateID.Issuer);
		}
		return null;
	}

	public object Clone()
	{
		return new AttributeCertificateHolder((Asn1Sequence)holder.ToAsn1Object());
	}

	public bool Match(X509Certificate x509Cert)
	{
		try
		{
			if (holder.BaseCertificateID != null)
			{
				return holder.BaseCertificateID.Serial.Value.Equals(x509Cert.SerialNumber) && MatchesDN(PrincipalUtilities.GetIssuerX509Principal(x509Cert), holder.BaseCertificateID.Issuer);
			}
			if (holder.EntityName != null && MatchesDN(PrincipalUtilities.GetSubjectX509Principal(x509Cert), holder.EntityName))
			{
				return true;
			}
			if (holder.ObjectDigestInfo != null)
			{
				IDigest digest = null;
				try
				{
					digest = DigestUtilities.GetDigest(DigestAlgorithm);
				}
				catch (Exception)
				{
					return false;
				}
				switch (DigestedObjectType)
				{
				case 0:
				{
					byte[] encoded2 = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(x509Cert.GetPublicKey()).GetEncoded();
					digest.BlockUpdate(encoded2, 0, encoded2.Length);
					break;
				}
				case 1:
				{
					byte[] encoded = x509Cert.GetEncoded();
					digest.BlockUpdate(encoded, 0, encoded.Length);
					break;
				}
				}
				if (!Arrays.AreEqual(DigestUtilities.DoFinal(digest), GetObjectDigest()))
				{
					return false;
				}
			}
		}
		catch (CertificateEncodingException)
		{
			return false;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is AttributeCertificateHolder))
		{
			return false;
		}
		AttributeCertificateHolder attributeCertificateHolder = (AttributeCertificateHolder)obj;
		return holder.Equals(attributeCertificateHolder.holder);
	}

	public override int GetHashCode()
	{
		return holder.GetHashCode();
	}

	public bool Match(object obj)
	{
		if (!(obj is X509Certificate))
		{
			return false;
		}
		return Match((X509Certificate)obj);
	}
}
