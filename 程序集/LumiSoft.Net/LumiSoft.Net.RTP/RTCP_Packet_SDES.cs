using System;
using System.Collections.Generic;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_SDES : RTCP_Packet
{
	private int m_Version = 2;

	private List<RTCP_Packet_SDES_Chunk> m_pChunks;

	public override int Version => m_Version;

	public override int Type => 202;

	public List<RTCP_Packet_SDES_Chunk> Chunks => m_pChunks;

	public override int Size
	{
		get
		{
			int num = 4;
			foreach (RTCP_Packet_SDES_Chunk pChunk in m_pChunks)
			{
				num += pChunk.Size;
			}
			return num;
		}
	}

	internal RTCP_Packet_SDES()
	{
		m_pChunks = new List<RTCP_Packet_SDES_Chunk>();
	}

	protected override void ParseInternal(byte[] buffer, ref int offset)
	{
		m_Version = buffer[offset] >> 6;
		bool num = Convert.ToBoolean((buffer[offset] >> 5) & 1);
		int num2 = buffer[offset++] & 0x1F;
		_ = buffer[offset++];
		int num3 = (buffer[offset++] << 8) | buffer[offset++];
		if (num)
		{
			base.PaddBytesCount = buffer[offset + num3];
		}
		for (int i = 0; i < num2; i++)
		{
			RTCP_Packet_SDES_Chunk rTCP_Packet_SDES_Chunk = new RTCP_Packet_SDES_Chunk();
			rTCP_Packet_SDES_Chunk.Parse(buffer, ref offset);
			m_pChunks.Add(rTCP_Packet_SDES_Chunk);
		}
	}

	public override void ToByte(byte[] buffer, ref int offset)
	{
		buffer[offset++] = (byte)(0x80u | ((uint)m_pChunks.Count & 0x1Fu));
		buffer[offset++] = 202;
		int num = offset;
		buffer[offset++] = 0;
		buffer[offset++] = 0;
		int num2 = offset;
		foreach (RTCP_Packet_SDES_Chunk pChunk in m_pChunks)
		{
			pChunk.ToByte(buffer, ref offset);
		}
		int num3 = (offset - num2) / 4;
		buffer[num] = (byte)((uint)(num3 >> 8) & 0xFFu);
		buffer[num + 1] = (byte)((uint)num3 & 0xFFu);
	}
}
