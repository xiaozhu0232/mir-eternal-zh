using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Agreement.Srp;

public class Srp6Server
{
	protected BigInteger N;

	protected BigInteger g;

	protected BigInteger v;

	protected SecureRandom random;

	protected IDigest digest;

	protected BigInteger A;

	protected BigInteger privB;

	protected BigInteger pubB;

	protected BigInteger u;

	protected BigInteger S;

	protected BigInteger M1;

	protected BigInteger M2;

	protected BigInteger Key;

	public virtual void Init(BigInteger N, BigInteger g, BigInteger v, IDigest digest, SecureRandom random)
	{
		this.N = N;
		this.g = g;
		this.v = v;
		this.random = random;
		this.digest = digest;
	}

	public virtual void Init(Srp6GroupParameters group, BigInteger v, IDigest digest, SecureRandom random)
	{
		Init(group.N, group.G, v, digest, random);
	}

	public virtual BigInteger GenerateServerCredentials()
	{
		BigInteger bigInteger = Srp6Utilities.CalculateK(digest, N, g);
		privB = SelectPrivateValue();
		pubB = bigInteger.Multiply(v).Mod(N).Add(g.ModPow(privB, N))
			.Mod(N);
		return pubB;
	}

	public virtual BigInteger CalculateSecret(BigInteger clientA)
	{
		A = Srp6Utilities.ValidatePublicValue(N, clientA);
		u = Srp6Utilities.CalculateU(digest, N, A, pubB);
		S = CalculateS();
		return S;
	}

	protected virtual BigInteger SelectPrivateValue()
	{
		return Srp6Utilities.GeneratePrivateValue(digest, N, g, random);
	}

	private BigInteger CalculateS()
	{
		return v.ModPow(u, N).Multiply(A).Mod(N)
			.ModPow(privB, N);
	}

	public virtual bool VerifyClientEvidenceMessage(BigInteger clientM1)
	{
		if (A == null || pubB == null || S == null)
		{
			throw new CryptoException("Impossible to compute and verify M1: some data are missing from the previous operations (A,B,S)");
		}
		BigInteger bigInteger = Srp6Utilities.CalculateM1(digest, N, A, pubB, S);
		if (bigInteger.Equals(clientM1))
		{
			M1 = clientM1;
			return true;
		}
		return false;
	}

	public virtual BigInteger CalculateServerEvidenceMessage()
	{
		if (A == null || M1 == null || S == null)
		{
			throw new CryptoException("Impossible to compute M2: some data are missing from the previous operations (A,M1,S)");
		}
		M2 = Srp6Utilities.CalculateM2(digest, N, A, M1, S);
		return M2;
	}

	public virtual BigInteger CalculateSessionKey()
	{
		if (S == null || M1 == null || M2 == null)
		{
			throw new CryptoException("Impossible to compute Key: some data are missing from the previous operations (S,M1,M2)");
		}
		Key = Srp6Utilities.CalculateKey(digest, N, S);
		return Key;
	}
}
