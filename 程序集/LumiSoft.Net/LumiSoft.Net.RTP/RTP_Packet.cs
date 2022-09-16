using System;
using System.Text;

namespace LumiSoft.Net.RTP;

public class RTP_Packet
{
	private int m_Version = 2;

	private bool m_IsMarker;

	private int m_PayloadType;

	private ushort m_SequenceNumber;

	private uint m_Timestamp;

	private uint m_SSRC;

	private uint[] m_CSRC;

	private byte[] m_Data;

	public int Version => m_Version;

	public bool IsPadded => false;

	public bool IsMarker
	{
		get
		{
			return m_IsMarker;
		}
		set
		{
			m_IsMarker = value;
		}
	}

	public int PayloadType
	{
		get
		{
			return m_PayloadType;
		}
		set
		{
			if (value < 0 || value > 128)
			{
				throw new ArgumentException("Payload value must be >= 0 and <= 128.");
			}
			m_PayloadType = value;
		}
	}

	public ushort SeqNo
	{
		get
		{
			return m_SequenceNumber;
		}
		set
		{
			m_SequenceNumber = value;
		}
	}

	public uint Timestamp
	{
		get
		{
			return m_Timestamp;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Timestamp value must be >= 1.");
			}
			m_Timestamp = value;
		}
	}

	public uint SSRC
	{
		get
		{
			return m_SSRC;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("SSRC value must be >= 1.");
			}
			m_SSRC = value;
		}
	}

	public uint[] CSRC
	{
		get
		{
			return m_CSRC;
		}
		set
		{
			m_CSRC = value;
		}
	}

	public uint[] Sources
	{
		get
		{
			uint[] array = new uint[1];
			if (m_CSRC != null)
			{
				array = new uint[1 + m_CSRC.Length];
			}
			array[0] = m_SSRC;
			Array.Copy(m_CSRC, array, m_CSRC.Length);
			return array;
		}
	}

	public byte[] Data
	{
		get
		{
			return m_Data;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Data");
			}
			m_Data = value;
		}
	}

	public static RTP_Packet Parse(byte[] buffer, int size)
	{
		RTP_Packet rTP_Packet = new RTP_Packet();
		rTP_Packet.ParseInternal(buffer, size);
		return rTP_Packet;
	}

	public void Validate()
	{
	}

	public void ToByte(byte[] buffer, ref int offset)
	{
		int num = 0;
		if (m_CSRC != null)
		{
			num = m_CSRC.Length;
		}
		buffer[offset++] = (byte)((uint)(m_Version << 6) | 0u | ((uint)num & 0xFu));
		buffer[offset++] = (byte)((uint)(Convert.ToInt32(m_IsMarker) << 7) | ((uint)m_PayloadType & 0x7Fu));
		buffer[offset++] = (byte)(m_SequenceNumber >> 8);
		buffer[offset++] = (byte)(m_SequenceNumber & 0xFFu);
		buffer[offset++] = (byte)((m_Timestamp >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_Timestamp >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_Timestamp >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_Timestamp & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_SSRC >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_SSRC & 0xFFu);
		if (m_CSRC != null)
		{
			uint[] cSRC = m_CSRC;
			for (int i = 0; i < cSRC.Length; i++)
			{
				int num2 = (int)cSRC[i];
				buffer[offset++] = (byte)((uint)(num2 >> 24) & 0xFFu);
				buffer[offset++] = (byte)((uint)(num2 >> 16) & 0xFFu);
				buffer[offset++] = (byte)((uint)(num2 >> 8) & 0xFFu);
				buffer[offset++] = (byte)((uint)num2 & 0xFFu);
			}
		}
		Array.Copy(m_Data, 0, buffer, offset, m_Data.Length);
		offset += m_Data.Length;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("----- RTP Packet\r\n");
		stringBuilder.Append("Version: " + m_Version + "\r\n");
		stringBuilder.Append("IsMaker: " + m_IsMarker + "\r\n");
		stringBuilder.Append("PayloadType: " + m_PayloadType + "\r\n");
		stringBuilder.Append("SeqNo: " + m_SequenceNumber + "\r\n");
		stringBuilder.Append("Timestamp: " + m_Timestamp + "\r\n");
		stringBuilder.Append("SSRC: " + m_SSRC + "\r\n");
		stringBuilder.Append("Data: " + m_Data.Length + " bytes.\r\n");
		return stringBuilder.ToString();
	}

	private void ParseInternal(byte[] buffer, int size)
	{
		int num = 0;
		m_Version = buffer[num] >> 6;
		Convert.ToBoolean((buffer[num] >> 5) & 1);
		bool flag = Convert.ToBoolean((buffer[num] >> 4) & 1);
		int num2 = buffer[num++] & 0xF;
		m_IsMarker = Convert.ToBoolean(buffer[num] >> 7);
		m_PayloadType = buffer[num++] & 0x7F;
		m_SequenceNumber = (ushort)((buffer[num++] << 8) | buffer[num++]);
		m_Timestamp = (uint)((buffer[num++] << 24) | (buffer[num++] << 16) | (buffer[num++] << 8) | buffer[num++]);
		m_SSRC = (uint)((buffer[num++] << 24) | (buffer[num++] << 16) | (buffer[num++] << 8) | buffer[num++]);
		m_CSRC = new uint[num2];
		for (int i = 0; i < num2; i++)
		{
			m_CSRC[i] = (uint)((buffer[num++] << 24) | (buffer[num++] << 16) | (buffer[num++] << 8) | buffer[num++]);
		}
		if (flag)
		{
			num++;
			num += buffer[num];
		}
		m_Data = new byte[size - num];
		Array.Copy(buffer, num, m_Data, 0, m_Data.Length);
	}
}
