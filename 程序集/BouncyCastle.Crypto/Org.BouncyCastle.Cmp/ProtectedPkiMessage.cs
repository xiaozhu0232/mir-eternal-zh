using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crmf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cmp;

public class ProtectedPkiMessage
{
	private readonly PkiMessage pkiMessage;

	public PkiHeader Header => pkiMessage.Header;

	public PkiBody Body => pkiMessage.Body;

	public bool HasPasswordBasedMacProtected => Header.ProtectionAlg.Algorithm.Equals(CmpObjectIdentifiers.passwordBasedMac);

	public ProtectedPkiMessage(GeneralPkiMessage pkiMessage)
	{
		if (!pkiMessage.HasProtection)
		{
			throw new ArgumentException("pki message not protected");
		}
		this.pkiMessage = pkiMessage.ToAsn1Structure();
	}

	public ProtectedPkiMessage(PkiMessage pkiMessage)
	{
		if (pkiMessage.Header.ProtectionAlg == null)
		{
			throw new ArgumentException("pki message not protected");
		}
		this.pkiMessage = pkiMessage;
	}

	public PkiMessage ToAsn1Message()
	{
		return pkiMessage;
	}

	public X509Certificate[] GetCertificates()
	{
		CmpCertificate[] extraCerts = pkiMessage.GetExtraCerts();
		if (extraCerts == null)
		{
			return new X509Certificate[0];
		}
		X509Certificate[] array = new X509Certificate[extraCerts.Length];
		for (int i = 0; i < extraCerts.Length; i++)
		{
			array[i] = new X509Certificate(X509CertificateStructure.GetInstance(extraCerts[i].GetEncoded()));
		}
		return array;
	}

	public bool Verify(IVerifierFactory verifierFactory)
	{
		IStreamCalculator streamCalculator = verifierFactory.CreateCalculator();
		IVerifier verifier = (IVerifier)Process(streamCalculator);
		return verifier.IsVerified(pkiMessage.Protection.GetBytes());
	}

	private object Process(IStreamCalculator streamCalculator)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.Add(pkiMessage.Header);
		asn1EncodableVector.Add(pkiMessage.Body);
		byte[] derEncoded = new DerSequence(asn1EncodableVector).GetDerEncoded();
		streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		streamCalculator.Stream.Flush();
		Platform.Dispose(streamCalculator.Stream);
		return streamCalculator.GetResult();
	}

	public bool Verify(PKMacBuilder pkMacBuilder, char[] password)
	{
		if (!CmpObjectIdentifiers.passwordBasedMac.Equals(pkiMessage.Header.ProtectionAlg.Algorithm))
		{
			throw new InvalidOperationException("protection algorithm is not mac based");
		}
		PbmParameter instance = PbmParameter.GetInstance(pkiMessage.Header.ProtectionAlg.Parameters);
		pkMacBuilder.SetParameters(instance);
		IBlockResult blockResult = (IBlockResult)Process(pkMacBuilder.Build(password).CreateCalculator());
		return Arrays.ConstantTimeAreEqual(blockResult.Collect(), pkiMessage.Protection.GetBytes());
	}
}
