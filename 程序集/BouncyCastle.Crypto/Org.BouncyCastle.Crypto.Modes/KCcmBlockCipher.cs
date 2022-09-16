using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes;

public class KCcmBlockCipher : IAeadBlockCipher, IAeadCipher
{
	private static readonly int BYTES_IN_INT = 4;

	private static readonly int BITS_IN_BYTE = 8;

	private static readonly int MAX_MAC_BIT_LENGTH = 512;

	private static readonly int MIN_MAC_BIT_LENGTH = 64;

	private IBlockCipher engine;

	private int macSize;

	private bool forEncryption;

	private byte[] initialAssociatedText;

	private byte[] mac;

	private byte[] macBlock;

	private byte[] nonce;

	private byte[] G1;

	private byte[] buffer;

	private byte[] s;

	private byte[] counter;

	private readonly MemoryStream associatedText = new MemoryStream();

	private readonly MemoryStream data = new MemoryStream();

	private int Nb_ = 4;

	public virtual string AlgorithmName => engine.AlgorithmName + "/KCCM";

	private void setNb(int Nb)
	{
		if (Nb == 4 || Nb == 6 || Nb == 8)
		{
			Nb_ = Nb;
			return;
		}
		throw new ArgumentException("Nb = 4 is recommended by DSTU7624 but can be changed to only 6 or 8 in this implementation");
	}

	public KCcmBlockCipher(IBlockCipher engine)
		: this(engine, 4)
	{
	}

	public KCcmBlockCipher(IBlockCipher engine, int Nb)
	{
		this.engine = engine;
		macSize = engine.GetBlockSize();
		nonce = new byte[engine.GetBlockSize()];
		initialAssociatedText = new byte[engine.GetBlockSize()];
		mac = new byte[engine.GetBlockSize()];
		macBlock = new byte[engine.GetBlockSize()];
		G1 = new byte[engine.GetBlockSize()];
		buffer = new byte[engine.GetBlockSize()];
		s = new byte[engine.GetBlockSize()];
		counter = new byte[engine.GetBlockSize()];
		setNb(Nb);
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		ICipherParameters parameters2;
		if (parameters is AeadParameters)
		{
			AeadParameters aeadParameters = (AeadParameters)parameters;
			if (aeadParameters.MacSize > MAX_MAC_BIT_LENGTH || aeadParameters.MacSize < MIN_MAC_BIT_LENGTH || aeadParameters.MacSize % 8 != 0)
			{
				throw new ArgumentException("Invalid mac size specified");
			}
			nonce = aeadParameters.GetNonce();
			macSize = aeadParameters.MacSize / BITS_IN_BYTE;
			initialAssociatedText = aeadParameters.GetAssociatedText();
			parameters2 = aeadParameters.Key;
		}
		else
		{
			if (!(parameters is ParametersWithIV))
			{
				throw new ArgumentException("Invalid parameters specified");
			}
			nonce = ((ParametersWithIV)parameters).GetIV();
			macSize = engine.GetBlockSize();
			initialAssociatedText = null;
			parameters2 = ((ParametersWithIV)parameters).Parameters;
		}
		mac = new byte[macSize];
		this.forEncryption = forEncryption;
		engine.Init(forEncryption: true, parameters2);
		counter[0] = 1;
		if (initialAssociatedText != null)
		{
			ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
		}
	}

	public virtual int GetBlockSize()
	{
		return engine.GetBlockSize();
	}

	public virtual IBlockCipher GetUnderlyingCipher()
	{
		return engine;
	}

	public virtual void ProcessAadByte(byte input)
	{
		associatedText.WriteByte(input);
	}

	public virtual void ProcessAadBytes(byte[] input, int inOff, int len)
	{
		associatedText.Write(input, inOff, len);
	}

	private void ProcessAAD(byte[] assocText, int assocOff, int assocLen, int dataLen)
	{
		if (assocLen - assocOff < engine.GetBlockSize())
		{
			throw new ArgumentException("authText buffer too short");
		}
		if (assocLen % engine.GetBlockSize() != 0)
		{
			throw new ArgumentException("padding not supported");
		}
		Array.Copy(nonce, 0, G1, 0, nonce.Length - Nb_ - 1);
		intToBytes(dataLen, buffer, 0);
		Array.Copy(buffer, 0, G1, nonce.Length - Nb_ - 1, BYTES_IN_INT);
		G1[G1.Length - 1] = getFlag(authTextPresents: true, macSize);
		engine.ProcessBlock(G1, 0, macBlock, 0);
		intToBytes(assocLen, buffer, 0);
		if (assocLen <= engine.GetBlockSize() - Nb_)
		{
			for (int i = 0; i < assocLen; i++)
			{
				byte[] array;
				byte[] array2 = (array = buffer);
				int num = i + Nb_;
				nint num2 = num;
				array2[num] = (byte)(array[num2] ^ assocText[assocOff + i]);
			}
			for (int j = 0; j < engine.GetBlockSize(); j++)
			{
				byte[] array;
				byte[] array3 = (array = macBlock);
				int num3 = j;
				nint num2 = num3;
				array3[num3] = (byte)(array[num2] ^ buffer[j]);
			}
			engine.ProcessBlock(macBlock, 0, macBlock, 0);
			return;
		}
		for (int k = 0; k < engine.GetBlockSize(); k++)
		{
			byte[] array;
			byte[] array4 = (array = macBlock);
			int num4 = k;
			nint num2 = num4;
			array4[num4] = (byte)(array[num2] ^ buffer[k]);
		}
		engine.ProcessBlock(macBlock, 0, macBlock, 0);
		for (int num5 = assocLen; num5 != 0; num5 -= engine.GetBlockSize())
		{
			for (int l = 0; l < engine.GetBlockSize(); l++)
			{
				byte[] array;
				byte[] array5 = (array = macBlock);
				int num6 = l;
				nint num2 = num6;
				array5[num6] = (byte)(array[num2] ^ assocText[l + assocOff]);
			}
			engine.ProcessBlock(macBlock, 0, macBlock, 0);
			assocOff += engine.GetBlockSize();
		}
	}

	public virtual int ProcessByte(byte input, byte[] output, int outOff)
	{
		data.WriteByte(input);
		return 0;
	}

	public virtual int ProcessBytes(byte[] input, int inOff, int inLen, byte[] output, int outOff)
	{
		Check.DataLength(input, inOff, inLen, "input buffer too short");
		data.Write(input, inOff, inLen);
		return 0;
	}

	public int ProcessPacket(byte[] input, int inOff, int len, byte[] output, int outOff)
	{
		Check.DataLength(input, inOff, len, "input buffer too short");
		Check.OutputLength(output, outOff, len, "output buffer too short");
		if (associatedText.Length > 0)
		{
			byte[] assocText = associatedText.GetBuffer();
			int assocLen = (int)associatedText.Length;
			int dataLen = (int)(forEncryption ? data.Length : ((int)data.Length - macSize));
			ProcessAAD(assocText, 0, assocLen, dataLen);
		}
		if (forEncryption)
		{
			Check.DataLength(len % engine.GetBlockSize() != 0, "partial blocks not supported");
			CalculateMac(input, inOff, len);
			engine.ProcessBlock(nonce, 0, s, 0);
			int num = len;
			while (num > 0)
			{
				ProcessBlock(input, inOff, len, output, outOff);
				num -= engine.GetBlockSize();
				inOff += engine.GetBlockSize();
				outOff += engine.GetBlockSize();
			}
			for (int i = 0; i < counter.Length; i++)
			{
				byte[] array;
				byte[] array2 = (array = s);
				int num2 = i;
				nint num3 = num2;
				array2[num2] = (byte)(array[num3] + counter[i]);
			}
			engine.ProcessBlock(s, 0, buffer, 0);
			for (int j = 0; j < macSize; j++)
			{
				output[outOff + j] = (byte)(buffer[j] ^ macBlock[j]);
			}
			Array.Copy(macBlock, 0, mac, 0, macSize);
			Reset();
			return len + macSize;
		}
		Check.DataLength((len - macSize) % engine.GetBlockSize() != 0, "partial blocks not supported");
		engine.ProcessBlock(nonce, 0, s, 0);
		int num4 = len / engine.GetBlockSize();
		for (int k = 0; k < num4; k++)
		{
			ProcessBlock(input, inOff, len, output, outOff);
			inOff += engine.GetBlockSize();
			outOff += engine.GetBlockSize();
		}
		if (len > inOff)
		{
			for (int l = 0; l < counter.Length; l++)
			{
				byte[] array;
				byte[] array3 = (array = s);
				int num5 = l;
				nint num3 = num5;
				array3[num5] = (byte)(array[num3] + counter[l]);
			}
			engine.ProcessBlock(s, 0, buffer, 0);
			for (int m = 0; m < macSize; m++)
			{
				output[outOff + m] = (byte)(buffer[m] ^ input[inOff + m]);
			}
			outOff += macSize;
		}
		for (int n = 0; n < counter.Length; n++)
		{
			byte[] array;
			byte[] array4 = (array = s);
			int num6 = n;
			nint num3 = num6;
			array4[num6] = (byte)(array[num3] + counter[n]);
		}
		engine.ProcessBlock(s, 0, buffer, 0);
		Array.Copy(output, outOff - macSize, buffer, 0, macSize);
		CalculateMac(output, 0, outOff - macSize);
		Array.Copy(macBlock, 0, mac, 0, macSize);
		byte[] array5 = new byte[macSize];
		Array.Copy(buffer, 0, array5, 0, macSize);
		if (!Arrays.ConstantTimeAreEqual(mac, array5))
		{
			throw new InvalidCipherTextException("mac check failed");
		}
		Reset();
		return len - macSize;
	}

	private void ProcessBlock(byte[] input, int inOff, int len, byte[] output, int outOff)
	{
		for (int i = 0; i < counter.Length; i++)
		{
			byte[] array;
			byte[] array2 = (array = s);
			int num = i;
			nint num2 = num;
			array2[num] = (byte)(array[num2] + counter[i]);
		}
		engine.ProcessBlock(s, 0, buffer, 0);
		for (int j = 0; j < engine.GetBlockSize(); j++)
		{
			output[outOff + j] = (byte)(buffer[j] ^ input[inOff + j]);
		}
	}

	private void CalculateMac(byte[] authText, int authOff, int len)
	{
		int num = len;
		while (num > 0)
		{
			for (int i = 0; i < engine.GetBlockSize(); i++)
			{
				byte[] array;
				byte[] array2 = (array = macBlock);
				int num2 = i;
				nint num3 = num2;
				array2[num2] = (byte)(array[num3] ^ authText[authOff + i]);
			}
			engine.ProcessBlock(macBlock, 0, macBlock, 0);
			num -= engine.GetBlockSize();
			authOff += engine.GetBlockSize();
		}
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		byte[] input = data.GetBuffer();
		int len = (int)data.Length;
		int result = ProcessPacket(input, 0, len, output, outOff);
		Reset();
		return result;
	}

	public virtual byte[] GetMac()
	{
		return Arrays.Clone(mac);
	}

	public virtual int GetUpdateOutputSize(int len)
	{
		return len;
	}

	public virtual int GetOutputSize(int len)
	{
		return len + macSize;
	}

	public virtual void Reset()
	{
		Arrays.Fill(G1, 0);
		Arrays.Fill(buffer, 0);
		Arrays.Fill(counter, 0);
		Arrays.Fill(macBlock, 0);
		counter[0] = 1;
		data.SetLength(0L);
		associatedText.SetLength(0L);
		if (initialAssociatedText != null)
		{
			ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
		}
	}

	private void intToBytes(int num, byte[] outBytes, int outOff)
	{
		outBytes[outOff + 3] = (byte)(num >> 24);
		outBytes[outOff + 2] = (byte)(num >> 16);
		outBytes[outOff + 1] = (byte)(num >> 8);
		outBytes[outOff] = (byte)num;
	}

	private byte getFlag(bool authTextPresents, int macSize)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (authTextPresents)
		{
			stringBuilder.Append("1");
		}
		else
		{
			stringBuilder.Append("0");
		}
		switch (macSize)
		{
		case 8:
			stringBuilder.Append("010");
			break;
		case 16:
			stringBuilder.Append("011");
			break;
		case 32:
			stringBuilder.Append("100");
			break;
		case 48:
			stringBuilder.Append("101");
			break;
		case 64:
			stringBuilder.Append("110");
			break;
		}
		string text = Convert.ToString(Nb_ - 1, 2);
		while (text.Length < 4)
		{
			text = new StringBuilder(text).Insert(0, "0").ToString();
		}
		stringBuilder.Append(text);
		return (byte)Convert.ToInt32(stringBuilder.ToString(), 2);
	}
}
