using System;
using System.Text;

namespace LumiSoft.Net.MIME;

public class MIME_h_ContentDisposition : MIME_h
{
	private bool m_IsModified;

	private string m_ParseValue;

	private string m_DispositionType = "";

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

	public override string Name => "Content-Disposition";

	public string DispositionType => m_DispositionType;

	public MIME_h_ParameterCollection Parameters => m_pParameters;

	public string Param_FileName
	{
		get
		{
			return Parameters["filename"];
		}
		set
		{
			m_pParameters["filename"] = value;
		}
	}

	public DateTime Param_CreationDate
	{
		get
		{
			string text = Parameters["creation-date"];
			if (text == null)
			{
				return DateTime.MinValue;
			}
			return MIME_Utils.ParseRfc2822DateTime(text);
		}
		set
		{
			if (value == DateTime.MinValue)
			{
				Parameters.Remove("creation-date");
			}
			else
			{
				Parameters["creation-date"] = MIME_Utils.DateTimeToRfc2822(value);
			}
		}
	}

	public DateTime Param_ModificationDate
	{
		get
		{
			string text = Parameters["modification-date"];
			if (text == null)
			{
				return DateTime.MinValue;
			}
			return MIME_Utils.ParseRfc2822DateTime(text);
		}
		set
		{
			if (value == DateTime.MinValue)
			{
				Parameters.Remove("modification-date");
			}
			else
			{
				Parameters["modification-date"] = MIME_Utils.DateTimeToRfc2822(value);
			}
		}
	}

	public DateTime Param_ReadDate
	{
		get
		{
			string text = Parameters["read-date"];
			if (text == null)
			{
				return DateTime.MinValue;
			}
			return MIME_Utils.ParseRfc2822DateTime(text);
		}
		set
		{
			if (value == DateTime.MinValue)
			{
				Parameters.Remove("read-date");
			}
			else
			{
				Parameters["read-date"] = MIME_Utils.DateTimeToRfc2822(value);
			}
		}
	}

	public long Param_Size
	{
		get
		{
			string text = Parameters["size"];
			if (text == null)
			{
				return -1L;
			}
			return Convert.ToInt64(text);
		}
		set
		{
			if (value < 0)
			{
				Parameters.Remove("size");
			}
			else
			{
				Parameters["size"] = value.ToString();
			}
		}
	}

	public MIME_h_ContentDisposition(string dispositionType)
	{
		if (dispositionType == null)
		{
			throw new ArgumentNullException("dispositionType");
		}
		if (dispositionType == string.Empty)
		{
			throw new ArgumentException("Argument 'dispositionType' value must be specified.");
		}
		m_DispositionType = dispositionType;
		m_pParameters = new MIME_h_ParameterCollection(this);
		m_IsModified = true;
	}

	private MIME_h_ContentDisposition()
	{
		m_pParameters = new MIME_h_ParameterCollection(this);
	}

	public static MIME_h_ContentDisposition Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string text = MIME_Encoding_EncodedWord.DecodeS(value);
		MIME_h_ContentDisposition mIME_h_ContentDisposition = new MIME_h_ContentDisposition();
		string[] array = text.Split(new char[1] { ':' }, 2);
		if (array.Length != 2)
		{
			throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
		}
		MIME_Reader mIME_Reader = new MIME_Reader(array[1]);
		string text2 = mIME_Reader.Token();
		if (text2 == null)
		{
			throw new ParseException("Invalid Content-Disposition: header field value '" + value + "'.");
		}
		mIME_h_ContentDisposition.m_DispositionType = text2.Trim();
		mIME_h_ContentDisposition.m_pParameters.Parse(mIME_Reader);
		mIME_h_ContentDisposition.m_ParseValue = value;
		return mIME_h_ContentDisposition;
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		if (reEncode || IsModified)
		{
			return "Content-Disposition: " + m_DispositionType + m_pParameters.ToString(parmetersCharset) + "\r\n";
		}
		return m_ParseValue;
	}
}
