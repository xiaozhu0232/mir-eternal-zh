using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LumiSoft.Net.AUTH;

public class AUTH_Gmail_OAuth1_3leg
{
	private string m_ConsumerKey;

	private string m_ConsumerSecret;

	private string m_Scope = "https://mail.google.com/ https://www.googleapis.com/auth/userinfo.email";

	private string m_RequestToken;

	private string m_RequestTokenSecret;

	private string m_AccessToken;

	private string m_AccessTokenSecret;

	private string m_Email;

	private Random m_pRandom;

	public string Email => m_Email;

	public AUTH_Gmail_OAuth1_3leg()
		: this("anonymous", "anonymous")
	{
	}

	public AUTH_Gmail_OAuth1_3leg(string consumerKey, string consumerSecret)
	{
		if (consumerKey == null)
		{
			throw new ArgumentNullException("consumerKey");
		}
		if (consumerKey == "")
		{
			throw new ArgumentException("Argument 'consumerKey' value must be specified.", "consumerKey");
		}
		if (consumerSecret == null)
		{
			throw new ArgumentNullException("consumerSecret");
		}
		if (consumerSecret == "")
		{
			throw new ArgumentException("Argument 'consumerSecret' value must be specified.", "consumerSecret");
		}
		m_ConsumerKey = consumerKey;
		m_ConsumerSecret = consumerSecret;
		m_pRandom = new Random();
	}

	public void GetRequestToken()
	{
		GetRequestToken("oob");
	}

	public void GetRequestToken(string callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (!string.IsNullOrEmpty(m_RequestToken))
		{
			throw new InvalidOperationException("Invalid state, you have already called this 'GetRequestToken' method.");
		}
		string text = GenerateTimeStamp();
		string text2 = GenerateNonce();
		string requestUriString = "https://www.google.com/accounts/OAuthGetRequestToken?scope=" + UrlEncode(m_Scope);
		string value = "https://www.google.com/accounts/OAuthGetRequestToken";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("oauth_callback=" + UrlEncode(callback));
		stringBuilder.Append("&oauth_consumer_key=" + UrlEncode(m_ConsumerKey));
		stringBuilder.Append("&oauth_nonce=" + UrlEncode(text2));
		stringBuilder.Append("&oauth_signature_method=" + UrlEncode("HMAC-SHA1"));
		stringBuilder.Append("&oauth_timestamp=" + UrlEncode(text));
		stringBuilder.Append("&oauth_version=" + UrlEncode("1.0"));
		stringBuilder.Append("&scope=" + UrlEncode(m_Scope));
		string signatureBase = "GET&" + UrlEncode(value) + "&" + UrlEncode(stringBuilder.ToString());
		string value2 = ComputeHmacSha1Signature(signatureBase, m_ConsumerSecret, null);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("Authorization: OAuth ");
		stringBuilder2.Append("oauth_version=\"1.0\", ");
		stringBuilder2.Append("oauth_nonce=\"" + text2 + "\", ");
		stringBuilder2.Append("oauth_timestamp=\"" + text + "\", ");
		stringBuilder2.Append("oauth_consumer_key=\"" + m_ConsumerKey + "\", ");
		stringBuilder2.Append("oauth_callback=\"" + UrlEncode(callback) + "\", ");
		stringBuilder2.Append("oauth_signature_method=\"HMAC-SHA1\", ");
		stringBuilder2.Append("oauth_signature=\"" + UrlEncode(value2) + "\"");
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(requestUriString);
		obj.Method = "GET";
		obj.Headers.Add(stringBuilder2.ToString());
		using WebResponse webResponse = obj.GetResponse();
		using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
		string[] array = HttpUtility.UrlDecode(streamReader.ReadToEnd()).Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (string.Equals(array2[0], "oauth_token", StringComparison.InvariantCultureIgnoreCase))
			{
				m_RequestToken = array2[1];
			}
			else if (string.Equals(array2[0], "oauth_token_secret", StringComparison.InvariantCultureIgnoreCase))
			{
				m_RequestTokenSecret = array2[1];
			}
		}
	}

	public string GetAuthorizationUrl()
	{
		if (m_RequestToken == null)
		{
			throw new InvalidOperationException("You need call method 'GetRequestToken' before.");
		}
		return "https://accounts.google.com/OAuthAuthorizeToken?oauth_token=" + UrlEncode(m_RequestToken) + "&hd=default";
	}

	public void GetAccessToken(string verificationCode)
	{
		if (verificationCode == null)
		{
			throw new ArgumentNullException("verificationCode");
		}
		if (verificationCode == "")
		{
			throw new ArgumentException("Argument 'verificationCode' value must be specified.", "verificationCode");
		}
		if (string.IsNullOrEmpty(m_RequestToken))
		{
			throw new InvalidOperationException("Invalid state, you need to call 'GetRequestToken' method first.");
		}
		if (!string.IsNullOrEmpty(m_AccessToken))
		{
			throw new InvalidOperationException("Invalid state, you have already called this 'GetAccessToken' method.");
		}
		string text = "https://www.google.com/accounts/OAuthGetAccessToken";
		string text2 = GenerateTimeStamp();
		string text3 = GenerateNonce();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("oauth_consumer_key=" + UrlEncode(m_ConsumerKey));
		stringBuilder.Append("&oauth_nonce=" + UrlEncode(text3));
		stringBuilder.Append("&oauth_signature_method=" + UrlEncode("HMAC-SHA1"));
		stringBuilder.Append("&oauth_timestamp=" + UrlEncode(text2));
		stringBuilder.Append("&oauth_token=" + UrlEncode(m_RequestToken));
		stringBuilder.Append("&oauth_verifier=" + UrlEncode(verificationCode));
		stringBuilder.Append("&oauth_version=" + UrlEncode("1.0"));
		string signatureBase = "GET&" + UrlEncode(text) + "&" + UrlEncode(stringBuilder.ToString());
		string value = ComputeHmacSha1Signature(signatureBase, m_ConsumerSecret, m_RequestTokenSecret);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("Authorization: OAuth ");
		stringBuilder2.Append("oauth_version=\"1.0\", ");
		stringBuilder2.Append("oauth_nonce=\"" + text3 + "\", ");
		stringBuilder2.Append("oauth_timestamp=\"" + text2 + "\", ");
		stringBuilder2.Append("oauth_consumer_key=\"" + m_ConsumerKey + "\", ");
		stringBuilder2.Append("oauth_verifier=\"" + UrlEncode(verificationCode) + "\", ");
		stringBuilder2.Append("oauth_token=\"" + UrlEncode(m_RequestToken) + "\", ");
		stringBuilder2.Append("oauth_signature_method=\"HMAC-SHA1\", ");
		stringBuilder2.Append("oauth_signature=\"" + UrlEncode(value) + "\"");
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(text);
		obj.Method = "GET";
		obj.Headers.Add(stringBuilder2.ToString());
		using WebResponse webResponse = obj.GetResponse();
		using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
		string[] array = HttpUtility.UrlDecode(streamReader.ReadToEnd()).Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (string.Equals(array2[0], "oauth_token", StringComparison.InvariantCultureIgnoreCase))
			{
				m_AccessToken = array2[1];
			}
			else if (string.Equals(array2[0], "oauth_token_secret", StringComparison.InvariantCultureIgnoreCase))
			{
				m_AccessTokenSecret = array2[1];
			}
		}
	}

	public string GetXOAuthStringForSmtp()
	{
		return GetXOAuthStringForSmtp((m_Email == null) ? GetUserEmail() : m_Email);
	}

	public string GetXOAuthStringForSmtp(string email)
	{
		if (email == null)
		{
			throw new ArgumentNullException("email");
		}
		if (email == "")
		{
			throw new ArgumentException("Argument 'email' value must be specified.", "email");
		}
		if (string.IsNullOrEmpty(m_AccessToken))
		{
			throw new InvalidOperationException("Invalid state, you need to call 'GetAccessToken' method first.");
		}
		string value = "https://mail.google.com/mail/b/" + email + "/smtp/";
		string value2 = GenerateTimeStamp();
		string value3 = GenerateNonce();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("oauth_consumer_key=" + UrlEncode(m_ConsumerKey));
		stringBuilder.Append("&oauth_nonce=" + UrlEncode(value3));
		stringBuilder.Append("&oauth_signature_method=" + UrlEncode("HMAC-SHA1"));
		stringBuilder.Append("&oauth_timestamp=" + UrlEncode(value2));
		stringBuilder.Append("&oauth_token=" + UrlEncode(m_AccessToken));
		stringBuilder.Append("&oauth_version=" + UrlEncode("1.0"));
		string signatureBase = "GET&" + UrlEncode(value) + "&" + UrlEncode(stringBuilder.ToString());
		string value4 = ComputeHmacSha1Signature(signatureBase, m_ConsumerSecret, m_AccessTokenSecret);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("GET ");
		stringBuilder2.Append(value);
		stringBuilder2.Append(" oauth_consumer_key=\"" + UrlEncode(m_ConsumerKey) + "\"");
		stringBuilder2.Append(",oauth_nonce=\"" + UrlEncode(value3) + "\"");
		stringBuilder2.Append(",oauth_signature=\"" + UrlEncode(value4) + "\"");
		stringBuilder2.Append(",oauth_signature_method=\"HMAC-SHA1\"");
		stringBuilder2.Append(",oauth_timestamp=\"" + UrlEncode(value2) + "\"");
		stringBuilder2.Append(",oauth_token=\"" + UrlEncode(m_AccessToken) + "\"");
		stringBuilder2.Append(",oauth_version=\"1.0\"");
		return stringBuilder2.ToString();
	}

	public string GetXOAuthStringForImap()
	{
		return GetXOAuthStringForImap((m_Email == null) ? GetUserEmail() : m_Email);
	}

	public string GetXOAuthStringForImap(string email)
	{
		if (email == null)
		{
			throw new ArgumentNullException("email");
		}
		if (email == "")
		{
			throw new ArgumentException("Argument 'email' value must be specified.", "email");
		}
		if (string.IsNullOrEmpty(m_AccessToken))
		{
			throw new InvalidOperationException("Invalid state, you need to call 'GetAccessToken' method first.");
		}
		string value = "https://mail.google.com/mail/b/" + email + "/imap/";
		string value2 = GenerateTimeStamp();
		string value3 = GenerateNonce();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("oauth_consumer_key=" + UrlEncode(m_ConsumerKey));
		stringBuilder.Append("&oauth_nonce=" + UrlEncode(value3));
		stringBuilder.Append("&oauth_signature_method=" + UrlEncode("HMAC-SHA1"));
		stringBuilder.Append("&oauth_timestamp=" + UrlEncode(value2));
		stringBuilder.Append("&oauth_token=" + UrlEncode(m_AccessToken));
		stringBuilder.Append("&oauth_version=" + UrlEncode("1.0"));
		string signatureBase = "GET&" + UrlEncode(value) + "&" + UrlEncode(stringBuilder.ToString());
		string value4 = ComputeHmacSha1Signature(signatureBase, m_ConsumerSecret, m_AccessTokenSecret);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("GET ");
		stringBuilder2.Append(value);
		stringBuilder2.Append(" oauth_consumer_key=\"" + UrlEncode(m_ConsumerKey) + "\"");
		stringBuilder2.Append(",oauth_nonce=\"" + UrlEncode(value3) + "\"");
		stringBuilder2.Append(",oauth_signature=\"" + UrlEncode(value4) + "\"");
		stringBuilder2.Append(",oauth_signature_method=\"HMAC-SHA1\"");
		stringBuilder2.Append(",oauth_timestamp=\"" + UrlEncode(value2) + "\"");
		stringBuilder2.Append(",oauth_token=\"" + UrlEncode(m_AccessToken) + "\"");
		stringBuilder2.Append(",oauth_version=\"1.0\"");
		return stringBuilder2.ToString();
	}

	public string GetUserEmail()
	{
		if (string.IsNullOrEmpty(m_AccessToken))
		{
			throw new InvalidOperationException("Invalid state, you need to call 'GetAccessToken' method first.");
		}
		string text = "https://www.googleapis.com/userinfo/email";
		string text2 = GenerateTimeStamp();
		string text3 = GenerateNonce();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("oauth_consumer_key=" + UrlEncode(m_ConsumerKey));
		stringBuilder.Append("&oauth_nonce=" + UrlEncode(text3));
		stringBuilder.Append("&oauth_signature_method=" + UrlEncode("HMAC-SHA1"));
		stringBuilder.Append("&oauth_timestamp=" + UrlEncode(text2));
		stringBuilder.Append("&oauth_token=" + UrlEncode(m_AccessToken));
		stringBuilder.Append("&oauth_version=" + UrlEncode("1.0"));
		string signatureBase = "GET&" + UrlEncode(text) + "&" + UrlEncode(stringBuilder.ToString());
		string value = ComputeHmacSha1Signature(signatureBase, m_ConsumerSecret, m_AccessTokenSecret);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("Authorization: OAuth ");
		stringBuilder2.Append("oauth_version=\"1.0\", ");
		stringBuilder2.Append("oauth_nonce=\"" + text3 + "\", ");
		stringBuilder2.Append("oauth_timestamp=\"" + text2 + "\", ");
		stringBuilder2.Append("oauth_consumer_key=\"" + m_ConsumerKey + "\", ");
		stringBuilder2.Append("oauth_token=\"" + UrlEncode(m_AccessToken) + "\", ");
		stringBuilder2.Append("oauth_signature_method=\"HMAC-SHA1\", ");
		stringBuilder2.Append("oauth_signature=\"" + UrlEncode(value) + "\"");
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(text);
		obj.Method = "GET";
		obj.Headers.Add(stringBuilder2.ToString());
		using (WebResponse webResponse = obj.GetResponse())
		{
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			string[] array = HttpUtility.UrlDecode(streamReader.ReadToEnd()).Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (string.Equals(array2[0], "email", StringComparison.InvariantCultureIgnoreCase))
				{
					m_Email = array2[1];
				}
			}
		}
		return m_Email;
	}

	private string UrlEncode(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string text = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in value)
		{
			if (text.IndexOf(c) != -1)
			{
				stringBuilder.Append(c);
			}
			else
			{
				stringBuilder.Append("%" + $"{(int)c:X2}");
			}
		}
		return stringBuilder.ToString();
	}

	private string ComputeHmacSha1Signature(string signatureBase, string consumerSecret, string tokenSecret)
	{
		if (signatureBase == null)
		{
			throw new ArgumentNullException("signatureBase");
		}
		if (consumerSecret == null)
		{
			throw new ArgumentNullException("consumerSecret");
		}
		return Convert.ToBase64String(new HMACSHA1
		{
			Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)))
		}.ComputeHash(Encoding.ASCII.GetBytes(signatureBase)));
	}

	private string GenerateTimeStamp()
	{
		return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
	}

	private string GenerateNonce()
	{
		return m_pRandom.Next(123400, 9999999).ToString();
	}
}
