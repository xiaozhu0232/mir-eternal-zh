using System;

namespace LumiSoft.Net.RTP;

public abstract class RTCP_Packet
{
	private int m_PaddBytesCount;

	public abstract int Version { get; }

	public bool IsPadded
	{
		get
		{
			if (m_PaddBytesCount > 0)
			{
				return true;
			}
			return false;
		}
	}

	public abstract int Type { get; }

	public int PaddBytesCount
	{
		get
		{
			return m_PaddBytesCount;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Property 'PaddBytesCount' value must be >= 0.");
			}
			m_PaddBytesCount = value;
		}
	}

	public abstract int Size { get; }

	public RTCP_Packet()
	{
	}

	public static RTCP_Packet Parse(byte[] buffer, ref int offset)
	{
		return Parse(buffer, ref offset, noException: false);
	}

	public static RTCP_Packet Parse(byte[] buffer, ref int offset, bool noException)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' value must be >= 0.");
		}
		int num = buffer[offset + 1];
		switch (num)
		{
		case 200:
		{
			RTCP_Packet_SR rTCP_Packet_SR = new RTCP_Packet_SR();
			rTCP_Packet_SR.ParseInternal(buffer, ref offset);
			return rTCP_Packet_SR;
		}
		case 201:
		{
			RTCP_Packet_RR rTCP_Packet_RR = new RTCP_Packet_RR();
			rTCP_Packet_RR.ParseInternal(buffer, ref offset);
			return rTCP_Packet_RR;
		}
		case 202:
		{
			RTCP_Packet_SDES rTCP_Packet_SDES = new RTCP_Packet_SDES();
			rTCP_Packet_SDES.ParseInternal(buffer, ref offset);
			return rTCP_Packet_SDES;
		}
		case 203:
		{
			RTCP_Packet_BYE rTCP_Packet_BYE = new RTCP_Packet_BYE();
			rTCP_Packet_BYE.ParseInternal(buffer, ref offset);
			return rTCP_Packet_BYE;
		}
		case 204:
		{
			RTCP_Packet_APP rTCP_Packet_APP = new RTCP_Packet_APP();
			rTCP_Packet_APP.ParseInternal(buffer, ref offset);
			return rTCP_Packet_APP;
		}
		default:
		{
			offset += 2;
			int num2 = (buffer[offset++] << 8) | buffer[offset++];
			offset += num2;
			if (noException)
			{
				return null;
			}
			throw new ArgumentException("Unknown RTCP packet type '" + num + "'.");
		}
		}
	}

	public abstract void ToByte(byte[] buffer, ref int offset);

	protected abstract void ParseInternal(byte[] buffer, ref int offset);
}
