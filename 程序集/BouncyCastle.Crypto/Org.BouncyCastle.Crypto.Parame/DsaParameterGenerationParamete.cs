using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DsaParameterGenerationParameters
{
	public const int DigitalSignatureUsage = 1;

	public const int KeyEstablishmentUsage = 2;

	private readonly int l;

	private readonly int n;

	private readonly int certainty;

	private readonly SecureRandom random;

	private readonly int usageIndex;

	public virtual int L => l;

	public virtual int N => n;

	public virtual int UsageIndex => usageIndex;

	public virtual int Certainty => certainty;

	public virtual SecureRandom Random => random;

	public DsaParameterGenerationParameters(int L, int N, int certainty, SecureRandom random)
		: this(L, N, certainty, random, -1)
	{
	}

	public DsaParameterGenerationParameters(int L, int N, int certainty, SecureRandom random, int usageIndex)
	{
		l = L;
		n = N;
		this.certainty = certainty;
		this.random = random;
		this.usageIndex = usageIndex;
	}
}
