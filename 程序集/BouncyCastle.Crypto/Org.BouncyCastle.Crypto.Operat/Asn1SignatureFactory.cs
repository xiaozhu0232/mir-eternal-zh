using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Operators;

public class Asn1SignatureFactory : ISignatureFactory
{
	private readonly AlgorithmIdentifier algID;

	private readonly string algorithm;

	private readonly AsymmetricKeyParameter privateKey;

	private readonly SecureRandom random;

	public object AlgorithmDetails => algID;

	public static IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

	public Asn1SignatureFactory(string algorithm, AsymmetricKeyParameter privateKey)
		: this(algorithm, privateKey, null)
	{
	}

	public Asn1SignatureFactory(string algorithm, AsymmetricKeyParameter privateKey, SecureRandom random)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		if (privateKey == null)
		{
			throw new ArgumentNullException("privateKey");
		}
		if (!privateKey.IsPrivate)
		{
			throw new ArgumentException("Key for signing must be private", "privateKey");
		}
		DerObjectIdentifier algorithmOid = X509Utilities.GetAlgorithmOid(algorithm);
		this.algorithm = algorithm;
		this.privateKey = privateKey;
		this.random = random;
		algID = X509Utilities.GetSigAlgID(algorithmOid, algorithm);
	}

	public IStreamCalculator CreateCalculator()
	{
		ISigner signer = SignerUtilities.InitSigner(algorithm, forSigning: true, privateKey, random);
		return new DefaultSignatureCalculator(signer);
	}
}
