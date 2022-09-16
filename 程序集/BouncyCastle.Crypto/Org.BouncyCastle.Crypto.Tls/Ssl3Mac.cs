using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class Ssl3Mac : IMac
{
	private const byte IPAD_BYTE = 54;

	private const byte OPAD_BYTE = 92;

	internal static readonly byte[] IPAD = GenPad(54, 48);

	internal static readonly byte[] OPAD = GenPad(92, 48);

	private readonly IDigest digest;

	private readonly int padLength;

	private byte[] secret;

	public virtual string AlgorithmName => digest.AlgorithmName + "/SSL3MAC";

	public Ssl3Mac(IDigest digest)
	{
		this.digest = digest;
		if (digest.GetDigestSize() == 20)
		{
			padLength = 40;
		}
		else
		{
			padLength = 48;
		}
	}

	public virtual void Init(ICipherParameters parameters)
	{
		secret = Arrays.Clone(((KeyParameter)parameters).GetKey());
		Reset();
	}

	public virtual int GetMacSize()
	{
		return digest.GetDigestSize();
	}

	public virtual void Update(byte input)
	{
		digest.Update(input);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int len)
	{
		digest.BlockUpdate(input, inOff, len);
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		byte[] array = new byte[digest.GetDigestSize()];
		digest.DoFinal(array, 0);
		digest.BlockUpdate(secret, 0, secret.Length);
		digest.BlockUpdate(OPAD, 0, padLength);
		digest.BlockUpdate(array, 0, array.Length);
		int result = digest.DoFinal(output, outOff);
		Reset();
		return result;
	}

	public virtual void Reset()
	{
		digest.Reset();
		digest.BlockUpdate(secret, 0, secret.Length);
		digest.BlockUpdate(IPAD, 0, padLength);
	}

	private static byte[] GenPad(byte b, int count)
	{
		byte[] array = new byte[count];
		Arrays.Fill(array, b);
		return array;
	}
}
