using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Apache.Bzip2;

public class CBZip2InputStream : Stream
{
	private const int START_BLOCK_STATE = 1;

	private const int RAND_PART_A_STATE = 2;

	private const int RAND_PART_B_STATE = 3;

	private const int RAND_PART_C_STATE = 4;

	private const int NO_RAND_PART_A_STATE = 5;

	private const int NO_RAND_PART_B_STATE = 6;

	private const int NO_RAND_PART_C_STATE = 7;

	private int last;

	private int origPtr;

	private int blockSize100k;

	private bool blockRandomised;

	private int bsBuff;

	private int bsLive;

	private CRC mCrc = new CRC();

	private bool[] inUse = new bool[256];

	private int nInUse;

	private char[] seqToUnseq = new char[256];

	private char[] unseqToSeq = new char[256];

	private char[] selector = new char[18002];

	private char[] selectorMtf = new char[18002];

	private int[] tt;

	private char[] ll8;

	private int[] unzftab = new int[256];

	private int[][] limit = InitIntArray(6, 258);

	private int[][] basev = InitIntArray(6, 258);

	private int[][] perm = InitIntArray(6, 258);

	private int[] minLens = new int[6];

	private Stream bsStream;

	private bool streamEnd = false;

	private int currentChar = -1;

	private int currentState = 1;

	private int storedBlockCRC;

	private int storedCombinedCRC;

	private int computedBlockCRC;

	private int computedCombinedCRC;

	private int i2;

	private int count;

	private int chPrev;

	private int ch2;

	private int i;

	private int tPos;

	private int rNToGo = 0;

	private int rTPos = 0;

	private int j2;

	private char z;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => 0L;

	public override long Position
	{
		get
		{
			return 0L;
		}
		set
		{
		}
	}

	private static void Cadvise()
	{
	}

	private static void CompressedStreamEOF()
	{
		Cadvise();
	}

	private void MakeMaps()
	{
		nInUse = 0;
		for (int i = 0; i < 256; i++)
		{
			if (inUse[i])
			{
				seqToUnseq[nInUse] = (char)i;
				unseqToSeq[i] = (char)nInUse;
				nInUse++;
			}
		}
	}

	public CBZip2InputStream(Stream zStream)
	{
		ll8 = null;
		tt = null;
		BsSetStream(zStream);
		Initialize();
		InitBlock();
		SetupBlock();
	}

	internal static int[][] InitIntArray(int n1, int n2)
	{
		int[][] array = new int[n1][];
		for (int i = 0; i < n1; i++)
		{
			array[i] = new int[n2];
		}
		return array;
	}

	internal static char[][] InitCharArray(int n1, int n2)
	{
		char[][] array = new char[n1][];
		for (int i = 0; i < n1; i++)
		{
			array[i] = new char[n2];
		}
		return array;
	}

	public override int ReadByte()
	{
		if (streamEnd)
		{
			return -1;
		}
		int result = currentChar;
		switch (currentState)
		{
		case 3:
			SetupRandPartB();
			break;
		case 4:
			SetupRandPartC();
			break;
		case 6:
			SetupNoRandPartB();
			break;
		case 7:
			SetupNoRandPartC();
			break;
		}
		return result;
	}

	private void Initialize()
	{
		char c = BsGetUChar();
		char c2 = BsGetUChar();
		if (c != 'B' && c2 != 'Z')
		{
			throw new IOException("Not a BZIP2 marked stream");
		}
		c = BsGetUChar();
		c2 = BsGetUChar();
		if (c != 'h' || c2 < '1' || c2 > '9')
		{
			BsFinishedWithStream();
			streamEnd = true;
		}
		else
		{
			SetDecompressStructureSizes(c2 - 48);
			computedCombinedCRC = 0;
		}
	}

	private void InitBlock()
	{
		char c = BsGetUChar();
		char c2 = BsGetUChar();
		char c3 = BsGetUChar();
		char c4 = BsGetUChar();
		char c5 = BsGetUChar();
		char c6 = BsGetUChar();
		if (c == '\u0017' && c2 == 'r' && c3 == 'E' && c4 == '8' && c5 == 'P' && c6 == '\u0090')
		{
			Complete();
			return;
		}
		if (c != '1' || c2 != 'A' || c3 != 'Y' || c4 != '&' || c5 != 'S' || c6 != 'Y')
		{
			BadBlockHeader();
			streamEnd = true;
			return;
		}
		storedBlockCRC = BsGetInt32();
		if (BsR(1) == 1)
		{
			blockRandomised = true;
		}
		else
		{
			blockRandomised = false;
		}
		GetAndMoveToFrontDecode();
		mCrc.InitialiseCRC();
		currentState = 1;
	}

	private void EndBlock()
	{
		computedBlockCRC = mCrc.GetFinalCRC();
		if (storedBlockCRC != computedBlockCRC)
		{
			CrcError();
		}
		computedCombinedCRC = (computedCombinedCRC << 1) | (int)((uint)computedCombinedCRC >> 31);
		computedCombinedCRC ^= computedBlockCRC;
	}

	private void Complete()
	{
		storedCombinedCRC = BsGetInt32();
		if (storedCombinedCRC != computedCombinedCRC)
		{
			CrcError();
		}
		BsFinishedWithStream();
		streamEnd = true;
	}

	private static void BlockOverrun()
	{
		Cadvise();
	}

	private static void BadBlockHeader()
	{
		Cadvise();
	}

	private static void CrcError()
	{
		Cadvise();
	}

	private void BsFinishedWithStream()
	{
		try
		{
			if (bsStream != null)
			{
				Platform.Dispose(bsStream);
				bsStream = null;
			}
		}
		catch
		{
		}
	}

	private void BsSetStream(Stream f)
	{
		bsStream = f;
		bsLive = 0;
		bsBuff = 0;
	}

	private int BsR(int n)
	{
		while (bsLive < n)
		{
			char c = '\0';
			try
			{
				c = (char)bsStream.ReadByte();
			}
			catch (IOException)
			{
				CompressedStreamEOF();
			}
			if (c == '\uffff')
			{
				CompressedStreamEOF();
			}
			int num = c;
			bsBuff = (bsBuff << 8) | (num & 0xFF);
			bsLive += 8;
		}
		int result = (bsBuff >> bsLive - n) & ((1 << n) - 1);
		bsLive -= n;
		return result;
	}

	private char BsGetUChar()
	{
		return (char)BsR(8);
	}

	private int BsGetint()
	{
		int num = 0;
		num = (num << 8) | BsR(8);
		num = (num << 8) | BsR(8);
		num = (num << 8) | BsR(8);
		return (num << 8) | BsR(8);
	}

	private int BsGetIntVS(int numBits)
	{
		return BsR(numBits);
	}

	private int BsGetInt32()
	{
		return BsGetint();
	}

	private void HbCreateDecodeTables(int[] limit, int[] basev, int[] perm, char[] length, int minLen, int maxLen, int alphaSize)
	{
		int num = 0;
		for (int i = minLen; i <= maxLen; i++)
		{
			for (int j = 0; j < alphaSize; j++)
			{
				if (length[j] == i)
				{
					perm[num] = j;
					num++;
				}
			}
		}
		for (int i = 0; i < 23; i++)
		{
			basev[i] = 0;
		}
		for (int i = 0; i < alphaSize; i++)
		{
			int[] array;
			int[] array2 = (array = basev);
			int num2 = length[i] + 1;
			nint num3 = num2;
			array2[num2] = array[num3] + 1;
		}
		for (int i = 1; i < 23; i++)
		{
			int[] array;
			int[] array3 = (array = basev);
			int num4 = i;
			nint num3 = num4;
			array3[num4] = array[num3] + basev[i - 1];
		}
		for (int i = 0; i < 23; i++)
		{
			limit[i] = 0;
		}
		int num5 = 0;
		for (int i = minLen; i <= maxLen; i++)
		{
			num5 += basev[i + 1] - basev[i];
			limit[i] = num5 - 1;
			num5 <<= 1;
		}
		for (int i = minLen + 1; i <= maxLen; i++)
		{
			basev[i] = (limit[i - 1] + 1 << 1) - basev[i];
		}
	}

	private void RecvDecodingTables()
	{
		char[][] array = InitCharArray(6, 258);
		bool[] array2 = new bool[16];
		for (int i = 0; i < 16; i++)
		{
			if (BsR(1) == 1)
			{
				array2[i] = true;
			}
			else
			{
				array2[i] = false;
			}
		}
		for (int i = 0; i < 256; i++)
		{
			inUse[i] = false;
		}
		for (int i = 0; i < 16; i++)
		{
			if (!array2[i])
			{
				continue;
			}
			for (int j = 0; j < 16; j++)
			{
				if (BsR(1) == 1)
				{
					inUse[i * 16 + j] = true;
				}
			}
		}
		MakeMaps();
		int num = nInUse + 2;
		int num2 = BsR(3);
		int num3 = BsR(15);
		for (int i = 0; i < num3; i++)
		{
			int j = 0;
			while (BsR(1) == 1)
			{
				j++;
			}
			selectorMtf[i] = (char)j;
		}
		char[] array3 = new char[6];
		for (char c = '\0'; c < num2; c = (char)(c + 1))
		{
			array3[(uint)c] = c;
		}
		for (int i = 0; i < num3; i++)
		{
			char c = selectorMtf[i];
			char c2 = array3[(uint)c];
			while (c > '\0')
			{
				array3[(uint)c] = array3[c - 1];
				c = (char)(c - 1);
			}
			array3[0] = c2;
			selector[i] = c2;
		}
		for (int k = 0; k < num2; k++)
		{
			int num4 = BsR(5);
			for (int i = 0; i < num; i++)
			{
				while (BsR(1) == 1)
				{
					num4 = ((BsR(1) != 0) ? (num4 - 1) : (num4 + 1));
				}
				array[k][i] = (char)num4;
			}
		}
		for (int k = 0; k < num2; k++)
		{
			int num5 = 32;
			int num6 = 0;
			for (int i = 0; i < num; i++)
			{
				if (array[k][i] > num6)
				{
					num6 = array[k][i];
				}
				if (array[k][i] < num5)
				{
					num5 = array[k][i];
				}
			}
			HbCreateDecodeTables(limit[k], basev[k], perm[k], array[k], num5, num6, num);
			minLens[k] = num5;
		}
	}

	private void GetAndMoveToFrontDecode()
	{
		char[] array = new char[256];
		int num = 100000 * blockSize100k;
		origPtr = BsGetIntVS(24);
		RecvDecodingTables();
		int num2 = nInUse + 1;
		int num3 = -1;
		int num4 = 0;
		for (int i = 0; i <= 255; i++)
		{
			unzftab[i] = 0;
		}
		for (int i = 0; i <= 255; i++)
		{
			array[i] = (char)i;
		}
		last = -1;
		if (num4 == 0)
		{
			num3++;
			num4 = 50;
		}
		num4--;
		int num5 = selector[num3];
		int num6 = minLens[num5];
		int num7 = BsR(num6);
		while (num7 > limit[num5][num6])
		{
			num6++;
			while (bsLive < 1)
			{
				char c = '\0';
				try
				{
					c = (char)bsStream.ReadByte();
				}
				catch (IOException)
				{
					CompressedStreamEOF();
				}
				if (c == '\uffff')
				{
					CompressedStreamEOF();
				}
				int num8 = c;
				bsBuff = (bsBuff << 8) | (num8 & 0xFF);
				bsLive += 8;
			}
			int num9 = (bsBuff >> bsLive - 1) & 1;
			bsLive--;
			num7 = (num7 << 1) | num9;
		}
		int num10 = perm[num5][num7 - basev[num5][num6]];
		while (num10 != num2)
		{
			int[] array2;
			nint num18;
			if (num10 == 0 || num10 == 1)
			{
				int num11 = -1;
				int num12 = 1;
				do
				{
					switch (num10)
					{
					case 0:
						num11 += num12;
						break;
					case 1:
						num11 += 2 * num12;
						break;
					}
					num12 *= 2;
					if (num4 == 0)
					{
						num3++;
						num4 = 50;
					}
					num4--;
					int num13 = selector[num3];
					int num14 = minLens[num13];
					int num15 = BsR(num14);
					while (num15 > limit[num13][num14])
					{
						num14++;
						while (bsLive < 1)
						{
							char c2 = '\0';
							try
							{
								c2 = (char)bsStream.ReadByte();
							}
							catch (IOException)
							{
								CompressedStreamEOF();
							}
							if (c2 == '\uffff')
							{
								CompressedStreamEOF();
							}
							int num16 = c2;
							bsBuff = (bsBuff << 8) | (num16 & 0xFF);
							bsLive += 8;
						}
						int num17 = (bsBuff >> bsLive - 1) & 1;
						bsLive--;
						num15 = (num15 << 1) | num17;
					}
					num10 = perm[num13][num15 - basev[num13][num14]];
				}
				while (num10 == 0 || num10 == 1);
				num11++;
				char c3 = seqToUnseq[(uint)array[0]];
				int[] array3 = (array2 = unzftab);
				num18 = (int)c3;
				array3[(uint)c3] = array2[num18] + num11;
				while (num11 > 0)
				{
					last++;
					ll8[last] = c3;
					num11--;
				}
				if (last >= num)
				{
					BlockOverrun();
				}
				continue;
			}
			last++;
			if (last >= num)
			{
				BlockOverrun();
			}
			char c4 = array[num10 - 1];
			int[] array4 = (array2 = unzftab);
			char num19 = seqToUnseq[(uint)c4];
			num18 = (int)num19;
			array4[(uint)num19] = array2[num18] + 1;
			ll8[last] = seqToUnseq[(uint)c4];
			int num20;
			for (num20 = num10 - 1; num20 > 3; num20 -= 4)
			{
				array[num20] = array[num20 - 1];
				array[num20 - 1] = array[num20 - 2];
				array[num20 - 2] = array[num20 - 3];
				array[num20 - 3] = array[num20 - 4];
			}
			while (num20 > 0)
			{
				array[num20] = array[num20 - 1];
				num20--;
			}
			array[0] = c4;
			if (num4 == 0)
			{
				num3++;
				num4 = 50;
			}
			num4--;
			int num21 = selector[num3];
			int num22 = minLens[num21];
			int num23 = BsR(num22);
			while (num23 > limit[num21][num22])
			{
				num22++;
				while (bsLive < 1)
				{
					char c5 = '\0';
					try
					{
						c5 = (char)bsStream.ReadByte();
					}
					catch (IOException)
					{
						CompressedStreamEOF();
					}
					int num24 = c5;
					bsBuff = (bsBuff << 8) | (num24 & 0xFF);
					bsLive += 8;
				}
				int num25 = (bsBuff >> bsLive - 1) & 1;
				bsLive--;
				num23 = (num23 << 1) | num25;
			}
			num10 = perm[num21][num23 - basev[num21][num22]];
		}
	}

	private void SetupBlock()
	{
		int[] array = new int[257];
		array[0] = 0;
		for (i = 1; i <= 256; i++)
		{
			array[i] = unzftab[i - 1];
		}
		for (i = 1; i <= 256; i++)
		{
			int[] array2;
			int[] array3 = (array2 = array);
			int num = i;
			nint num2 = num;
			array3[num] = array2[num2] + array[i - 1];
		}
		for (i = 0; i <= last; i++)
		{
			char c = ll8[i];
			tt[array[(uint)c]] = i;
			int[] array2;
			int[] array4 = (array2 = array);
			nint num2 = (int)c;
			array4[(uint)c] = array2[num2] + 1;
		}
		array = null;
		tPos = tt[origPtr];
		count = 0;
		i2 = 0;
		ch2 = 256;
		if (blockRandomised)
		{
			rNToGo = 0;
			rTPos = 0;
			SetupRandPartA();
		}
		else
		{
			SetupNoRandPartA();
		}
	}

	private void SetupRandPartA()
	{
		if (i2 <= last)
		{
			chPrev = ch2;
			ch2 = ll8[tPos];
			tPos = tt[tPos];
			if (rNToGo == 0)
			{
				rNToGo = BZip2Constants.rNums[rTPos];
				rTPos++;
				if (rTPos == 512)
				{
					rTPos = 0;
				}
			}
			rNToGo--;
			ch2 ^= ((rNToGo == 1) ? 1 : 0);
			i2++;
			currentChar = ch2;
			currentState = 3;
			mCrc.UpdateCRC(ch2);
		}
		else
		{
			EndBlock();
			InitBlock();
			SetupBlock();
		}
	}

	private void SetupNoRandPartA()
	{
		if (i2 <= last)
		{
			chPrev = ch2;
			ch2 = ll8[tPos];
			tPos = tt[tPos];
			i2++;
			currentChar = ch2;
			currentState = 6;
			mCrc.UpdateCRC(ch2);
		}
		else
		{
			EndBlock();
			InitBlock();
			SetupBlock();
		}
	}

	private void SetupRandPartB()
	{
		if (ch2 != chPrev)
		{
			currentState = 2;
			count = 1;
			SetupRandPartA();
			return;
		}
		count++;
		if (count >= 4)
		{
			z = ll8[tPos];
			tPos = tt[tPos];
			if (rNToGo == 0)
			{
				rNToGo = BZip2Constants.rNums[rTPos];
				rTPos++;
				if (rTPos == 512)
				{
					rTPos = 0;
				}
			}
			rNToGo--;
			z ^= ((rNToGo == 1) ? '\u0001' : '\0');
			j2 = 0;
			currentState = 4;
			SetupRandPartC();
		}
		else
		{
			currentState = 2;
			SetupRandPartA();
		}
	}

	private void SetupRandPartC()
	{
		if (j2 < z)
		{
			currentChar = ch2;
			mCrc.UpdateCRC(ch2);
			j2++;
		}
		else
		{
			currentState = 2;
			i2++;
			count = 0;
			SetupRandPartA();
		}
	}

	private void SetupNoRandPartB()
	{
		if (ch2 != chPrev)
		{
			currentState = 5;
			count = 1;
			SetupNoRandPartA();
			return;
		}
		count++;
		if (count >= 4)
		{
			z = ll8[tPos];
			tPos = tt[tPos];
			currentState = 7;
			j2 = 0;
			SetupNoRandPartC();
		}
		else
		{
			currentState = 5;
			SetupNoRandPartA();
		}
	}

	private void SetupNoRandPartC()
	{
		if (j2 < z)
		{
			currentChar = ch2;
			mCrc.UpdateCRC(ch2);
			j2++;
		}
		else
		{
			currentState = 5;
			i2++;
			count = 0;
			SetupNoRandPartA();
		}
	}

	private void SetDecompressStructureSizes(int newSize100k)
	{
		if (0 <= newSize100k && newSize100k <= 9 && 0 <= blockSize100k)
		{
			_ = blockSize100k;
			_ = 9;
		}
		blockSize100k = newSize100k;
		if (newSize100k != 0)
		{
			int num = 100000 * newSize100k;
			ll8 = new char[num];
			tt = new int[num];
		}
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = -1;
		int i;
		for (i = 0; i < count; i++)
		{
			num = ReadByte();
			if (num == -1)
			{
				break;
			}
			buffer[i + offset] = (byte)num;
		}
		return i;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return 0L;
	}

	public override void SetLength(long value)
	{
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
	}
}
