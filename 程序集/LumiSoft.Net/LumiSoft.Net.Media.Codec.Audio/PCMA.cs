using System;

namespace LumiSoft.Net.Media.Codec.Audio;

public class PCMA : AudioCodec
{
	private static readonly byte[] ALawCompressTable = new byte[128]
	{
		1, 1, 2, 2, 3, 3, 3, 3, 4, 4,
		4, 4, 4, 4, 4, 4, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7
	};

	private static readonly short[] ALawDecompressTable = new short[256]
	{
		-5504, -5248, -6016, -5760, -4480, -4224, -4992, -4736, -7552, -7296,
		-8064, -7808, -6528, -6272, -7040, -6784, -2752, -2624, -3008, -2880,
		-2240, -2112, -2496, -2368, -3776, -3648, -4032, -3904, -3264, -3136,
		-3520, -3392, -22016, -20992, -24064, -23040, -17920, -16896, -19968, -18944,
		-30208, -29184, -32256, -31232, -26112, -25088, -28160, -27136, -11008, -10496,
		-12032, -11520, -8960, -8448, -9984, -9472, -15104, -14592, -16128, -15616,
		-13056, -12544, -14080, -13568, -344, -328, -376, -360, -280, -264,
		-312, -296, -472, -456, -504, -488, -408, -392, -440, -424,
		-88, -72, -120, -104, -24, -8, -56, -40, -216, -200,
		-248, -232, -152, -136, -184, -168, -1376, -1312, -1504, -1440,
		-1120, -1056, -1248, -1184, -1888, -1824, -2016, -1952, -1632, -1568,
		-1760, -1696, -688, -656, -752, -720, -560, -528, -624, -592,
		-944, -912, -1008, -976, -816, -784, -880, -848, 5504, 5248,
		6016, 5760, 4480, 4224, 4992, 4736, 7552, 7296, 8064, 7808,
		6528, 6272, 7040, 6784, 2752, 2624, 3008, 2880, 2240, 2112,
		2496, 2368, 3776, 3648, 4032, 3904, 3264, 3136, 3520, 3392,
		22016, 20992, 24064, 23040, 17920, 16896, 19968, 18944, 30208, 29184,
		32256, 31232, 26112, 25088, 28160, 27136, 11008, 10496, 12032, 11520,
		8960, 8448, 9984, 9472, 15104, 14592, 16128, 15616, 13056, 12544,
		14080, 13568, 344, 328, 376, 360, 280, 264, 312, 296,
		472, 456, 504, 488, 408, 392, 440, 424, 88, 72,
		120, 104, 24, 8, 56, 40, 216, 200, 248, 232,
		152, 136, 184, 168, 1376, 1312, 1504, 1440, 1120, 1056,
		1248, 1184, 1888, 1824, 2016, 1952, 1632, 1568, 1760, 1696,
		688, 656, 752, 720, 560, 528, 624, 592, 944, 912,
		1008, 976, 816, 784, 880, 848
	};

	private AudioFormat m_pAudioFormat = new AudioFormat(8000, 16, 1);

	private AudioFormat m_pCompressedAudioFormat = new AudioFormat(8000, 8, 1);

	public override string Name => "PCMA";

	public override AudioFormat AudioFormat => m_pAudioFormat;

	public override AudioFormat CompressedAudioFormat => m_pCompressedAudioFormat;

	public override byte[] Encode(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentException("Argument 'offset' is out of range.");
		}
		if (count < 1 || count + offset > buffer.Length)
		{
			throw new ArgumentException("Argument 'count' is out of range.");
		}
		if (count % 2 != 0)
		{
			throw new ArgumentException("Invalid 'count' value, it doesn't contain 16-bit boundaries.");
		}
		int num = 0;
		byte[] array = new byte[count / 2];
		while (num < array.Length)
		{
			short sample = (short)((buffer[offset + 1] << 8) | buffer[offset]);
			offset += 2;
			array[num++] = LinearToALawSample(sample);
		}
		return array;
	}

	public override byte[] Decode(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentException("Argument 'offse't is out of range.");
		}
		if (count < 1 || count + offset > buffer.Length)
		{
			throw new ArgumentException("Argument 'count' is out of range.");
		}
		int num = 0;
		byte[] array = new byte[count * 2];
		for (int i = offset; i < buffer.Length; i++)
		{
			short num2 = ALawDecompressTable[buffer[i]];
			array[num++] = (byte)((uint)num2 & 0xFFu);
			array[num++] = (byte)((uint)(num2 >> 8) & 0xFFu);
		}
		return array;
	}

	private static byte LinearToALawSample(short sample)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		byte b = 0;
		num = (~sample >> 8) & 0x80;
		if (num == 0)
		{
			sample = (short)(-sample);
		}
		if (sample > 32635)
		{
			sample = 32635;
		}
		if (sample >= 256)
		{
			num2 = ALawCompressTable[(sample >> 8) & 0x7F];
			num3 = (sample >> num2 + 3) & 0xF;
			b = (byte)((num2 << 4) | num3);
		}
		else
		{
			b = (byte)(sample >> 4);
		}
		return (byte)(b ^ (byte)((uint)num ^ 0x55u));
	}
}
