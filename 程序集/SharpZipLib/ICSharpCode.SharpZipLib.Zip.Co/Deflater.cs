using System;

namespace ICSharpCode.SharpZipLib.Zip.Compression;

public class Deflater
{
	public static int BEST_COMPRESSION = 9;

	public static int BEST_SPEED = 1;

	public static int DEFAULT_COMPRESSION = -1;

	public static int NO_COMPRESSION = 0;

	public static int DEFLATED = 8;

	private static int IS_SETDICT = 1;

	private static int IS_FLUSHING = 4;

	private static int IS_FINISHING = 8;

	private static int INIT_STATE = 0;

	private static int SETDICT_STATE = 1;

	private static int BUSY_STATE = 16;

	private static int FLUSHING_STATE = 20;

	private static int FINISHING_STATE = 28;

	private static int FINISHED_STATE = 30;

	private static int CLOSED_STATE = 127;

	private int level;

	private bool noZlibHeaderOrFooter;

	private int state;

	private long totalOut;

	private DeflaterPending pending;

	private DeflaterEngine engine;

	public int Adler => engine.Adler;

	public int TotalIn => engine.TotalIn;

	public long TotalOut => totalOut;

	public bool IsFinished => state == FINISHED_STATE && pending.IsFlushed;

	public bool IsNeedingInput => engine.NeedsInput();

	public Deflater()
		: this(DEFAULT_COMPRESSION, noZlibHeaderOrFooter: false)
	{
	}

	public Deflater(int lvl)
		: this(lvl, noZlibHeaderOrFooter: false)
	{
	}

	public Deflater(int level, bool noZlibHeaderOrFooter)
	{
		if (level == DEFAULT_COMPRESSION)
		{
			level = 6;
		}
		else if (level < NO_COMPRESSION || level > BEST_COMPRESSION)
		{
			throw new ArgumentOutOfRangeException("level");
		}
		pending = new DeflaterPending();
		engine = new DeflaterEngine(pending);
		this.noZlibHeaderOrFooter = noZlibHeaderOrFooter;
		SetStrategy(DeflateStrategy.Default);
		SetLevel(level);
		Reset();
	}

	public void Reset()
	{
		state = (noZlibHeaderOrFooter ? BUSY_STATE : INIT_STATE);
		totalOut = 0L;
		pending.Reset();
		engine.Reset();
	}

	public void Flush()
	{
		state |= IS_FLUSHING;
	}

	public void Finish()
	{
		state |= IS_FLUSHING | IS_FINISHING;
	}

	public void SetInput(byte[] input)
	{
		SetInput(input, 0, input.Length);
	}

	public void SetInput(byte[] input, int off, int len)
	{
		if ((state & IS_FINISHING) != 0)
		{
			throw new InvalidOperationException("finish()/end() already called");
		}
		engine.SetInput(input, off, len);
	}

	public void SetLevel(int lvl)
	{
		if (lvl == DEFAULT_COMPRESSION)
		{
			lvl = 6;
		}
		else if (lvl < NO_COMPRESSION || lvl > BEST_COMPRESSION)
		{
			throw new ArgumentOutOfRangeException("lvl");
		}
		if (level != lvl)
		{
			level = lvl;
			engine.SetLevel(lvl);
		}
	}

	public int GetLevel()
	{
		return level;
	}

	public void SetStrategy(DeflateStrategy strategy)
	{
		engine.Strategy = strategy;
	}

	public int Deflate(byte[] output)
	{
		return Deflate(output, 0, output.Length);
	}

	public int Deflate(byte[] output, int offset, int length)
	{
		int num = length;
		if (state == CLOSED_STATE)
		{
			throw new InvalidOperationException("Deflater closed");
		}
		if (state < BUSY_STATE)
		{
			int num2 = DEFLATED + 112 << 8;
			int num3 = level - 1 >> 1;
			if (num3 < 0 || num3 > 3)
			{
				num3 = 3;
			}
			num2 |= num3 << 6;
			if ((state & IS_SETDICT) != 0)
			{
				num2 |= 0x20;
			}
			num2 += 31 - num2 % 31;
			pending.WriteShortMSB(num2);
			if ((state & IS_SETDICT) != 0)
			{
				int adler = engine.Adler;
				engine.ResetAdler();
				pending.WriteShortMSB(adler >> 16);
				pending.WriteShortMSB(adler & 0xFFFF);
			}
			state = BUSY_STATE | (state & (IS_FLUSHING | IS_FINISHING));
		}
		while (true)
		{
			bool flag = true;
			int num4 = pending.Flush(output, offset, length);
			offset += num4;
			totalOut += num4;
			length -= num4;
			if (length == 0 || state == FINISHED_STATE)
			{
				break;
			}
			if (engine.Deflate((state & IS_FLUSHING) != 0, (state & IS_FINISHING) != 0))
			{
				continue;
			}
			if (state == BUSY_STATE)
			{
				return num - length;
			}
			if (state == FLUSHING_STATE)
			{
				if (level != NO_COMPRESSION)
				{
					for (int num5 = 8 + (-pending.BitCount & 7); num5 > 0; num5 -= 10)
					{
						pending.WriteBits(2, 10);
					}
				}
				state = BUSY_STATE;
			}
			else if (state == FINISHING_STATE)
			{
				pending.AlignToByte();
				if (!noZlibHeaderOrFooter)
				{
					int adler2 = engine.Adler;
					pending.WriteShortMSB(adler2 >> 16);
					pending.WriteShortMSB(adler2 & 0xFFFF);
				}
				state = FINISHED_STATE;
			}
		}
		return num - length;
	}

	public void SetDictionary(byte[] dict)
	{
		SetDictionary(dict, 0, dict.Length);
	}

	public void SetDictionary(byte[] dict, int offset, int length)
	{
		if (state != INIT_STATE)
		{
			throw new InvalidOperationException();
		}
		state = SETDICT_STATE;
		engine.SetDictionary(dict, offset, length);
	}
}
