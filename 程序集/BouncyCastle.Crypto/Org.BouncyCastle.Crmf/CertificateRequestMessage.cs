using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;

namespace Org.BouncyCastle.Crmf;

public class CertificateRequestMessage
{
	public static readonly int popRaVerified = 0;

	public static readonly int popSigningKey = 1;

	public static readonly int popKeyEncipherment = 2;

	public static readonly int popKeyAgreement = 3;

	private readonly CertReqMsg certReqMsg;

	private readonly Controls controls;

	public bool HasControls => controls != null;

	public bool HasProofOfPossession => certReqMsg.Popo != null;

	public int ProofOfPossession => certReqMsg.Popo.Type;

	public bool HasSigningKeyProofOfPossessionWithPkMac
	{
		get
		{
			ProofOfPossession popo = certReqMsg.Popo;
			if (popo.Type == popSigningKey)
			{
				PopoSigningKey instance = PopoSigningKey.GetInstance(popo.Object);
				return instance.PoposkInput.PublicKeyMac != null;
			}
			return false;
		}
	}

	private static CertReqMsg ParseBytes(byte[] encoding)
	{
		return CertReqMsg.GetInstance(encoding);
	}

	public CertificateRequestMessage(byte[] encoded)
		: this(CertReqMsg.GetInstance(encoded))
	{
	}

	public CertificateRequestMessage(CertReqMsg certReqMsg)
	{
		this.certReqMsg = certReqMsg;
		controls = certReqMsg.CertReq.Controls;
	}

	public CertReqMsg ToAsn1Structure()
	{
		return certReqMsg;
	}

	public CertTemplate GetCertTemplate()
	{
		return certReqMsg.CertReq.CertTemplate;
	}

	public bool HasControl(DerObjectIdentifier objectIdentifier)
	{
		return FindControl(objectIdentifier) != null;
	}

	public IControl GetControl(DerObjectIdentifier type)
	{
		AttributeTypeAndValue attributeTypeAndValue = FindControl(type);
		if (attributeTypeAndValue != null)
		{
			if (attributeTypeAndValue.Type.Equals(CrmfObjectIdentifiers.id_regCtrl_pkiArchiveOptions))
			{
				return new PkiArchiveControl(PkiArchiveOptions.GetInstance(attributeTypeAndValue.Value));
			}
			if (attributeTypeAndValue.Type.Equals(CrmfObjectIdentifiers.id_regCtrl_regToken))
			{
				return new RegTokenControl(DerUtf8String.GetInstance(attributeTypeAndValue.Value));
			}
			if (attributeTypeAndValue.Type.Equals(CrmfObjectIdentifiers.id_regCtrl_authenticator))
			{
				return new AuthenticatorControl(DerUtf8String.GetInstance(attributeTypeAndValue.Value));
			}
		}
		return null;
	}

	public AttributeTypeAndValue FindControl(DerObjectIdentifier type)
	{
		if (controls == null)
		{
			return null;
		}
		AttributeTypeAndValue[] array = controls.ToAttributeTypeAndValueArray();
		AttributeTypeAndValue result = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Type.Equals(type))
			{
				result = array[i];
				break;
			}
		}
		return result;
	}

	public bool IsValidSigningKeyPop(IVerifierFactoryProvider verifierProvider)
	{
		ProofOfPossession popo = certReqMsg.Popo;
		if (popo.Type == popSigningKey)
		{
			PopoSigningKey instance = PopoSigningKey.GetInstance(popo.Object);
			if (instance.PoposkInput != null && instance.PoposkInput.PublicKeyMac != null)
			{
				throw new InvalidOperationException("verification requires password check");
			}
			return verifySignature(verifierProvider, instance);
		}
		throw new InvalidOperationException("not Signing Key type of proof of possession");
	}

	private bool verifySignature(IVerifierFactoryProvider verifierFactoryProvider, PopoSigningKey signKey)
	{
		IStreamCalculator streamCalculator;
		try
		{
			IVerifierFactory verifierFactory = verifierFactoryProvider.CreateVerifierFactory(signKey.AlgorithmIdentifier);
			streamCalculator = verifierFactory.CreateCalculator();
		}
		catch (Exception ex)
		{
			throw new CrmfException("unable to create verifier: " + ex.Message, ex);
		}
		if (signKey.PoposkInput != null)
		{
			byte[] derEncoded = signKey.GetDerEncoded();
			streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		}
		else
		{
			byte[] derEncoded2 = certReqMsg.CertReq.GetDerEncoded();
			streamCalculator.Stream.Write(derEncoded2, 0, derEncoded2.Length);
		}
		DefaultVerifierResult defaultVerifierResult = (DefaultVerifierResult)streamCalculator.GetResult();
		return defaultVerifierResult.IsVerified(signKey.Signature.GetBytes());
	}

	public byte[] GetEncoded()
	{
		return certReqMsg.GetEncoded();
	}
}
