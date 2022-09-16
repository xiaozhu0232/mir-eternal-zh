using System;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_h_ReturnPath : MIME_h
{
	private bool m_IsModified;

	private string m_Address;

	public override bool IsModified => m_IsModified;

	public override string Name => "Return-Path";

	public string Address => m_Address;

	public Mail_h_ReturnPath(string address)
	{
		m_Address = address;
	}

	public static Mail_h_ReturnPath Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string[] array = value.Split(new char[1] { ':' }, 2);
		if (array.Length != 2)
		{
			throw new ParseException("Invalid header field value '" + value + "'.");
		}
		Mail_h_ReturnPath mail_h_ReturnPath = new Mail_h_ReturnPath(null);
		MIME_Reader mIME_Reader = new MIME_Reader(array[1].Trim());
		mIME_Reader.ToFirstChar();
		if (!mIME_Reader.StartsWith("<"))
		{
			mail_h_ReturnPath.m_Address = mIME_Reader.ToEnd();
		}
		else
		{
			mail_h_ReturnPath.m_Address = mIME_Reader.ReadParenthesized();
		}
		return mail_h_ReturnPath;
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		if (string.IsNullOrEmpty(m_Address))
		{
			return "Return-Path: <>\r\n";
		}
		return "Return-Path: <" + m_Address + ">\r\n";
	}
}
