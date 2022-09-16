using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_ServerStatus : IMAP_r_u
{
	private string m_ResponseCode = "";

	private IMAP_t_orc m_pOptionalResponse;

	private string m_ResponseText = "";

	public string ResponseCode => m_ResponseCode;

	public IMAP_t_orc OptionalResponse => m_pOptionalResponse;

	public string ResponseText => m_ResponseText;

	public bool IsError => !m_ResponseCode.Equals("OK", StringComparison.InvariantCultureIgnoreCase);

	[Obsolete("Use property OptionalResponse instead.")]
	public string OptionalResponseCode
	{
		get
		{
			if (m_pOptionalResponse == null)
			{
				return null;
			}
			return m_pOptionalResponse.ToString().Split(' ')[0];
		}
	}

	[Obsolete("Use property OptionalResponse instead.")]
	public string OptionalResponseArgs
	{
		get
		{
			if (m_pOptionalResponse == null)
			{
				return null;
			}
			string[] array = m_pOptionalResponse.ToString().Split(new char[1] { ' ' }, 2);
			if (array.Length != 2)
			{
				return "";
			}
			return array[1];
		}
	}

	public IMAP_r_u_ServerStatus(string responseCode, string responseText)
		: this(responseCode, null, responseText)
	{
	}

	public IMAP_r_u_ServerStatus(string responseCode, IMAP_t_orc optionalResponse, string responseText)
	{
		if (responseCode == null)
		{
			throw new ArgumentNullException("responseCode");
		}
		if (responseCode == string.Empty)
		{
			throw new ArgumentException("The argument 'responseCode' value must be specified.", "responseCode");
		}
		m_ResponseCode = responseCode;
		m_pOptionalResponse = optionalResponse;
		m_ResponseText = responseText;
	}

	public static IMAP_r_u_ServerStatus Parse(string responseLine)
	{
		if (responseLine == null)
		{
			throw new ArgumentNullException("responseLine");
		}
		string[] array = responseLine.Split(new char[1] { ' ' }, 3);
		_ = array[0];
		string responseCode = array[1];
		IMAP_t_orc optionalResponse = null;
		string responseText = array[2];
		if (array[2].StartsWith("["))
		{
			StringReader stringReader = new StringReader(array[2]);
			optionalResponse = IMAP_t_orc.Parse(stringReader);
			responseText = stringReader.ReadToEnd();
		}
		return new IMAP_r_u_ServerStatus(responseCode, optionalResponse, responseText);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* " + m_ResponseCode + " ");
		if (m_pOptionalResponse != null)
		{
			stringBuilder.Append("[" + m_pOptionalResponse.ToString() + "] ");
		}
		stringBuilder.Append(m_ResponseText + "\r\n");
		return stringBuilder.ToString();
	}
}
