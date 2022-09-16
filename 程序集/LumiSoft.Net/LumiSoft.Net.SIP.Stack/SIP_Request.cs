using System;
using System.IO;
using System.Net;
using System.Text;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Request : SIP_Message
{
	private SIP_RequestLine m_pRequestLine;

	private SIP_Flow m_pFlow;

	private IPEndPoint m_pLocalEP;

	private IPEndPoint m_pRemoteEP;

	public SIP_RequestLine RequestLine => m_pRequestLine;

	internal SIP_Flow Flow
	{
		get
		{
			return m_pFlow;
		}
		set
		{
			m_pFlow = value;
		}
	}

	internal IPEndPoint LocalEndPoint
	{
		get
		{
			return m_pLocalEP;
		}
		set
		{
			m_pLocalEP = value;
		}
	}

	internal IPEndPoint RemoteEndPoint
	{
		get
		{
			return m_pRemoteEP;
		}
		set
		{
			m_pRemoteEP = value;
		}
	}

	public SIP_Request(string method)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		m_pRequestLine = new SIP_RequestLine(method, new AbsoluteUri());
	}

	public SIP_Request Copy()
	{
		SIP_Request sIP_Request = Parse(ToByteData());
		sIP_Request.Flow = m_pFlow;
		sIP_Request.LocalEndPoint = m_pLocalEP;
		sIP_Request.RemoteEndPoint = m_pRemoteEP;
		return sIP_Request;
	}

	public void Validate()
	{
		if (!RequestLine.Version.ToUpper().StartsWith("SIP/2.0"))
		{
			throw new SIP_ParseException("Not supported SIP version '" + RequestLine.Version + "' !");
		}
		if (base.Via.GetTopMostValue() == null)
		{
			throw new SIP_ParseException("Via: header field is missing !");
		}
		if (base.Via.GetTopMostValue().Branch == null)
		{
			throw new SIP_ParseException("Via: header field branch parameter is missing !");
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
		if (base.MaxForwards == -1)
		{
			base.MaxForwards = 70;
		}
		if (SIP_Utils.MethodCanEstablishDialog(RequestLine.Method))
		{
			if (base.Contact.GetAllValues().Length == 0)
			{
				throw new SIP_ParseException("Contact: header field is missing, method that can establish a dialog MUST provide a SIP or SIPS URI !");
			}
			if (base.Contact.GetAllValues().Length > 1)
			{
				throw new SIP_ParseException("There may be only 1 Contact: header for the method that can establish a dialog !");
			}
			if (!base.Contact.GetTopMostValue().Address.IsSipOrSipsUri)
			{
				throw new SIP_ParseException("Method that can establish a dialog MUST have SIP or SIPS uri in Contact: header !");
			}
		}
	}

	public static SIP_Request Parse(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return Parse(new MemoryStream(data));
	}

	public static SIP_Request Parse(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		string[] array = new StreamLineReader(stream)
		{
			Encoding = "utf-8"
		}.ReadLineString().Split(' ');
		if (array.Length != 3)
		{
			throw new Exception("Invalid SIP request data ! Method line doesn't contain: SIP-Method SIP-URI SIP-Version.");
		}
		SIP_Request sIP_Request = new SIP_Request(array[0]);
		sIP_Request.RequestLine.Uri = AbsoluteUri.Parse(array[1]);
		sIP_Request.RequestLine.Version = array[2];
		sIP_Request.InternalParse(stream);
		return sIP_Request;
	}

	public void ToStream(Stream stream)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(m_pRequestLine.ToString());
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
