using System;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_ReportBlock
{
	private uint m_SSRC;

	private uint m_FractionLost;

	private uint m_CumulativePacketsLost;

	private uint m_ExtHighestSeqNumber;

	private uint m_Jitter;

	private uint m_LastSR;

	private uint m_DelaySinceLastSR;

	public uint SSRC => m_SSRC;

	public uint FractionLost
	{
		get
		{
			return m_FractionLost;
		}
		set
		{
			m_FractionLost = value;
		}
	}

	public uint CumulativePacketsLost
	{
		get
		{
			return m_CumulativePacketsLost;
		}
		set
		{
			m_CumulativePacketsLost = value;
		}
	}

	public uint ExtendedHighestSeqNo
	{
		get
		{
			return m_ExtHighestSeqNumber;
		}
		set
		{
			m_ExtHighestSeqNumber = value;
		}
	}

	public uint Jitter
	{
		get
		{
			return m_Jitter;
		}
		set
		{
			m_Jitter = value;
		}
	}

	public uint LastSR
	{
		get
		{
			return m_LastSR;
		}
		set
		{
			m_LastSR = value;
		}
	}

	public uint DelaySinceLastSR
	{
		get
		{
			return m_DelaySinceLastSR;
		}
		set
		{
			m_DelaySinceLastSR = value;
		}
	}

	internal RTCP_Packet_ReportBlock(uint ssrc)
	{
		m_SSRC = ssrc;
	}

	internal RTCP_Packet_ReportBlock()
	{
	}

	public void Parse(byte[] buffer, int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' value must be >= 0.");
		}
		m_SSRC = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_FractionLost = buffer[offset++];
		m_CumulativePacketsLost = (uint)((buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_ExtHighestSeqNumber = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_Jitter = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_LastSR = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_DelaySinceLastSR = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
	}

	public void ToByte(byte[] buffer, ref int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' must be >= 0.");
		}
		if (offset + 24 > buffer.Length)
		{
			throw new ArgumentException("Argument 'buffer' has not enough room to store report block.");
		}
		buffer[offset++] = (byte)((m_SSRC >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_SSRC & 0xFFu);
		buffer[offset++] = (byte)m_FractionLost;
		buffer[offset++] = (byte)((m_CumulativePacketsLost >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_CumulativePacketsLost >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_CumulativePacketsLost & 0xFFu);
		buffer[offset++] = (byte)((m_ExtHighestSeqNumber >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_ExtHighestSeqNumber >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_ExtHighestSeqNumber >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_ExtHighestSeqNumber & 0xFFu);
		buffer[offset++] = (byte)((m_Jitter >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_Jitter >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_Jitter >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_Jitter & 0xFFu);
		buffer[offset++] = (byte)((m_LastSR >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_LastSR >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_LastSR >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_LastSR & 0xFFu);
		buffer[offset++] = (byte)((m_DelaySinceLastSR >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_DelaySinceLastSR >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_DelaySinceLastSR >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_DelaySinceLastSR & 0xFFu);
	}
}
