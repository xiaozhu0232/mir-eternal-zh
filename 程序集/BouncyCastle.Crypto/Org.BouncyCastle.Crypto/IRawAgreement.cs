namespace Org.BouncyCastle.Crypto;

public interface IRawAgreement
{
	int AgreementSize { get; }

	void Init(ICipherParameters parameters);

	void CalculateAgreement(ICipherParameters publicKey, byte[] buf, int off);
}
