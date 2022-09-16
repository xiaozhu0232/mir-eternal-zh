using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Crypto.Operators;

public class Asn1KeyWrapper : IKeyWrapper
{
	private string algorithm;

	private IKeyWrapper wrapper;

	public object AlgorithmDetails => wrapper.AlgorithmDetails;

	public Asn1KeyWrapper(string algorithm, X509Certificate cert)
	{
		this.algorithm = algorithm;
		wrapper = KeyWrapperUtil.WrapperForName(algorithm, cert.GetPublicKey());
	}

	public IBlockResult Wrap(byte[] keyData)
	{
		return wrapper.Wrap(keyData);
	}
}
