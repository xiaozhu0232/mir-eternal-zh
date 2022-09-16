using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Blake2sDigest : IDigest
{
	private const int ROUNDS = 10;

	private const int BLOCK_LENGTH_BYTES = 64;

	private static readonly uint[] blake2s_IV = new uint[8] { 1779033703u, 3144134277u, 1013904242u, 2773480762u, 1359893119u, 2600822924u, 528734635u, 1541459225u };

	private static readonly byte[,] blake2s_sigma = new byte[10, 16]
	{
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15
		},
		{
			14, 10, 4, 8, 9, 15, 13, 6, 1, 12,
			0, 2, 11, 7, 5, 3
		},
		{
			11, 8, 12, 0, 5, 2, 15, 13, 10, 14,
			3, 6, 7, 1, 9, 4
		},
		{
			7, 9, 3, 1, 13, 12, 11, 14, 2, 6,
			5, 10, 4, 0, 15, 8
		},
		{
			9, 0, 5, 7, 2, 4, 10, 15, 14, 1,
			11, 12, 6, 8, 3, 13
		},
		{
			2, 12, 6, 10, 0, 11, 8, 3, 4, 13,
			7, 5, 15, 14, 1, 9
		},
		{
			12, 5, 1, 15, 14, 13, 4, 10, 0, 7,
			6, 3, 9, 2, 8, 11
		},
		{
			13, 11, 7, 14, 12, 1, 3, 9, 5, 0,
			15, 4, 8, 6, 2, 10
		},
		{
			6, 15, 14, 9, 11, 3, 0, 8, 12, 2,
			13, 7, 1, 4, 10, 5
		},
		{
			10, 2, 8, 4, 7, 6, 1, 5, 15, 11,
			9, 14, 3, 12, 13, 0
		}
	};

	private int digestLength = 32;

	private int keyLength = 0;

	private byte[] salt = null;

	private byte[] personalization = null;

	private byte[] key = null;

	private byte[] buffer = null;

	private int bufferPos = 0;

	private uint[] internalState = new uint[16];

	private uint[] chainValue = null;

	private uint t0 = 0u;

	private uint t1 = 0u;

	private uint f0 = 0u;

	public virtual string AlgorithmName => "BLAKE2s";

	public Blake2sDigest()
		: this(256)
	{
	}

	public Blake2sDigest(Blake2sDigest digest)
	{
		bufferPos = digest.bufferPos;
		buffer = Arrays.Clone(digest.buffer);
		keyLength = digest.keyLength;
		key = Arrays.Clone(digest.key);
		digestLength = digest.digestLength;
		chainValue = Arrays.Clone(digest.chainValue);
		personalization = Arrays.Clone(digest.personalization);
	}

	public Blake2sDigest(int digestBits)
	{
		if (digestBits < 8 || digestBits > 256 || digestBits % 8 != 0)
		{
			throw new ArgumentException("BLAKE2s digest bit length must be a multiple of 8 and not greater than 256");
		}
		buffer = new byte[64];
		keyLength = 0;
		digestLength = digestBits / 8;
		Init();
	}

	public Blake2sDigest(byte[] key)
	{
		buffer = new byte[64];
		if (key != null)
		{
			if (key.Length > 32)
			{
				throw new ArgumentException("Keys > 32 are not supported");
			}
			this.key = new byte[key.Length];
			Array.Copy(key, 0, this.key, 0, key.Length);
			keyLength = key.Length;
			Array.Copy(key, 0, buffer, 0, key.Length);
			bufferPos = 64;
		}
		digestLength = 32;
		Init();
	}

	public Blake2sDigest(byte[] key, int digestBytes, byte[] salt, byte[] personalization)
	{
		if (digestBytes < 1 || digestBytes > 32)
		{
			throw new ArgumentException("Invalid digest length (required: 1 - 32)");
		}
		digestLength = digestBytes;
		buffer = new byte[64];
		if (salt != null)
		{
			if (salt.Length != 8)
			{
				throw new ArgumentException("Salt length must be exactly 8 bytes");
			}
			this.salt = new byte[8];
			Array.Copy(salt, 0, this.salt, 0, salt.Length);
		}
		if (personalization != null)
		{
			if (personalization.Length != 8)
			{
				throw new ArgumentException("Personalization length must be exactly 8 bytes");
			}
			this.personalization = new byte[8];
			Array.Copy(personalization, 0, this.personalization, 0, personalization.Length);
		}
		if (key != null)
		{
			if (key.Length > 32)
			{
				throw new ArgumentException("Keys > 32 bytes are not supported");
			}
			this.key = new byte[key.Length];
			Array.Copy(key, 0, this.key, 0, key.Length);
			keyLength = key.Length;
			Array.Copy(key, 0, buffer, 0, key.Length);
			bufferPos = 64;
		}
		Init();
	}

	private void Init()
	{
		if (chainValue == null)
		{
			chainValue = new uint[8];
			chainValue[0] = blake2s_IV[0] ^ ((uint)(digestLength | (keyLength << 8)) | 0x1010000u);
			chainValue[1] = blake2s_IV[1];
			chainValue[2] = blake2s_IV[2];
			chainValue[3] = blake2s_IV[3];
			chainValue[4] = blake2s_IV[4];
			chainValue[5] = blake2s_IV[5];
			if (salt != null)
			{
				uint[] array;
				(array = chainValue)[4] = array[4] ^ Pack.LE_To_UInt32(salt, 0);
				(array = chainValue)[5] = array[5] ^ Pack.LE_To_UInt32(salt, 4);
			}
			chainValue[6] = blake2s_IV[6];
			chainValue[7] = blake2s_IV[7];
			if (personalization != null)
			{
				uint[] array;
				(array = chainValue)[6] = array[6] ^ Pack.LE_To_UInt32(personalization, 0);
				(array = chainValue)[7] = array[7] ^ Pack.LE_To_UInt32(personalization, 4);
			}
		}
	}

	private void InitializeInternalState()
	{
		Array.Copy(chainValue, 0, internalState, 0, chainValue.Length);
		Array.Copy(blake2s_IV, 0, internalState, chainValue.Length, 4);
		internalState[12] = t0 ^ blake2s_IV[4];
		internalState[13] = t1 ^ blake2s_IV[5];
		internalState[14] = f0 ^ blake2s_IV[6];
		internalState[15] = blake2s_IV[7];
	}

	public virtual void Update(byte b)
	{
		if (64 - bufferPos == 0)
		{
			t0 += 64u;
			if (t0 == 0)
			{
				t1++;
			}
			Compress(buffer, 0);
			Array.Clear(buffer, 0, buffer.Length);
			buffer[0] = b;
			bufferPos = 1;
		}
		else
		{
			buffer[bufferPos] = b;
			bufferPos++;
		}
	}

	public virtual void BlockUpdate(byte[] message, int offset, int len)
	{
		if (message == null || len == 0)
		{
			return;
		}
		int num = 0;
		if (bufferPos != 0)
		{
			num = 64 - bufferPos;
			if (num >= len)
			{
				Array.Copy(message, offset, buffer, bufferPos, len);
				bufferPos += len;
				return;
			}
			Array.Copy(message, offset, buffer, bufferPos, num);
			t0 += 64u;
			if (t0 == 0)
			{
				t1++;
			}
			Compress(buffer, 0);
			bufferPos = 0;
			Array.Clear(buffer, 0, buffer.Length);
		}
		int num2 = offset + len - 64;
		int i;
		for (i = offset + num; i < num2; i += 64)
		{
			t0 += 64u;
			if (t0 == 0)
			{
				t1++;
			}
			Compress(message, i);
		}
		Array.Copy(message, i, buffer, 0, offset + len - i);
		bufferPos += offset + len - i;
	}

	public virtual int DoFinal(byte[] output, int outOffset)
	{
		f0 = uint.MaxValue;
		t0 += (uint)bufferPos;
		if (t0 < 0 && bufferPos > 0L - (long)t0)
		{
			t1++;
		}
		Compress(buffer, 0);
		Array.Clear(buffer, 0, buffer.Length);
		Array.Clear(internalState, 0, internalState.Length);
		for (int i = 0; i < chainValue.Length && i * 4 < digestLength; i++)
		{
			byte[] sourceArray = Pack.UInt32_To_LE(chainValue[i]);
			if (i * 4 < digestLength - 4)
			{
				Array.Copy(sourceArray, 0, output, outOffset + i * 4, 4);
			}
			else
			{
				Array.Copy(sourceArray, 0, output, outOffset + i * 4, digestLength - i * 4);
			}
		}
		Array.Clear(chainValue, 0, chainValue.Length);
		Reset();
		return digestLength;
	}

	public virtual void Reset()
	{
		bufferPos = 0;
		f0 = 0u;
		t0 = 0u;
		t1 = 0u;
		chainValue = null;
		Array.Clear(buffer, 0, buffer.Length);
		if (key != null)
		{
			Array.Copy(key, 0, buffer, 0, key.Length);
			bufferPos = 64;
		}
		Init();
	}

	private void Compress(byte[] message, int messagePos)
	{
		InitializeInternalState();
		uint[] array = new uint[16];
		for (int i = 0; i < 16; i++)
		{
			array[i] = Pack.LE_To_UInt32(message, messagePos + i * 4);
		}
		for (int j = 0; j < 10; j++)
		{
			G(array[blake2s_sigma[j, 0]], array[blake2s_sigma[j, 1]], 0, 4, 8, 12);
			G(array[blake2s_sigma[j, 2]], array[blake2s_sigma[j, 3]], 1, 5, 9, 13);
			G(array[blake2s_sigma[j, 4]], array[blake2s_sigma[j, 5]], 2, 6, 10, 14);
			G(array[blake2s_sigma[j, 6]], array[blake2s_sigma[j, 7]], 3, 7, 11, 15);
			G(array[blake2s_sigma[j, 8]], array[blake2s_sigma[j, 9]], 0, 5, 10, 15);
			G(array[blake2s_sigma[j, 10]], array[blake2s_sigma[j, 11]], 1, 6, 11, 12);
			G(array[blake2s_sigma[j, 12]], array[blake2s_sigma[j, 13]], 2, 7, 8, 13);
			G(array[blake2s_sigma[j, 14]], array[blake2s_sigma[j, 15]], 3, 4, 9, 14);
		}
		for (int k = 0; k < chainValue.Length; k++)
		{
			chainValue[k] = chainValue[k] ^ internalState[k] ^ internalState[k + 8];
		}
	}

	private void G(uint m1, uint m2, int posA, int posB, int posC, int posD)
	{
		internalState[posA] = internalState[posA] + internalState[posB] + m1;
		internalState[posD] = rotr32(internalState[posD] ^ internalState[posA], 16);
		internalState[posC] += internalState[posD];
		internalState[posB] = rotr32(internalState[posB] ^ internalState[posC], 12);
		internalState[posA] = internalState[posA] + internalState[posB] + m2;
		internalState[posD] = rotr32(internalState[posD] ^ internalState[posA], 8);
		internalState[posC] += internalState[posD];
		internalState[posB] = rotr32(internalState[posB] ^ internalState[posC], 7);
	}

	private uint rotr32(uint x, int rot)
	{
		return (x >> rot) | (x << -rot);
	}

	public virtual int GetDigestSize()
	{
		return digestLength;
	}

	public virtual int GetByteLength()
	{
		return 64;
	}

	public virtual void ClearKey()
	{
		if (key != null)
		{
			Array.Clear(key, 0, key.Length);
			Array.Clear(buffer, 0, buffer.Length);
		}
	}

	public virtual void ClearSalt()
	{
		if (salt != null)
		{
			Array.Clear(salt, 0, salt.Length);
		}
	}
}
