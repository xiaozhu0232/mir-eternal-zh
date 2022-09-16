using System;

namespace Org.BouncyCastle.Utilities.Zlib;

internal sealed class InfCodes
{
	private const int Z_OK = 0;

	private const int Z_STREAM_END = 1;

	private const int Z_NEED_DICT = 2;

	private const int Z_ERRNO = -1;

	private const int Z_STREAM_ERROR = -2;

	private const int Z_DATA_ERROR = -3;

	private const int Z_MEM_ERROR = -4;

	private const int Z_BUF_ERROR = -5;

	private const int Z_VERSION_ERROR = -6;

	private const int START = 0;

	private const int LEN = 1;

	private const int LENEXT = 2;

	private const int DIST = 3;

	private const int DISTEXT = 4;

	private const int COPY = 5;

	private const int LIT = 6;

	private const int WASH = 7;

	private const int END = 8;

	private const int BADCODE = 9;

	private static readonly int[] inflate_mask = new int[17]
	{
		0, 1, 3, 7, 15, 31, 63, 127, 255, 511,
		1023, 2047, 4095, 8191, 16383, 32767, 65535
	};

	private int mode;

	private int len;

	private int[] tree;

	private int tree_index = 0;

	private int need;

	private int lit;

	private int get;

	private int dist;

	private byte lbits;

	private byte dbits;

	private int[] ltree;

	private int ltree_index;

	private int[] dtree;

	private int dtree_index;

	internal InfCodes()
	{
	}

	internal void init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, ZStream z)
	{
		mode = 0;
		lbits = (byte)bl;
		dbits = (byte)bd;
		ltree = tl;
		ltree_index = tl_index;
		dtree = td;
		dtree_index = td_index;
		tree = null;
	}

	internal int proc(InfBlocks s, ZStream z, int r)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		num3 = z.next_in_index;
		int num4 = z.avail_in;
		num = s.bitb;
		num2 = s.bitk;
		int num5 = s.write;
		int num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
		while (true)
		{
			switch (mode)
			{
			case 0:
				if (num6 >= 258 && num4 >= 10)
				{
					s.bitb = num;
					s.bitk = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.write = num5;
					r = inflate_fast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, s, z);
					num3 = z.next_in_index;
					num4 = z.avail_in;
					num = s.bitb;
					num2 = s.bitk;
					num5 = s.write;
					num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
					if (r != 0)
					{
						mode = ((r == 1) ? 7 : 9);
						break;
					}
				}
				need = lbits;
				tree = ltree;
				tree_index = ltree_index;
				mode = 1;
				goto case 1;
			case 1:
			{
				int num7;
				for (num7 = need; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.bitb = num;
					s.bitk = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.write = num5;
					return s.inflate_flush(z, r);
				}
				int num8 = (tree_index + (num & inflate_mask[num7])) * 3;
				num >>= tree[num8 + 1];
				num2 -= tree[num8 + 1];
				int num9 = tree[num8];
				if (num9 == 0)
				{
					lit = tree[num8 + 2];
					mode = 6;
					break;
				}
				if (((uint)num9 & 0x10u) != 0)
				{
					get = num9 & 0xF;
					len = tree[num8 + 2];
					mode = 2;
					break;
				}
				if ((num9 & 0x40) == 0)
				{
					need = num9;
					tree_index = num8 / 3 + tree[num8 + 2];
					break;
				}
				if (((uint)num9 & 0x20u) != 0)
				{
					mode = 7;
					break;
				}
				mode = 9;
				z.msg = "invalid literal/length code";
				r = -3;
				s.bitb = num;
				s.bitk = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.write = num5;
				return s.inflate_flush(z, r);
			}
			case 2:
			{
				int num7;
				for (num7 = get; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.bitb = num;
					s.bitk = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.write = num5;
					return s.inflate_flush(z, r);
				}
				len += num & inflate_mask[num7];
				num >>= num7;
				num2 -= num7;
				need = dbits;
				tree = dtree;
				tree_index = dtree_index;
				mode = 3;
				goto case 3;
			}
			case 3:
			{
				int num7;
				for (num7 = need; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.bitb = num;
					s.bitk = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.write = num5;
					return s.inflate_flush(z, r);
				}
				int num8 = (tree_index + (num & inflate_mask[num7])) * 3;
				num >>= tree[num8 + 1];
				num2 -= tree[num8 + 1];
				int num9 = tree[num8];
				if (((uint)num9 & 0x10u) != 0)
				{
					get = num9 & 0xF;
					dist = tree[num8 + 2];
					mode = 4;
					break;
				}
				if ((num9 & 0x40) == 0)
				{
					need = num9;
					tree_index = num8 / 3 + tree[num8 + 2];
					break;
				}
				mode = 9;
				z.msg = "invalid distance code";
				r = -3;
				s.bitb = num;
				s.bitk = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.write = num5;
				return s.inflate_flush(z, r);
			}
			case 4:
			{
				int num7;
				for (num7 = get; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.bitb = num;
					s.bitk = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.write = num5;
					return s.inflate_flush(z, r);
				}
				dist += num & inflate_mask[num7];
				num >>= num7;
				num2 -= num7;
				mode = 5;
				goto case 5;
			}
			case 5:
			{
				int i;
				for (i = num5 - dist; i < 0; i += s.end)
				{
				}
				while (len != 0)
				{
					if (num6 == 0)
					{
						if (num5 == s.end && s.read != 0)
						{
							num5 = 0;
							num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
						}
						if (num6 == 0)
						{
							s.write = num5;
							r = s.inflate_flush(z, r);
							num5 = s.write;
							num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
							if (num5 == s.end && s.read != 0)
							{
								num5 = 0;
								num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
							}
							if (num6 == 0)
							{
								s.bitb = num;
								s.bitk = num2;
								z.avail_in = num4;
								z.total_in += num3 - z.next_in_index;
								z.next_in_index = num3;
								s.write = num5;
								return s.inflate_flush(z, r);
							}
						}
					}
					s.window[num5++] = s.window[i++];
					num6--;
					if (i == s.end)
					{
						i = 0;
					}
					len--;
				}
				mode = 0;
				break;
			}
			case 6:
				if (num6 == 0)
				{
					if (num5 == s.end && s.read != 0)
					{
						num5 = 0;
						num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
					}
					if (num6 == 0)
					{
						s.write = num5;
						r = s.inflate_flush(z, r);
						num5 = s.write;
						num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
						if (num5 == s.end && s.read != 0)
						{
							num5 = 0;
							num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
						}
						if (num6 == 0)
						{
							s.bitb = num;
							s.bitk = num2;
							z.avail_in = num4;
							z.total_in += num3 - z.next_in_index;
							z.next_in_index = num3;
							s.write = num5;
							return s.inflate_flush(z, r);
						}
					}
				}
				r = 0;
				s.window[num5++] = (byte)lit;
				num6--;
				mode = 0;
				break;
			case 7:
				if (num2 > 7)
				{
					num2 -= 8;
					num4++;
					num3--;
				}
				s.write = num5;
				r = s.inflate_flush(z, r);
				num5 = s.write;
				num6 = ((num5 < s.read) ? (s.read - num5 - 1) : (s.end - num5));
				if (s.read != s.write)
				{
					s.bitb = num;
					s.bitk = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.write = num5;
					return s.inflate_flush(z, r);
				}
				mode = 8;
				goto case 8;
			case 8:
				r = 1;
				s.bitb = num;
				s.bitk = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.write = num5;
				return s.inflate_flush(z, r);
			case 9:
				r = -3;
				s.bitb = num;
				s.bitk = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.write = num5;
				return s.inflate_flush(z, r);
			default:
				r = -2;
				s.bitb = num;
				s.bitk = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.write = num5;
				return s.inflate_flush(z, r);
			}
		}
	}

	internal void free(ZStream z)
	{
	}

	internal int inflate_fast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InfBlocks s, ZStream z)
	{
		int next_in_index = z.next_in_index;
		int num = z.avail_in;
		int num2 = s.bitb;
		int num3 = s.bitk;
		int num4 = s.write;
		int num5 = ((num4 < s.read) ? (s.read - num4 - 1) : (s.end - num4));
		int num6 = inflate_mask[bl];
		int num7 = inflate_mask[bd];
		int num12;
		while (true)
		{
			if (num3 < 20)
			{
				num--;
				num2 |= (z.next_in[next_in_index++] & 0xFF) << num3;
				num3 += 8;
				continue;
			}
			int num8 = num2 & num6;
			int[] array = tl;
			int num9 = tl_index;
			int num10 = (num9 + num8) * 3;
			int num11;
			if ((num11 = array[num10]) == 0)
			{
				num2 >>= array[num10 + 1];
				num3 -= array[num10 + 1];
				s.window[num4++] = (byte)array[num10 + 2];
				num5--;
			}
			else
			{
				while (true)
				{
					num2 >>= array[num10 + 1];
					num3 -= array[num10 + 1];
					if (((uint)num11 & 0x10u) != 0)
					{
						num11 &= 0xF;
						num12 = array[num10 + 2] + (num2 & inflate_mask[num11]);
						num2 >>= num11;
						for (num3 -= num11; num3 < 15; num3 += 8)
						{
							num--;
							num2 |= (z.next_in[next_in_index++] & 0xFF) << num3;
						}
						num8 = num2 & num7;
						array = td;
						num9 = td_index;
						num10 = (num9 + num8) * 3;
						num11 = array[num10];
						while (true)
						{
							num2 >>= array[num10 + 1];
							num3 -= array[num10 + 1];
							if (((uint)num11 & 0x10u) != 0)
							{
								break;
							}
							if ((num11 & 0x40) == 0)
							{
								num8 += array[num10 + 2];
								num8 += num2 & inflate_mask[num11];
								num10 = (num9 + num8) * 3;
								num11 = array[num10];
								continue;
							}
							z.msg = "invalid distance code";
							num12 = z.avail_in - num;
							num12 = ((num3 >> 3 < num12) ? (num3 >> 3) : num12);
							num += num12;
							next_in_index -= num12;
							num3 -= num12 << 3;
							s.bitb = num2;
							s.bitk = num3;
							z.avail_in = num;
							z.total_in += next_in_index - z.next_in_index;
							z.next_in_index = next_in_index;
							s.write = num4;
							return -3;
						}
						for (num11 &= 0xF; num3 < num11; num3 += 8)
						{
							num--;
							num2 |= (z.next_in[next_in_index++] & 0xFF) << num3;
						}
						int num13 = array[num10 + 2] + (num2 & inflate_mask[num11]);
						num2 >>= num11;
						num3 -= num11;
						num5 -= num12;
						int num14;
						if (num4 >= num13)
						{
							num14 = num4 - num13;
							if (num4 - num14 > 0 && 2 > num4 - num14)
							{
								s.window[num4++] = s.window[num14++];
								s.window[num4++] = s.window[num14++];
								num12 -= 2;
							}
							else
							{
								Array.Copy(s.window, num14, s.window, num4, 2);
								num4 += 2;
								num14 += 2;
								num12 -= 2;
							}
						}
						else
						{
							num14 = num4 - num13;
							do
							{
								num14 += s.end;
							}
							while (num14 < 0);
							num11 = s.end - num14;
							if (num12 > num11)
							{
								num12 -= num11;
								if (num4 - num14 > 0 && num11 > num4 - num14)
								{
									do
									{
										s.window[num4++] = s.window[num14++];
									}
									while (--num11 != 0);
								}
								else
								{
									Array.Copy(s.window, num14, s.window, num4, num11);
									num4 += num11;
									num14 += num11;
									num11 = 0;
								}
								num14 = 0;
							}
						}
						if (num4 - num14 > 0 && num12 > num4 - num14)
						{
							do
							{
								s.window[num4++] = s.window[num14++];
							}
							while (--num12 != 0);
							break;
						}
						Array.Copy(s.window, num14, s.window, num4, num12);
						num4 += num12;
						num14 += num12;
						num12 = 0;
						break;
					}
					if ((num11 & 0x40) == 0)
					{
						num8 += array[num10 + 2];
						num8 += num2 & inflate_mask[num11];
						num10 = (num9 + num8) * 3;
						if ((num11 = array[num10]) == 0)
						{
							num2 >>= array[num10 + 1];
							num3 -= array[num10 + 1];
							s.window[num4++] = (byte)array[num10 + 2];
							num5--;
							break;
						}
						continue;
					}
					if (((uint)num11 & 0x20u) != 0)
					{
						num12 = z.avail_in - num;
						num12 = ((num3 >> 3 < num12) ? (num3 >> 3) : num12);
						num += num12;
						next_in_index -= num12;
						num3 -= num12 << 3;
						s.bitb = num2;
						s.bitk = num3;
						z.avail_in = num;
						z.total_in += next_in_index - z.next_in_index;
						z.next_in_index = next_in_index;
						s.write = num4;
						return 1;
					}
					z.msg = "invalid literal/length code";
					num12 = z.avail_in - num;
					num12 = ((num3 >> 3 < num12) ? (num3 >> 3) : num12);
					num += num12;
					next_in_index -= num12;
					num3 -= num12 << 3;
					s.bitb = num2;
					s.bitk = num3;
					z.avail_in = num;
					z.total_in += next_in_index - z.next_in_index;
					z.next_in_index = next_in_index;
					s.write = num4;
					return -3;
				}
			}
			if (num5 < 258 || num < 10)
			{
				break;
			}
		}
		num12 = z.avail_in - num;
		num12 = ((num3 >> 3 < num12) ? (num3 >> 3) : num12);
		num += num12;
		next_in_index -= num12;
		num3 -= num12 << 3;
		s.bitb = num2;
		s.bitk = num3;
		z.avail_in = num;
		z.total_in += next_in_index - z.next_in_index;
		z.next_in_index = next_in_index;
		s.write = num4;
		return 0;
	}
}
