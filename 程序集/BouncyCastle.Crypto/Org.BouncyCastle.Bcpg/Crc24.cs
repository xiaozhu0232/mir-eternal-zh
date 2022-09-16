using System;

namespace Org.BouncyCastle.Bcpg;

public class Crc24
{
	private const int Crc24Init = 11994318;

	private const int Crc24Poly = 25578747;

	private int crc = 11994318;

	public int Value => crc;

	public void Update(int b)
	{
		crc ^= b << 16;
		for (int i = 0; i < 8; i++)
		{
			crc <<= 1;
			if (((uint)crc & 0x1000000u) != 0)
			{
				crc ^= 25578747;
			}
		}
	}

	[Obsolete("Use 'Value' property instead")]
	public int GetValue()
	{
		return crc;
	}

	public void Reset()
	{
		crc = 11994318;
	}
}
