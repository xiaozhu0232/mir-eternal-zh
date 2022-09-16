using System;
using System.Text;

namespace LumiSoft.Net.RTP;

public class RTCP_Packet_SDES_Chunk
{
	private uint m_Source;

	private string m_CName;

	private string m_Name;

	private string m_Email;

	private string m_Phone;

	private string m_Location;

	private string m_Tool;

	private string m_Note;

	public uint Source => m_Source;

	public string CName => m_CName;

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (Encoding.UTF8.GetByteCount(value) > 255)
			{
				throw new ArgumentException("Property 'Name' value must be <= 255 bytes.");
			}
			m_Name = value;
		}
	}

	public string Email
	{
		get
		{
			return m_Email;
		}
		set
		{
			if (Encoding.UTF8.GetByteCount(value) > 255)
			{
				throw new ArgumentException("Property 'Email' value must be <= 255 bytes.");
			}
			m_Email = value;
		}
	}

	public string Phone
	{
		get
		{
			return m_Phone;
		}
		set
		{
			if (Encoding.UTF8.GetByteCount(value) > 255)
			{
				throw new ArgumentException("Property 'Phone' value must be <= 255 bytes.");
			}
			m_Phone = value;
		}
	}

	public string Location
	{
		get
		{
			return m_Location;
		}
		set
		{
			if (Encoding.UTF8.GetByteCount(value) > 255)
			{
				throw new ArgumentException("Property 'Location' value must be <= 255 bytes.");
			}
			m_Location = value;
		}
	}

	public string Tool
	{
		get
		{
			return m_Tool;
		}
		set
		{
			if (Encoding.UTF8.GetByteCount(value) > 255)
			{
				throw new ArgumentException("Property 'Tool' value must be <= 255 bytes.");
			}
			m_Tool = value;
		}
	}

	public string Note
	{
		get
		{
			return m_Note;
		}
		set
		{
			if (Encoding.UTF8.GetByteCount(value) > 255)
			{
				throw new ArgumentException("Property 'Note' value must be <= 255 bytes.");
			}
			m_Note = value;
		}
	}

	public int Size
	{
		get
		{
			int num = 4;
			if (!string.IsNullOrEmpty(m_CName))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_CName);
			}
			if (!string.IsNullOrEmpty(m_Name))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_Name);
			}
			if (!string.IsNullOrEmpty(m_Email))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_Email);
			}
			if (!string.IsNullOrEmpty(m_Phone))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_Phone);
			}
			if (!string.IsNullOrEmpty(m_Location))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_Location);
			}
			if (!string.IsNullOrEmpty(m_Tool))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_Tool);
			}
			if (!string.IsNullOrEmpty(m_Note))
			{
				num += 2;
				num += Encoding.UTF8.GetByteCount(m_Note);
			}
			for (num++; num % 4 > 0; num++)
			{
			}
			return num;
		}
	}

	public RTCP_Packet_SDES_Chunk(uint source, string cname)
	{
		if (source == 0)
		{
			throw new ArgumentException("Argument 'source' value must be > 0.");
		}
		if (string.IsNullOrEmpty(cname))
		{
			throw new ArgumentException("Argument 'cname' value may not be null or empty.");
		}
		m_Source = source;
		m_CName = cname;
	}

	internal RTCP_Packet_SDES_Chunk()
	{
	}

	public void Parse(byte[] buffer, ref int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Argument 'offset' value must be >= 0.");
		}
		int num = offset;
		m_Source = (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
		while (offset < buffer.Length && buffer[offset] != 0)
		{
			int num2 = buffer[offset++];
			int num3 = buffer[offset++];
			switch (num2)
			{
			case 1:
				m_CName = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			case 2:
				m_Name = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			case 3:
				m_Email = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			case 4:
				m_Phone = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			case 5:
				m_Location = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			case 6:
				m_Tool = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			case 7:
				m_Note = Encoding.UTF8.GetString(buffer, offset, num3);
				break;
			default:
				_ = 8;
				break;
			}
			offset += num3;
		}
		offset++;
		offset += (offset - num) % 4;
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
		int num = offset;
		buffer[offset++] = (byte)((m_Source >> 24) & 0xFFu);
		buffer[offset++] = (byte)((m_Source >> 16) & 0xFFu);
		buffer[offset++] = (byte)((m_Source >> 8) & 0xFFu);
		buffer[offset++] = (byte)(m_Source & 0xFFu);
		if (!string.IsNullOrEmpty(m_CName))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(m_CName);
			buffer[offset++] = 1;
			buffer[offset++] = (byte)bytes.Length;
			Array.Copy(bytes, 0, buffer, offset, bytes.Length);
			offset += bytes.Length;
		}
		if (!string.IsNullOrEmpty(m_Name))
		{
			byte[] bytes2 = Encoding.UTF8.GetBytes(m_Name);
			buffer[offset++] = 2;
			buffer[offset++] = (byte)bytes2.Length;
			Array.Copy(bytes2, 0, buffer, offset, bytes2.Length);
			offset += bytes2.Length;
		}
		if (!string.IsNullOrEmpty(m_Email))
		{
			byte[] bytes3 = Encoding.UTF8.GetBytes(m_Email);
			buffer[offset++] = 3;
			buffer[offset++] = (byte)bytes3.Length;
			Array.Copy(bytes3, 0, buffer, offset, bytes3.Length);
			offset += bytes3.Length;
		}
		if (!string.IsNullOrEmpty(m_Phone))
		{
			byte[] bytes4 = Encoding.UTF8.GetBytes(m_Phone);
			buffer[offset++] = 4;
			buffer[offset++] = (byte)bytes4.Length;
			Array.Copy(bytes4, 0, buffer, offset, bytes4.Length);
			offset += bytes4.Length;
		}
		if (!string.IsNullOrEmpty(m_Location))
		{
			byte[] bytes5 = Encoding.UTF8.GetBytes(m_Location);
			buffer[offset++] = 5;
			buffer[offset++] = (byte)bytes5.Length;
			Array.Copy(bytes5, 0, buffer, offset, bytes5.Length);
			offset += bytes5.Length;
		}
		if (!string.IsNullOrEmpty(m_Tool))
		{
			byte[] bytes6 = Encoding.UTF8.GetBytes(m_Tool);
			buffer[offset++] = 6;
			buffer[offset++] = (byte)bytes6.Length;
			Array.Copy(bytes6, 0, buffer, offset, bytes6.Length);
			offset += bytes6.Length;
		}
		if (!string.IsNullOrEmpty(m_Note))
		{
			byte[] bytes7 = Encoding.UTF8.GetBytes(m_Note);
			buffer[offset++] = 7;
			buffer[offset++] = (byte)bytes7.Length;
			Array.Copy(bytes7, 0, buffer, offset, bytes7.Length);
			offset += bytes7.Length;
		}
		buffer[offset++] = 0;
		while ((offset - num) % 4 > 0)
		{
			buffer[offset++] = 0;
		}
	}
}
