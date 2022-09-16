using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Operators;

public class Asn1VerifierFactory : IVerifierFactory
{
	private readonly AlgorithmIdentifier algID;

	private readonly AsymmetricKeyParameter publicKey;

	public object AlgorithmDetails => algID;

	public Asn1VerifierFactory(string algorithm, AsymmetricKeyParameter publicKey)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		if (publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		if (publicKey.IsPrivate)
		{
			throw new ArgumentException("Key for verifying must be public", "publicKey");
		}
		DerObjectIdentifier algorithmOid = X509Utilities.GetAlgorithmOid(algorithm);
		this.publicKey = publicKey;
		algID = X509Utilities.GetSigAlgID(algorithmOid, algorithm);
	}

	public Asn1VerifierFactory(AlgorithmIdentifier algorithm, AsymmetricKeyParameter publicKey)
	{
		this.publicKey = publicKey;
		algID = algorithm;
	}

	public IStreamCalculator CreateCalculator()
	{
		ISigner signer = SignerUtilities.InitSigner(X509Utilities.GetSignatureName(algID), forSigning: false, publicKey, null);
		return new DefaultVerifierCalculator(signer);
	}
}
