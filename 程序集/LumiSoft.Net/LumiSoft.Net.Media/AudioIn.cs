using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.Media;

public class AudioIn : Stream
{
	private class WaveIn
	{
		private delegate void waveInProc(IntPtr hdrvr, int uMsg, int dwUser, int dwParam1, int dwParam2);

		private class BufferItem
		{
			private GCHandle m_HeaderHandle;

			private GCHandle m_DataHandle;

			private int m_DataSize;

			public GCHandle HeaderHandle => m_HeaderHandle;

			public WAVEHDR Header => (WAVEHDR)m_HeaderHandle.Target;

			public GCHandle DataHandle => m_DataHandle;

			public byte[] Data => (byte[])m_DataHandle.Target;

			public int DataSize => m_DataSize;

			public BufferItem(ref GCHandle headerHandle, ref GCHandle dataHandle, int dataSize)
			{
				m_HeaderHandle = headerHandle;
				m_DataHandle = dataHandle;
				m_DataSize = dataSize;
			}

			public void Dispose()
			{
				m_HeaderHandle.Free();
				m_DataHandle.Free();
			}
		}

		private class MMSYSERR
		{
			public const int NOERROR = 0;

			public const int ERROR = 1;

			public const int BADDEVICEID = 2;

			public const int NOTENABLED = 3;

			public const int ALLOCATED = 4;

			public const int INVALHANDLE = 5;

			public const int NODRIVER = 6;

			public const int NOMEM = 7;

			public const int NOTSUPPORTED = 8;

			public const int BADERRNUM = 9;

			public const int INVALFLAG = 1;

			public const int INVALPARAM = 11;

			public const int HANDLEBUSY = 12;

			public const int INVALIDALIAS = 13;

			public const int BADDB = 14;

			public const int KEYNOTFOUND = 15;

			public const int READERROR = 16;

			public const int WRITEERROR = 17;

			public const int DELETEERROR = 18;

			public const int VALNOTFOUND = 19;

			public const int NODRIVERCB = 20;

			public const int LASTERROR = 20;
		}

		private class WavConstants
		{
			public const int MM_WOM_OPEN = 955;

			public const int MM_WOM_CLOSE = 956;

			public const int MM_WOM_DONE = 957;

			public const int MM_WIM_OPEN = 958;

			public const int MM_WIM_CLOSE = 959;

			public const int MM_WIM_DATA = 960;

			public const int CALLBACK_FUNCTION = 196608;

			public const int WAVERR_STILLPLAYING = 33;

			public const int WHDR_DONE = 1;

			public const int WHDR_PREPARED = 2;

			public const int WHDR_BEGINLOOP = 4;

			public const int WHDR_ENDLOOP = 8;

			public const int WHDR_INQUEUE = 16;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class WAVEFORMATEX
		{
			public ushort wFormatTag;

			public ushort nChannels;

			public uint nSamplesPerSec;

			public uint nAvgBytesPerSec;

			public ushort nBlockAlign;

			public ushort wBitsPerSample;

			public ushort cbSize;
		}

		private struct WAVEOUTCAPS
		{
			public ushort wMid;

			public ushort wPid;

			public uint vDriverVersion;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;

			public uint dwFormats;

			public ushort wChannels;

			public ushort wReserved1;

			public uint dwSupport;
		}

		private struct WAVEHDR
		{
			public IntPtr lpData;

			public uint dwBufferLength;

			public uint dwBytesRecorded;

			public IntPtr dwUser;

			public uint dwFlags;

			public uint dwLoops;

			public IntPtr lpNext;

			public uint reserved;
		}

		internal class WavFormat
		{
			public const int PCM = 1;
		}

		private bool m_IsDisposed;

		private AudioInDevice m_pInDevice;

		private int m_SamplesPerSec = 8000;

		private int m_BitsPerSample = 8;

		private int m_Channels = 1;

		private int m_BufferSize = 400;

		private IntPtr m_pWavDevHandle = IntPtr.Zero;

		private int m_BlockSize;

		private BufferItem m_pCurrentBuffer;

		private CircleCollection<BufferItem> m_pBuffers;

		private waveInProc m_pWaveInProc;

		private bool m_IsRecording;

		private FifoBuffer m_pReadBuffer;

		private object m_pLock = new object();

		public static AudioInDevice[] Devices
		{
			get
			{
				List<AudioInDevice> list = new List<AudioInDevice>();
				int num = waveInGetNumDevs();
				for (int i = 0; i < num; i++)
				{
					WAVEOUTCAPS pwoc = default(WAVEOUTCAPS);
					if (waveInGetDevCaps((uint)i, ref pwoc, Marshal.SizeOf((object)pwoc)) == 0)
					{
						list.Add(new AudioInDevice(i, pwoc.szPname, pwoc.wChannels));
					}
				}
				return list.ToArray();
			}
		}

		public bool IsDisposed => m_IsDisposed;

		public AudioInDevice InputDevice
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_pInDevice;
			}
		}

		public int SamplesPerSec
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_SamplesPerSec;
			}
		}

		public int BitsPerSample
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_BitsPerSample;
			}
		}

		public int Channels
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_Channels;
			}
		}

		public int BufferSize
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_BufferSize;
			}
		}

		public int BlockSize
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_BlockSize;
			}
		}

		public FifoBuffer ReadBuffer
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WavRecorder");
				}
				return m_pReadBuffer;
			}
		}

		[DllImport("winmm.dll")]
		private static extern int waveInAddBuffer(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

		[DllImport("winmm.dll")]
		private static extern int waveInClose(IntPtr hWaveOut);

		[DllImport("winmm.dll")]
		private static extern uint waveInGetDevCaps(uint hwo, ref WAVEOUTCAPS pwoc, int cbwoc);

		[DllImport("winmm.dll")]
		private static extern int waveInGetNumDevs();

		[DllImport("winmm.dll")]
		private static extern int waveInOpen(out IntPtr hWaveOut, int uDeviceID, WAVEFORMATEX lpFormat, waveInProc dwCallback, int dwInstance, int dwFlags);

		[DllImport("winmm.dll")]
		private static extern int waveInPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

		[DllImport("winmm.dll")]
		private static extern int waveInReset(IntPtr hWaveOut);

		[DllImport("winmm.dll")]
		private static extern int waveInStart(IntPtr hWaveOut);

		[DllImport("winmm.dll")]
		private static extern int waveInStop(IntPtr hWaveOut);

		[DllImport("winmm.dll")]
		private static extern int waveInUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

		public WaveIn(AudioInDevice device, int samplesPerSec, int bitsPerSample, int channels, int bufferSize)
		{
			if (device == null)
			{
				throw new ArgumentNullException("device");
			}
			if (samplesPerSec < 8000)
			{
				throw new ArgumentException("Argument 'samplesPerSec' value must be >= 8000.");
			}
			if (bitsPerSample < 8)
			{
				throw new ArgumentException("Argument 'bitsPerSample' value must be >= 8.");
			}
			if (channels < 1)
			{
				throw new ArgumentException("Argument 'channels' value must be >= 1.");
			}
			m_pInDevice = device;
			m_SamplesPerSec = samplesPerSec;
			m_BitsPerSample = bitsPerSample;
			m_Channels = channels;
			m_BufferSize = bufferSize;
			m_BlockSize = m_Channels * (m_BitsPerSample / 8);
			m_pBuffers = new CircleCollection<BufferItem>();
			m_pReadBuffer = new FifoBuffer(32000);
			WAVEFORMATEX lpFormat = new WAVEFORMATEX
			{
				wFormatTag = 1,
				nChannels = (ushort)m_Channels,
				nSamplesPerSec = (uint)samplesPerSec,
				nAvgBytesPerSec = (uint)(m_SamplesPerSec * m_Channels * (m_BitsPerSample / 8)),
				nBlockAlign = (ushort)m_BlockSize,
				wBitsPerSample = (ushort)m_BitsPerSample,
				cbSize = 0
			};
			m_pWaveInProc = OnWaveInProc;
			int num = waveInOpen(out m_pWavDevHandle, m_pInDevice.Index, lpFormat, m_pWaveInProc, 0, 196608);
			if (num != 0)
			{
				throw new Exception("Failed to open wav device, error: " + num + ".");
			}
			CreateBuffers();
		}

		~WaveIn()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (m_IsDisposed)
			{
				return;
			}
			m_IsDisposed = true;
			try
			{
				waveInReset(m_pWavDevHandle);
				BufferItem[] array = m_pBuffers.ToArray();
				foreach (BufferItem bufferItem in array)
				{
					waveInUnprepareHeader(m_pWavDevHandle, bufferItem.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)bufferItem.Header));
					bufferItem.Dispose();
				}
				waveInClose(m_pWavDevHandle);
				m_pInDevice = null;
				m_pWavDevHandle = IntPtr.Zero;
			}
			catch
			{
			}
		}

		public void Start()
		{
			if (!m_IsRecording)
			{
				m_IsRecording = true;
				int num = waveInStart(m_pWavDevHandle);
				if (num != 0)
				{
					throw new Exception("Failed to start wav device, error: " + num + ".");
				}
			}
		}

		public void Stop()
		{
			if (m_IsRecording)
			{
				m_IsRecording = false;
				int num = waveInStop(m_pWavDevHandle);
				if (num != 0)
				{
					throw new Exception("Failed to stop wav device, error: " + num + ".");
				}
			}
		}

		private void OnWaveInProc(IntPtr hdrvr, int uMsg, int dwUser, int dwParam1, int dwParam2)
		{
			if (m_IsDisposed)
			{
				return;
			}
			lock (m_pLock)
			{
				try
				{
					if (uMsg != 960)
					{
						return;
					}
					m_pReadBuffer.Write(m_pCurrentBuffer.Data, 0, m_pCurrentBuffer.Data.Length, ignoreBufferFull: true);
					BufferItem buffer = m_pCurrentBuffer;
					m_pCurrentBuffer = m_pBuffers.Next();
					ThreadPool.QueueUserWorkItem(delegate
					{
						try
						{
							if (!m_IsDisposed)
							{
								waveInUnprepareHeader(m_pWavDevHandle, buffer.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)buffer.Header));
								waveInPrepareHeader(m_pWavDevHandle, buffer.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)buffer.Header));
								waveInAddBuffer(m_pWavDevHandle, buffer.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)buffer.Header));
							}
						}
						catch
						{
						}
					});
				}
				catch
				{
				}
			}
		}

		private void ProcessActiveBuffer(object state)
		{
			try
			{
				lock (m_pBuffers)
				{
					waveInUnprepareHeader(m_pWavDevHandle, m_pCurrentBuffer.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_pCurrentBuffer.Header));
					waveInPrepareHeader(m_pWavDevHandle, m_pCurrentBuffer.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_pCurrentBuffer.Header));
					waveInAddBuffer(m_pWavDevHandle, m_pCurrentBuffer.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_pCurrentBuffer.Header));
				}
			}
			catch
			{
			}
		}

		private void CreateBuffers()
		{
			while (m_pBuffers.Count < 10)
			{
				byte[] array = new byte[m_BufferSize];
				GCHandle dataHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				WAVEHDR wAVEHDR = default(WAVEHDR);
				wAVEHDR.lpData = dataHandle.AddrOfPinnedObject();
				wAVEHDR.dwBufferLength = (uint)array.Length;
				wAVEHDR.dwBytesRecorded = 0u;
				wAVEHDR.dwUser = IntPtr.Zero;
				wAVEHDR.dwFlags = 0u;
				wAVEHDR.dwLoops = 0u;
				wAVEHDR.lpNext = IntPtr.Zero;
				wAVEHDR.reserved = 0u;
				GCHandle headerHandle = GCHandle.Alloc(wAVEHDR, GCHandleType.Pinned);
				int num = 0;
				num = waveInPrepareHeader(m_pWavDevHandle, headerHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)wAVEHDR));
				if (num != 0)
				{
					throw new Exception("Error preparing wave in buffer, error: " + num + ".");
				}
				m_pBuffers.Add(new BufferItem(ref headerHandle, ref dataHandle, m_BufferSize));
				num = waveInAddBuffer(m_pWavDevHandle, headerHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)wAVEHDR));
				if (num != 0)
				{
					throw new Exception("Error adding wave in buffer, error: " + num + ".");
				}
			}
			m_pCurrentBuffer = m_pBuffers[0];
		}
	}

	private bool m_IsDisposed;

	private AudioInDevice m_pDevice;

	private int m_SamplesPerSec = 8000;

	private int m_BitsPerSample = 16;

	private int m_Channels = 1;

	private WaveIn m_pWaveIn;

	public static AudioInDevice[] Devices => WaveIn.Devices;

	public override bool CanRead
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return true;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return false;
		}
	}

	public override long Length
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			throw new NotSupportedException();
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			throw new NotSupportedException();
		}
	}

	public int SamplesPerSec
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SamplesPerSec;
		}
	}

	public int BitsPerSample
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_BitsPerSample;
		}
	}

	public int Channels
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Channels;
		}
	}

	public int BlockSize
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Channels * (m_BitsPerSample / 8);
		}
	}

	public int Available
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pWaveIn.ReadBuffer.Available;
		}
	}

	public AudioIn(AudioInDevice device, int samplesPerSec, int bitsPerSample, int channels)
	{
		if (device == null)
		{
			throw new ArgumentNullException("device");
		}
		if (samplesPerSec < 1)
		{
			throw new ArgumentException("Argument 'samplesPerSec' value must be >= 1.", "samplesPerSec");
		}
		if (bitsPerSample < 8)
		{
			throw new ArgumentException("Argument 'bitsPerSample' value must be >= 8.", "bitsPerSample");
		}
		if (channels < 1)
		{
			throw new ArgumentException("Argument 'channels' value must be >= 1.", "channels");
		}
		m_pDevice = device;
		m_SamplesPerSec = samplesPerSec;
		m_BitsPerSample = bitsPerSample;
		m_Channels = channels;
		m_pWaveIn = new WaveIn(device, samplesPerSec, bitsPerSample, channels, 320);
		m_pWaveIn.Start();
	}

	public new void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pWaveIn.Dispose();
			m_pWaveIn = null;
		}
	}

	public override void Flush()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' value must be >= 0.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' is bigger than than argument 'buffer' can store.");
		}
		while (m_pWaveIn.ReadBuffer.Available == 0)
		{
			Thread.Sleep(1);
		}
		return m_pWaveIn.ReadBuffer.Read(buffer, offset, count - count % m_pWaveIn.BlockSize);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		throw new NotSupportedException();
	}
}
