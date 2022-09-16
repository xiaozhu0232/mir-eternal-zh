using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ECKeyGenerationParameters : KeyGenerationParameters
{
	private readonly ECDomainParameters domainParams;

	private readonly DerObjectIdentifier publicKeyParamSet;

	public ECDomainParameters DomainParameters => domainParams;

	public DerObjectIdentifier PublicKeyParamSet => publicKeyParamSet;

	public ECKeyGenerationParameters(ECDomainParameters domainParameters, SecureRandom random)
		: base(random, domainParameters.N.BitLength)
	{
		domainParams = domainParameters;
	}

	public ECKeyGenerationParameters(DerObjectIdentifier publicKeyParamSet, SecureRandom random)
		: this(ECKeyParameters.LookupParameters(publicKeyParamSet), random)
	{
		this.publicKeyParamSet = publicKeyParamSet;
	}
}
