using System;
using System.Collections.Generic;
using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.RTP;

namespace LumiSoft.Net.Media;

public class AudioIn_RTP : IDisposable
{
	private bool m_IsDisposed;

	private bool m_IsRunning;

	private AudioInDevice m_pAudioInDevice;

	private int m_AudioFrameSize = 20;

	private Dictionary<int, AudioCodec> m_pAudioCodecs;

	private RTP_SendStream m_pRTP_Stream;

	private AudioCodec m_pActiveCodec;

	private _WaveIn m_pWaveIn;

	private uint m_RtpTimeStamp;

	public bool IsDisposed => m_IsDisposed;

	public bool IsRunning
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_IsRunning;
		}
	}

	public AudioInDevice AudioInDevice
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAudioInDevice;
		}
		set
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				throw new ArgumentNullException("AudioInDevice");
			}
			m_pAudioInDevice = value;
			if (IsRunning)
			{
				Stop();
				Start();
			}
		}
	}

	public RTP_SendStream RTP_Stream
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRTP_Stream;
		}
	}

	public AudioCodec AudioCodec
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pActiveCodec;
		}
	}

	public Dictionary<int, AudioCodec> AudioCodecs
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAudioCodecs;
		}
		set
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				throw new ArgumentNullException("AudioCodecs");
			}
			m_pAudioCodecs = value;
		}
	}

	public event EventHandler<ExceptionEventArgs> Error;

	public AudioIn_RTP(AudioInDevice audioInDevice, int audioFrameSize, Dictionary<int, AudioCodec> codecs, RTP_SendStream stream)
	{
		if (audioInDevice == null)
		{
			throw new ArgumentNullException("audioInDevice");
		}
		if (codecs == null)
		{
			throw new ArgumentNullException("codecs");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pAudioInDevice = audioInDevice;
		m_AudioFrameSize = audioFrameSize;
		m_pAudioCodecs = codecs;
		m_pRTP_Stream = stream;
		m_pRTP_Stream.Session.PayloadChanged += m_pRTP_Stream_PayloadChanged;
		m_pAudioCodecs.TryGetValue(m_pRTP_Stream.Session.Payload, out m_pActiveCodec);
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			Stop();
			m_IsDisposed = true;
			this.Error = null;
			m_pAudioInDevice = null;
			m_pAudioCodecs = null;
			m_pRTP_Stream.Session.PayloadChanged -= m_pRTP_Stream_PayloadChanged;
			m_pRTP_Stream = null;
			m_pActiveCodec = null;
		}
	}

	private void m_pRTP_Stream_PayloadChanged(object sender, EventArgs e)
	{
		if (m_IsRunning)
		{
			Stop();
			m_pActiveCodec = null;
			m_pAudioCodecs.TryGetValue(m_pRTP_Stream.Session.Payload, out m_pActiveCodec);
			Start();
		}
	}

	private void m_pWaveIn_AudioFrameReceived(object sender, EventArgs<byte[]> e)
	{
		try
		{
			if (m_RtpTimeStamp == 0 || m_RtpTimeStamp > m_pRTP_Stream.Session.RtpClock.RtpTimestamp)
			{
				m_RtpTimeStamp = m_pRTP_Stream.Session.RtpClock.RtpTimestamp;
			}
			else
			{
				m_RtpTimeStamp += (uint)m_pRTP_Stream.Session.RtpClock.MillisecondsToRtpTicks(m_AudioFrameSize);
			}
			if (m_pActiveCodec != null)
			{
				RTP_Packet rTP_Packet = new RTP_Packet();
				rTP_Packet.Data = m_pActiveCodec.Encode(e.Value, 0, e.Value.Length);
				rTP_Packet.Timestamp = m_RtpTimeStamp;
				m_pRTP_Stream.Send(rTP_Packet);
			}
		}
		catch (Exception x)
		{
			if (!IsDisposed)
			{
				OnError(x);
			}
		}
	}

	public void Start()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!m_IsRunning)
		{
			m_IsRunning = true;
			if (m_pActiveCodec != null)
			{
				int bufferSize = m_pActiveCodec.AudioFormat.SamplesPerSecond / (1000 / m_AudioFrameSize) * (m_pActiveCodec.AudioFormat.BitsPerSample / 8);
				m_pWaveIn = new _WaveIn(m_pAudioInDevice, m_pActiveCodec.AudioFormat.SamplesPerSecond, m_pActiveCodec.AudioFormat.BitsPerSample, 1, bufferSize);
				m_pWaveIn.AudioFrameReceived += m_pWaveIn_AudioFrameReceived;
				m_pWaveIn.Start();
			}
		}
	}

	public void Stop()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_IsRunning)
		{
			if (m_pWaveIn != null)
			{
				m_pWaveIn.Dispose();
				m_pWaveIn = null;
			}
			m_IsRunning = false;
		}
	}

	private void OnError(Exception x)
	{
		if (this.Error != null)
		{
			this.Error(this, new ExceptionEventArgs(x));
		}
	}
}
