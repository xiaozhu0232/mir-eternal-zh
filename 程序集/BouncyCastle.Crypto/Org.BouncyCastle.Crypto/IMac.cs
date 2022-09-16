namespace Org.BouncyCastle.Crypto;

public interface IMac
{
	string AlgorithmName { get; }

	void Init(ICipherParameters parameters);

	int GetMacSize();

	void Update(byte input);

	void BlockUpdate(byte[] input, int inOff, int len);

	int DoFinal(byte[] output, int outOff);

	void Reset();
}
