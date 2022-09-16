using System;
using ICSharpCode.SharpZipLib.Checksums;

namespace ICSharpCode.SharpZipLib.Encryption;

internal class PkzipClassicCryptoBase
{
	private uint[] keys = null;

	protected byte TransformByte()
	{
		uint num = (keys[2] & 0xFFFFu) | 2u;
		return (byte)(num * (num ^ 1) >> 8);
	}

	protected void SetKeys(byte[] keyData)
	{
		if (keyData == null)
		{
			throw new ArgumentNullException("keyData");
		}
		if (keyData.Length != 12)
		{
			throw new InvalidOperationException("Keys not valid");
		}
		keys = new uint[3];
		keys[0] = (uint)((keyData[3] << 24) | (keyData[2] << 16) | (keyData[1] << 8) | keyData[0]);
		keys[1] = (uint)((keyData[7] << 24) | (keyData[6] << 16) | (keyData[5] << 8) | keyData[4]);
		keys[2] = (uint)((keyData[11] << 24) | (keyData[10] << 16) | (keyData[9] << 8) | keyData[8]);
	}

	protected void UpdateKeys(byte ch)
	{
		keys[0] = Crc32.ComputeCrc32(keys[0], ch);
		keys[1] = keys[1] + (byte)keys[0];
		keys[1] = keys[1] * 134775813 + 1;
		keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
	}

	protected void Reset()
	{
		keys[0] = 0u;
		keys[1] = 0u;
		keys[2] = 0u;
	}
}
