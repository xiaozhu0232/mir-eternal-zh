using System;

namespace LumiSoft.Net.POP3.Server;

public class POP3_e_GetTopOfMessage : EventArgs
{
	private POP3_ServerMessage m_pMessage;

	private int m_LineCount;

	private byte[] m_pData;

	public POP3_ServerMessage Message => m_pMessage;

	public int LineCount => m_LineCount;

	public byte[] Data
	{
		get
		{
			return m_pData;
		}
		set
		{
			m_pData = value;
		}
	}

	internal POP3_e_GetTopOfMessage(POP3_ServerMessage message, int lines)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		if (lines < 0)
		{
			throw new ArgumentException("Argument 'lines' value must be >= 0.", "lines");
		}
		m_pMessage = message;
		m_LineCount = lines;
	}
}
