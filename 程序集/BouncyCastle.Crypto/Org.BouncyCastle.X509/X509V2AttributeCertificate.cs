using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509;

public class X509V2AttributeCertificate : X509ExtensionBase, IX509AttributeCertificate, IX509Extension
{
	private readonly AttributeCertificate cert;

	private readonly DateTime notBefore;

	private readonly DateTime notAfter;

	public virtual int Version => cert.ACInfo.Version.IntValueExact + 1;

	public virtual BigInteger SerialNumber => cert.ACInfo.SerialNumber.Value;

	public virtual AttributeCertificateHolder Holder => new AttributeCertificateHolder((Asn1Sequence)cert.ACInfo.Holder.ToAsn1Object());

	public virtual AttributeCertificateIssuer Issuer => new AttributeCertificateIssuer(cert.ACInfo.Issuer);

	public virtual DateTime NotBefore => notBefore;

	public virtual DateTime NotAfter => notAfter;

	public virtual bool IsValidNow => IsValid(DateTime.UtcNow);

	public virtual AlgorithmIdentifier SignatureAlgorithm => cert.SignatureAlgorithm;

	private static AttributeCertificate GetObject(Stream input)
	{
		try
		{
			return AttributeCertificate.GetInstance(Asn1Object.FromStream(input));
		}
		catch (IOException ex)
		{
			throw ex;
		}
		catch (Exception innerException)
		{
			throw new IOException("exception decoding certificate structure", innerException);
		}
	}

	public X509V2AttributeCertificate(Stream encIn)
		: this(GetObject(encIn))
	{
	}

	public X509V2AttributeCertificate(byte[] encoded)
		: this(new MemoryStream(encoded, writable: false))
	{
	}

	internal X509V2AttributeCertificate(AttributeCertificate cert)
	{
		this.cert = cert;
		try
		{
			notAfter = cert.ACInfo.AttrCertValidityPeriod.NotAfterTime.ToDateTime();
			notBefore = cert.ACInfo.AttrCertValidityPeriod.NotBeforeTime.ToDateTime();
		}
		catch (Exception innerException)
		{
			throw new IOException("invalid data structure in certificate!", innerException);
		}
	}

	public virtual bool[] GetIssuerUniqueID()
	{
		DerBitString issuerUniqueID = cert.ACInfo.IssuerUniqueID;
		if (issuerUniqueID != null)
		{
			byte[] bytes = issuerUniqueID.GetBytes();
			bool[] array = new bool[bytes.Length * 8 - issuerUniqueID.PadBits];
			for (int i = 0; i != array.Length; i++)
			{
				array[i] = (bytes[i / 8] & (128 >> i % 8)) != 0;
			}
			return array;
		}
		return null;
	}

	public virtual bool IsValid(DateTime date)
	{
		if (date.CompareTo((object)NotBefore) >= 0)
		{
			return date.CompareTo((object)NotAfter) <= 0;
		}
		return false;
	}

	public virtual void CheckValidity()
	{
		CheckValidity(DateTime.UtcNow);
	}

	public virtual void CheckValidity(DateTime date)
	{
		if (date.CompareTo((object)NotAfter) > 0)
		{
			throw new CertificateExpiredException("certificate expired on " + NotAfter);
		}
		if (date.CompareTo((object)NotBefore) < 0)
		{
			throw new CertificateNotYetValidException("certificate not valid until " + NotBefore);
		}
	}

	public virtual byte[] GetSignature()
	{
		return cert.GetSignatureOctets();
	}

	public virtual void Verify(AsymmetricKeyParameter key)
	{
		CheckSignature(new Asn1VerifierFactory(cert.SignatureAlgorithm, key));
	}

	public virtual void Verify(IVerifierFactoryProvider verifierProvider)
	{
		CheckSignature(verifierProvider.CreateVerifierFactory(cert.SignatureAlgorithm));
	}

	protected virtual void CheckSignature(IVerifierFactory verifier)
	{
		if (!cert.SignatureAlgorithm.Equals(cert.ACInfo.Signature))
		{
			throw new CertificateException("Signature algorithm in certificate info not same as outer certificate");
		}
		IStreamCalculator streamCalculator = verifier.CreateCalculator();
		try
		{
			byte[] encoded = cert.ACInfo.GetEncoded();
			streamCalculator.Stream.Write(encoded, 0, encoded.Length);
			Platform.Dispose(streamCalculator.Stream);
		}
		catch (IOException exception)
		{
			throw new SignatureException("Exception encoding certificate info object", exception);
		}
		if (!((IVerifier)streamCalculator.GetResult()).IsVerified(GetSignature()))
		{
			throw new InvalidKeyException("Public key presented not for certificate signature");
		}
	}

	public virtual byte[] GetEncoded()
	{
		return cert.GetEncoded();
	}

	protected override X509Extensions GetX509Extensions()
	{
		return cert.ACInfo.Extensions;
	}

	public virtual X509Attribute[] GetAttributes()
	{
		Asn1Sequence attributes = cert.ACInfo.Attributes;
		X509Attribute[] array = new X509Attribute[attributes.Count];
		for (int i = 0; i != attributes.Count; i++)
		{
			array[i] = new X509Attribute(attributes[i]);
		}
		return array;
	}

	public virtual X509Attribute[] GetAttributes(string oid)
	{
		Asn1Sequence attributes = cert.ACInfo.Attributes;
		IList list = Platform.CreateArrayList();
		for (int i = 0; i != attributes.Count; i++)
		{
			X509Attribute x509Attribute = new X509Attribute(attributes[i]);
			if (x509Attribute.Oid.Equals(oid))
			{
				list.Add(x509Attribute);
			}
		}
		if (list.Count < 1)
		{
			return null;
		}
		X509Attribute[] array = new X509Attribute[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			array[j] = (X509Attribute)list[j];
		}
		return array;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is X509V2AttributeCertificate x509V2AttributeCertificate))
		{
			return false;
		}
		return cert.Equals(x509V2AttributeCertificate.cert);
	}

	public override int GetHashCode()
	{
		return cert.GetHashCode();
	}
}
