using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Encodings;

public class Pkcs1Encoding : IAsymmetricBlockCipher
{
	public const string StrictLengthEnabledProperty = "Org.BouncyCastle.Pkcs1.Strict";

	private const int HeaderLength = 10;

	private static readonly bool[] strictLengthEnabled;

	private SecureRandom random;

	private IAsymmetricBlockCipher engine;

	private bool forEncryption;

	private bool forPrivateKey;

	private bool useStrictLength;

	private int pLen = -1;

	private byte[] fallback = null;

	private byte[] blockBuffer = null;

	public static bool StrictLengthEnabled
	{
		get
		{
			return strictLengthEnabled[0];
		}
		set
		{
			strictLengthEnabled[0] = value;
		}
	}

	public string AlgorithmName => engine.AlgorithmName + "/PKCS1Padding";

	static Pkcs1Encoding()
	{
		string environmentVariable = Platform.GetEnvironmentVariable("Org.BouncyCastle.Pkcs1.Strict");
		strictLengthEnabled = new bool[1] { environmentVariable == null || Platform.EqualsIgnoreCase("true", environmentVariable) };
	}

	public Pkcs1Encoding(IAsymmetricBlockCipher cipher)
	{
		engine = cipher;
		useStrictLength = StrictLengthEnabled;
	}

	public Pkcs1Encoding(IAsymmetricBlockCipher cipher, int pLen)
	{
		engine = cipher;
		useStrictLength = StrictLengthEnabled;
		this.pLen = pLen;
	}

	public Pkcs1Encoding(IAsymmetricBlockCipher cipher, byte[] fallback)
	{
		engine = cipher;
		useStrictLength = StrictLengthEnabled;
		this.fallback = fallback;
		pLen = fallback.Length;
	}

	public IAsymmetricBlockCipher GetUnderlyingCipher()
	{
		return engine;
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		AsymmetricKeyParameter asymmetricKeyParameter;
		if (parameters is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
			random = parametersWithRandom.Random;
			asymmetricKeyParameter = (AsymmetricKeyParameter)parametersWithRandom.Parameters;
		}
		else
		{
			random = new SecureRandom();
			asymmetricKeyParameter = (AsymmetricKeyParameter)parameters;
		}
		engine.Init(forEncryption, parameters);
		forPrivateKey = asymmetricKeyParameter.IsPrivate;
		this.forEncryption = forEncryption;
		blockBuffer = new byte[engine.GetOutputBlockSize()];
		if (pLen > 0 && fallback == null && random == null)
		{
			throw new ArgumentException("encoder requires random");
		}
	}

	public int GetInputBlockSize()
	{
		int inputBlockSize = engine.GetInputBlockSize();
		if (!forEncryption)
		{
			return inputBlockSize;
		}
		return inputBlockSize - 10;
	}

	public int GetOutputBlockSize()
	{
		int outputBlockSize = engine.GetOutputBlockSize();
		if (!forEncryption)
		{
			return outputBlockSize - 10;
		}
		return outputBlockSize;
	}

	public byte[] ProcessBlock(byte[] input, int inOff, int length)
	{
		if (!forEncryption)
		{
			return DecodeBlock(input, inOff, length);
		}
		return EncodeBlock(input, inOff, length);
	}

	private byte[] EncodeBlock(byte[] input, int inOff, int inLen)
	{
		if (inLen > GetInputBlockSize())
		{
			throw new ArgumentException("input data too large", "inLen");
		}
		byte[] array = new byte[engine.GetInputBlockSize()];
		if (forPrivateKey)
		{
			array[0] = 1;
			for (int i = 1; i != array.Length - inLen - 1; i++)
			{
				array[i] = byte.MaxValue;
			}
		}
		else
		{
			random.NextBytes(array);
			array[0] = 2;
			for (int j = 1; j != array.Length - inLen - 1; j++)
			{
				while (array[j] == 0)
				{
					array[j] = (byte)random.NextInt();
				}
			}
		}
		array[array.Length - inLen - 1] = 0;
		Array.Copy(input, inOff, array, array.Length - inLen, inLen);
		return engine.ProcessBlock(array, 0, array.Length);
	}

	private static int CheckPkcs1Encoding(byte[] encoded, int pLen)
	{
		int num = 0;
		num |= encoded[0] ^ 2;
		int num2 = encoded.Length - (pLen + 1);
		for (int i = 1; i < num2; i++)
		{
			int num3 = encoded[i];
			num3 |= num3 >> 1;
			num3 |= num3 >> 2;
			num3 |= num3 >> 4;
			num |= (num3 & 1) - 1;
		}
		num |= encoded[encoded.Length - (pLen + 1)];
		num |= num >> 1;
		num |= num >> 2;
		num |= num >> 4;
		return ~((num & 1) - 1);
	}

	private byte[] DecodeBlockOrRandom(byte[] input, int inOff, int inLen)
	{
		if (!forPrivateKey)
		{
			throw new InvalidCipherTextException("sorry, this method is only for decryption, not for signing");
		}
		byte[] array = engine.ProcessBlock(input, inOff, inLen);
		byte[] array2;
		if (fallback == null)
		{
			array2 = new byte[pLen];
			random.NextBytes(array2);
		}
		else
		{
			array2 = fallback;
		}
		byte[] array3 = ((useStrictLength & (array.Length != engine.GetOutputBlockSize())) ? blockBuffer : array);
		int num = CheckPkcs1Encoding(array3, pLen);
		byte[] array4 = new byte[pLen];
		for (int i = 0; i < pLen; i++)
		{
			array4[i] = (byte)((array3[i + (array3.Length - pLen)] & ~num) | (array2[i] & num));
		}
		Arrays.Fill(array3, 0);
		return array4;
	}

	private byte[] DecodeBlock(byte[] input, int inOff, int inLen)
	{
		if (pLen != -1)
		{
			return DecodeBlockOrRandom(input, inOff, inLen);
		}
		byte[] array = engine.ProcessBlock(input, inOff, inLen);
		bool flag = useStrictLength & (array.Length != engine.GetOutputBlockSize());
		byte[] array2 = ((array.Length >= GetOutputBlockSize()) ? array : blockBuffer);
		byte b = (byte)((!forPrivateKey) ? 1u : 2u);
		byte b2 = array2[0];
		bool flag2 = b2 != b;
		int num = FindStart(b2, array2);
		num++;
		if (flag2 || num < 10)
		{
			Arrays.Fill(array2, 0);
			throw new InvalidCipherTextException("block incorrect");
		}
		if (flag)
		{
			Arrays.Fill(array2, 0);
			throw new InvalidCipherTextException("block incorrect size");
		}
		byte[] array3 = new byte[array2.Length - num];
		Array.Copy(array2, num, array3, 0, array3.Length);
		return array3;
	}

	private int FindStart(byte type, byte[] block)
	{
		int num = -1;
		bool flag = false;
		for (int i = 1; i != block.Length; i++)
		{
			byte b = block[i];
			if (b == 0 && num < 0)
			{
				num = i;
			}
			flag = flag || (type == 1 && num < 0 && b != byte.MaxValue);
		}
		if (!flag)
		{
			return num;
		}
		return -1;
	}
}
