using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_RR : RTCP_Packet
{
	private int m_Version = 2;

	private uint m_SSRC;

	private List<RTCP_Packet_ReportBlock> m_pReportBlocks;

	public override int Version => m_Version;

	public override int Type => 201;

	public uint SSRC
	{
		get
		{
			return m_SSRC;
		}
		set
		{
			m_SSRC = value;
		}
	}

	public List<RTCP_Packet_ReportBlock> ReportBlocks => m_pReportBlocks;

	public override int Size => 8 + 24 * m_pReportBlocks.Count;

	internal RTCP_Packet_RR()
	{
		m_pReportBlocks = new List<RTCP_Packet_ReportBlock>();
	}

	internal RTCP_Packet_RR(uint ssrc)
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
		int num = (4 + m_pReportBlocks.Count * 24) / 4;
		buffer[offset++] = (byte)(0x80u | ((uint)m_pReportBlocks.Count & 0x1Fu));
		buffer[offset++] = 201;
		buffer[offset++] = (byte)((uint)(num >> 8) & 0xFFu);
		buffer[offset++] = (byte)((uint)num & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_SSRC & 0xFFu);
		foreach (RTCP_Packet_ReportBlock pReportBlock in m_pReportBlocks)
		{
			pReportBlock.ToByte(buffer, ref offset);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Type: RR");
		stringBuilder.AppendLine("Version: " + m_Version);
		stringBuilder.AppendLine("SSRC: " + m_SSRC);
		stringBuilder.AppendLine("Report blocks: " + m_pReportBlocks.Count);
		return stringBuilder.ToString();
	}
}
