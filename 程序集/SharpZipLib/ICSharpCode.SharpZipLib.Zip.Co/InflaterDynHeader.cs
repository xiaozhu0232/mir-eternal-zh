using System;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip.Compression;

internal class InflaterDynHeader
{
	private const int LNUM = 0;

	private const int DNUM = 1;

	private const int BLNUM = 2;

	private const int BLLENS = 3;

	private const int LENS = 4;

	private const int REPS = 5;

	private static readonly int[] repMin = new int[3] { 3, 3, 11 };

	private static readonly int[] repBits = new int[3] { 2, 3, 7 };

	private byte[] blLens;

	private byte[] litdistLens;

	private InflaterHuffmanTree blTree;

	private int mode;

	private int lnum;

	private int dnum;

	private int blnum;

	private int num;

	private int repSymbol;

	private byte lastLen;

	private int ptr;

	private static readonly int[] BL_ORDER = new int[19]
	{
		16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
		11, 4, 12, 3, 13, 2, 14, 1, 15
	};

	public bool Decode(StreamManipulator input)
	{
		while (true)
		{
			bool flag = true;
			switch (mode)
			{
			case 0:
				lnum = input.PeekBits(5);
				if (lnum < 0)
				{
					return false;
				}
				lnum += 257;
				input.DropBits(5);
				mode = 1;
				goto case 1;
			case 1:
				dnum = input.PeekBits(5);
				if (dnum < 0)
				{
					return false;
				}
				dnum++;
				input.DropBits(5);
				this.num = lnum + dnum;
				litdistLens = new byte[this.num];
				mode = 2;
				goto case 2;
			case 2:
				blnum = input.PeekBits(4);
				if (blnum < 0)
				{
					return false;
				}
				blnum += 4;
				input.DropBits(4);
				blLens = new byte[19];
				ptr = 0;
				mode = 3;
				goto case 3;
			case 3:
				while (ptr < blnum)
				{
					int num = input.PeekBits(3);
					if (num < 0)
					{
						return false;
					}
					input.DropBits(3);
					blLens[BL_ORDER[ptr]] = (byte)num;
					ptr++;
				}
				blTree = new InflaterHuffmanTree(blLens);
				blLens = null;
				ptr = 0;
				mode = 4;
				goto case 4;
			case 4:
			{
				int symbol;
				while (((symbol = blTree.GetSymbol(input)) & -16) == 0)
				{
					litdistLens[ptr++] = (lastLen = (byte)symbol);
					if (ptr == this.num)
					{
						return true;
					}
				}
				if (symbol < 0)
				{
					return false;
				}
				if (symbol >= 17)
				{
					lastLen = 0;
				}
				else if (ptr == 0)
				{
					throw new SharpZipBaseException();
				}
				repSymbol = symbol - 16;
				mode = 5;
				break;
			}
			case 5:
				break;
			default:
				continue;
			}
			int n = repBits[repSymbol];
			int num2 = input.PeekBits(n);
			if (num2 < 0)
			{
				return false;
			}
			input.DropBits(n);
			num2 += repMin[repSymbol];
			if (ptr + num2 > this.num)
			{
				throw new SharpZipBaseException();
			}
			while (num2-- > 0)
			{
				litdistLens[ptr++] = lastLen;
			}
			if (ptr == this.num)
			{
				break;
			}
			mode = 4;
		}
		return true;
	}

	public InflaterHuffmanTree BuildLitLenTree()
	{
		byte[] array = new byte[lnum];
		Array.Copy(litdistLens, 0, array, 0, lnum);
		return new InflaterHuffmanTree(array);
	}

	public InflaterHuffmanTree BuildDistTree()
	{
		byte[] array = new byte[dnum];
		Array.Copy(litdistLens, lnum, array, 0, dnum);
		return new InflaterHuffmanTree(array);
	}
}
