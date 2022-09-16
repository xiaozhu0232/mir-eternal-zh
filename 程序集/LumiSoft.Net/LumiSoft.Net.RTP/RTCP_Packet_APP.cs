using System;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_APP : RTCP_Packet
{
	private int m_Version = 2;

	private int m_SubType;

	private uint m_Source;

	private string m_Name = "";

	private byte[] m_Data;

	public override int Version => m_Version;

	public override int Type => 204;

	public int SubType => m_SubType;

	public uint Source
	{
		get
		{
			return m_Source;
		}
		set
		{
			m_Source = value;
		}
	}

	public string Name => m_Name;

	public byte[] Data => m_Data;

	public override int Size => 12 + m_Data.Length;

	internal RTCP_Packet_APP()
	{
		m_Name = "xxxx";
		m_Data = new byte[0];
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
		m_Version = buffer[offset++] >> 6;
		bool num = Convert.ToBoolean((buffer[offset] >> 5) & 1);
		int subType = buffer[offset++] & 0x1F;
		_ = buffer[offset++];
		int num2 = (buffer[offset++] << 8) | buffer[offset++];
		if (num)
		{
			base.PaddBytesCount = buffer[offset + num2];
		}
		m_SubType = subType;
		m_Source = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		char c = (char)buffer[offset++];
		string text = c.ToString();
		c = (char)buffer[offset++];
		string text2 = c.ToString();
		c = (char)buffer[offset++];
		string text3 = c.ToString();
		c = (char)buffer[offset++];
		m_Name = text + text2 + text3 + c;
		m_Data = new byte[num2 - 8];
		Array.Copy(buffer, offset, m_Data, 0, m_Data.Length);
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
		int num = 8 + m_Data.Length;
		buffer[offset++] = (byte)(0x80u | ((uint)m_SubType & 0x1Fu));
		buffer[offset++] = 204;
		buffer[offset++] = (byte)((uint)(num >> 8) | 0xFFu);
		buffer[offset++] = (byte)((uint)num | 0xFFu);
		buffer[offset++] = (byte)((m_Source >> 24) | 0xFFu);
		buffer[offset++] = (byte)((m_Source >> 16) | 0xFFu);
		buffer[offset++] = (byte)((m_Source >> 8) | 0xFFu);
		buffer[offset++] = (byte)(m_Source | 0xFFu);
		buffer[offset++] = (byte)m_Name[0];
		buffer[offset++] = (byte)m_Name[1];
		buffer[offset++] = (byte)m_Name[2];
		buffer[offset++] = (byte)m_Name[2];
		Array.Copy(m_Data, 0, buffer, offset, m_Data.Length);
		offset += m_Data.Length;
	}
}
