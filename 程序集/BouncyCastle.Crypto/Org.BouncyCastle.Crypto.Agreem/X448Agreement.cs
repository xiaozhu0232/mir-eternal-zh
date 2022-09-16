using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Agreement;

public sealed class X448Agreement : IRawAgreement
{
	private X448PrivateKeyParameters privateKey;

	public int AgreementSize => X448PrivateKeyParameters.SecretSize;

	public void Init(ICipherParameters parameters)
	{
		privateKey = (X448PrivateKeyParameters)parameters;
	}

	public void CalculateAgreement(ICipherParameters publicKey, byte[] buf, int off)
	{
		privateKey.GenerateSecret((X448PublicKeyParameters)publicKey, buf, off);
	}
}
