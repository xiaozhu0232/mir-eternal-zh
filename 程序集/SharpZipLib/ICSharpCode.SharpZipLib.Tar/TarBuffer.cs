using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarBuffer
{
	public const int BlockSize = 512;

	public const int DefaultBlockFactor = 20;

	public const int DefaultRecordSize = 10240;

	private Stream inputStream;

	private Stream outputStream;

	private byte[] recordBuffer;

	private int currentBlockIndex;

	private int currentRecordIndex;

	private int recordSize = 10240;

	private int blockFactor = 20;

	public int RecordSize => recordSize;

	public int BlockFactor => blockFactor;

	protected TarBuffer()
	{
	}

	public static TarBuffer CreateInputTarBuffer(Stream inputStream)
	{
		return CreateInputTarBuffer(inputStream, 20);
	}

	public static TarBuffer CreateInputTarBuffer(Stream inputStream, int blockFactor)
	{
		TarBuffer tarBuffer = new TarBuffer();
		tarBuffer.inputStream = inputStream;
		tarBuffer.outputStream = null;
		tarBuffer.Initialize(blockFactor);
		return tarBuffer;
	}

	public static TarBuffer CreateOutputTarBuffer(Stream outputStream)
	{
		return CreateOutputTarBuffer(outputStream, 20);
	}

	public static TarBuffer CreateOutputTarBuffer(Stream outputStream, int blockFactor)
	{
		TarBuffer tarBuffer = new TarBuffer();
		tarBuffer.inputStream = null;
		tarBuffer.outputStream = outputStream;
		tarBuffer.Initialize(blockFactor);
		return tarBuffer;
	}

	private void Initialize(int blockFactor)
	{
		this.blockFactor = blockFactor;
		recordSize = blockFactor * 512;
		recordBuffer = new byte[RecordSize];
		if (inputStream != null)
		{
			currentRecordIndex = -1;
			currentBlockIndex = BlockFactor;
		}
		else
		{
			currentRecordIndex = 0;
			currentBlockIndex = 0;
		}
	}

	public int GetBlockFactor()
	{
		return blockFactor;
	}

	public int GetRecordSize()
	{
		return recordSize;
	}

	public bool IsEOFBlock(byte[] block)
	{
		int i = 0;
		for (int num = 512; i < num; i++)
		{
			if (block[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public void SkipBlock()
	{
		if (inputStream == null)
		{
			throw new TarException("no input stream defined");
		}
		if (currentBlockIndex < BlockFactor || ReadRecord())
		{
			currentBlockIndex++;
		}
	}

	public byte[] ReadBlock()
	{
		if (inputStream == null)
		{
			throw new TarException("TarBuffer.ReadBlock - no input stream defined");
		}
		if (currentBlockIndex >= BlockFactor && !ReadRecord())
		{
			return null;
		}
		byte[] array = new byte[512];
		Array.Copy(recordBuffer, currentBlockIndex * 512, array, 0, 512);
		currentBlockIndex++;
		return array;
	}

	private bool ReadRecord()
	{
		if (inputStream == null)
		{
			throw new TarException("no input stream stream defined");
		}
		currentBlockIndex = 0;
		int num = 0;
		int num2 = RecordSize;
		while (num2 > 0)
		{
			long num3 = inputStream.Read(recordBuffer, num, num2);
			if (num3 <= 0)
			{
				break;
			}
			num += (int)num3;
			num2 -= (int)num3;
		}
		currentRecordIndex++;
		return true;
	}

	public int GetCurrentBlockNum()
	{
		return currentBlockIndex;
	}

	public int GetCurrentRecordNum()
	{
		return currentRecordIndex;
	}

	public void WriteBlock(byte[] block)
	{
		if (outputStream == null)
		{
			throw new TarException("TarBuffer.WriteBlock - no output stream defined");
		}
		if (block.Length != 512)
		{
			throw new TarException("TarBuffer.WriteBlock - block to write has length '" + block.Length + "' which is not the block size of '" + 512 + "'");
		}
		if (currentBlockIndex >= BlockFactor)
		{
			WriteRecord();
		}
		Array.Copy(block, 0, recordBuffer, currentBlockIndex * 512, 512);
		currentBlockIndex++;
	}

	public void WriteBlock(byte[] buf, int offset)
	{
		if (outputStream == null)
		{
			throw new TarException("TarBuffer.WriteBlock - no output stream stream defined");
		}
		if (offset + 512 > buf.Length)
		{
			throw new TarException("TarBuffer.WriteBlock - record has length '" + buf.Length + "' with offset '" + offset + "' which is less than the record size of '" + recordSize + "'");
		}
		if (currentBlockIndex >= BlockFactor)
		{
			WriteRecord();
		}
		Array.Copy(buf, offset, recordBuffer, currentBlockIndex * 512, 512);
		currentBlockIndex++;
	}

	private void WriteRecord()
	{
		if (outputStream == null)
		{
			throw new TarException("TarBuffer.WriteRecord no output stream defined");
		}
		outputStream.Write(recordBuffer, 0, RecordSize);
		outputStream.Flush();
		currentBlockIndex = 0;
		currentRecordIndex++;
	}

	private void Flush()
	{
		if (outputStream == null)
		{
			throw new TarException("TarBuffer.Flush no output stream defined");
		}
		if (currentBlockIndex > 0)
		{
			WriteRecord();
		}
		outputStream.Flush();
	}

	public void Close()
	{
		if (outputStream != null)
		{
			Flush();
			outputStream.Close();
			outputStream = null;
		}
		else if (inputStream != null)
		{
			inputStream.Close();
			inputStream = null;
		}
	}
}
