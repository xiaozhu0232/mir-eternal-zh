using System;
using System.Collections.Generic;

namespace LumiSoft.Net.RTP;

public class RTCP_CompoundPacket
{
	private List<RTCP_Packet> m_pPackets;

	public List<RTCP_Packet> Packets => m_pPackets;

	internal int TotalSize
	{
		get
		{
			int num = 0;
			foreach (RTCP_Packet pPacket in m_pPackets)
			{
				num += pPacket.Size;
			}
			return num;
		}
	}

	internal RTCP_CompoundPacket()
	{
		m_pPackets = new List<RTCP_Packet>();
	}

	public static RTCP_CompoundPacket Parse(byte[] buffer, int count)
	{
		int offset = 0;
		RTCP_CompoundPacket rTCP_CompoundPacket = new RTCP_CompoundPacket();
		while (offset < count)
		{
			RTCP_Packet rTCP_Packet = RTCP_Packet.Parse(buffer, ref offset, noException: true);
			if (rTCP_Packet != null)
			{
				rTCP_CompoundPacket.m_pPackets.Add(rTCP_Packet);
			}
		}
		return rTCP_CompoundPacket;
	}

	public byte[] ToByte()
	{
		byte[] array = new byte[TotalSize];
		int offset = 0;
		ToByte(array, ref offset);
		return array;
	}

	public void ToByte(byte[] buffer, ref int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' value must be >= 0.");
		}
		foreach (RTCP_Packet pPacket in m_pPackets)
		{
			pPacket.ToByte(buffer, ref offset);
		}
	}

	public void Validate()
	{
		if (m_pPackets.Count == 0)
		{
			throw new ArgumentException("No RTCP packets.");
		}
		for (int i = 0; i < m_pPackets.Count; i++)
		{
			RTCP_Packet rTCP_Packet = m_pPackets[i];
			if (rTCP_Packet.Version != 2)
			{
				throw new ArgumentException("RTP version field must equal 2.");
			}
			if (i < m_pPackets.Count - 1 && rTCP_Packet.IsPadded)
			{
				throw new ArgumentException("Only the last packet in RTCP compound packet may be padded.");
			}
		}
		if (m_pPackets[0].Type != 200 || m_pPackets[0].Type != 201)
		{
			throw new ArgumentException("The first RTCP packet in a compound packet must be equal to SR or RR.");
		}
	}
}
