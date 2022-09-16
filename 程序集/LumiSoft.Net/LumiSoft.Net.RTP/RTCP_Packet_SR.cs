using System;
using System.Collections.Generic;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_SR : RTCP_Packet
{
	private int m_Version = 2;

	private uint m_SSRC;

	private ulong m_NtpTimestamp;

	private uint m_RtpTimestamp;

	private uint m_SenderPacketCount;

	private uint m_SenderOctetCount;

	private List<RTCP_Packet_ReportBlock> m_pReportBlocks;

	public override int Version => m_Version;

	public override int Type => 200;

	public uint SSRC => m_SSRC;

	public ulong NtpTimestamp
	{
		get
		{
			return m_NtpTimestamp;
		}
		set
		{
			m_NtpTimestamp = value;
		}
	}

	public uint RtpTimestamp
	{
		get
		{
			return m_RtpTimestamp;
		}
		set
		{
			m_RtpTimestamp = value;
		}
	}

	public uint SenderPacketCount
	{
		get
		{
			return m_SenderPacketCount;
		}
		set
		{
			m_SenderPacketCount = value;
		}
	}

	public uint SenderOctetCount
	{
		get
		{
			return m_SenderOctetCount;
		}
		set
		{
			m_SenderOctetCount = value;
		}
	}

	public List<RTCP_Packet_ReportBlock> ReportBlocks => m_pReportBlocks;

	public override int Size => 28 + 24 * m_pReportBlocks.Count;

	internal RTCP_Packet_SR(uint ssrc)
	{
		m_SSRC = ssrc;
		m_pReportBlocks = new List<RTCP_Packet_ReportBlock>();
	}

	internal RTCP_Packet_SR()
	{
		m_pReportBlocks = new List<RTCP_Packet_ReportBlock>();
	}

	protected override void ParseInternal(byte[] buffer, ref int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' value must be >= 0.");
		}
		m_Version = buffer[offset] >> 6;
		bool num = Convert.ToBoolean((buffer[offset] >> 5) & 1);
		int num2 = buffer[offset++] & 0x1F;
		_ = buffer[offset++];
		int num3 = (buffer[offset++] << 8) | buffer[offset++];
		if (num)
		{
			base.PaddBytesCount = buffer[offset + num3];
		}
		m_SSRC = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_NtpTimestamp = (ulong)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++] | (buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_RtpTimestamp = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_SenderPacketCount = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		m_SenderOctetCount = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		for (int i = 0; i < num2; i++)
		{
			RTCP_Packet_ReportBlock rTCP_Packet_ReportBlock = new RTCP_Packet_ReportBlock();
			rTCP_Packet_ReportBlock.Parse(buffer, offset);
			m_pReportBlocks.Add(rTCP_Packet_ReportBlock);
			offset += 24;
		}
	}

	public override void ToByte(byte[] buffer, ref int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' value must be >= 0.");
		}
		int num = (24 + m_pReportBlocks.Count * 24) / 4;
		buffer[offset++] = (byte)(0x80u | ((uint)m_pReportBlocks.Count & 0x1Fu));
		buffer[offset++] = 200;
		buffer[offset++] = (byte)((uint)(num >> 8) & 0xFFu);
		buffer[offset++] = (byte)((uint)num & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_SSRC & 0xFFu);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 56) & 0xFF);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 48) & 0xFF);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 40) & 0xFF);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 32) & 0xFF);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 24) & 0xFF);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 16) & 0xFF);
		buffer[offset++] = (byte)((m_NtpTimestamp >> 8) & 0xFF);
		buffer[offset++] = (byte)(m_NtpTimestamp & 0xFF);
		buffer[offset++] = (byte)((m_RtpTimestamp >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_RtpTimestamp >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_RtpTimestamp >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_RtpTimestamp & 0xFFu);
		buffer[offset++] = (byte)((m_SenderPacketCount >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_SenderPacketCount >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_SenderPacketCount >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_SenderPacketCount & 0xFFu);
		buffer[offset++] = (byte)((m_SenderOctetCount >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_SenderOctetCount >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_SenderOctetCount >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_SenderOctetCount & 0xFFu);
		foreach (RTCP_Packet_ReportBlock pReportBlock in m_pReportBlocks)
		{
			pReportBlock.ToByte(buffer, ref offset);
		}
	}
}
