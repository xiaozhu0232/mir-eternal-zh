using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_ServerStatus : IMAP_r
{
	private string m_CommandTag = "";

	private string m_ResponseCode = "";

	private IMAP_t_orc m_pOptionalResponse;

	private string m_ResponseText = "";

	public string CommandTag => m_CommandTag;

	public string ResponseCode => m_ResponseCode;

	public IMAP_t_orc OptionalResponse => m_pOptionalResponse;

	public string ResponseText => m_ResponseText;

	public bool IsError
	{
		get
		{
			if (m_ResponseCode.Equals("NO", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			if (m_ResponseCode.Equals("BAD", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}
	}

	public bool IsContinue => m_ResponseCode.Equals("+", StringComparison.InvariantCultureIgnoreCase);

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

	public IMAP_r_ServerStatus(string commandTag, string responseCode, string responseText)
		: this(commandTag, responseCode, null, responseText)
	{
	}

	public IMAP_r_ServerStatus(string commandTag, string responseCode, IMAP_t_orc optionalResponse, string responseText)
	{
		if (commandTag == null)
		{
			throw new ArgumentNullException("commandTag");
		}
		if (commandTag == string.Empty)
		{
			throw new ArgumentException("The argument 'commandTag' value must be specified.", "commandTag");
		}
		if (responseCode == null)
		{
			throw new ArgumentNullException("responseCode");
		}
		if (responseCode == string.Empty)
		{
			throw new ArgumentException("The argument 'responseCode' value must be specified.", "responseCode");
		}
		m_CommandTag = commandTag;
		m_ResponseCode = responseCode;
		m_pOptionalResponse = optionalResponse;
		m_ResponseText = responseText;
	}

	internal IMAP_r_ServerStatus(string responseCode, string responseText)
	{
		m_ResponseCode = responseCode;
		m_ResponseText = responseText;
	}

	public static IMAP_r_ServerStatus Parse(string responseLine)
	{
		if (responseLine == null)
		{
			throw new ArgumentNullException("responseLine");
		}
		if (responseLine.StartsWith("+"))
		{
			string[] array = responseLine.Split(new char[1] { ' ' }, 2);
			string responseText = ((array.Length == 2) ? array[1] : null);
			return new IMAP_r_ServerStatus("+", "+", responseText);
		}
		string[] array2 = responseLine.Split(new char[1] { ' ' }, 3);
		string commandTag = array2[0];
		string responseCode = array2[1];
		IMAP_t_orc optionalResponse = null;
		string responseText2 = array2[2];
		if (array2[2].StartsWith("["))
		{
			StringReader stringReader = new StringReader(array2[2]);
			optionalResponse = IMAP_t_orc.Parse(stringReader);
			responseText2 = stringReader.ReadToEnd();
		}
		return new IMAP_r_ServerStatus(commandTag, responseCode, optionalResponse, responseText2);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(m_CommandTag))
		{
			stringBuilder.Append(m_CommandTag + " ");
		}
		stringBuilder.Append(m_ResponseCode + " ");
		if (m_pOptionalResponse != null)
		{
			stringBuilder.Append("[" + m_pOptionalResponse.ToString() + "] ");
		}
		stringBuilder.Append(m_ResponseText + "\r\n");
		return stringBuilder.ToString();
	}
}
