using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace LumiSoft.Net.Media;

public class AudioOut : IDisposable
{
	private class WaveOut
	{
		private delegate void waveOutProc(IntPtr hdrvr, int uMsg, int dwUser, int dwParam1, int dwParam2);

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

		private class WavMethods
		{
			[DllImport("winmm.dll")]
			public static extern int waveOutClose(IntPtr hWaveOut);

			[DllImport("winmm.dll")]
			public static extern uint waveOutGetDevCaps(uint hwo, ref WAVEOUTCAPS pwoc, int cbwoc);

			[DllImport("winmm.dll")]
			public static extern int waveOutGetNumDevs();

			[DllImport("winmm.dll")]
			public static extern int waveOutGetPosition(IntPtr hWaveOut, out int lpInfo, int uSize);

			[DllImport("winmm.dll")]
			public static extern int waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);

			[DllImport("winmm.dll")]
			public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, WAVEFORMATEX lpFormat, waveOutProc dwCallback, int dwInstance, int dwFlags);

			[DllImport("winmm.dll")]
			public static extern int waveOutPause(IntPtr hWaveOut);

			[DllImport("winmm.dll")]
			public static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

			[DllImport("winmm.dll")]
			public static extern int waveOutReset(IntPtr hWaveOut);

			[DllImport("winmm.dll")]
			public static extern int waveOutRestart(IntPtr hWaveOut);

			[DllImport("winmm.dll")]
			public static extern int waveOutSetVolume(IntPtr hWaveOut, int dwVolume);

			[DllImport("winmm.dll")]
			public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

			[DllImport("winmm.dll")]
			public static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);
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

		private class PlayItem
		{
			private GCHandle m_HeaderHandle;

			private GCHandle m_DataHandle;

			private int m_DataSize;

			public GCHandle HeaderHandle => m_HeaderHandle;

			public WAVEHDR Header => (WAVEHDR)m_HeaderHandle.Target;

			public GCHandle DataHandle => m_DataHandle;

			public int DataSize => m_DataSize;

			public PlayItem(ref GCHandle headerHandle, ref GCHandle dataHandle, int dataSize)
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

		private AudioOutDevice m_pOutDevice;

		private int m_SamplesPerSec = 8000;

		private int m_BitsPerSample = 16;

		private int m_Channels = 1;

		private int m_MinBuffer = 1200;

		private IntPtr m_pWavDevHandle = IntPtr.Zero;

		private int m_BlockSize;

		private int m_BytesBuffered;

		private bool m_IsPaused;

		private List<PlayItem> m_pPlayItems;

		private waveOutProc m_pWaveOutProc;

		private bool m_IsDisposed;

		public static AudioOutDevice[] Devices
		{
			get
			{
				List<AudioOutDevice> list = new List<AudioOutDevice>();
				int num = WavMethods.waveOutGetNumDevs();
				for (int i = 0; i < num; i++)
				{
					WAVEOUTCAPS pwoc = default(WAVEOUTCAPS);
					if (WavMethods.waveOutGetDevCaps((uint)i, ref pwoc, Marshal.SizeOf((object)pwoc)) == 0)
					{
						list.Add(new AudioOutDevice(i, pwoc.szPname, pwoc.wChannels));
					}
				}
				return list.ToArray();
			}
		}

		public bool IsDisposed => m_IsDisposed;

		public bool IsPlaying
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException("WaveOut");
				}
				if (m_pPlayItems.Count > 0)
				{
					return true;
				}
				return false;
			}
		}

		public int Volume
		{
			get
			{
				int dwVolume = 0;
				WavMethods.waveOutGetVolume(m_pWavDevHandle, out dwVolume);
				return (int)((double)(int)(ushort)(dwVolume & 0xFFFF) / 655.35);
			}
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentException("Property 'Volume' value must be >=0 and <= 100.");
				}
				int num = (int)((double)value * 655.35);
				WavMethods.waveOutSetVolume(m_pWavDevHandle, (num << 16) | (num & 0xFFFF));
			}
		}

		public int BytesBuffered => m_BytesBuffered;

		public WaveOut(AudioOutDevice outputDevice, int samplesPerSec, int bitsPerSample, int channels)
		{
			if (outputDevice == null)
			{
				throw new ArgumentNullException("outputDevice");
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
			m_pOutDevice = outputDevice;
			m_SamplesPerSec = samplesPerSec;
			m_BitsPerSample = bitsPerSample;
			m_Channels = channels;
			m_BlockSize = m_Channels * (m_BitsPerSample / 8);
			m_pPlayItems = new List<PlayItem>();
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
			m_pWaveOutProc = OnWaveOutProc;
			int num = WavMethods.waveOutOpen(out m_pWavDevHandle, m_pOutDevice.Index, lpFormat, m_pWaveOutProc, 0, 196608);
			if (num != 0)
			{
				throw new Exception("Failed to open wav device, error: " + num + ".");
			}
		}

		~WaveOut()
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
				WavMethods.waveOutReset(m_pWavDevHandle);
				WavMethods.waveOutClose(m_pWavDevHandle);
				foreach (PlayItem pPlayItem in m_pPlayItems)
				{
					WavMethods.waveOutUnprepareHeader(m_pWavDevHandle, pPlayItem.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)pPlayItem.Header));
					pPlayItem.Dispose();
				}
				WavMethods.waveOutClose(m_pWavDevHandle);
				m_pOutDevice = null;
				m_pWavDevHandle = IntPtr.Zero;
				m_pPlayItems = null;
				m_pWaveOutProc = null;
			}
			catch
			{
			}
		}

		private void OnWaveOutProc(IntPtr hdrvr, int uMsg, int dwUser, int dwParam1, int dwParam2)
		{
			try
			{
				if (uMsg == 957)
				{
					ThreadPool.QueueUserWorkItem(OnCleanUpFirstBlock);
				}
			}
			catch
			{
			}
		}

		private void OnCleanUpFirstBlock(object state)
		{
			if (m_IsDisposed)
			{
				return;
			}
			try
			{
				lock (m_pPlayItems)
				{
					PlayItem playItem = m_pPlayItems[0];
					WavMethods.waveOutUnprepareHeader(m_pWavDevHandle, playItem.HeaderHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)playItem.Header));
					m_pPlayItems.Remove(playItem);
					m_BytesBuffered -= playItem.DataSize;
					playItem.Dispose();
				}
			}
			catch
			{
			}
		}

		public void Play(byte[] audioData, int offset, int count)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("WaveOut");
			}
			if (audioData == null)
			{
				throw new ArgumentNullException("audioData");
			}
			if (count % m_BlockSize != 0)
			{
				throw new ArgumentException("Audio data is not n * BlockSize.");
			}
			byte[] array = new byte[count];
			Array.Copy(audioData, offset, array, 0, count);
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
			if (WavMethods.waveOutPrepareHeader(m_pWavDevHandle, headerHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)wAVEHDR)) == 0)
			{
				PlayItem item = new PlayItem(ref headerHandle, ref dataHandle, array.Length);
				m_pPlayItems.Add(item);
				m_BytesBuffered += array.Length;
				if (m_BytesBuffered < 1000)
				{
					if (!m_IsPaused)
					{
						WavMethods.waveOutPause(m_pWavDevHandle);
						m_IsPaused = true;
					}
				}
				else if (m_IsPaused && m_BytesBuffered > m_MinBuffer)
				{
					WavMethods.waveOutRestart(m_pWavDevHandle);
					m_IsPaused = false;
				}
				WavMethods.waveOutWrite(m_pWavDevHandle, headerHandle.AddrOfPinnedObject(), Marshal.SizeOf((object)wAVEHDR));
			}
			else
			{
				dataHandle.Free();
				headerHandle.Free();
			}
		}
	}

	private bool m_IsDisposed;

	private AudioOutDevice m_pDevice;

	private AudioFormat m_pAudioFormat;

	private WaveOut m_pWaveOut;

	public static AudioOutDevice[] Devices => WaveOut.Devices;

	public AudioOutDevice OutputDevice
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pDevice;
		}
	}

	public AudioFormat AudioFormat
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAudioFormat;
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
			return m_pAudioFormat.SamplesPerSecond;
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
			return m_pAudioFormat.BitsPerSample;
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
			return m_pAudioFormat.Channels;
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
			return m_pAudioFormat.Channels * (m_pAudioFormat.BitsPerSample / 8);
		}
	}

	public int Volume
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pWaveOut.Volume;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 0 || value > 100)
			{
				throw new ArgumentException("Property 'Volume' value must be >=0 and <= 100.");
			}
			m_pWaveOut.Volume = value;
		}
	}

	public int BytesBuffered
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pWaveOut.BytesBuffered;
		}
	}

	public AudioOut(AudioOutDevice device, AudioFormat format)
	{
		if (device == null)
		{
			throw new ArgumentNullException("device");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		m_pDevice = device;
		m_pAudioFormat = format;
		m_pWaveOut = new WaveOut(device, format.SamplesPerSecond, format.BitsPerSample, format.Channels);
	}

	public AudioOut(AudioOutDevice device, int samplesPerSec, int bitsPerSample, int channels)
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
		m_pAudioFormat = new AudioFormat(samplesPerSec, bitsPerSample, channels);
		m_pWaveOut = new WaveOut(device, samplesPerSec, bitsPerSample, channels);
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pWaveOut.Dispose();
			m_pWaveOut = null;
		}
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || count > buffer.Length + offset)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count % BlockSize != 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' is not n * BlockSize.");
		}
		m_pWaveOut.Play(buffer, offset, count);
	}
}
