using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_DigestMD5_Response
{
	private AUTH_SASL_DigestMD5_Challenge m_pChallenge;

	private string m_UserName;

	private string m_Password;

	private string m_Realm;

	private string m_Nonce;

	private string m_Cnonce;

	private int m_NonceCount;

	private string m_Qop;

	private string m_DigestUri;

	private string m_Response;

	private string m_Charset;

	private string m_Cipher;

	private string m_Authzid;

	public string UserName => m_UserName;

	public string Realm => m_Realm;

	public string Nonce => m_Nonce;

	public string Cnonce => m_Cnonce;

	public int NonceCount => m_NonceCount;

	public string Qop => m_Qop;

	public string DigestUri => m_DigestUri;

	public string Response => m_Response;

	public string Charset => m_Charset;

	public string Cipher => m_Cipher;

	public string Authzid => m_Authzid;

	public AUTH_SASL_DigestMD5_Response(AUTH_SASL_DigestMD5_Challenge challenge, string realm, string userName, string password, string cnonce, int nonceCount, string qop, string digestUri)
	{
		if (challenge == null)
		{
			throw new ArgumentNullException("challenge");
		}
		if (realm == null)
		{
			throw new ArgumentNullException("realm");
		}
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (cnonce == null)
		{
			throw new ArgumentNullException("cnonce");
		}
		if (qop == null)
		{
			throw new ArgumentNullException("qop");
		}
		if (digestUri == null)
		{
			throw new ArgumentNullException("digestUri");
		}
		m_pChallenge = challenge;
		m_Realm = realm;
		m_UserName = userName;
		m_Password = password;
		m_Nonce = m_pChallenge.Nonce;
		m_Cnonce = cnonce;
		m_NonceCount = nonceCount;
		m_Qop = qop;
		m_DigestUri = digestUri;
		m_Response = CalculateResponse(userName, password);
		m_Charset = challenge.Charset;
	}

	private AUTH_SASL_DigestMD5_Response()
	{
	}

	public static AUTH_SASL_DigestMD5_Response Parse(string digestResponse)
	{
		if (digestResponse == null)
		{
			throw new ArgumentNullException(digestResponse);
		}
		AUTH_SASL_DigestMD5_Response aUTH_SASL_DigestMD5_Response = new AUTH_SASL_DigestMD5_Response();
		aUTH_SASL_DigestMD5_Response.m_Realm = "";
		string[] array = TextUtils.SplitQuotedString(digestResponse, ',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' }, 2);
			string text = array2[0].Trim();
			if (array2.Length == 2)
			{
				if (text.ToLower() == "username")
				{
					aUTH_SASL_DigestMD5_Response.m_UserName = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "realm")
				{
					aUTH_SASL_DigestMD5_Response.m_Realm = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "nonce")
				{
					aUTH_SASL_DigestMD5_Response.m_Nonce = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "cnonce")
				{
					aUTH_SASL_DigestMD5_Response.m_Cnonce = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "nc")
				{
					aUTH_SASL_DigestMD5_Response.m_NonceCount = int.Parse(TextUtils.UnQuoteString(array2[1]), NumberStyles.HexNumber);
				}
				else if (text.ToLower() == "qop")
				{
					aUTH_SASL_DigestMD5_Response.m_Qop = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "digest-uri")
				{
					aUTH_SASL_DigestMD5_Response.m_DigestUri = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "response")
				{
					aUTH_SASL_DigestMD5_Response.m_Response = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "charset")
				{
					aUTH_SASL_DigestMD5_Response.m_Charset = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "cipher")
				{
					aUTH_SASL_DigestMD5_Response.m_Cipher = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "authzid")
				{
					aUTH_SASL_DigestMD5_Response.m_Authzid = TextUtils.UnQuoteString(array2[1]);
				}
			}
		}
		if (string.IsNullOrEmpty(aUTH_SASL_DigestMD5_Response.UserName))
		{
			throw new ParseException("The response-string doesn't contain required parameter 'username' value.");
		}
		if (string.IsNullOrEmpty(aUTH_SASL_DigestMD5_Response.Nonce))
		{
			throw new ParseException("The response-string doesn't contain required parameter 'nonce' value.");
		}
		if (string.IsNullOrEmpty(aUTH_SASL_DigestMD5_Response.Cnonce))
		{
			throw new ParseException("The response-string doesn't contain required parameter 'cnonce' value.");
		}
		if (aUTH_SASL_DigestMD5_Response.NonceCount < 1)
		{
			throw new ParseException("The response-string doesn't contain required parameter 'nc' value.");
		}
		if (string.IsNullOrEmpty(aUTH_SASL_DigestMD5_Response.Response))
		{
			throw new ParseException("The response-string doesn't contain required parameter 'response' value.");
		}
		return aUTH_SASL_DigestMD5_Response;
	}

	public bool Authenticate(string userName, string password)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (Response == CalculateResponse(userName, password))
		{
			return true;
		}
		return false;
	}

	public string ToResponse()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("username=\"" + UserName + "\"");
		stringBuilder.Append(",realm=\"" + Realm + "\"");
		stringBuilder.Append(",nonce=\"" + Nonce + "\"");
		stringBuilder.Append(",cnonce=\"" + Cnonce + "\"");
		stringBuilder.Append(",nc=" + NonceCount.ToString("x8"));
		stringBuilder.Append(",qop=" + Qop);
		stringBuilder.Append(",digest-uri=\"" + DigestUri + "\"");
		stringBuilder.Append(",response=" + Response);
		if (!string.IsNullOrEmpty(Charset))
		{
			stringBuilder.Append(",charset=" + Charset);
		}
		if (!string.IsNullOrEmpty(Cipher))
		{
			stringBuilder.Append(",cipher=\"" + Cipher + "\"");
		}
		if (!string.IsNullOrEmpty(Authzid))
		{
			stringBuilder.Append(",authzid=\"" + Authzid + "\"");
		}
		return stringBuilder.ToString();
	}

	public string ToRspauthResponse(string userName, string password)
	{
		byte[] value = null;
		if (string.IsNullOrEmpty(Qop) || Qop.ToLower() == "auth")
		{
			value = Encoding.UTF8.GetBytes(":" + DigestUri);
		}
		else if (Qop.ToLower() == "auth-int" || Qop.ToLower() == "auth-conf")
		{
			value = Encoding.UTF8.GetBytes(":" + DigestUri + ":00000000000000000000000000000000");
		}
		if (Qop.ToLower() == "auth")
		{
			return "rspauth=" + hex(kd(hex(h(a1(userName, password))), m_Nonce + ":" + NonceCount.ToString("x8") + ":" + Cnonce + ":" + Qop + ":" + hex(h(value))));
		}
		throw new ArgumentException("Invalid 'qop' value '" + Qop + "'.");
	}

	private string CalculateResponse(string userName, string password)
	{
		if (string.IsNullOrEmpty(Qop) || Qop.ToLower() == "auth")
		{
			return hex(kd(hex(h(a1(userName, password))), m_Nonce + ":" + NonceCount.ToString("x8") + ":" + Cnonce + ":" + Qop + ":" + hex(h(a2()))));
		}
		throw new ArgumentException("Invalid 'qop' value '" + Qop + "'.");
	}

	private byte[] a1(string userName, string password)
	{
		if (string.IsNullOrEmpty(Authzid))
		{
			byte[] array = h(Encoding.UTF8.GetBytes(userName + ":" + Realm + ":" + password));
			byte[] bytes = Encoding.UTF8.GetBytes(":" + m_Nonce + ":" + Cnonce);
			byte[] array2 = new byte[array.Length + bytes.Length];
			Array.Copy(array, 0, array2, 0, array.Length);
			Array.Copy(bytes, 0, array2, array.Length, bytes.Length);
			return array2;
		}
		byte[] array3 = h(Encoding.UTF8.GetBytes(userName + ":" + Realm + ":" + password));
		byte[] bytes2 = Encoding.UTF8.GetBytes(":" + m_Nonce + ":" + Cnonce + ":" + Authzid);
		byte[] array4 = new byte[array3.Length + bytes2.Length];
		Array.Copy(array3, 0, array4, 0, array3.Length);
		Array.Copy(bytes2, 0, array4, array3.Length, bytes2.Length);
		return array4;
	}

	private byte[] a2()
	{
		if (string.IsNullOrEmpty(Qop) || Qop.ToLower() == "auth")
		{
			return Encoding.UTF8.GetBytes("AUTHENTICATE:" + DigestUri);
		}
		if (Qop.ToLower() == "auth-int" || Qop.ToLower() == "auth-conf")
		{
			return Encoding.UTF8.GetBytes("AUTHENTICATE:" + DigestUri + ":00000000000000000000000000000000");
		}
		throw new ArgumentException("Invalid 'qop' value '" + Qop + "'.");
	}

	private byte[] h(byte[] value)
	{
		using MD5 mD = new MD5CryptoServiceProvider();
		return mD.ComputeHash(value);
	}

	private byte[] kd(string secret, string data)
	{
		return h(Encoding.UTF8.GetBytes(secret + ":" + data));
	}

	private string hex(byte[] value)
	{
		return Net_Utils.ToHex(value);
	}
}
