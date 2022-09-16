using System;
using System.Text;

namespace LumiSoft.Net.MIME;

public class MIME_h_ContentType : MIME_h
{
	private bool m_IsModified;

	private string m_ParseValue;

	private string m_Type = "";

	private string m_SubType = "";

	private MIME_h_ParameterCollection m_pParameters;

	public override bool IsModified
	{
		get
		{
			if (!m_IsModified)
			{
				return m_pParameters.IsModified;
			}
			return true;
		}
	}

	public override string Name => "Content-Type";

	public string Type => m_Type;

	public string SubType => m_SubType;

	[Obsolete("Mispelled 'TypeWithSubype', use TypeWithSubtype instead !")]
	public string TypeWithSubype => m_Type + "/" + m_SubType;

	public string TypeWithSubtype => m_Type + "/" + m_SubType;

	public MIME_h_ParameterCollection Parameters => m_pParameters;

	public string Param_Name
	{
		get
		{
			return m_pParameters["name"];
		}
		set
		{
			m_pParameters["name"] = value;
		}
	}

	public string Param_Charset
	{
		get
		{
			return m_pParameters["charset"];
		}
		set
		{
			m_pParameters["charset"] = value;
		}
	}

	public string Param_Boundary
	{
		get
		{
			return m_pParameters["boundary"];
		}
		set
		{
			m_pParameters["boundary"] = value;
		}
	}

	public MIME_h_ContentType(string mediaType)
	{
		if (mediaType == null)
		{
			throw new ArgumentNullException(mediaType);
		}
		string[] array = mediaType.Split(new char[1] { '/' }, 2);
		if (array.Length == 2)
		{
			if (array[0] == "" || !MIME_Reader.IsToken(array[0]))
			{
				throw new ArgumentException("Invalid argument 'mediaType' value '" + mediaType + "', value must be token.");
			}
			if (array[1] == "" || !MIME_Reader.IsToken(array[1]))
			{
				throw new ArgumentException("Invalid argument 'mediaType' value '" + mediaType + "', value must be token.");
			}
			m_Type = array[0];
			m_SubType = array[1];
			m_pParameters = new MIME_h_ParameterCollection(this);
			m_IsModified = true;
			return;
		}
		throw new ArgumentException("Invalid argument 'mediaType' value '" + mediaType + "'.");
	}

	private MIME_h_ContentType()
	{
		m_pParameters = new MIME_h_ParameterCollection(this);
	}

	public static MIME_h_ContentType Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string text = MIME_Encoding_EncodedWord.DecodeS(value);
		MIME_h_ContentType mIME_h_ContentType = new MIME_h_ContentType();
		string[] array = text.Split(new char[1] { ':' }, 2);
		if (array.Length != 2)
		{
			throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
		}
		MIME_Reader mIME_Reader = new MIME_Reader(array[1]);
		string text2 = mIME_Reader.Token();
		if (text2 == null)
		{
			throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
		}
		mIME_h_ContentType.m_Type = text2;
		if (mIME_Reader.Char(readToFirstChar: false) != 47)
		{
			throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
		}
		string text3 = mIME_Reader.Token();
		if (text3 == null)
		{
			throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
		}
		mIME_h_ContentType.m_SubType = text3;
		if (mIME_Reader.Available > 0)
		{
			mIME_h_ContentType.m_pParameters.Parse(mIME_Reader);
		}
		mIME_h_ContentType.m_ParseValue = value;
		mIME_h_ContentType.m_IsModified = false;
		return mIME_h_ContentType;
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		if (!reEncode && !IsModified)
		{
			return m_ParseValue;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Content-Type: " + m_Type + "/" + m_SubType);
		stringBuilder.Append(m_pParameters.ToString(parmetersCharset));
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
