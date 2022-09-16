using System;
using System.Collections;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Asn1.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509;

public class X509Certificate : X509ExtensionBase
{
	private readonly X509CertificateStructure c;

	private readonly string sigAlgName;

	private readonly byte[] sigAlgParams;

	private readonly BasicConstraints basicConstraints;

	private readonly bool[] keyUsage;

	private readonly object cacheLock = new object();

	private AsymmetricKeyParameter publicKeyValue;

	private volatile bool hashValueSet;

	private volatile int hashValue;

	public virtual X509CertificateStructure CertificateStructure => c;

	public virtual bool IsValidNow => IsValid(DateTime.UtcNow);

	public virtual int Version => c.Version;

	public virtual BigInteger SerialNumber => c.SerialNumber.Value;

	public virtual X509Name IssuerDN => c.Issuer;

	public virtual X509Name SubjectDN => c.Subject;

	public virtual DateTime NotBefore => c.StartDate.ToDateTime();

	public virtual DateTime NotAfter => c.EndDate.ToDateTime();

	public virtual string SigAlgName => sigAlgName;

	public virtual string SigAlgOid => c.SignatureAlgorithm.Algorithm.Id;

	public virtual DerBitString IssuerUniqueID => c.TbsCertificate.IssuerUniqueID;

	public virtual DerBitString SubjectUniqueID => c.TbsCertificate.SubjectUniqueID;

	protected X509Certificate()
	{
	}

	public X509Certificate(X509CertificateStructure c)
	{
		this.c = c;
		try
		{
			sigAlgName = X509SignatureUtilities.GetSignatureName(c.SignatureAlgorithm);
			sigAlgParams = c.SignatureAlgorithm.Parameters?.GetEncoded("DER");
		}
		catch (Exception ex)
		{
			throw new CrlException("Certificate contents invalid: " + ex);
		}
		try
		{
			Asn1OctetString extensionValue = GetExtensionValue(new DerObjectIdentifier("2.5.29.19"));
			if (extensionValue != null)
			{
				basicConstraints = BasicConstraints.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue));
			}
		}
		catch (Exception ex2)
		{
			throw new CertificateParsingException("cannot construct BasicConstraints: " + ex2);
		}
		try
		{
			Asn1OctetString extensionValue2 = GetExtensionValue(new DerObjectIdentifier("2.5.29.15"));
			if (extensionValue2 != null)
			{
				DerBitString instance = DerBitString.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue2));
				byte[] bytes = instance.GetBytes();
				int num = bytes.Length * 8 - instance.PadBits;
				keyUsage = new bool[(num < 9) ? 9 : num];
				for (int i = 0; i != num; i++)
				{
					keyUsage[i] = (bytes[i / 8] & (128 >> i % 8)) != 0;
				}
			}
			else
			{
				keyUsage = null;
			}
		}
		catch (Exception ex3)
		{
			throw new CertificateParsingException("cannot construct KeyUsage: " + ex3);
		}
	}

	public virtual bool IsValid(DateTime time)
	{
		if (time.CompareTo((object)NotBefore) >= 0)
		{
			return time.CompareTo((object)NotAfter) <= 0;
		}
		return false;
	}

	public virtual void CheckValidity()
	{
		CheckValidity(DateTime.UtcNow);
	}

	public virtual void CheckValidity(DateTime time)
	{
		if (time.CompareTo((object)NotAfter) > 0)
		{
			throw new CertificateExpiredException("certificate expired on " + c.EndDate.GetTime());
		}
		if (time.CompareTo((object)NotBefore) < 0)
		{
			throw new CertificateNotYetValidException("certificate not valid until " + c.StartDate.GetTime());
		}
	}

	public virtual byte[] GetTbsCertificate()
	{
		return c.TbsCertificate.GetDerEncoded();
	}

	public virtual byte[] GetSignature()
	{
		return c.GetSignatureOctets();
	}

	public virtual byte[] GetSigAlgParams()
	{
		return Arrays.Clone(sigAlgParams);
	}

	public virtual bool[] GetKeyUsage()
	{
		return Arrays.Clone(keyUsage);
	}

	public virtual IList GetExtendedKeyUsage()
	{
		Asn1OctetString extensionValue = GetExtensionValue(new DerObjectIdentifier("2.5.29.37"));
		if (extensionValue == null)
		{
			return null;
		}
		try
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue));
			IList list = Platform.CreateArrayList();
			foreach (DerObjectIdentifier item in instance)
			{
				list.Add(item.Id);
			}
			return list;
		}
		catch (Exception exception)
		{
			throw new CertificateParsingException("error processing extended key usage extension", exception);
		}
	}

	public virtual int GetBasicConstraints()
	{
		if (basicConstraints != null && basicConstraints.IsCA())
		{
			if (basicConstraints.PathLenConstraint == null)
			{
				return int.MaxValue;
			}
			return basicConstraints.PathLenConstraint.IntValue;
		}
		return -1;
	}

	public virtual ICollection GetSubjectAlternativeNames()
	{
		return GetAlternativeNames("2.5.29.17");
	}

	public virtual ICollection GetIssuerAlternativeNames()
	{
		return GetAlternativeNames("2.5.29.18");
	}

	protected virtual ICollection GetAlternativeNames(string oid)
	{
		Asn1OctetString extensionValue = GetExtensionValue(new DerObjectIdentifier(oid));
		if (extensionValue == null)
		{
			return null;
		}
		Asn1Object obj = X509ExtensionUtilities.FromExtensionValue(extensionValue);
		GeneralNames instance = GeneralNames.GetInstance(obj);
		IList list = Platform.CreateArrayList();
		GeneralName[] names = instance.GetNames();
		foreach (GeneralName generalName in names)
		{
			IList list2 = Platform.CreateArrayList();
			list2.Add(generalName.TagNo);
			list2.Add(generalName.Name.ToString());
			list.Add(list2);
		}
		return list;
	}

	protected override X509Extensions GetX509Extensions()
	{
		if (c.Version < 3)
		{
			return null;
		}
		return c.TbsCertificate.Extensions;
	}

	public virtual AsymmetricKeyParameter GetPublicKey()
	{
		lock (cacheLock)
		{
			if (publicKeyValue != null)
			{
				return publicKeyValue;
			}
		}
		AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(c.SubjectPublicKeyInfo);
		lock (cacheLock)
		{
			if (publicKeyValue == null)
			{
				publicKeyValue = asymmetricKeyParameter;
			}
			return publicKeyValue;
		}
	}

	public virtual byte[] GetEncoded()
	{
		return c.GetDerEncoded();
	}

	public override bool Equals(object other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is X509Certificate x509Certificate))
		{
			return false;
		}
		if (hashValueSet && x509Certificate.hashValueSet)
		{
			if (hashValue != x509Certificate.hashValue)
			{
				return false;
			}
		}
		else if (!c.Signature.Equals(x509Certificate.c.Signature))
		{
			return false;
		}
		return c.Equals(x509Certificate.c);
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
		stringBuilder.Append("  [0]         Version: ").Append(Version).Append(newLine);
		stringBuilder.Append("         SerialNumber: ").Append(SerialNumber).Append(newLine);
		stringBuilder.Append("             IssuerDN: ").Append(IssuerDN).Append(newLine);
		stringBuilder.Append("           Start Date: ").Append(NotBefore).Append(newLine);
		stringBuilder.Append("           Final Date: ").Append(NotAfter).Append(newLine);
		stringBuilder.Append("            SubjectDN: ").Append(SubjectDN).Append(newLine);
		stringBuilder.Append("           Public Key: ").Append(GetPublicKey()).Append(newLine);
		stringBuilder.Append("  Signature Algorithm: ").Append(SigAlgName).Append(newLine);
		byte[] signature = GetSignature();
		stringBuilder.Append("            Signature: ").Append(Hex.ToHexString(signature, 0, 20)).Append(newLine);
		for (int i = 20; i < signature.Length; i += 20)
		{
			int length = System.Math.Min(20, signature.Length - i);
			stringBuilder.Append("                       ").Append(Hex.ToHexString(signature, i, length)).Append(newLine);
		}
		X509Extensions extensions = c.TbsCertificate.Extensions;
		if (extensions != null)
		{
			IEnumerator enumerator = extensions.ExtensionOids.GetEnumerator();
			if (enumerator.MoveNext())
			{
				stringBuilder.Append("       Extensions: \n");
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
						if (derObjectIdentifier.Equals(X509Extensions.BasicConstraints))
						{
							stringBuilder.Append(BasicConstraints.GetInstance(asn1Object));
						}
						else if (derObjectIdentifier.Equals(X509Extensions.KeyUsage))
						{
							stringBuilder.Append(KeyUsage.GetInstance(asn1Object));
						}
						else if (derObjectIdentifier.Equals(MiscObjectIdentifiers.NetscapeCertType))
						{
							stringBuilder.Append(new NetscapeCertType((DerBitString)asn1Object));
						}
						else if (derObjectIdentifier.Equals(MiscObjectIdentifiers.NetscapeRevocationUrl))
						{
							stringBuilder.Append(new NetscapeRevocationUrl((DerIA5String)asn1Object));
						}
						else if (derObjectIdentifier.Equals(MiscObjectIdentifiers.VerisignCzagExtension))
						{
							stringBuilder.Append(new VerisignCzagExtension((DerIA5String)asn1Object));
						}
						else
						{
							stringBuilder.Append(derObjectIdentifier.Id);
							stringBuilder.Append(" value = ").Append(Asn1Dump.DumpAsString(asn1Object));
						}
					}
					catch (Exception)
					{
						stringBuilder.Append(derObjectIdentifier.Id);
						stringBuilder.Append(" value = ").Append("*****");
					}
				}
				stringBuilder.Append(newLine);
			}
			while (enumerator.MoveNext());
		}
		return stringBuilder.ToString();
	}

	public virtual void Verify(AsymmetricKeyParameter key)
	{
		CheckSignature(new Asn1VerifierFactory(c.SignatureAlgorithm, key));
	}

	public virtual void Verify(IVerifierFactoryProvider verifierProvider)
	{
		CheckSignature(verifierProvider.CreateVerifierFactory(c.SignatureAlgorithm));
	}

	protected virtual void CheckSignature(IVerifierFactory verifier)
	{
		if (!IsAlgIDEqual(c.SignatureAlgorithm, c.TbsCertificate.Signature))
		{
			throw new CertificateException("signature algorithm in TBS cert not same as outer cert");
		}
		_ = c.SignatureAlgorithm.Parameters;
		IStreamCalculator streamCalculator = verifier.CreateCalculator();
		byte[] tbsCertificate = GetTbsCertificate();
		streamCalculator.Stream.Write(tbsCertificate, 0, tbsCertificate.Length);
		Platform.Dispose(streamCalculator.Stream);
		if (!((IVerifier)streamCalculator.GetResult()).IsVerified(GetSignature()))
		{
			throw new InvalidKeyException("Public key presented not for certificate signature");
		}
	}

	private static bool IsAlgIDEqual(AlgorithmIdentifier id1, AlgorithmIdentifier id2)
	{
		if (!id1.Algorithm.Equals(id2.Algorithm))
		{
			return false;
		}
		Asn1Encodable parameters = id1.Parameters;
		Asn1Encodable parameters2 = id2.Parameters;
		if (parameters == null == (parameters2 == null))
		{
			return object.Equals(parameters, parameters2);
		}
		if (parameters != null)
		{
			return parameters.ToAsn1Object() is Asn1Null;
		}
		return parameters2.ToAsn1Object() is Asn1Null;
	}
}
