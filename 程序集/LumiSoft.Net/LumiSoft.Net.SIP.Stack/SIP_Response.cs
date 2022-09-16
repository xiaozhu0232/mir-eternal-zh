using System;
using System.Globalization;
using System.IO;
using System.Text;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Response : SIP_Message
{
	private SIP_Request m_pRequest;

	private double m_SipVersion = 2.0;

	private int m_StatusCode = 100;

	private string m_ReasonPhrase = "";

	public SIP_Request Request => m_pRequest;

	public double SipVersion
	{
		get
		{
			return m_SipVersion;
		}
		set
		{
			if (value < 1.0)
			{
				throw new ArgumentException("Property SIP version must be >= 1.0 !");
			}
			m_SipVersion = value;
		}
	}

	public SIP_StatusCodeType StatusCodeType
	{
		get
		{
			if (m_StatusCode >= 100 && m_StatusCode < 200)
			{
				return SIP_StatusCodeType.Provisional;
			}
			if (m_StatusCode >= 200 && m_StatusCode < 300)
			{
				return SIP_StatusCodeType.Success;
			}
			if (m_StatusCode >= 300 && m_StatusCode < 400)
			{
				return SIP_StatusCodeType.Redirection;
			}
			if (m_StatusCode >= 400 && m_StatusCode < 500)
			{
				return SIP_StatusCodeType.RequestFailure;
			}
			if (m_StatusCode >= 500 && m_StatusCode < 600)
			{
				return SIP_StatusCodeType.ServerFailure;
			}
			if (m_StatusCode >= 600 && m_StatusCode < 700)
			{
				return SIP_StatusCodeType.GlobalFailure;
			}
			throw new Exception("Unknown SIP StatusCodeType !");
		}
	}

	public int StatusCode
	{
		get
		{
			return m_StatusCode;
		}
		set
		{
			if (value < 1 || value > 999)
			{
				throw new ArgumentException("Property 'StatusCode' value must be >= 100 && <= 999 !");
			}
			m_StatusCode = value;
		}
	}

	public string ReasonPhrase
	{
		get
		{
			return m_ReasonPhrase;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ReasonPhrase");
			}
			m_ReasonPhrase = value;
		}
	}

	public string StatusCode_ReasonPhrase
	{
		get
		{
			return m_StatusCode + " " + m_ReasonPhrase;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("StatusCode_ReasonPhrase");
			}
			string[] array = value.Split(new char[1] { ' ' }, 2);
			if (array.Length != 2)
			{
				throw new ArgumentException("Invalid property 'StatusCode_ReasonPhrase' Reason-Phrase value !");
			}
			try
			{
				StatusCode = Convert.ToInt32(array[0]);
			}
			catch
			{
				throw new ArgumentException("Invalid property 'StatusCode_ReasonPhrase' Status-Code value !");
			}
			ReasonPhrase = array[1];
		}
	}

	public SIP_Response()
	{
	}

	internal SIP_Response(SIP_Request request)
	{
		m_pRequest = request;
	}

	public SIP_Response Copy()
	{
		return Parse(ToByteData());
	}

	public void Validate()
	{
		if (base.Via.GetTopMostValue() == null)
		{
			throw new SIP_ParseException("Via: header field is missing !");
		}
		if (base.Via.GetTopMostValue().Branch == null)
		{
			throw new SIP_ParseException("Via: header fields branch parameter is missing !");
		}
		if (base.To == null)
		{
			throw new SIP_ParseException("To: header field is missing !");
		}
		if (base.From == null)
		{
			throw new SIP_ParseException("From: header field is missing !");
		}
		if (base.CallID == null)
		{
			throw new SIP_ParseException("CallID: header field is missing !");
		}
		if (base.CSeq == null)
		{
			throw new SIP_ParseException("CSeq: header field is missing !");
		}
	}

	public static SIP_Response Parse(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return Parse(new MemoryStream(data));
	}

	public static SIP_Response Parse(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SIP_Response sIP_Response = new SIP_Response();
		string[] array = new StreamLineReader(stream)
		{
			Encoding = "utf-8"
		}.ReadLineString().Split(new char[1] { ' ' }, 3);
		if (array.Length != 3)
		{
			throw new SIP_ParseException("Invalid SIP Status-Line syntax ! Syntax: {SIP-Version SP Status-Code SP Reason-Phrase}.");
		}
		try
		{
			sIP_Response.SipVersion = Convert.ToDouble(array[0].Split('/')[1], NumberFormatInfo.InvariantInfo);
		}
		catch
		{
			throw new SIP_ParseException("Invalid Status-Line SIP-Version value !");
		}
		try
		{
			sIP_Response.StatusCode = Convert.ToInt32(array[1]);
		}
		catch
		{
			throw new SIP_ParseException("Invalid Status-Line Status-Code value !");
		}
		sIP_Response.ReasonPhrase = array[2];
		sIP_Response.InternalParse(stream);
		return sIP_Response;
	}

	public void ToStream(Stream stream)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("SIP/" + SipVersion.ToString("f1").Replace(',', '.') + " " + StatusCode + " " + ReasonPhrase + "\r\n");
		stream.Write(bytes, 0, bytes.Length);
		InternalToStream(stream);
	}

	public byte[] ToByteData()
	{
		MemoryStream memoryStream = new MemoryStream();
		ToStream(memoryStream);
		return memoryStream.ToArray();
	}

	public override string ToString()
	{
		return Encoding.UTF8.GetString(ToByteData());
	}
}
