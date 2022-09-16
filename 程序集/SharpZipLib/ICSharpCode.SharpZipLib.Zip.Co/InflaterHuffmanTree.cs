using System;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip.Compression;

public class InflaterHuffmanTree
{
	private static int MAX_BITLEN;

	private short[] tree;

	public static InflaterHuffmanTree defLitLenTree;

	public static InflaterHuffmanTree defDistTree;

	static InflaterHuffmanTree()
	{
		MAX_BITLEN = 15;
		try
		{
			byte[] array = new byte[288];
			int num = 0;
			while (num < 144)
			{
				array[num++] = 8;
			}
			while (num < 256)
			{
				array[num++] = 9;
			}
			while (num < 280)
			{
				array[num++] = 7;
			}
			while (num < 288)
			{
				array[num++] = 8;
			}
			defLitLenTree = new InflaterHuffmanTree(array);
			array = new byte[32];
			num = 0;
			while (num < 32)
			{
				array[num++] = 5;
			}
			defDistTree = new InflaterHuffmanTree(array);
		}
		catch (Exception)
		{
			throw new SharpZipBaseException("InflaterHuffmanTree: static tree length illegal");
		}
	}

	public InflaterHuffmanTree(byte[] codeLengths)
	{
		BuildTree(codeLengths);
	}

	private void BuildTree(byte[] codeLengths)
	{
		int[] array = new int[MAX_BITLEN + 1];
		int[] array2 = new int[MAX_BITLEN + 1];
		for (int i = 0; i < codeLengths.Length; i++)
		{
			int num = codeLengths[i];
			if (num > 0)
			{
				array[num]++;
			}
		}
		int num2 = 0;
		int num3 = 512;
		for (int num = 1; num <= MAX_BITLEN; num++)
		{
			array2[num] = num2;
			num2 += array[num] << 16 - num;
			if (num >= 10)
			{
				int num4 = array2[num] & 0x1FF80;
				int num5 = num2 & 0x1FF80;
				num3 += num5 - num4 >> 16 - num;
			}
		}
		tree = new short[num3];
		int num6 = 512;
		for (int num = MAX_BITLEN; num >= 10; num--)
		{
			int num5 = num2 & 0x1FF80;
			num2 -= array[num] << 16 - num;
			int num4 = num2 & 0x1FF80;
			for (int i = num4; i < num5; i += 128)
			{
				tree[DeflaterHuffman.BitReverse(i)] = (short)((-num6 << 4) | num);
				num6 += 1 << num - 9;
			}
		}
		for (int i = 0; i < codeLengths.Length; i++)
		{
			int num = codeLengths[i];
			if (num == 0)
			{
				continue;
			}
			num2 = array2[num];
			int num7 = DeflaterHuffman.BitReverse(num2);
			if (num <= 9)
			{
				do
				{
					tree[num7] = (short)((i << 4) | num);
					num7 += 1 << num;
				}
				while (num7 < 512);
			}
			else
			{
				int num8 = tree[num7 & 0x1FF];
				int num9 = 1 << (num8 & 0xF);
				num8 = -(num8 >> 4);
				do
				{
					tree[num8 | (num7 >> 9)] = (short)((i << 4) | num);
					num7 += 1 << num;
				}
				while (num7 < num9);
			}
			array2[num] = num2 + (1 << 16 - num);
		}
	}

	public int GetSymbol(StreamManipulator input)
	{
		int availableBits;
		int num;
		int num2;
		if ((num = input.PeekBits(9)) >= 0)
		{
			if ((num2 = tree[num]) >= 0)
			{
				input.DropBits(num2 & 0xF);
				return num2 >> 4;
			}
			int num3 = -(num2 >> 4);
			int n = num2 & 0xF;
			if ((num = input.PeekBits(n)) >= 0)
			{
				num2 = tree[num3 | (num >> 9)];
				input.DropBits(num2 & 0xF);
				return num2 >> 4;
			}
			availableBits = input.AvailableBits;
			num = input.PeekBits(availableBits);
			num2 = tree[num3 | (num >> 9)];
			if ((num2 & 0xF) <= availableBits)
			{
				input.DropBits(num2 & 0xF);
				return num2 >> 4;
			}
			return -1;
		}
		availableBits = input.AvailableBits;
		num = input.PeekBits(availableBits);
		num2 = tree[num];
		if (num2 >= 0 && (num2 & 0xF) <= availableBits)
		{
			input.DropBits(num2 & 0xF);
			return num2 >> 4;
		}
		return -1;
	}
}
