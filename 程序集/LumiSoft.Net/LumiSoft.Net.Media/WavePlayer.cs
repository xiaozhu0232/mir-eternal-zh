using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LumiSoft.Net.Media;

public class WavePlayer
{
	private class RIFF_Chunk
	{
		private uint m_ChunkSize;

		private string m_Format = "";

		public string ChunkID => "RIFF";

		public uint ChunkSize => m_ChunkSize;

		public string Format => m_Format;

		public void Parse(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			m_ChunkSize = reader.ReadUInt32();
			m_Format = new string(reader.ReadChars(4)).Trim();
		}
	}

	private class fmt_Chunk
	{
		private uint m_ChunkSize;

		private int m_AudioFormat;

		private int m_NumberOfChannels;

		private int m_SampleRate;

		private int m_AvgBytesPerSec;

		private int m_BlockAlign;

		private int m_BitsPerSample;

		public string ChunkID => "fmt";

		public uint ChunkSize => m_ChunkSize;

		public int AudioFormat => m_AudioFormat;

		public int NumberOfChannels => m_NumberOfChannels;

		public int SampleRate => m_SampleRate;

		public int AvgBytesPerSec => m_AvgBytesPerSec;

		public int BlockAlign => m_BlockAlign;

		public int BitsPerSample => m_BitsPerSample;

		public void Parse(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			m_ChunkSize = reader.ReadUInt32();
			m_AudioFormat = reader.ReadInt16();
			m_NumberOfChannels = reader.ReadInt16();
			m_SampleRate = reader.ReadInt32();
			m_AvgBytesPerSec = reader.ReadInt32();
			m_BlockAlign = reader.ReadInt16();
			m_BitsPerSample = reader.ReadInt16();
			for (int i = 0; i < m_ChunkSize - 16; i++)
			{
				reader.ReadByte();
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("ChunkSize: " + m_ChunkSize);
			stringBuilder.AppendLine("AudioFormat: " + m_AudioFormat);
			stringBuilder.AppendLine("Channels: " + m_NumberOfChannels);
			stringBuilder.AppendLine("SampleRate: " + m_SampleRate);
			stringBuilder.AppendLine("AvgBytesPerSec: " + m_AvgBytesPerSec);
			stringBuilder.AppendLine("BlockAlign: " + m_BlockAlign);
			stringBuilder.AppendLine("BitsPerSample: " + m_BitsPerSample);
			return stringBuilder.ToString();
		}
	}

	private class data_Chunk
	{
		private uint m_ChunkSize;

		public string ChunkID => "data";

		public uint ChunkSize => m_ChunkSize;

		public void Parse(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			m_ChunkSize = reader.ReadUInt32();
		}
	}

	private class WavReader
	{
		private BinaryReader m_pBinaryReader;

		public WavReader(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			m_pBinaryReader = reader;
		}

		public string Read_ChunkID()
		{
			char[] array = m_pBinaryReader.ReadChars(4);
			if (array.Length == 0)
			{
				return null;
			}
			return new string(array).Trim();
		}

		public RIFF_Chunk Read_RIFF()
		{
			RIFF_Chunk rIFF_Chunk = new RIFF_Chunk();
			rIFF_Chunk.Parse(m_pBinaryReader);
			return rIFF_Chunk;
		}

		public fmt_Chunk Read_fmt()
		{
			fmt_Chunk fmt_Chunk = new fmt_Chunk();
			fmt_Chunk.Parse(m_pBinaryReader);
			return fmt_Chunk;
		}

		public data_Chunk Read_data()
		{
			data_Chunk data_Chunk = new data_Chunk();
			data_Chunk.Parse(m_pBinaryReader);
			return data_Chunk;
		}

		public void SkipChunk()
		{
			uint num = m_pBinaryReader.ReadUInt32();
			m_pBinaryReader.BaseStream.Position += num;
		}
	}

	private bool m_IsPlaying;

	private bool m_Stop;

	private AudioOutDevice m_pOutputDevice;

	public WavePlayer(AudioOutDevice device)
	{
		if (device == null)
		{
			throw new ArgumentNullException("device");
		}
		m_pOutputDevice = device;
	}

	public void Play(string file, int count)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		Play(File.OpenRead(file), count);
	}

	public void Play(Stream stream, int count)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (m_IsPlaying)
		{
			Stop();
		}
		m_IsPlaying = true;
		m_Stop = false;
		ThreadPool.QueueUserWorkItem(delegate
		{
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				WavReader wavReader = new WavReader(binaryReader);
				if (!string.Equals(wavReader.Read_ChunkID(), "riff", StringComparison.InvariantCultureIgnoreCase))
				{
					throw new ArgumentNullException("Invalid wave file, RIFF header missing.");
				}
				wavReader.Read_RIFF();
				wavReader.Read_ChunkID();
				fmt_Chunk fmt_Chunk = wavReader.Read_fmt();
				using AudioOut audioOut = new AudioOut(m_pOutputDevice, fmt_Chunk.SampleRate, fmt_Chunk.BitsPerSample, fmt_Chunk.NumberOfChannels);
				long position = binaryReader.BaseStream.Position;
				for (int i = 0; i < count; i++)
				{
					binaryReader.BaseStream.Position = position;
					while (true)
					{
						string text = wavReader.Read_ChunkID();
						if (text == null || binaryReader.BaseStream.Length - binaryReader.BaseStream.Position < 4)
						{
							break;
						}
						if (string.Equals(text, "data", StringComparison.InvariantCultureIgnoreCase))
						{
							data_Chunk data_Chunk = wavReader.Read_data();
							int j = 0;
							byte[] array = new byte[8000];
							int num;
							for (; j < data_Chunk.ChunkSize; j += num)
							{
								if (m_Stop)
								{
									m_IsPlaying = false;
									return;
								}
								num = binaryReader.Read(array, 0, (int)Math.Min(array.Length, data_Chunk.ChunkSize - j));
								audioOut.Write(array, 0, num);
								while (m_IsPlaying && audioOut.BytesBuffered >= array.Length * 2)
								{
									Thread.Sleep(10);
								}
							}
						}
						else
						{
							wavReader.SkipChunk();
						}
					}
				}
				while (m_IsPlaying && audioOut.BytesBuffered > 0)
				{
					Thread.Sleep(10);
				}
			}
			m_IsPlaying = false;
		});
	}

	public void Stop()
	{
		m_Stop = true;
		while (m_IsPlaying)
		{
			Thread.Sleep(5);
		}
	}
}
