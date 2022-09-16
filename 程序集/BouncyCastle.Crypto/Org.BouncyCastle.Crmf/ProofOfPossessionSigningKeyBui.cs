using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crmf;

public class ProofOfPossessionSigningKeyBuilder
{
	private CertRequest _certRequest;

	private SubjectPublicKeyInfo _pubKeyInfo;

	private GeneralName _name;

	private PKMacValue _publicKeyMAC;

	public ProofOfPossessionSigningKeyBuilder(CertRequest certRequest)
	{
		_certRequest = certRequest;
	}

	public ProofOfPossessionSigningKeyBuilder(SubjectPublicKeyInfo pubKeyInfo)
	{
		_pubKeyInfo = pubKeyInfo;
	}

	public ProofOfPossessionSigningKeyBuilder SetSender(GeneralName name)
	{
		_name = name;
		return this;
	}

	public ProofOfPossessionSigningKeyBuilder SetPublicKeyMac(PKMacBuilder generator, char[] password)
	{
		IMacFactory macFactory = generator.Build(password);
		IStreamCalculator streamCalculator = macFactory.CreateCalculator();
		byte[] derEncoded = _pubKeyInfo.GetDerEncoded();
		streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		streamCalculator.Stream.Flush();
		Platform.Dispose(streamCalculator.Stream);
		_publicKeyMAC = new PKMacValue((AlgorithmIdentifier)macFactory.AlgorithmDetails, new DerBitString(((IBlockResult)streamCalculator.GetResult()).Collect()));
		return this;
	}

	public PopoSigningKey Build(ISignatureFactory signer)
	{
		if (_name != null && _publicKeyMAC != null)
		{
			throw new InvalidOperationException("name and publicKeyMAC cannot both be set.");
		}
		IStreamCalculator streamCalculator = signer.CreateCalculator();
		PopoSigningKeyInput popoSigningKeyInput;
		if (_certRequest != null)
		{
			popoSigningKeyInput = null;
			byte[] derEncoded = _certRequest.GetDerEncoded();
			streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		}
		else if (_name != null)
		{
			popoSigningKeyInput = new PopoSigningKeyInput(_name, _pubKeyInfo);
			byte[] derEncoded = popoSigningKeyInput.GetDerEncoded();
			streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		}
		else
		{
			popoSigningKeyInput = new PopoSigningKeyInput(_publicKeyMAC, _pubKeyInfo);
			byte[] derEncoded = popoSigningKeyInput.GetDerEncoded();
			streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		}
		streamCalculator.Stream.Flush();
		Platform.Dispose(streamCalculator.Stream);
		DefaultSignatureResult defaultSignatureResult = (DefaultSignatureResult)streamCalculator.GetResult();
		return new PopoSigningKey(popoSigningKeyInput, (AlgorithmIdentifier)signer.AlgorithmDetails, new DerBitString(defaultSignatureResult.Collect()));
	}
}
