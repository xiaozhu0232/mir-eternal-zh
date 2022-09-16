using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsDheKeyExchange : TlsDHKeyExchange
{
	protected TlsSignerCredentials mServerCredentials = null;

	[Obsolete("Use constructor that takes a TlsDHVerifier")]
	public TlsDheKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, DHParameters dhParameters)
		: this(keyExchange, supportedSignatureAlgorithms, new DefaultTlsDHVerifier(), dhParameters)
	{
	}

	public TlsDheKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, TlsDHVerifier dhVerifier, DHParameters dhParameters)
		: base(keyExchange, supportedSignatureAlgorithms, dhVerifier, dhParameters)
	{
	}

	public override void ProcessServerCredentials(TlsCredentials serverCredentials)
	{
		if (!(serverCredentials is TlsSignerCredentials))
		{
			throw new TlsFatalAlert(80);
		}
		ProcessServerCertificate(serverCredentials.Certificate);
		mServerCredentials = (TlsSignerCredentials)serverCredentials;
	}

	public override byte[] GenerateServerKeyExchange()
	{
		if (mDHParameters == null)
		{
			throw new TlsFatalAlert(80);
		}
		DigestInputBuffer digestInputBuffer = new DigestInputBuffer();
		mDHAgreePrivateKey = TlsDHUtilities.GenerateEphemeralServerKeyExchange(mContext.SecureRandom, mDHParameters, digestInputBuffer);
		SignatureAndHashAlgorithm signatureAndHashAlgorithm = TlsUtilities.GetSignatureAndHashAlgorithm(mContext, mServerCredentials);
		IDigest digest = TlsUtilities.CreateHash(signatureAndHashAlgorithm);
		SecurityParameters securityParameters = mContext.SecurityParameters;
		digest.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
		digest.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
		digestInputBuffer.UpdateDigest(digest);
		byte[] hash = DigestUtilities.DoFinal(digest);
		byte[] signature = mServerCredentials.GenerateCertificateSignature(hash);
		DigitallySigned digitallySigned = new DigitallySigned(signatureAndHashAlgorithm, signature);
		digitallySigned.Encode(digestInputBuffer);
		return digestInputBuffer.ToArray();
	}

	public override void ProcessServerKeyExchange(Stream input)
	{
		SecurityParameters securityParameters = mContext.SecurityParameters;
		SignerInputBuffer signerInputBuffer = new SignerInputBuffer();
		Stream input2 = new TeeInputStream(input, signerInputBuffer);
		mDHParameters = TlsDHUtilities.ReceiveDHParameters(mDHVerifier, input2);
		mDHAgreePublicKey = new DHPublicKeyParameters(TlsDHUtilities.ReadDHParameter(input2), mDHParameters);
		DigitallySigned digitallySigned = ParseSignature(input);
		ISigner signer = InitVerifyer(mTlsSigner, digitallySigned.Algorithm, securityParameters);
		signerInputBuffer.UpdateSigner(signer);
		if (!signer.VerifySignature(digitallySigned.Signature))
		{
			throw new TlsFatalAlert(51);
		}
	}

	protected virtual ISigner InitVerifyer(TlsSigner tlsSigner, SignatureAndHashAlgorithm algorithm, SecurityParameters securityParameters)
	{
		ISigner signer = tlsSigner.CreateVerifyer(algorithm, mServerPublicKey);
		signer.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
		signer.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
		return signer;
	}
}
