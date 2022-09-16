using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DHKeyGenerationParameters : KeyGenerationParameters
{
	private readonly DHParameters parameters;

	public DHParameters Parameters => parameters;

	public DHKeyGenerationParameters(SecureRandom random, DHParameters parameters)
		: base(random, GetStrength(parameters))
	{
		this.parameters = parameters;
	}

	internal static int GetStrength(DHParameters parameters)
	{
		if (parameters.L == 0)
		{
			return parameters.P.BitLength;
		}
		return parameters.L;
	}
}
