using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Utilities;

public abstract class Integers
{
	private static readonly byte[] DeBruijnTZ = new byte[32]
	{
		0, 1, 2, 24, 3, 19, 6, 25, 22, 4,
		20, 10, 16, 7, 12, 26, 31, 23, 18, 5,
		21, 9, 15, 11, 30, 17, 8, 14, 29, 13,
		28, 27
	};

	public static int NumberOfLeadingZeros(int i)
	{
		if (i <= 0)
		{
			return (~i >> 26) & 0x20;
		}
		uint num = (uint)i;
		int num2 = 1;
		if (num >> 16 == 0)
		{
			num2 += 16;
			num <<= 16;
		}
		if (num >> 24 == 0)
		{
			num2 += 8;
			num <<= 8;
		}
		if (num >> 28 == 0)
		{
			num2 += 4;
			num <<= 4;
		}
		if (num >> 30 == 0)
		{
			num2 += 2;
			num <<= 2;
		}
		return num2 - (int)(num >> 31);
	}

	public static int NumberOfTrailingZeros(int i)
	{
		return DeBruijnTZ[(uint)((i & -i) * 81224991) >> 27];
	}

	public static int Reverse(int i)
	{
		i = (int)Bits.BitPermuteStepSimple((uint)i, 1431655765u, 1);
		i = (int)Bits.BitPermuteStepSimple((uint)i, 858993459u, 2);
		i = (int)Bits.BitPermuteStepSimple((uint)i, 252645135u, 4);
		return ReverseBytes(i);
	}

	public static int ReverseBytes(int i)
	{
		return RotateLeft(i & -16711936, 8) | RotateLeft(i & 0xFF00FF, 24);
	}

	public static int RotateLeft(int i, int distance)
	{
		return (i << distance) ^ (int)((uint)i >> -distance);
	}

	[CLSCompliant(false)]
	public static uint RotateLeft(uint i, int distance)
	{
		return (i << distance) ^ (i >> -distance);
	}

	public static int RotateRight(int i, int distance)
	{
		return (int)((uint)i >> distance) ^ (i << -distance);
	}

	[CLSCompliant(false)]
	public static uint RotateRight(uint i, int distance)
	{
		return (i >> distance) ^ (i << -distance);
	}
}
