using System;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Checksums;

namespace ICSharpCode.SharpZipLib.Encryption;

public abstract class PkzipClassic : SymmetricAlgorithm
{
	public static byte[] GenerateKeys(byte[] seed)
	{
		if (seed == null)
		{
			throw new ArgumentNullException("seed");
		}
		if (seed.Length == 0)
		{
			throw new ArgumentException("seed");
		}
		uint[] array = new uint[3] { 305419896u, 591751049u, 878082192u };
		for (int i = 0; i < seed.Length; i++)
		{
			array[0] = Crc32.ComputeCrc32(array[0], seed[i]);
			array[1] = array[1] + (byte)array[0];
			array[1] = array[1] * 134775813 + 1;
			array[2] = Crc32.ComputeCrc32(array[2], (byte)(array[1] >> 24));
		}
		return new byte[12]
		{
			(byte)(array[0] & 0xFFu),
			(byte)((array[0] >> 8) & 0xFFu),
			(byte)((array[0] >> 16) & 0xFFu),
			(byte)((array[0] >> 24) & 0xFFu),
			(byte)(array[1] & 0xFFu),
			(byte)((array[1] >> 8) & 0xFFu),
			(byte)((array[1] >> 16) & 0xFFu),
			(byte)((array[1] >> 24) & 0xFFu),
			(byte)(array[2] & 0xFFu),
			(byte)((array[2] >> 8) & 0xFFu),
			(byte)((array[2] >> 16) & 0xFFu),
			(byte)((array[2] >> 24) & 0xFFu)
		};
	}
}
