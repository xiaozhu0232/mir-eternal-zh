namespace Org.BouncyCastle.Math.Raw;

internal abstract class Interleave
{
	private const ulong M32 = 1431655765uL;

	private const ulong M64 = 6148914691236517205uL;

	private const ulong M64R = 12297829382473034410uL;

	internal static uint Expand8to16(uint x)
	{
		x &= 0xFFu;
		x = (x | (x << 4)) & 0xF0Fu;
		x = (x | (x << 2)) & 0x3333u;
		x = (x | (x << 1)) & 0x5555u;
		return x;
	}

	internal static uint Expand16to32(uint x)
	{
		x &= 0xFFFFu;
		x = (x | (x << 8)) & 0xFF00FFu;
		x = (x | (x << 4)) & 0xF0F0F0Fu;
		x = (x | (x << 2)) & 0x33333333u;
		x = (x | (x << 1)) & 0x55555555u;
		return x;
	}

	internal static ulong Expand32to64(uint x)
	{
		x = Bits.BitPermuteStep(x, 65280u, 8);
		x = Bits.BitPermuteStep(x, 15728880u, 4);
		x = Bits.BitPermuteStep(x, 202116108u, 2);
		x = Bits.BitPermuteStep(x, 572662306u, 1);
		return (((ulong)(x >> 1) & 0x55555555uL) << 32) | ((ulong)x & 0x55555555uL);
	}

	internal static void Expand64To128(ulong x, ulong[] z, int zOff)
	{
		x = Bits.BitPermuteStep(x, 4294901760uL, 16);
		x = Bits.BitPermuteStep(x, 280375465148160uL, 8);
		x = Bits.BitPermuteStep(x, 67555025218437360uL, 4);
		x = Bits.BitPermuteStep(x, 868082074056920076uL, 2);
		x = Bits.BitPermuteStep(x, 2459565876494606882uL, 1);
		z[zOff] = x & 0x5555555555555555uL;
		z[zOff + 1] = (x >> 1) & 0x5555555555555555uL;
	}

	internal static void Expand64To128(ulong[] xs, int xsOff, int xsLen, ulong[] zs, int zsOff)
	{
		for (int i = 0; i < xsLen; i++)
		{
			Expand64To128(xs[xsOff + i], zs, zsOff);
			zsOff += 2;
		}
	}

	internal static void Expand64To128Rev(ulong x, ulong[] z, int zOff)
	{
		x = Bits.BitPermuteStep(x, 4294901760uL, 16);
		x = Bits.BitPermuteStep(x, 280375465148160uL, 8);
		x = Bits.BitPermuteStep(x, 67555025218437360uL, 4);
		x = Bits.BitPermuteStep(x, 868082074056920076uL, 2);
		x = Bits.BitPermuteStep(x, 2459565876494606882uL, 1);
		z[zOff] = x & 0xAAAAAAAAAAAAAAAAuL;
		z[zOff + 1] = (x << 1) & 0xAAAAAAAAAAAAAAAAuL;
	}

	internal static uint Shuffle(uint x)
	{
		x = Bits.BitPermuteStep(x, 65280u, 8);
		x = Bits.BitPermuteStep(x, 15728880u, 4);
		x = Bits.BitPermuteStep(x, 202116108u, 2);
		x = Bits.BitPermuteStep(x, 572662306u, 1);
		return x;
	}

	internal static ulong Shuffle(ulong x)
	{
		x = Bits.BitPermuteStep(x, 4294901760uL, 16);
		x = Bits.BitPermuteStep(x, 280375465148160uL, 8);
		x = Bits.BitPermuteStep(x, 67555025218437360uL, 4);
		x = Bits.BitPermuteStep(x, 868082074056920076uL, 2);
		x = Bits.BitPermuteStep(x, 2459565876494606882uL, 1);
		return x;
	}

	internal static uint Shuffle2(uint x)
	{
		x = Bits.BitPermuteStep(x, 11141290u, 7);
		x = Bits.BitPermuteStep(x, 52428u, 14);
		x = Bits.BitPermuteStep(x, 15728880u, 4);
		x = Bits.BitPermuteStep(x, 65280u, 8);
		return x;
	}

	internal static uint Unshuffle(uint x)
	{
		x = Bits.BitPermuteStep(x, 572662306u, 1);
		x = Bits.BitPermuteStep(x, 202116108u, 2);
		x = Bits.BitPermuteStep(x, 15728880u, 4);
		x = Bits.BitPermuteStep(x, 65280u, 8);
		return x;
	}

	internal static ulong Unshuffle(ulong x)
	{
		x = Bits.BitPermuteStep(x, 2459565876494606882uL, 1);
		x = Bits.BitPermuteStep(x, 868082074056920076uL, 2);
		x = Bits.BitPermuteStep(x, 67555025218437360uL, 4);
		x = Bits.BitPermuteStep(x, 280375465148160uL, 8);
		x = Bits.BitPermuteStep(x, 4294901760uL, 16);
		return x;
	}

	internal static uint Unshuffle2(uint x)
	{
		x = Bits.BitPermuteStep(x, 65280u, 8);
		x = Bits.BitPermuteStep(x, 15728880u, 4);
		x = Bits.BitPermuteStep(x, 52428u, 14);
		x = Bits.BitPermuteStep(x, 11141290u, 7);
		return x;
	}
}
