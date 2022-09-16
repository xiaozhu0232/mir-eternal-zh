using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Utilities;

public abstract class Longs
{
	public static long Reverse(long i)
	{
		i = (long)Bits.BitPermuteStepSimple((ulong)i, 6148914691236517205uL, 1);
		i = (long)Bits.BitPermuteStepSimple((ulong)i, 3689348814741910323uL, 2);
		i = (long)Bits.BitPermuteStepSimple((ulong)i, 1085102592571150095uL, 4);
		return ReverseBytes(i);
	}

	[CLSCompliant(false)]
	public static ulong Reverse(ulong i)
	{
		i = Bits.BitPermuteStepSimple(i, 6148914691236517205uL, 1);
		i = Bits.BitPermuteStepSimple(i, 3689348814741910323uL, 2);
		i = Bits.BitPermuteStepSimple(i, 1085102592571150095uL, 4);
		return ReverseBytes(i);
	}

	public static long ReverseBytes(long i)
	{
		return RotateLeft(i & -72057589759737856L, 8) | RotateLeft(i & 0xFF000000FF0000L, 24) | RotateLeft(i & 0xFF000000FF00L, 40) | RotateLeft(i & 0xFF000000FFL, 56);
	}

	[CLSCompliant(false)]
	public static ulong ReverseBytes(ulong i)
	{
		return RotateLeft(i & 0xFF000000FF000000uL, 8) | RotateLeft(i & 0xFF000000FF0000uL, 24) | RotateLeft(i & 0xFF000000FF00uL, 40) | RotateLeft(i & 0xFF000000FFuL, 56);
	}

	public static long RotateLeft(long i, int distance)
	{
		return (i << distance) ^ (long)((ulong)i >> -distance);
	}

	[CLSCompliant(false)]
	public static ulong RotateLeft(ulong i, int distance)
	{
		return (i << distance) ^ (i >> -distance);
	}

	public static long RotateRight(long i, int distance)
	{
		return (long)((ulong)i >> distance) ^ (i << -distance);
	}

	[CLSCompliant(false)]
	public static ulong RotateRight(ulong i, int distance)
	{
		return (i >> distance) ^ (i << -distance);
	}
}
