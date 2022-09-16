using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ElGamalKeyGenerationParameters : KeyGenerationParameters
{
	private readonly ElGamalParameters parameters;

	public ElGamalParameters Parameters => parameters;

	public ElGamalKeyGenerationParameters(SecureRandom random, ElGamalParameters parameters)
		: base(random, GetStrength(parameters))
	{
		this.parameters = parameters;
	}

	internal static int GetStrength(ElGamalParameters parameters)
	{
		if (parameters.L == 0)
		{
			return parameters.P.BitLength;
		}
		return parameters.L;
	}
}
