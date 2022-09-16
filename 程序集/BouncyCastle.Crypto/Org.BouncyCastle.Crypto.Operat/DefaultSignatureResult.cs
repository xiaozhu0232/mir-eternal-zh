namespace Org.BouncyCastle.Crypto.Operators;

public class DefaultSignatureResult : IBlockResult
{
	private readonly ISigner mSigner;

	public DefaultSignatureResult(ISigner signer)
	{
		mSigner = signer;
	}

	public byte[] Collect()
	{
		return mSigner.GenerateSignature();
	}

	public int Collect(byte[] sig, int sigOff)
	{
		byte[] array = Collect();
		array.CopyTo(sig, sigOff);
		return array.Length;
	}
}
