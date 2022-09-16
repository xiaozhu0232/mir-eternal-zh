using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class Auth_HttpDigest
{
	private string m_Method = "";

	private string m_Realm = "";

	private string m_Nonce = "";

	private string m_Opaque = "";

	private string m_Algorithm = "";

	private string m_Response = "";

	private string m_UserName = "";

	private string m_Password = "";

	private string m_Uri = "";

	private string m_Qop = "";

	private string m_Cnonce = "";

	private int m_NonceCount = 1;

	private string m_Charset = "";

	public string RequestMethod
	{
		get
		{
			return m_Method;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_Method = value;
		}
	}

	public string Realm
	{
		get
		{
			return m_Realm;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_Realm = value;
		}
	}

	public string Nonce
	{
		get
		{
			return m_Nonce;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Nonce value can't be null or empty !");
			}
			m_Nonce = value;
		}
	}

	public string Opaque
	{
		get
		{
			return m_Opaque;
		}
		set
		{
			m_Opaque = value;
		}
	}

	public string Algorithm
	{
		get
		{
			return m_Algorithm;
		}
		set
		{
			m_Algorithm = value;
		}
	}

	public string Response => m_Response;

	public string UserName
	{
		get
		{
			return m_UserName;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_UserName = value;
		}
	}

	public string Password
	{
		get
		{
			return m_Password;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_Password = value;
		}
	}

	public string Uri
	{
		get
		{
			return m_Uri;
		}
		set
		{
			m_Uri = value;
		}
	}

	public string Qop
	{
		get
		{
			return m_Qop;
		}
		set
		{
			m_Qop = value;
		}
	}

	public string CNonce
	{
		get
		{
			return m_Cnonce;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_Cnonce = value;
		}
	}

	public int NonceCount
	{
		get
		{
			return m_NonceCount;
		}
		set
		{
			m_NonceCount = value;
		}
	}

	public Auth_HttpDigest(string digestResponse, string requestMethod)
	{
		m_Method = requestMethod;
		Parse(digestResponse);
	}

	public Auth_HttpDigest(string userName, string password, string cnonce, string uri, string digestResponse, string requestMethod)
	{
		Parse(digestResponse);
		m_UserName = userName;
		m_Password = password;
		m_Method = requestMethod;
		m_Cnonce = cnonce;
		m_Uri = uri;
		m_Qop = "auth";
		m_NonceCount = 1;
		m_Response = CalculateResponse(m_UserName, m_Password);
	}

	public Auth_HttpDigest(string realm, string nonce, string opaque)
	{
		m_Realm = realm;
		m_Nonce = nonce;
		m_Opaque = opaque;
	}

	public bool Authenticate(string userName, string password)
	{
		if (Response == CalculateResponse(userName, password))
		{
			return true;
		}
		return false;
	}

	private void Parse(string digestResponse)
	{
		string[] array = TextUtils.SplitQuotedString(digestResponse, ',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' }, 2);
			string a = array2[0].Trim();
			if (array2.Length == 2)
			{
				if (string.Equals(a, "realm", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Realm = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "nonce", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Nonce = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "uri", StringComparison.InvariantCultureIgnoreCase) || string.Equals(a, "digest-uri", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Uri = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "qop", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Qop = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "nc", StringComparison.InvariantCultureIgnoreCase))
				{
					m_NonceCount = Convert.ToInt32(TextUtils.UnQuoteString(array2[1]));
				}
				else if (string.Equals(a, "cnonce", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Cnonce = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "response", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Response = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "opaque", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Opaque = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "username", StringComparison.InvariantCultureIgnoreCase))
				{
					m_UserName = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "algorithm", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Algorithm = TextUtils.UnQuoteString(array2[1]);
				}
				else if (string.Equals(a, "charset", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Charset = TextUtils.UnQuoteString(array2[1]);
				}
			}
		}
	}

	public string CalculateRspAuth(string userName, string password)
	{
		string text = "";
		string text2 = "";
		if (string.IsNullOrEmpty(Algorithm) || string.Equals(Algorithm, "md5", StringComparison.InvariantCultureIgnoreCase))
		{
			text = userName + ":" + Realm + ":" + password;
		}
		else
		{
			if (!string.Equals(Algorithm, "md5-sess", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new ArgumentException("Invalid Algorithm value '" + Algorithm + "' !");
			}
			text = Net_Utils.ComputeMd5(userName + ":" + Realm + ":" + password, hex: false) + ":" + Nonce + ":" + CNonce;
		}
		if (string.IsNullOrEmpty(Qop) || string.Equals(Qop, "auth", StringComparison.InvariantCultureIgnoreCase))
		{
			text2 = ":" + Uri;
			if (!string.IsNullOrEmpty(Qop))
			{
				return Net_Utils.ComputeMd5(Net_Utils.ComputeMd5(text, hex: true) + ":" + Nonce + ":" + NonceCount.ToString("x8") + ":" + CNonce + ":" + Qop + ":" + Net_Utils.ComputeMd5(text2, hex: true), hex: true);
			}
			return Net_Utils.ComputeMd5(Net_Utils.ComputeMd5(text, hex: true) + ":" + Nonce + ":" + Net_Utils.ComputeMd5(text2, hex: true), hex: true);
		}
		throw new ArgumentException("Invalid qop value '" + Qop + "' !");
	}

	public string CalculateResponse(string userName, string password)
	{
		string text = "";
		if (string.IsNullOrEmpty(Algorithm) || string.Equals(Algorithm, "md5", StringComparison.InvariantCultureIgnoreCase))
		{
			text = userName + ":" + Realm + ":" + password;
		}
		else
		{
			if (!string.Equals(Algorithm, "md5-sess", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new ArgumentException("Invalid 'algorithm' value '" + Algorithm + "'.");
			}
			text = H(userName + ":" + Realm + ":" + password) + ":" + Nonce + ":" + CNonce;
		}
		string text2 = "";
		if (string.IsNullOrEmpty(Qop) || string.Equals(Qop, "auth", StringComparison.InvariantCultureIgnoreCase))
		{
			text2 = RequestMethod + ":" + Uri;
			if (string.Equals(Qop, "auth", StringComparison.InvariantCultureIgnoreCase) || string.Equals(Qop, "auth-int", StringComparison.InvariantCultureIgnoreCase))
			{
				return KD(H(text), Nonce + ":" + NonceCount.ToString("x8") + ":" + CNonce + ":" + Qop + ":" + H(text2));
			}
			if (string.IsNullOrEmpty(Qop))
			{
				return KD(H(text), Nonce + ":" + H(text2));
			}
			throw new ArgumentException("Invalid 'qop' value '" + Qop + "'.");
		}
		throw new ArgumentException("Invalid 'qop' value '" + Qop + "'.");
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("realm=\"" + m_Realm + "\",");
		stringBuilder.Append("username=\"" + m_UserName + "\",");
		if (!string.IsNullOrEmpty(m_Qop))
		{
			stringBuilder.Append("qop=\"" + m_Qop + "\",");
		}
		stringBuilder.Append("nonce=\"" + m_Nonce + "\",");
		stringBuilder.Append("nc=\"" + m_NonceCount + "\",");
		stringBuilder.Append("cnonce=\"" + m_Cnonce + "\",");
		stringBuilder.Append("response=\"" + m_Response + "\",");
		stringBuilder.Append("opaque=\"" + m_Opaque + "\",");
		stringBuilder.Append("uri=\"" + m_Uri + "\"");
		return stringBuilder.ToString();
	}

	public string ToChallenge()
	{
		return ToChallenge(addAuthMethod: true);
	}

	public string ToChallenge(bool addAuthMethod)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (addAuthMethod)
		{
			stringBuilder.Append("digest ");
		}
		stringBuilder.Append("realm=" + TextUtils.QuoteString(m_Realm) + ",");
		if (!string.IsNullOrEmpty(m_Qop))
		{
			stringBuilder.Append("qop=" + TextUtils.QuoteString(m_Qop) + ",");
		}
		stringBuilder.Append("nonce=" + TextUtils.QuoteString(m_Nonce) + ",");
		stringBuilder.Append("opaque=" + TextUtils.QuoteString(m_Opaque));
		return stringBuilder.ToString();
	}

	public string ToAuthorization()
	{
		return ToAuthorization(addAuthMethod: true);
	}

	public string ToAuthorization(bool addAuthMethod)
	{
		string text = "";
		text = ((!string.IsNullOrEmpty(m_Password)) ? CalculateResponse(m_UserName, m_Password) : m_Response);
		StringBuilder stringBuilder = new StringBuilder();
		if (addAuthMethod)
		{
			stringBuilder.Append("Digest ");
		}
		stringBuilder.Append("realm=\"" + m_Realm + "\",");
		stringBuilder.Append("username=\"" + m_UserName + "\",");
		stringBuilder.Append("nonce=\"" + m_Nonce + "\",");
		if (!string.IsNullOrEmpty(m_Uri))
		{
			stringBuilder.Append("uri=\"" + m_Uri + "\",");
		}
		if (!string.IsNullOrEmpty(m_Qop))
		{
			stringBuilder.Append("qop=" + m_Qop + ",");
		}
		if (!string.IsNullOrEmpty(m_Qop))
		{
			stringBuilder.Append("nc=" + m_NonceCount.ToString("x8") + ",");
		}
		if (!string.IsNullOrEmpty(m_Cnonce))
		{
			stringBuilder.Append("cnonce=\"" + m_Cnonce + "\",");
		}
		stringBuilder.Append("response=\"" + text + "\",");
		if (!string.IsNullOrEmpty(m_Algorithm))
		{
			stringBuilder.Append("algorithm=\"" + m_Algorithm + "\",");
		}
		if (!string.IsNullOrEmpty(m_Opaque))
		{
			stringBuilder.Append("opaque=\"" + m_Opaque + "\",");
		}
		if (!string.IsNullOrEmpty(m_Charset))
		{
			stringBuilder.Append("charset=" + m_Charset + ",");
		}
		string text2 = stringBuilder.ToString().Trim();
		if (text2.EndsWith(","))
		{
			text2 = text2.Substring(0, text2.Length - 1);
		}
		return text2;
	}

	private string H(string value)
	{
		return Net_Utils.ComputeMd5(value, hex: true);
	}

	private string KD(string key, string data)
	{
		return H(key + ":" + data);
	}

	public static string CreateNonce()
	{
		return Guid.NewGuid().ToString().Replace("-", "");
	}

	public static string CreateOpaque()
	{
		return Guid.NewGuid().ToString().Replace("-", "");
	}

	[Obsolete("Mispell error, use ToChallenge method instead.")]
	public string ToChallange()
	{
		return ToChallange(addAuthMethod: true);
	}

	[Obsolete("Mispell error, use ToChallenge method instead.")]
	public string ToChallange(bool addAuthMethod)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (addAuthMethod)
		{
			stringBuilder.Append("digest ");
		}
		stringBuilder.Append("realm=" + TextUtils.QuoteString(m_Realm) + ",");
		if (!string.IsNullOrEmpty(m_Qop))
		{
			stringBuilder.Append("qop=" + TextUtils.QuoteString(m_Qop) + ",");
		}
		stringBuilder.Append("nonce=" + TextUtils.QuoteString(m_Nonce) + ",");
		stringBuilder.Append("opaque=" + TextUtils.QuoteString(m_Opaque));
		return stringBuilder.ToString();
	}
}
