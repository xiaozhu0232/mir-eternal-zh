using System;

namespace ICSharpCode.SharpZipLib.Zip.Compression;

public class DeflaterHuffman
{
	public class Tree
	{
		public short[] freqs;

		public byte[] length;

		public int minNumCodes;

		public int numCodes;

		private short[] codes;

		private int[] bl_counts;

		private int maxLength;

		private DeflaterHuffman dh;

		public Tree(DeflaterHuffman dh, int elems, int minCodes, int maxLength)
		{
			this.dh = dh;
			minNumCodes = minCodes;
			this.maxLength = maxLength;
			freqs = new short[elems];
			bl_counts = new int[maxLength];
		}

		public void Reset()
		{
			for (int i = 0; i < freqs.Length; i++)
			{
				freqs[i] = 0;
			}
			codes = null;
			length = null;
		}

		public void WriteSymbol(int code)
		{
			dh.pending.WriteBits(codes[code] & 0xFFFF, length[code]);
		}

		public void CheckEmpty()
		{
			bool flag = true;
			for (int i = 0; i < freqs.Length; i++)
			{
				if (freqs[i] != 0)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				throw new SharpZipBaseException("!Empty");
			}
		}

		public void SetStaticCodes(short[] stCodes, byte[] stLength)
		{
			codes = stCodes;
			length = stLength;
		}

		public void BuildCodes()
		{
			int num = freqs.Length;
			int[] array = new int[maxLength];
			int num2 = 0;
			codes = new short[freqs.Length];
			for (int i = 0; i < maxLength; i++)
			{
				array[i] = num2;
				num2 += bl_counts[i] << 15 - i;
			}
			bool flag = 0 == 0;
			for (int j = 0; j < numCodes; j++)
			{
				int i = length[j];
				if (i > 0)
				{
					codes[j] = BitReverse(array[i - 1]);
					array[i - 1] += 1 << 16 - i;
				}
			}
		}

		private void BuildLength(int[] childs)
		{
			length = new byte[freqs.Length];
			int num = childs.Length / 2;
			int num2 = (num + 1) / 2;
			int num3 = 0;
			for (int i = 0; i < maxLength; i++)
			{
				bl_counts[i] = 0;
			}
			int[] array = new int[num];
			array[num - 1] = 0;
			for (int i = num - 1; i >= 0; i--)
			{
				if (childs[2 * i + 1] != -1)
				{
					int num4 = array[i] + 1;
					if (num4 > maxLength)
					{
						num4 = maxLength;
						num3++;
					}
					array[childs[2 * i]] = (array[childs[2 * i + 1]] = num4);
				}
				else
				{
					int num4 = array[i];
					bl_counts[num4 - 1]++;
					length[childs[2 * i]] = (byte)array[i];
				}
			}
			if (num3 == 0)
			{
				return;
			}
			int num5 = maxLength - 1;
			while (true)
			{
				if (bl_counts[--num5] != 0)
				{
					do
					{
						bl_counts[num5]--;
						bl_counts[++num5]++;
						num3 -= 1 << maxLength - 1 - num5;
					}
					while (num3 > 0 && num5 < maxLength - 1);
					if (num3 <= 0)
					{
						break;
					}
				}
			}
			bl_counts[maxLength - 1] += num3;
			bl_counts[maxLength - 2] -= num3;
			int num6 = 2 * num2;
			for (int num7 = maxLength; num7 != 0; num7--)
			{
				int num8 = bl_counts[num7 - 1];
				while (num8 > 0)
				{
					int num9 = 2 * childs[num6++];
					if (childs[num9 + 1] == -1)
					{
						length[childs[num9]] = (byte)num7;
						num8--;
					}
				}
			}
		}

		public void BuildTree()
		{
			int num = freqs.Length;
			int[] array = new int[num];
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				int num4 = freqs[i];
				if (num4 != 0)
				{
					int num5 = num2++;
					int num6;
					while (num5 > 0 && freqs[array[num6 = (num5 - 1) / 2]] > num4)
					{
						array[num5] = array[num6];
						num5 = num6;
					}
					array[num5] = i;
					num3 = i;
				}
			}
			while (num2 < 2)
			{
				int num7 = ((num3 < 2) ? (++num3) : 0);
				array[num2++] = num7;
			}
			numCodes = Math.Max(num3 + 1, minNumCodes);
			int num8 = num2;
			int[] array2 = new int[4 * num2 - 2];
			int[] array3 = new int[2 * num2 - 1];
			int num9 = num8;
			for (int j = 0; j < num2; j++)
			{
				int num7 = (array2[2 * j] = array[j]);
				array2[2 * j + 1] = -1;
				array3[j] = freqs[num7] << 8;
				array[j] = j;
			}
			do
			{
				int num10 = array[0];
				int num11 = array[--num2];
				int num6 = 0;
				int num12;
				for (num12 = 1; num12 < num2; num12 = num12 * 2 + 1)
				{
					if (num12 + 1 < num2 && array3[array[num12]] > array3[array[num12 + 1]])
					{
						num12++;
					}
					array[num6] = array[num12];
					num6 = num12;
				}
				int num13 = array3[num11];
				while ((num12 = num6) > 0 && array3[array[num6 = (num12 - 1) / 2]] > num13)
				{
					array[num12] = array[num6];
				}
				array[num12] = num11;
				int num14 = array[0];
				num11 = num9++;
				array2[2 * num11] = num10;
				array2[2 * num11 + 1] = num14;
				int num15 = Math.Min(array3[num10] & 0xFF, array3[num14] & 0xFF);
				num13 = (array3[num11] = array3[num10] + array3[num14] - num15 + 1);
				num6 = 0;
				for (num12 = 1; num12 < num2; num12 = num6 * 2 + 1)
				{
					if (num12 + 1 < num2 && array3[array[num12]] > array3[array[num12 + 1]])
					{
						num12++;
					}
					array[num6] = array[num12];
					num6 = num12;
				}
				while ((num12 = num6) > 0 && array3[array[num6 = (num12 - 1) / 2]] > num13)
				{
					array[num12] = array[num6];
				}
				array[num12] = num11;
			}
			while (num2 > 1);
			if (array[0] != array2.Length / 2 - 1)
			{
				throw new SharpZipBaseException("Heap invariant violated");
			}
			BuildLength(array2);
		}

		public int GetEncodedLength()
		{
			int num = 0;
			for (int i = 0; i < freqs.Length; i++)
			{
				num += freqs[i] * length[i];
			}
			return num;
		}

		public void CalcBLFreq(Tree blTree)
		{
			int num = -1;
			int num2 = 0;
			while (num2 < numCodes)
			{
				int num3 = 1;
				int num4 = length[num2];
				int num5;
				int num6;
				if (num4 == 0)
				{
					num5 = 138;
					num6 = 3;
				}
				else
				{
					num5 = 6;
					num6 = 3;
					if (num != num4)
					{
						blTree.freqs[num4]++;
						num3 = 0;
					}
				}
				num = num4;
				num2++;
				while (num2 < numCodes && num == length[num2])
				{
					num2++;
					if (++num3 >= num5)
					{
						break;
					}
				}
				if (num3 < num6)
				{
					blTree.freqs[num] += (short)num3;
				}
				else if (num != 0)
				{
					blTree.freqs[REP_3_6]++;
				}
				else if (num3 <= 10)
				{
					blTree.freqs[REP_3_10]++;
				}
				else
				{
					blTree.freqs[REP_11_138]++;
				}
			}
		}

		public void WriteTree(Tree blTree)
		{
			int num = -1;
			int num2 = 0;
			while (num2 < numCodes)
			{
				int num3 = 1;
				int num4 = length[num2];
				int num5;
				int num6;
				if (num4 == 0)
				{
					num5 = 138;
					num6 = 3;
				}
				else
				{
					num5 = 6;
					num6 = 3;
					if (num != num4)
					{
						blTree.WriteSymbol(num4);
						num3 = 0;
					}
				}
				num = num4;
				num2++;
				while (num2 < numCodes && num == length[num2])
				{
					num2++;
					if (++num3 >= num5)
					{
						break;
					}
				}
				if (num3 < num6)
				{
					while (num3-- > 0)
					{
						blTree.WriteSymbol(num);
					}
				}
				else if (num != 0)
				{
					blTree.WriteSymbol(REP_3_6);
					dh.pending.WriteBits(num3 - 3, 2);
				}
				else if (num3 <= 10)
				{
					blTree.WriteSymbol(REP_3_10);
					dh.pending.WriteBits(num3 - 3, 3);
				}
				else
				{
					blTree.WriteSymbol(REP_11_138);
					dh.pending.WriteBits(num3 - 11, 7);
				}
			}
		}
	}

	private static int BUFSIZE;

	private static int LITERAL_NUM;

	private static int DIST_NUM;

	private static int BITLEN_NUM;

	private static int REP_3_6;

	private static int REP_3_10;

	private static int REP_11_138;

	private static int EOF_SYMBOL;

	private static int[] BL_ORDER;

	private static byte[] bit4Reverse;

	public DeflaterPending pending;

	private Tree literalTree;

	private Tree distTree;

	private Tree blTree;

	private short[] d_buf;

	private byte[] l_buf;

	private int last_lit;

	private int extra_bits;

	private static short[] staticLCodes;

	private static byte[] staticLLength;

	private static short[] staticDCodes;

	private static byte[] staticDLength;

	public static short BitReverse(int toReverse)
	{
		return (short)((bit4Reverse[toReverse & 0xF] << 12) | (bit4Reverse[(toReverse >> 4) & 0xF] << 8) | (bit4Reverse[(toReverse >> 8) & 0xF] << 4) | bit4Reverse[toReverse >> 12]);
	}

	static DeflaterHuffman()
	{
		BUFSIZE = 16384;
		LITERAL_NUM = 286;
		DIST_NUM = 30;
		BITLEN_NUM = 19;
		REP_3_6 = 16;
		REP_3_10 = 17;
		REP_11_138 = 18;
		EOF_SYMBOL = 256;
		BL_ORDER = new int[19]
		{
			16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
			11, 4, 12, 3, 13, 2, 14, 1, 15
		};
		bit4Reverse = new byte[16]
		{
			0, 8, 4, 12, 2, 10, 6, 14, 1, 9,
			5, 13, 3, 11, 7, 15
		};
		staticLCodes = new short[LITERAL_NUM];
		staticLLength = new byte[LITERAL_NUM];
		int num = 0;
		while (num < 144)
		{
			staticLCodes[num] = BitReverse(48 + num << 8);
			staticLLength[num++] = 8;
		}
		while (num < 256)
		{
			staticLCodes[num] = BitReverse(256 + num << 7);
			staticLLength[num++] = 9;
		}
		while (num < 280)
		{
			staticLCodes[num] = BitReverse(-256 + num << 9);
			staticLLength[num++] = 7;
		}
		while (num < LITERAL_NUM)
		{
			staticLCodes[num] = BitReverse(-88 + num << 8);
			staticLLength[num++] = 8;
		}
		staticDCodes = new short[DIST_NUM];
		staticDLength = new byte[DIST_NUM];
		for (num = 0; num < DIST_NUM; num++)
		{
			staticDCodes[num] = BitReverse(num << 11);
			staticDLength[num] = 5;
		}
	}

	public DeflaterHuffman(DeflaterPending pending)
	{
		this.pending = pending;
		literalTree = new Tree(this, LITERAL_NUM, 257, 15);
		distTree = new Tree(this, DIST_NUM, 1, 15);
		blTree = new Tree(this, BITLEN_NUM, 4, 7);
		d_buf = new short[BUFSIZE];
		l_buf = new byte[BUFSIZE];
	}

	public void Reset()
	{
		last_lit = 0;
		extra_bits = 0;
		literalTree.Reset();
		distTree.Reset();
		blTree.Reset();
	}

	private int Lcode(int len)
	{
		if (len == 255)
		{
			return 285;
		}
		int num = 257;
		while (len >= 8)
		{
			num += 4;
			len >>= 1;
		}
		return num + len;
	}

	private int Dcode(int distance)
	{
		int num = 0;
		while (distance >= 4)
		{
			num += 2;
			distance >>= 1;
		}
		return num + distance;
	}

	public void SendAllTrees(int blTreeCodes)
	{
		blTree.BuildCodes();
		literalTree.BuildCodes();
		distTree.BuildCodes();
		pending.WriteBits(literalTree.numCodes - 257, 5);
		pending.WriteBits(distTree.numCodes - 1, 5);
		pending.WriteBits(blTreeCodes - 4, 4);
		for (int i = 0; i < blTreeCodes; i++)
		{
			pending.WriteBits(blTree.length[BL_ORDER[i]], 3);
		}
		literalTree.WriteTree(blTree);
		distTree.WriteTree(blTree);
	}

	public void CompressBlock()
	{
		for (int i = 0; i < last_lit; i++)
		{
			int num = l_buf[i] & 0xFF;
			int num2 = d_buf[i];
			if (num2-- != 0)
			{
				int num3 = Lcode(num);
				literalTree.WriteSymbol(num3);
				int num4 = (num3 - 261) / 4;
				if (num4 > 0 && num4 <= 5)
				{
					pending.WriteBits(num & ((1 << num4) - 1), num4);
				}
				int num5 = Dcode(num2);
				distTree.WriteSymbol(num5);
				num4 = num5 / 2 - 1;
				if (num4 > 0)
				{
					pending.WriteBits(num2 & ((1 << num4) - 1), num4);
				}
			}
			else
			{
				literalTree.WriteSymbol(num);
			}
		}
		literalTree.WriteSymbol(EOF_SYMBOL);
	}

	public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
	{
		pending.WriteBits(lastBlock ? 1 : 0, 3);
		pending.AlignToByte();
		pending.WriteShort(storedLength);
		pending.WriteShort(~storedLength);
		pending.WriteBlock(stored, storedOffset, storedLength);
		Reset();
	}

	public void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
	{
		literalTree.freqs[EOF_SYMBOL]++;
		literalTree.BuildTree();
		distTree.BuildTree();
		literalTree.CalcBLFreq(blTree);
		distTree.CalcBLFreq(blTree);
		blTree.BuildTree();
		int num = 4;
		for (int num2 = 18; num2 > num; num2--)
		{
			if (blTree.length[BL_ORDER[num2]] > 0)
			{
				num = num2 + 1;
			}
		}
		int num3 = 14 + num * 3 + blTree.GetEncodedLength() + literalTree.GetEncodedLength() + distTree.GetEncodedLength() + extra_bits;
		int num4 = extra_bits;
		for (int num2 = 0; num2 < LITERAL_NUM; num2++)
		{
			num4 += literalTree.freqs[num2] * staticLLength[num2];
		}
		for (int num2 = 0; num2 < DIST_NUM; num2++)
		{
			num4 += distTree.freqs[num2] * staticDLength[num2];
		}
		if (num3 >= num4)
		{
			num3 = num4;
		}
		if (storedOffset >= 0 && storedLength + 4 < num3 >> 3)
		{
			FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
		}
		else if (num3 == num4)
		{
			pending.WriteBits(2 + (lastBlock ? 1 : 0), 3);
			literalTree.SetStaticCodes(staticLCodes, staticLLength);
			distTree.SetStaticCodes(staticDCodes, staticDLength);
			CompressBlock();
			Reset();
		}
		else
		{
			pending.WriteBits(4 + (lastBlock ? 1 : 0), 3);
			SendAllTrees(num);
			CompressBlock();
			Reset();
		}
	}

	public bool IsFull()
	{
		return last_lit >= BUFSIZE;
	}

	public bool TallyLit(int lit)
	{
		d_buf[last_lit] = 0;
		l_buf[last_lit++] = (byte)lit;
		literalTree.freqs[lit]++;
		return IsFull();
	}

	public bool TallyDist(int dist, int len)
	{
		d_buf[last_lit] = (short)dist;
		l_buf[last_lit++] = (byte)(len - 3);
		int num = Lcode(len - 3);
		literalTree.freqs[num]++;
		if (num >= 265 && num < 285)
		{
			extra_bits += (num - 261) / 4;
		}
		int num2 = Dcode(dist - 1);
		distTree.freqs[num2]++;
		if (num2 >= 4)
		{
			extra_bits += num2 / 2 - 1;
		}
		return IsFull();
	}
}
