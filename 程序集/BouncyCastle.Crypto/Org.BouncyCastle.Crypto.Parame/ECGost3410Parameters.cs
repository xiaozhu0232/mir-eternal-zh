using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ECGost3410Parameters : ECNamedDomainParameters
{
	private readonly DerObjectIdentifier _publicKeyParamSet;

	private readonly DerObjectIdentifier _digestParamSet;

	private readonly DerObjectIdentifier _encryptionParamSet;

	public DerObjectIdentifier PublicKeyParamSet => _publicKeyParamSet;

	public DerObjectIdentifier DigestParamSet => _digestParamSet;

	public DerObjectIdentifier EncryptionParamSet => _encryptionParamSet;

	public ECGost3410Parameters(ECNamedDomainParameters dp, DerObjectIdentifier publicKeyParamSet, DerObjectIdentifier digestParamSet, DerObjectIdentifier encryptionParamSet)
		: base(dp.Name, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
	{
		_publicKeyParamSet = publicKeyParamSet;
		_digestParamSet = digestParamSet;
		_encryptionParamSet = encryptionParamSet;
	}

	public ECGost3410Parameters(ECDomainParameters dp, DerObjectIdentifier publicKeyParamSet, DerObjectIdentifier digestParamSet, DerObjectIdentifier encryptionParamSet)
		: base(publicKeyParamSet, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
	{
		_publicKeyParamSet = publicKeyParamSet;
		_digestParamSet = digestParamSet;
		_encryptionParamSet = encryptionParamSet;
	}
}
