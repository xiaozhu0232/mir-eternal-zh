using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Prng;

public class VmpcRandomGenerator : IRandomGenerator
{
	private byte n = 0;

	private byte[] P = new byte[256]
	{
		187, 44, 98, 127, 181, 170, 212, 13, 129, 254,
		178, 130, 203, 160, 161, 8, 24, 113, 86, 232,
		73, 2, 16, 196, 222, 53, 165, 236, 128, 18,
		184, 105, 218, 47, 117, 204, 162, 9, 54, 3,
		97, 45, 253, 224, 221, 5, 67, 144, 173, 200,
		225, 175, 87, 155, 76, 216, 81, 174, 80, 133,
		60, 10, 228, 243, 156, 38, 35, 83, 201, 131,
		151, 70, 177, 153, 100, 49, 119, 213, 29, 214,
		120, 189, 94, 176, 138, 34, 56, 248, 104, 43,
		42, 197, 211, 247, 188, 111, 223, 4, 229, 149,
		62, 37, 134, 166, 11, 143, 241, 36, 14, 215,
		64, 179, 207, 126, 6, 21, 154, 77, 28, 163,
		219, 50, 146, 88, 17, 39, 244, 89, 208, 78,
		106, 23, 91, 172, 255, 7, 192, 101, 121, 252,
		199, 205, 118, 66, 93, 231, 58, 52, 122, 48,
		40, 15, 115, 1, 249, 209, 210, 25, 233, 145,
		185, 90, 237, 65, 109, 180, 195, 158, 191, 99,
		250, 31, 51, 96, 71, 137, 240, 150, 26, 95,
		147, 61, 55, 75, 217, 168, 193, 27, 246, 57,
		139, 183, 12, 32, 206, 136, 110, 182, 116, 142,
		141, 22, 41, 242, 135, 245, 235, 112, 227, 251,
		85, 159, 198, 68, 74, 69, 125, 226, 107, 92,
		108, 102, 169, 140, 238, 132, 19, 167, 30, 157,
		220, 103, 72, 186, 46, 230, 164, 171, 124, 148,
		0, 33, 239, 234, 190, 202, 114, 79, 82, 152,
		63, 194, 20, 123, 59, 84
	};

	private byte s = 190;

	public virtual void AddSeedMaterial(byte[] seed)
	{
		for (int i = 0; i < seed.Length; i++)
		{
			s = P[(s + P[n & 0xFF] + seed[i]) & 0xFF];
			byte b = P[n & 0xFF];
			P[n & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b;
			n = (byte)((uint)(n + 1) & 0xFFu);
		}
	}

	public virtual void AddSeedMaterial(long seed)
	{
		AddSeedMaterial(Pack.UInt64_To_BE((ulong)seed));
	}

	public virtual void NextBytes(byte[] bytes)
	{
		NextBytes(bytes, 0, bytes.Length);
	}

	public virtual void NextBytes(byte[] bytes, int start, int len)
	{
		lock (P)
		{
			int num = start + len;
			for (int i = start; i != num; i++)
			{
				s = P[(s + P[n & 0xFF]) & 0xFF];
				bytes[i] = P[(P[P[s & 0xFF] & 0xFF] + 1) & 0xFF];
				byte b = P[n & 0xFF];
				P[n & 0xFF] = P[s & 0xFF];
				P[s & 0xFF] = b;
				n = (byte)((uint)(n + 1) & 0xFFu);
			}
		}
	}
}
