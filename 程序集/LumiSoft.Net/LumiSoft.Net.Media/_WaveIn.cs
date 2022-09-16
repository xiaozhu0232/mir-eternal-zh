using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LumiSoft.Net.Media;

internal class _WaveIn
{
	private delegate void waveInProc(IntPtr hdrvr, int uMsg, IntPtr dwUser, IntPtr dwParam1, IntPtr dwParam2);

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

		public const int WAVE_FORMAT_DIRECT = 8;

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

	[StructLayout(LayoutKind.Sequential)]
	private class WAVEHDR
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

	private class BufferItem
	{
		private IntPtr m_WavDevHandle = IntPtr.Zero;

		private WAVEHDR m_Header;

		private byte[] m_pBuffer;

		private GCHandle m_ThisHandle;

		private GCHandle m_HeaderHandle;

		private GCHandle m_DataHandle;

		private int m_DataSize;

		private EventArgs<byte[]> m_pEventArgs;

		public WAVEHDR Header => m_Header;

		public GCHandle HeaderHandle => m_HeaderHandle;

		public byte[] Data => m_pBuffer;

		public int DataSize => m_DataSize;

		public EventArgs<byte[]> EventArgs => m_pEventArgs;

		public BufferItem(IntPtr wavDevHandle, int dataSize)
		{
			m_WavDevHandle = wavDevHandle;
			m_ThisHandle = GCHandle.Alloc(this);
			m_pBuffer = new byte[dataSize];
			m_DataHandle = GCHandle.Alloc(m_pBuffer, GCHandleType.Pinned);
			m_Header = new WAVEHDR();
			m_Header.lpData = m_DataHandle.AddrOfPinnedObject();
			m_Header.dwBufferLength = (uint)dataSize;
			m_Header.dwBytesRecorded = 0u;
			m_Header.dwUser = (IntPtr)m_ThisHandle;
			m_Header.dwFlags = 0u;
			m_Header.dwLoops = 0u;
			m_Header.lpNext = IntPtr.Zero;
			m_Header.reserved = 0u;
			m_HeaderHandle = GCHandle.Alloc(m_Header, GCHandleType.Pinned);
			m_pEventArgs = new EventArgs<byte[]>(m_pBuffer);
		}

		public void Dispose()
		{
			waveInUnprepareHeader(m_WavDevHandle, m_HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_Header));
			m_ThisHandle.Free();
			m_HeaderHandle.Free();
			m_DataHandle.Free();
			m_pEventArgs = null;
		}

		internal void Queue(bool unprepare)
		{
			m_Header.dwFlags = 0u;
			int num = 0;
			if (unprepare)
			{
				num = waveInUnprepareHeader(m_WavDevHandle, m_HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_Header));
				if (num != 0)
				{
					throw new Exception("Error unpreparing wave in buffer, error: " + num + ".");
				}
			}
			num = waveInPrepareHeader(m_WavDevHandle, m_HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_Header));
			if (num != 0)
			{
				throw new Exception("Error preparing wave in buffer, error: " + num + ".");
			}
			num = waveInAddBuffer(m_WavDevHandle, m_HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)m_Header));
			if (num != 0)
			{
				throw new Exception("Error adding wave in buffer, error: " + num + ".");
			}
		}
	}

	private object m_pLock = new object();

	private bool m_IsDisposed;

	private AudioInDevice m_pInDevice;

	private int m_SamplesPerSec = 8000;

	private int m_BitsPerSample = 8;

	private int m_Channels = 1;

	private int m_BufferSize = 400;

	private IntPtr m_pWavDevHandle = IntPtr.Zero;

	private int m_BlockSize;

	private Dictionary<long, BufferItem> m_pBuffers;

	private waveInProc m_pWaveInProc;

	private bool m_IsRecording;

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

	public event EventHandler<EventArgs<byte[]>> AudioFrameReceived;

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

	public _WaveIn(AudioInDevice device, int samplesPerSec, int bitsPerSample, int channels, int bufferSize)
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
		m_pBuffers = new Dictionary<long, BufferItem>();
		WAVEFORMATEX lpFormat = new WAVEFORMATEX
		{
			wFormatTag = 1,
			nChannels = (ushort)m_Channels,
			nSamplesPerSec = (uint)samplesPerSec,
			nAvgBytesPerSec = (uint)(m_SamplesPerSec * (m_Channels * (m_BitsPerSample / 8))),
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

	~_WaveIn()
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
			lock (m_pLock)
			{
				waveInReset(m_pWavDevHandle);
				foreach (BufferItem value in m_pBuffers.Values)
				{
					value.Dispose();
				}
				waveInClose(m_pWavDevHandle);
				m_pInDevice = null;
				m_pWavDevHandle = IntPtr.Zero;
				this.AudioFrameReceived = null;
			}
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

	private void OnWaveInProc(IntPtr hdrvr, int uMsg, IntPtr dwUser, IntPtr dwParam1, IntPtr dwParam2)
	{
		if (m_IsDisposed)
		{
			return;
		}
		try
		{
			if (uMsg != 960)
			{
				return;
			}
			try
			{
				if (m_IsDisposed)
				{
					return;
				}
				lock (m_pLock)
				{
					BufferItem bufferItem = m_pBuffers[dwParam1.ToInt64()];
					OnAudioFrameReceived(bufferItem.EventArgs);
					bufferItem.Queue(unprepare: true);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
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
			BufferItem bufferItem = new BufferItem(m_pWavDevHandle, m_BufferSize);
			m_pBuffers.Add(bufferItem.HeaderHandle.AddrOfPinnedObject().ToInt64(), bufferItem);
			bufferItem.Queue(unprepare: false);
		}
	}

	private void OnAudioFrameReceived(EventArgs<byte[]> eArgs)
	{
		if (this.AudioFrameReceived != null)
		{
			this.AudioFrameReceived(this, eArgs);
		}
	}
}
