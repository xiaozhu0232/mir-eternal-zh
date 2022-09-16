using System;
using System.Collections;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509;

public class X509Crl : X509ExtensionBase
{
	private readonly CertificateList c;

	private readonly string sigAlgName;

	private readonly byte[] sigAlgParams;

	private readonly bool isIndirect;

	private volatile bool hashValueSet;

	private volatile int hashValue;

	public virtual int Version => c.Version;

	public virtual X509Name IssuerDN => c.Issuer;

	public virtual DateTime ThisUpdate => c.ThisUpdate.ToDateTime();

	public virtual DateTimeObject NextUpdate
	{
		get
		{
			if (c.NextUpdate != null)
			{
				return new DateTimeObject(c.NextUpdate.ToDateTime());
			}
			return null;
		}
	}

	public virtual string SigAlgName => sigAlgName;

	public virtual string SigAlgOid => c.SignatureAlgorithm.Algorithm.Id;

	protected virtual bool IsIndirectCrl
	{
		get
		{
			Asn1OctetString extensionValue = GetExtensionValue(X509Extensions.IssuingDistributionPoint);
			bool result = false;
			try
			{
				if (extensionValue != null)
				{
					return IssuingDistributionPoint.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue)).IsIndirectCrl;
				}
				return result;
			}
			catch (Exception ex)
			{
				throw new CrlException("Exception reading IssuingDistributionPoint" + ex);
			}
		}
	}

	public X509Crl(CertificateList c)
	{
		this.c = c;
		try
		{
			sigAlgName = X509SignatureUtilities.GetSignatureName(c.SignatureAlgorithm);
			sigAlgParams = c.SignatureAlgorithm.Parameters?.GetEncoded("DER");
			isIndirect = IsIndirectCrl;
		}
		catch (Exception ex)
		{
			throw new CrlException("CRL contents invalid: " + ex);
		}
	}

	protected override X509Extensions GetX509Extensions()
	{
		if (c.Version < 2)
		{
			return null;
		}
		return c.TbsCertList.Extensions;
	}

	public virtual byte[] GetEncoded()
	{
		try
		{
			return c.GetDerEncoded();
		}
		catch (Exception ex)
		{
			throw new CrlException(ex.ToString());
		}
	}

	public virtual void Verify(AsymmetricKeyParameter publicKey)
	{
		Verify(new Asn1VerifierFactoryProvider(publicKey));
	}

	public virtual void Verify(IVerifierFactoryProvider verifierProvider)
	{
		CheckSignature(verifierProvider.CreateVerifierFactory(c.SignatureAlgorithm));
	}

	protected virtual void CheckSignature(IVerifierFactory verifier)
	{
		if (!c.SignatureAlgorithm.Equals(c.TbsCertList.Signature))
		{
			throw new CrlException("Signature algorithm on CertificateList does not match TbsCertList.");
		}
		_ = c.SignatureAlgorithm.Parameters;
		IStreamCalculator streamCalculator = verifier.CreateCalculator();
		byte[] tbsCertList = GetTbsCertList();
		streamCalculator.Stream.Write(tbsCertList, 0, tbsCertList.Length);
		Platform.Dispose(streamCalculator.Stream);
		if (!((IVerifier)streamCalculator.GetResult()).IsVerified(GetSignature()))
		{
			throw new InvalidKeyException("CRL does not verify with supplied public key.");
		}
	}

	private ISet LoadCrlEntries()
	{
		ISet set = new HashSet();
		IEnumerable revokedCertificateEnumeration = c.GetRevokedCertificateEnumeration();
		X509Name previousCertificateIssuer = IssuerDN;
		foreach (CrlEntry item in revokedCertificateEnumeration)
		{
			X509CrlEntry x509CrlEntry = new X509CrlEntry(item, isIndirect, previousCertificateIssuer);
			set.Add(x509CrlEntry);
			previousCertificateIssuer = x509CrlEntry.GetCertificateIssuer();
		}
		return set;
	}

	public virtual X509CrlEntry GetRevokedCertificate(BigInteger serialNumber)
	{
		IEnumerable revokedCertificateEnumeration = c.GetRevokedCertificateEnumeration();
		X509Name previousCertificateIssuer = IssuerDN;
		foreach (CrlEntry item in revokedCertificateEnumeration)
		{
			X509CrlEntry x509CrlEntry = new X509CrlEntry(item, isIndirect, previousCertificateIssuer);
			if (serialNumber.Equals(item.UserCertificate.Value))
			{
				return x509CrlEntry;
			}
			previousCertificateIssuer = x509CrlEntry.GetCertificateIssuer();
		}
		return null;
	}

	public virtual ISet GetRevokedCertificates()
	{
		ISet set = LoadCrlEntries();
		if (set.Count > 0)
		{
			return set;
		}
		return null;
	}

	public virtual byte[] GetTbsCertList()
	{
		try
		{
			return c.TbsCertList.GetDerEncoded();
		}
		catch (Exception ex)
		{
			throw new CrlException(ex.ToString());
		}
	}

	public virtual byte[] GetSignature()
	{
		return c.GetSignatureOctets();
	}

	public virtual byte[] GetSigAlgParams()
	{
		return Arrays.Clone(sigAlgParams);
	}

	public override bool Equals(object other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is X509Crl x509Crl))
		{
			return false;
		}
		if (hashValueSet && x509Crl.hashValueSet)
		{
			if (hashValue != x509Crl.hashValue)
			{
				return false;
			}
		}
		else if (!c.Signature.Equals(x509Crl.c.Signature))
		{
			return false;
		}
		return c.Equals(x509Crl.c);
	}

	public override int GetHashCode()
	{
		if (!hashValueSet)
		{
			hashValue = c.GetHashCode();
			hashValueSet = true;
		}
		return hashValue;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string newLine = Platform.NewLine;
		stringBuilder.Append("              Version: ").Append(Version).Append(newLine);
		stringBuilder.Append("             IssuerDN: ").Append(IssuerDN).Append(newLine);
		stringBuilder.Append("          This update: ").Append(ThisUpdate).Append(newLine);
		stringBuilder.Append("          Next update: ").Append(NextUpdate).Append(newLine);
		stringBuilder.Append("  Signature Algorithm: ").Append(SigAlgName).Append(newLine);
		byte[] signature = GetSignature();
		stringBuilder.Append("            Signature: ");
		stringBuilder.Append(Hex.ToHexString(signature, 0, 20)).Append(newLine);
		for (int i = 20; i < signature.Length; i += 20)
		{
			int length = System.Math.Min(20, signature.Length - i);
			stringBuilder.Append("                       ");
			stringBuilder.Append(Hex.ToHexString(signature, i, length)).Append(newLine);
		}
		X509Extensions extensions = c.TbsCertList.Extensions;
		if (extensions != null)
		{
			IEnumerator enumerator = extensions.ExtensionOids.GetEnumerator();
			if (enumerator.MoveNext())
			{
				stringBuilder.Append("           Extensions: ").Append(newLine);
			}
			do
			{
				DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)enumerator.Current;
				X509Extension extension = extensions.GetExtension(derObjectIdentifier);
				if (extension.Value != null)
				{
					Asn1Object asn1Object = X509ExtensionUtilities.FromExtensionValue(extension.Value);
					stringBuilder.Append("                       critical(").Append(extension.IsCritical).Append(") ");
					try
					{
						if (derObjectIdentifier.Equals(X509Extensions.CrlNumber))
						{
							stringBuilder.Append(new CrlNumber(DerInteger.GetInstance(asn1Object).PositiveValue)).Append(newLine);
							continue;
						}
						if (derObjectIdentifier.Equals(X509Extensions.DeltaCrlIndicator))
						{
							stringBuilder.Append("Base CRL: " + new CrlNumber(DerInteger.GetInstance(asn1Object).PositiveValue)).Append(newLine);
							continue;
						}
						if (derObjectIdentifier.Equals(X509Extensions.IssuingDistributionPoint))
						{
							stringBuilder.Append(IssuingDistributionPoint.GetInstance((Asn1Sequence)asn1Object)).Append(newLine);
							continue;
						}
						if (derObjectIdentifier.Equals(X509Extensions.CrlDistributionPoints))
						{
							stringBuilder.Append(CrlDistPoint.GetInstance((Asn1Sequence)asn1Object)).Append(newLine);
							continue;
						}
						if (derObjectIdentifier.Equals(X509Extensions.FreshestCrl))
						{
							stringBuilder.Append(CrlDistPoint.GetInstance((Asn1Sequence)asn1Object)).Append(newLine);
							continue;
						}
						stringBuilder.Append(derObjectIdentifier.Id);
						stringBuilder.Append(" value = ").Append(Asn1Dump.DumpAsString(asn1Object)).Append(newLine);
					}
					catch (Exception)
					{
						stringBuilder.Append(derObjectIdentifier.Id);
						stringBuilder.Append(" value = ").Append("*****").Append(newLine);
					}
				}
				else
				{
					stringBuilder.Append(newLine);
				}
			}
			while (enumerator.MoveNext());
		}
		ISet revokedCertificates = GetRevokedCertificates();
		if (revokedCertificates != null)
		{
			foreach (X509CrlEntry item in revokedCertificates)
			{
				stringBuilder.Append(item);
				stringBuilder.Append(newLine);
			}
		}
		return stringBuilder.ToString();
	}

	public virtual bool IsRevoked(X509Certificate cert)
	{
		CrlEntry[] revokedCertificates = c.GetRevokedCertificates();
		if (revokedCertificates != null)
		{
			BigInteger serialNumber = cert.SerialNumber;
			for (int i = 0; i < revokedCertificates.Length; i++)
			{
				if (revokedCertificates[i].UserCertificate.Value.Equals(serialNumber))
				{
					return true;
				}
			}
		}
		return false;
	}
}
