using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_DigestMD5_Challenge
{
	private string[] m_Realm;

	private string m_Nonce;

	private string[] m_QopOptions;

	private bool m_Stale;

	private int m_Maxbuf;

	private string m_Charset;

	private string m_Algorithm;

	private string m_CipherOpts;

	public string[] Realm => m_Realm;

	public string Nonce => m_Nonce;

	public string[] QopOptions => m_QopOptions;

	public bool Stale => m_Stale;

	public int Maxbuf => m_Maxbuf;

	public string Charset => m_Charset;

	public string Algorithm => m_Algorithm;

	public string CipherOpts => m_CipherOpts;

	public AUTH_SASL_DigestMD5_Challenge(string[] realm, string nonce, string[] qopOptions, bool stale)
	{
		if (realm == null)
		{
			throw new ArgumentNullException("realm");
		}
		if (nonce == null)
		{
			throw new ArgumentNullException("nonce");
		}
		if (qopOptions == null)
		{
			throw new ArgumentNullException("qopOptions");
		}
		m_Realm = realm;
		m_Nonce = nonce;
		m_QopOptions = qopOptions;
		m_Stale = stale;
		m_Charset = "utf-8";
		m_Algorithm = "md5-sess";
	}

	private AUTH_SASL_DigestMD5_Challenge()
	{
	}

	public static AUTH_SASL_DigestMD5_Challenge Parse(string challenge)
	{
		if (challenge == null)
		{
			throw new ArgumentNullException("challenge");
		}
		AUTH_SASL_DigestMD5_Challenge aUTH_SASL_DigestMD5_Challenge = new AUTH_SASL_DigestMD5_Challenge();
		string[] array = TextUtils.SplitQuotedString(challenge, ',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' }, 2);
			string text = array2[0].Trim();
			if (array2.Length == 2)
			{
				if (text.ToLower() == "realm")
				{
					aUTH_SASL_DigestMD5_Challenge.m_Realm = TextUtils.UnQuoteString(array2[1]).Split(',');
				}
				else if (text.ToLower() == "nonce")
				{
					aUTH_SASL_DigestMD5_Challenge.m_Nonce = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "qop")
				{
					aUTH_SASL_DigestMD5_Challenge.m_QopOptions = TextUtils.UnQuoteString(array2[1]).Split(',');
				}
				else if (text.ToLower() == "stale")
				{
					aUTH_SASL_DigestMD5_Challenge.m_Stale = Convert.ToBoolean(TextUtils.UnQuoteString(array2[1]));
				}
				else if (text.ToLower() == "maxbuf")
				{
					aUTH_SASL_DigestMD5_Challenge.m_Maxbuf = Convert.ToInt32(TextUtils.UnQuoteString(array2[1]));
				}
				else if (text.ToLower() == "charset")
				{
					aUTH_SASL_DigestMD5_Challenge.m_Charset = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "algorithm")
				{
					aUTH_SASL_DigestMD5_Challenge.m_Algorithm = TextUtils.UnQuoteString(array2[1]);
				}
				else if (text.ToLower() == "cipher-opts")
				{
					aUTH_SASL_DigestMD5_Challenge.m_CipherOpts = TextUtils.UnQuoteString(array2[1]);
				}
			}
		}
		if (string.IsNullOrEmpty(aUTH_SASL_DigestMD5_Challenge.Nonce))
		{
			throw new ParseException("The challenge-string doesn't contain required parameter 'nonce' value.");
		}
		if (string.IsNullOrEmpty(aUTH_SASL_DigestMD5_Challenge.Algorithm))
		{
			throw new ParseException("The challenge-string doesn't contain required parameter 'algorithm' value.");
		}
		return aUTH_SASL_DigestMD5_Challenge;
	}

	public string ToChallenge()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("realm=\"" + Net_Utils.ArrayToString(Realm, ",") + "\"");
		stringBuilder.Append(",nonce=\"" + Nonce + "\"");
		if (QopOptions != null)
		{
			stringBuilder.Append(",qop=\"" + Net_Utils.ArrayToString(QopOptions, ",") + "\"");
		}
		if (Stale)
		{
			stringBuilder.Append(",stale=true");
		}
		if (Maxbuf > 0)
		{
			stringBuilder.Append(",maxbuf=" + Maxbuf);
		}
		if (!string.IsNullOrEmpty(Charset))
		{
			stringBuilder.Append(",charset=" + Charset);
		}
		stringBuilder.Append(",algorithm=" + Algorithm);
		if (!string.IsNullOrEmpty(CipherOpts))
		{
			stringBuilder.Append(",cipher-opts=\"" + CipherOpts + "\"");
		}
		return stringBuilder.ToString();
	}
}
