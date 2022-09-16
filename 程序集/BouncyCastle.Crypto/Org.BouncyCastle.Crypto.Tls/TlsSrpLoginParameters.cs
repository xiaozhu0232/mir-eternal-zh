using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsSrpLoginParameters
{
	protected readonly Srp6GroupParameters mGroup;

	protected readonly BigInteger mVerifier;

	protected readonly byte[] mSalt;

	public virtual Srp6GroupParameters Group => mGroup;

	public virtual byte[] Salt => mSalt;

	public virtual BigInteger Verifier => mVerifier;

	public TlsSrpLoginParameters(Srp6GroupParameters group, BigInteger verifier, byte[] salt)
	{
		mGroup = group;
		mVerifier = verifier;
		mSalt = salt;
	}
}
