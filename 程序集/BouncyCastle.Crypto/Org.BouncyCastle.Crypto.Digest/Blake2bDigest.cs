using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Blake2bDigest : IDigest
{
	private const int ROUNDS = 12;

	private const int BLOCK_LENGTH_BYTES = 128;

	private static readonly ulong[] blake2b_IV = new ulong[8] { 7640891576956012808uL, 13503953896175478587uL, 4354685564936845355uL, 11912009170470909681uL, 5840696475078001361uL, 11170449401992604703uL, 2270897969802886507uL, 6620516959819538809uL };

	private static readonly byte[,] blake2b_sigma = new byte[12, 16]
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
		},
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15
		},
		{
			14, 10, 4, 8, 9, 15, 13, 6, 1, 12,
			0, 2, 11, 7, 5, 3
		}
	};

	private int digestLength = 64;

	private int keyLength = 0;

	private byte[] salt = null;

	private byte[] personalization = null;

	private byte[] key = null;

	private byte[] buffer = null;

	private int bufferPos = 0;

	private ulong[] internalState = new ulong[16];

	private ulong[] chainValue = null;

	private ulong t0 = 0uL;

	private ulong t1 = 0uL;

	private ulong f0 = 0uL;

	public virtual string AlgorithmName => "BLAKE2b";

	public Blake2bDigest()
		: this(512)
	{
	}

	public Blake2bDigest(Blake2bDigest digest)
	{
		bufferPos = digest.bufferPos;
		buffer = Arrays.Clone(digest.buffer);
		keyLength = digest.keyLength;
		key = Arrays.Clone(digest.key);
		digestLength = digest.digestLength;
		chainValue = Arrays.Clone(digest.chainValue);
		personalization = Arrays.Clone(digest.personalization);
		salt = Arrays.Clone(digest.salt);
		t0 = digest.t0;
		t1 = digest.t1;
		f0 = digest.f0;
	}

	public Blake2bDigest(int digestSize)
	{
		if (digestSize < 8 || digestSize > 512 || digestSize % 8 != 0)
		{
			throw new ArgumentException("BLAKE2b digest bit length must be a multiple of 8 and not greater than 512");
		}
		buffer = new byte[128];
		keyLength = 0;
		digestLength = digestSize / 8;
		Init();
	}

	public Blake2bDigest(byte[] key)
	{
		buffer = new byte[128];
		if (key != null)
		{
			this.key = new byte[key.Length];
			Array.Copy(key, 0, this.key, 0, key.Length);
			if (key.Length > 64)
			{
				throw new ArgumentException("Keys > 64 are not supported");
			}
			keyLength = key.Length;
			Array.Copy(key, 0, buffer, 0, key.Length);
			bufferPos = 128;
		}
		digestLength = 64;
		Init();
	}

	public Blake2bDigest(byte[] key, int digestLength, byte[] salt, byte[] personalization)
	{
		if (digestLength < 1 || digestLength > 64)
		{
			throw new ArgumentException("Invalid digest length (required: 1 - 64)");
		}
		this.digestLength = digestLength;
		buffer = new byte[128];
		if (salt != null)
		{
			if (salt.Length != 16)
			{
				throw new ArgumentException("salt length must be exactly 16 bytes");
			}
			this.salt = new byte[16];
			Array.Copy(salt, 0, this.salt, 0, salt.Length);
		}
		if (personalization != null)
		{
			if (personalization.Length != 16)
			{
				throw new ArgumentException("personalization length must be exactly 16 bytes");
			}
			this.personalization = new byte[16];
			Array.Copy(personalization, 0, this.personalization, 0, personalization.Length);
		}
		if (key != null)
		{
			if (key.Length > 64)
			{
				throw new ArgumentException("Keys > 64 are not supported");
			}
			this.key = new byte[key.Length];
			Array.Copy(key, 0, this.key, 0, key.Length);
			keyLength = key.Length;
			Array.Copy(key, 0, buffer, 0, key.Length);
			bufferPos = 128;
		}
		Init();
	}

	private void Init()
	{
		if (chainValue == null)
		{
			chainValue = new ulong[8];
			chainValue[0] = blake2b_IV[0] ^ (ulong)(int)((uint)(digestLength | (keyLength << 8)) | 0x1010000u);
			chainValue[1] = blake2b_IV[1];
			chainValue[2] = blake2b_IV[2];
			chainValue[3] = blake2b_IV[3];
			chainValue[4] = blake2b_IV[4];
			chainValue[5] = blake2b_IV[5];
			if (salt != null)
			{
				ulong[] array;
				(array = chainValue)[4] = array[4] ^ Pack.LE_To_UInt64(salt, 0);
				(array = chainValue)[5] = array[5] ^ Pack.LE_To_UInt64(salt, 8);
			}
			chainValue[6] = blake2b_IV[6];
			chainValue[7] = blake2b_IV[7];
			if (personalization != null)
			{
				ulong[] array;
				(array = chainValue)[6] = array[6] ^ Pack.LE_To_UInt64(personalization, 0);
				(array = chainValue)[7] = array[7] ^ Pack.LE_To_UInt64(personalization, 8);
			}
		}
	}

	private void InitializeInternalState()
	{
		Array.Copy(chainValue, 0, internalState, 0, chainValue.Length);
		Array.Copy(blake2b_IV, 0, internalState, chainValue.Length, 4);
		internalState[12] = t0 ^ blake2b_IV[4];
		internalState[13] = t1 ^ blake2b_IV[5];
		internalState[14] = f0 ^ blake2b_IV[6];
		internalState[15] = blake2b_IV[7];
	}

	public virtual void Update(byte b)
	{
		int num = 0;
		if (128 - bufferPos == 0)
		{
			t0 += 128uL;
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
			num = 128 - bufferPos;
			if (num >= len)
			{
				Array.Copy(message, offset, buffer, bufferPos, len);
				bufferPos += len;
				return;
			}
			Array.Copy(message, offset, buffer, bufferPos, num);
			t0 += 128uL;
			if (t0 == 0)
			{
				t1++;
			}
			Compress(buffer, 0);
			bufferPos = 0;
			Array.Clear(buffer, 0, buffer.Length);
		}
		int num2 = offset + len - 128;
		int i;
		for (i = offset + num; i < num2; i += 128)
		{
			t0 += 128uL;
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
		f0 = ulong.MaxValue;
		t0 += (ulong)bufferPos;
		if (bufferPos > 0 && t0 == 0)
		{
			t1++;
		}
		Compress(buffer, 0);
		Array.Clear(buffer, 0, buffer.Length);
		Array.Clear(internalState, 0, internalState.Length);
		for (int i = 0; i < chainValue.Length && i * 8 < digestLength; i++)
		{
			byte[] sourceArray = Pack.UInt64_To_LE(chainValue[i]);
			if (i * 8 < digestLength - 8)
			{
				Array.Copy(sourceArray, 0, output, outOffset + i * 8, 8);
			}
			else
			{
				Array.Copy(sourceArray, 0, output, outOffset + i * 8, digestLength - i * 8);
			}
		}
		Array.Clear(chainValue, 0, chainValue.Length);
		Reset();
		return digestLength;
	}

	public virtual void Reset()
	{
		bufferPos = 0;
		f0 = 0uL;
		t0 = 0uL;
		t1 = 0uL;
		chainValue = null;
		Array.Clear(buffer, 0, buffer.Length);
		if (key != null)
		{
			Array.Copy(key, 0, buffer, 0, key.Length);
			bufferPos = 128;
		}
		Init();
	}

	private void Compress(byte[] message, int messagePos)
	{
		InitializeInternalState();
		ulong[] array = new ulong[16];
		for (int i = 0; i < 16; i++)
		{
			array[i] = Pack.LE_To_UInt64(message, messagePos + i * 8);
		}
		for (int j = 0; j < 12; j++)
		{
			G(array[blake2b_sigma[j, 0]], array[blake2b_sigma[j, 1]], 0, 4, 8, 12);
			G(array[blake2b_sigma[j, 2]], array[blake2b_sigma[j, 3]], 1, 5, 9, 13);
			G(array[blake2b_sigma[j, 4]], array[blake2b_sigma[j, 5]], 2, 6, 10, 14);
			G(array[blake2b_sigma[j, 6]], array[blake2b_sigma[j, 7]], 3, 7, 11, 15);
			G(array[blake2b_sigma[j, 8]], array[blake2b_sigma[j, 9]], 0, 5, 10, 15);
			G(array[blake2b_sigma[j, 10]], array[blake2b_sigma[j, 11]], 1, 6, 11, 12);
			G(array[blake2b_sigma[j, 12]], array[blake2b_sigma[j, 13]], 2, 7, 8, 13);
			G(array[blake2b_sigma[j, 14]], array[blake2b_sigma[j, 15]], 3, 4, 9, 14);
		}
		for (int k = 0; k < chainValue.Length; k++)
		{
			chainValue[k] = chainValue[k] ^ internalState[k] ^ internalState[k + 8];
		}
	}

	private void G(ulong m1, ulong m2, int posA, int posB, int posC, int posD)
	{
		internalState[posA] = internalState[posA] + internalState[posB] + m1;
		internalState[posD] = Rotr64(internalState[posD] ^ internalState[posA], 32);
		internalState[posC] += internalState[posD];
		internalState[posB] = Rotr64(internalState[posB] ^ internalState[posC], 24);
		internalState[posA] = internalState[posA] + internalState[posB] + m2;
		internalState[posD] = Rotr64(internalState[posD] ^ internalState[posA], 16);
		internalState[posC] += internalState[posD];
		internalState[posB] = Rotr64(internalState[posB] ^ internalState[posC], 63);
	}

	private static ulong Rotr64(ulong x, int rot)
	{
		return (x >> rot) | (x << -rot);
	}

	public virtual int GetDigestSize()
	{
		return digestLength;
	}

	public virtual int GetByteLength()
	{
		return 128;
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
