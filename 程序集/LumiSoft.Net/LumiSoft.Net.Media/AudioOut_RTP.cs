using System;
using System.Collections.Generic;
using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.RTP;

namespace LumiSoft.Net.Media;

public class AudioOut_RTP : IDisposable
{
	private bool m_IsDisposed;

	private bool m_IsRunning;

	private AudioOutDevice m_pAudioOutDevice;

	private RTP_ReceiveStream m_pRTP_Stream;

	private Dictionary<int, AudioCodec> m_pAudioCodecs;

	private AudioOut m_pAudioOut;

	private AudioCodec m_pActiveCodec;

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

	public AudioOutDevice AudioOutDevice
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAudioOutDevice;
		}
		set
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				throw new ArgumentNullException("AudioOutDevice");
			}
			m_pAudioOutDevice = value;
			if (IsRunning)
			{
				Stop();
				Start();
			}
		}
	}

	public Dictionary<int, AudioCodec> Codecs
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAudioCodecs;
		}
	}

	public AudioCodec ActiveCodec
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

	public event EventHandler<ExceptionEventArgs> Error;

	public AudioOut_RTP(AudioOutDevice audioOutDevice, RTP_ReceiveStream stream, Dictionary<int, AudioCodec> codecs)
	{
		if (audioOutDevice == null)
		{
			throw new ArgumentNullException("audioOutDevice");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (codecs == null)
		{
			throw new ArgumentNullException("codecs");
		}
		m_pAudioOutDevice = audioOutDevice;
		m_pRTP_Stream = stream;
		m_pAudioCodecs = codecs;
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			Stop();
			this.Error = null;
			m_pAudioOutDevice = null;
			m_pRTP_Stream = null;
			m_pAudioCodecs = null;
			m_pActiveCodec = null;
		}
	}

	private void m_pRTP_Stream_PacketReceived(object sender, RTP_PacketEventArgs e)
	{
		if (m_IsDisposed)
		{
			return;
		}
		try
		{
			AudioCodec value = null;
			if (m_pAudioCodecs.TryGetValue(e.Packet.PayloadType, out value))
			{
				m_pActiveCodec = value;
				if (m_pAudioOut == null)
				{
					m_pAudioOut = new AudioOut(m_pAudioOutDevice, value.AudioFormat);
				}
				else if (!m_pAudioOut.AudioFormat.Equals(value.AudioFormat))
				{
					m_pAudioOut.Dispose();
					m_pAudioOut = new AudioOut(m_pAudioOutDevice, value.AudioFormat);
				}
				byte[] array = value.Decode(e.Packet.Data, 0, e.Packet.Data.Length);
				m_pAudioOut.Write(array, 0, array.Length);
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
			m_pRTP_Stream.PacketReceived += m_pRTP_Stream_PacketReceived;
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
			m_IsRunning = false;
			m_pRTP_Stream.PacketReceived -= m_pRTP_Stream_PacketReceived;
			if (m_pAudioOut != null)
			{
				m_pAudioOut.Dispose();
				m_pAudioOut = null;
			}
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
