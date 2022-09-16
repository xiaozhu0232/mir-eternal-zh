using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crmf;

public class CertificateRequestMessageBuilder
{
	private readonly BigInteger _certReqId;

	private X509ExtensionsGenerator _extGenerator;

	private CertTemplateBuilder _templateBuilder;

	private IList _controls = Platform.CreateArrayList();

	private ISignatureFactory _popSigner;

	private PKMacBuilder _pkMacBuilder;

	private char[] _password;

	private GeneralName _sender;

	private int _popoType = 2;

	private PopoPrivKey _popoPrivKey;

	private Asn1Null _popRaVerified;

	private PKMacValue _agreeMac;

	public CertificateRequestMessageBuilder(BigInteger certReqId)
	{
		_certReqId = certReqId;
		_extGenerator = new X509ExtensionsGenerator();
		_templateBuilder = new CertTemplateBuilder();
	}

	public CertificateRequestMessageBuilder SetPublicKey(SubjectPublicKeyInfo publicKeyInfo)
	{
		if (publicKeyInfo != null)
		{
			_templateBuilder.SetPublicKey(publicKeyInfo);
		}
		return this;
	}

	public CertificateRequestMessageBuilder SetIssuer(X509Name issuer)
	{
		if (issuer != null)
		{
			_templateBuilder.SetIssuer(issuer);
		}
		return this;
	}

	public CertificateRequestMessageBuilder SetSubject(X509Name subject)
	{
		if (subject != null)
		{
			_templateBuilder.SetSubject(subject);
		}
		return this;
	}

	public CertificateRequestMessageBuilder SetSerialNumber(BigInteger serialNumber)
	{
		if (serialNumber != null)
		{
			_templateBuilder.SetSerialNumber(new DerInteger(serialNumber));
		}
		return this;
	}

	public CertificateRequestMessageBuilder SetValidity(Time notBefore, Time notAfter)
	{
		_templateBuilder.SetValidity(new OptionalValidity(notBefore, notAfter));
		return this;
	}

	public CertificateRequestMessageBuilder AddExtension(DerObjectIdentifier oid, bool critical, Asn1Encodable value)
	{
		_extGenerator.AddExtension(oid, critical, value);
		return this;
	}

	public CertificateRequestMessageBuilder AddExtension(DerObjectIdentifier oid, bool critical, byte[] value)
	{
		_extGenerator.AddExtension(oid, critical, value);
		return this;
	}

	public CertificateRequestMessageBuilder AddControl(IControl control)
	{
		_controls.Add(control);
		return this;
	}

	public CertificateRequestMessageBuilder SetProofOfPossessionSignKeySigner(ISignatureFactory popoSignatureFactory)
	{
		if (_popoPrivKey != null || _popRaVerified != null || _agreeMac != null)
		{
			throw new InvalidOperationException("only one proof of possession is allowed.");
		}
		_popSigner = popoSignatureFactory;
		return this;
	}

	public CertificateRequestMessageBuilder SetProofOfPossessionSubsequentMessage(SubsequentMessage msg)
	{
		if (_popoPrivKey != null || _popRaVerified != null || _agreeMac != null)
		{
			throw new InvalidOperationException("only one proof of possession is allowed.");
		}
		_popoType = 2;
		_popoPrivKey = new PopoPrivKey(msg);
		return this;
	}

	public CertificateRequestMessageBuilder SetProofOfPossessionSubsequentMessage(int type, SubsequentMessage msg)
	{
		if (_popoPrivKey != null || _popRaVerified != null || _agreeMac != null)
		{
			throw new InvalidOperationException("only one proof of possession is allowed.");
		}
		if (type != 2 && type != 3)
		{
			throw new ArgumentException("type must be ProofOfPossession.TYPE_KEY_ENCIPHERMENT || ProofOfPossession.TYPE_KEY_AGREEMENT");
		}
		_popoType = type;
		_popoPrivKey = new PopoPrivKey(msg);
		return this;
	}

	public CertificateRequestMessageBuilder SetProofOfPossessionAgreeMac(PKMacValue macValue)
	{
		if (_popSigner != null || _popRaVerified != null || _popoPrivKey != null)
		{
			throw new InvalidOperationException("only one proof of possession allowed");
		}
		_agreeMac = macValue;
		return this;
	}

	public CertificateRequestMessageBuilder SetProofOfPossessionRaVerified()
	{
		if (_popSigner != null || _popoPrivKey != null)
		{
			throw new InvalidOperationException("only one proof of possession allowed");
		}
		_popRaVerified = DerNull.Instance;
		return this;
	}

	public CertificateRequestMessageBuilder SetAuthInfoPKMAC(PKMacBuilder pkmacFactory, char[] password)
	{
		_pkMacBuilder = pkmacFactory;
		_password = password;
		return this;
	}

	public CertificateRequestMessageBuilder SetAuthInfoSender(X509Name sender)
	{
		return SetAuthInfoSender(new GeneralName(sender));
	}

	public CertificateRequestMessageBuilder SetAuthInfoSender(GeneralName sender)
	{
		_sender = sender;
		return this;
	}

	public CertificateRequestMessage Build()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(_certReqId));
		if (!_extGenerator.IsEmpty)
		{
			_templateBuilder.SetExtensions(_extGenerator.Generate());
		}
		asn1EncodableVector.Add(_templateBuilder.Build());
		if (_controls.Count > 0)
		{
			Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
			foreach (object control2 in _controls)
			{
				IControl control = (IControl)control2;
				asn1EncodableVector2.Add(new AttributeTypeAndValue(control.Type, control.Value));
			}
			asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
		}
		CertRequest instance = CertRequest.GetInstance(new DerSequence(asn1EncodableVector));
		asn1EncodableVector = new Asn1EncodableVector(instance);
		if (_popSigner != null)
		{
			CertTemplate certTemplate = instance.CertTemplate;
			if (certTemplate.Subject == null || certTemplate.PublicKey == null)
			{
				SubjectPublicKeyInfo publicKey = instance.CertTemplate.PublicKey;
				ProofOfPossessionSigningKeyBuilder proofOfPossessionSigningKeyBuilder = new ProofOfPossessionSigningKeyBuilder(publicKey);
				if (_sender != null)
				{
					proofOfPossessionSigningKeyBuilder.SetSender(_sender);
				}
				else
				{
					proofOfPossessionSigningKeyBuilder.SetPublicKeyMac(_pkMacBuilder, _password);
				}
				asn1EncodableVector.Add(new ProofOfPossession(proofOfPossessionSigningKeyBuilder.Build(_popSigner)));
			}
			else
			{
				ProofOfPossessionSigningKeyBuilder proofOfPossessionSigningKeyBuilder2 = new ProofOfPossessionSigningKeyBuilder(instance);
				asn1EncodableVector.Add(new ProofOfPossession(proofOfPossessionSigningKeyBuilder2.Build(_popSigner)));
			}
		}
		else if (_popoPrivKey != null)
		{
			asn1EncodableVector.Add(new ProofOfPossession(_popoType, _popoPrivKey));
		}
		else if (_agreeMac != null)
		{
			asn1EncodableVector.Add(new ProofOfPossession(3, PopoPrivKey.GetInstance(new DerTaggedObject(explicitly: false, 3, _agreeMac), isExplicit: true)));
		}
		else if (_popRaVerified != null)
		{
			asn1EncodableVector.Add(new ProofOfPossession());
		}
		return new CertificateRequestMessage(CertReqMsg.GetInstance(new DerSequence(asn1EncodableVector)));
	}
}
