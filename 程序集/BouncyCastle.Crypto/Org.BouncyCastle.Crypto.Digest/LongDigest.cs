using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public abstract class LongDigest : IDigest, IMemoable
{
	private int MyByteLength = 128;

	private byte[] xBuf;

	private int xBufOff;

	private long byteCount1;

	private long byteCount2;

	internal ulong H1;

	internal ulong H2;

	internal ulong H3;

	internal ulong H4;

	internal ulong H5;

	internal ulong H6;

	internal ulong H7;

	internal ulong H8;

	private ulong[] W = new ulong[80];

	private int wOff;

	internal static readonly ulong[] K = new ulong[80]
	{
		4794697086780616226uL, 8158064640168781261uL, 13096744586834688815uL, 16840607885511220156uL, 4131703408338449720uL, 6480981068601479193uL, 10538285296894168987uL, 12329834152419229976uL, 15566598209576043074uL, 1334009975649890238uL,
		2608012711638119052uL, 6128411473006802146uL, 8268148722764581231uL, 9286055187155687089uL, 11230858885718282805uL, 13951009754708518548uL, 16472876342353939154uL, 17275323862435702243uL, 1135362057144423861uL, 2597628984639134821uL,
		3308224258029322869uL, 5365058923640841347uL, 6679025012923562964uL, 8573033837759648693uL, 10970295158949994411uL, 12119686244451234320uL, 12683024718118986047uL, 13788192230050041572uL, 14330467153632333762uL, 15395433587784984357uL,
		489312712824947311uL, 1452737877330783856uL, 2861767655752347644uL, 3322285676063803686uL, 5560940570517711597uL, 5996557281743188959uL, 7280758554555802590uL, 8532644243296465576uL, 9350256976987008742uL, 10552545826968843579uL,
		11727347734174303076uL, 12113106623233404929uL, 14000437183269869457uL, 14369950271660146224uL, 15101387698204529176uL, 15463397548674623760uL, 17586052441742319658uL, 1182934255886127544uL, 1847814050463011016uL, 2177327727835720531uL,
		2830643537854262169uL, 3796741975233480872uL, 4115178125766777443uL, 5681478168544905931uL, 6601373596472566643uL, 7507060721942968483uL, 8399075790359081724uL, 8693463985226723168uL, 9568029438360202098uL, 10144078919501101548uL,
		10430055236837252648uL, 11840083180663258601uL, 13761210420658862357uL, 14299343276471374635uL, 14566680578165727644uL, 15097957966210449927uL, 16922976911328602910uL, 17689382322260857208uL, 500013540394364858uL, 748580250866718886uL,
		1242879168328830382uL, 1977374033974150939uL, 2944078676154940804uL, 3659926193048069267uL, 4368137639120453308uL, 4836135668995329356uL, 5532061633213252278uL, 6448918945643986474uL, 6902733635092675308uL, 7801388544844847127uL
	};

	public abstract string AlgorithmName { get; }

	internal LongDigest()
	{
		xBuf = new byte[8];
		Reset();
	}

	internal LongDigest(LongDigest t)
	{
		xBuf = new byte[t.xBuf.Length];
		CopyIn(t);
	}

	protected void CopyIn(LongDigest t)
	{
		Array.Copy(t.xBuf, 0, xBuf, 0, t.xBuf.Length);
		xBufOff = t.xBufOff;
		byteCount1 = t.byteCount1;
		byteCount2 = t.byteCount2;
		H1 = t.H1;
		H2 = t.H2;
		H3 = t.H3;
		H4 = t.H4;
		H5 = t.H5;
		H6 = t.H6;
		H7 = t.H7;
		H8 = t.H8;
		Array.Copy(t.W, 0, W, 0, t.W.Length);
		wOff = t.wOff;
	}

	public void Update(byte input)
	{
		xBuf[xBufOff++] = input;
		if (xBufOff == xBuf.Length)
		{
			ProcessWord(xBuf, 0);
			xBufOff = 0;
		}
		byteCount1++;
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		while (xBufOff != 0 && length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
		while (length > xBuf.Length)
		{
			ProcessWord(input, inOff);
			inOff += xBuf.Length;
			length -= xBuf.Length;
			byteCount1 += xBuf.Length;
		}
		while (length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
	}

	public void Finish()
	{
		AdjustByteCounts();
		long lowW = byteCount1 << 3;
		long hiW = byteCount2;
		Update(128);
		while (xBufOff != 0)
		{
			Update(0);
		}
		ProcessLength(lowW, hiW);
		ProcessBlock();
	}

	public virtual void Reset()
	{
		byteCount1 = 0L;
		byteCount2 = 0L;
		xBufOff = 0;
		for (int i = 0; i < xBuf.Length; i++)
		{
			xBuf[i] = 0;
		}
		wOff = 0;
		Array.Clear(W, 0, W.Length);
	}

	internal void ProcessWord(byte[] input, int inOff)
	{
		W[wOff] = Pack.BE_To_UInt64(input, inOff);
		if (++wOff == 16)
		{
			ProcessBlock();
		}
	}

	private void AdjustByteCounts()
	{
		if (byteCount1 > 2305843009213693951L)
		{
			byteCount2 += (long)((ulong)byteCount1 >> 61);
			byteCount1 &= 2305843009213693951L;
		}
	}

	internal void ProcessLength(long lowW, long hiW)
	{
		if (wOff > 14)
		{
			ProcessBlock();
		}
		W[14] = (ulong)hiW;
		W[15] = (ulong)lowW;
	}

	internal void ProcessBlock()
	{
		AdjustByteCounts();
		for (int i = 16; i <= 79; i++)
		{
			W[i] = Sigma1(W[i - 2]) + W[i - 7] + Sigma0(W[i - 15]) + W[i - 16];
		}
		ulong num = H1;
		ulong num2 = H2;
		ulong num3 = H3;
		ulong num4 = H4;
		ulong num5 = H5;
		ulong num6 = H6;
		ulong num7 = H7;
		ulong num8 = H8;
		int num9 = 0;
		for (int j = 0; j < 10; j++)
		{
			num8 += Sum1(num5) + Ch(num5, num6, num7) + K[num9] + W[num9++];
			num4 += num8;
			num8 += Sum0(num) + Maj(num, num2, num3);
			num7 += Sum1(num4) + Ch(num4, num5, num6) + K[num9] + W[num9++];
			num3 += num7;
			num7 += Sum0(num8) + Maj(num8, num, num2);
			num6 += Sum1(num3) + Ch(num3, num4, num5) + K[num9] + W[num9++];
			num2 += num6;
			num6 += Sum0(num7) + Maj(num7, num8, num);
			num5 += Sum1(num2) + Ch(num2, num3, num4) + K[num9] + W[num9++];
			num += num5;
			num5 += Sum0(num6) + Maj(num6, num7, num8);
			num4 += Sum1(num) + Ch(num, num2, num3) + K[num9] + W[num9++];
			num8 += num4;
			num4 += Sum0(num5) + Maj(num5, num6, num7);
			num3 += Sum1(num8) + Ch(num8, num, num2) + K[num9] + W[num9++];
			num7 += num3;
			num3 += Sum0(num4) + Maj(num4, num5, num6);
			num2 += Sum1(num7) + Ch(num7, num8, num) + K[num9] + W[num9++];
			num6 += num2;
			num2 += Sum0(num3) + Maj(num3, num4, num5);
			num += Sum1(num6) + Ch(num6, num7, num8) + K[num9] + W[num9++];
			num5 += num;
			num += Sum0(num2) + Maj(num2, num3, num4);
		}
		H1 += num;
		H2 += num2;
		H3 += num3;
		H4 += num4;
		H5 += num5;
		H6 += num6;
		H7 += num7;
		H8 += num8;
		wOff = 0;
		Array.Clear(W, 0, 16);
	}

	private static ulong Ch(ulong x, ulong y, ulong z)
	{
		return (x & y) ^ (~x & z);
	}

	private static ulong Maj(ulong x, ulong y, ulong z)
	{
		return (x & y) ^ (x & z) ^ (y & z);
	}

	private static ulong Sum0(ulong x)
	{
		return ((x << 36) | (x >> 28)) ^ ((x << 30) | (x >> 34)) ^ ((x << 25) | (x >> 39));
	}

	private static ulong Sum1(ulong x)
	{
		return ((x << 50) | (x >> 14)) ^ ((x << 46) | (x >> 18)) ^ ((x << 23) | (x >> 41));
	}

	private static ulong Sigma0(ulong x)
	{
		return ((x << 63) | (x >> 1)) ^ ((x << 56) | (x >> 8)) ^ (x >> 7);
	}

	private static ulong Sigma1(ulong x)
	{
		return ((x << 45) | (x >> 19)) ^ ((x << 3) | (x >> 61)) ^ (x >> 6);
	}

	public int GetByteLength()
	{
		return MyByteLength;
	}

	public abstract int GetDigestSize();

	public abstract int DoFinal(byte[] output, int outOff);

	public abstract IMemoable Copy();

	public abstract void Reset(IMemoable t);
}
