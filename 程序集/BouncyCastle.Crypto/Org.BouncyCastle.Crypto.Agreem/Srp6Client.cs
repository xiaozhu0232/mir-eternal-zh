using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Agreement.Srp;

public class Srp6Client
{
	protected BigInteger N;

	protected BigInteger g;

	protected BigInteger privA;

	protected BigInteger pubA;

	protected BigInteger B;

	protected BigInteger x;

	protected BigInteger u;

	protected BigInteger S;

	protected BigInteger M1;

	protected BigInteger M2;

	protected BigInteger Key;

	protected IDigest digest;

	protected SecureRandom random;

	public virtual void Init(BigInteger N, BigInteger g, IDigest digest, SecureRandom random)
	{
		this.N = N;
		this.g = g;
		this.digest = digest;
		this.random = random;
	}

	public virtual void Init(Srp6GroupParameters group, IDigest digest, SecureRandom random)
	{
		Init(group.N, group.G, digest, random);
	}

	public virtual BigInteger GenerateClientCredentials(byte[] salt, byte[] identity, byte[] password)
	{
		x = Srp6Utilities.CalculateX(digest, N, salt, identity, password);
		privA = SelectPrivateValue();
		pubA = g.ModPow(privA, N);
		return pubA;
	}

	public virtual BigInteger CalculateSecret(BigInteger serverB)
	{
		B = Srp6Utilities.ValidatePublicValue(N, serverB);
		u = Srp6Utilities.CalculateU(digest, N, pubA, B);
		S = CalculateS();
		return S;
	}

	protected virtual BigInteger SelectPrivateValue()
	{
		return Srp6Utilities.GeneratePrivateValue(digest, N, g, random);
	}

	private BigInteger CalculateS()
	{
		BigInteger val = Srp6Utilities.CalculateK(digest, N, g);
		BigInteger e = u.Multiply(x).Add(privA);
		BigInteger n = g.ModPow(x, N).Multiply(val).Mod(N);
		return B.Subtract(n).Mod(N).ModPow(e, N);
	}

	public virtual BigInteger CalculateClientEvidenceMessage()
	{
		if (pubA == null || B == null || S == null)
		{
			throw new CryptoException("Impossible to compute M1: some data are missing from the previous operations (A,B,S)");
		}
		M1 = Srp6Utilities.CalculateM1(digest, N, pubA, B, S);
		return M1;
	}

	public virtual bool VerifyServerEvidenceMessage(BigInteger serverM2)
	{
		if (pubA == null || M1 == null || S == null)
		{
			throw new CryptoException("Impossible to compute and verify M2: some data are missing from the previous operations (A,M1,S)");
		}
		BigInteger bigInteger = Srp6Utilities.CalculateM2(digest, N, pubA, M1, S);
		if (bigInteger.Equals(serverM2))
		{
			M2 = serverM2;
			return true;
		}
		return false;
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
