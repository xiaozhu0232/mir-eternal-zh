using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Agreement.Srp;

public class Srp6VerifierGenerator
{
	protected BigInteger N;

	protected BigInteger g;

	protected IDigest digest;

	public virtual void Init(BigInteger N, BigInteger g, IDigest digest)
	{
		this.N = N;
		this.g = g;
		this.digest = digest;
	}

	public virtual void Init(Srp6GroupParameters group, IDigest digest)
	{
		Init(group.N, group.G, digest);
	}

	public virtual BigInteger GenerateVerifier(byte[] salt, byte[] identity, byte[] password)
	{
		BigInteger e = Srp6Utilities.CalculateX(digest, N, salt, identity, password);
		return g.ModPow(e, N);
	}
}
