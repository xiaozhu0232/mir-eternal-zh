using System;
using System.Text;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_BYE : RTCP_Packet
{
	private int m_Version = 2;

	private uint[] m_Sources;

	private string m_LeavingReason = "";

	public override int Version => m_Version;

	public override int Type => 203;

	public uint[] Sources
	{
		get
		{
			return m_Sources;
		}
		set
		{
			if (value.Length > 31)
			{
				throw new ArgumentException("Property 'Sources' can accomodate only 31 entries.");
			}
			m_Sources = value;
		}
	}

	public string LeavingReason
	{
		get
		{
			return m_LeavingReason;
		}
		set
		{
			m_LeavingReason = value;
		}
	}

	public override int Size
	{
		get
		{
			int num = 4;
			if (m_Sources != null)
			{
				num += 4 * m_Sources.Length;
			}
			if (!string.IsNullOrEmpty(m_LeavingReason))
			{
				num++;
				num += Encoding.UTF8.GetByteCount(m_LeavingReason);
			}
			return num;
		}
	}

	internal RTCP_Packet_BYE()
	{
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
		int num4 = num3 * 4;
		if (num)
		{
			base.PaddBytesCount = buffer[offset + num3];
		}
		m_Sources = new uint[num2];
		for (int i = 0; i < num2; i++)
		{
			m_Sources[i] = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		}
		if (num4 > offset)
		{
			int num5 = buffer[offset++];
			m_LeavingReason = Encoding.UTF8.GetString(buffer, offset, num5);
			offset += num5;
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
		byte[] array = new byte[0];
		if (!string.IsNullOrEmpty(m_LeavingReason))
		{
			array = Encoding.UTF8.GetBytes(m_LeavingReason);
		}
		int num = 0;
		num += m_Sources.Length;
		if (!string.IsNullOrEmpty(m_LeavingReason))
		{
			num += (int)Math.Ceiling((decimal)((4 + array.Length) / 4));
		}
		buffer[offset++] = (byte)(0x80u | ((uint)m_Sources.Length & 0x1Fu));
		buffer[offset++] = 203;
		buffer[offset++] = (byte)((uint)(num >> 8) & 0xFFu);
		buffer[offset++] = (byte)((uint)num & 0xFFu);
		uint[] sources = m_Sources;
		for (int i = 0; i < sources.Length; i++)
		{
			int num2 = (int)sources[i];
			buffer[offset++] = (byte)((num2 & 0xFF000000u) >> 24);
			buffer[offset++] = (byte)((num2 & 0xFF0000) >> 16);
			buffer[offset++] = (byte)((num2 & 0xFF00) >> 8);
			buffer[offset++] = (byte)((uint)num2 & 0xFFu);
		}
		if (!string.IsNullOrEmpty(m_LeavingReason))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(m_LeavingReason);
			buffer[offset++] = (byte)bytes.Length;
			Array.Copy(bytes, 0, buffer, offset, bytes.Length);
			offset += bytes.Length;
		}
	}
}
