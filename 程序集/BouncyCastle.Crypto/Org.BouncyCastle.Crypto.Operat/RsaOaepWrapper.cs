using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Operators;

internal class RsaOaepWrapper : IKeyWrapper, IKeyUnwrapper
{
	private readonly AlgorithmIdentifier algId;

	private readonly IAsymmetricBlockCipher engine;

	public object AlgorithmDetails => algId;

	public RsaOaepWrapper(bool forWrapping, ICipherParameters parameters, DerObjectIdentifier digestOid)
	{
		AlgorithmIdentifier algorithmIdentifier = new AlgorithmIdentifier(digestOid, DerNull.Instance);
		algId = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdRsaesOaep, new RsaesOaepParameters(algorithmIdentifier, new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, algorithmIdentifier), RsaesOaepParameters.DefaultPSourceAlgorithm));
		engine = new OaepEncoding(new RsaBlindedEngine(), DigestUtilities.GetDigest(digestOid));
		engine.Init(forWrapping, parameters);
	}

	public IBlockResult Unwrap(byte[] cipherText, int offset, int length)
	{
		return new SimpleBlockResult(engine.ProcessBlock(cipherText, offset, length));
	}

	public IBlockResult Wrap(byte[] keyData)
	{
		return new SimpleBlockResult(engine.ProcessBlock(keyData, 0, keyData.Length));
	}
}
