using System;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.MIME;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP;

public class SIP_Utils
{
	public static string ParseAddress(string to)
	{
		try
		{
			string text = to;
			if (to.IndexOf('<') > -1 && to.IndexOf('<') < to.IndexOf('>'))
			{
				text = to.Substring(to.IndexOf('<') + 1, to.IndexOf('>') - to.IndexOf('<') - 1);
			}
			if (text.IndexOf(':') > -1)
			{
				text = text.Substring(text.IndexOf(':') + 1).Split(':')[0];
			}
			return text;
		}
		catch
		{
			throw new ArgumentException("Invalid SIP header To: '" + to + "' value !");
		}
	}

	public static AbsoluteUri UriToRequestUri(AbsoluteUri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (uri is SIP_Uri)
		{
			SIP_Uri obj = (SIP_Uri)uri;
			obj.Parameters.Remove("method");
			obj.Header = null;
			return obj;
		}
		return uri;
	}

	public static bool IsSipOrSipsUri(string value)
	{
		try
		{
			SIP_Uri.Parse(value);
			return true;
		}
		catch
		{
		}
		return false;
	}

	public static bool IsTelUri(string uri)
	{
		uri = uri.ToLower();
		try
		{
			if (uri.StartsWith("tel:"))
			{
				return true;
			}
			if (IsSipOrSipsUri(uri))
			{
				SIP_Uri sIP_Uri = SIP_Uri.Parse(uri);
				if (sIP_Uri.User.StartsWith("+"))
				{
					return true;
				}
				if (sIP_Uri.Param_User != null && sIP_Uri.Param_User.ToLower() == "phone")
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	public static SIP_t_Credentials GetCredentials(SIP_Request request, string realm)
	{
		SIP_SingleValueHF<SIP_t_Credentials>[] headerFields = request.ProxyAuthorization.HeaderFields;
		foreach (SIP_SingleValueHF<SIP_t_Credentials> sIP_SingleValueHF in headerFields)
		{
			if (sIP_SingleValueHF.ValueX.Method.ToLower() == "digest" && new Auth_HttpDigest(sIP_SingleValueHF.ValueX.AuthData, request.RequestLine.Method).Realm.ToLower() == realm.ToLower())
			{
				return sIP_SingleValueHF.ValueX;
			}
		}
		return null;
	}

	public static bool ContainsOptionTag(SIP_t_OptionTag[] tags, string tag)
	{
		for (int i = 0; i < tags.Length; i++)
		{
			if (tags[i].OptionTag.ToLower() == tag)
			{
				return true;
			}
		}
		return false;
	}

	public static bool MethodCanEstablishDialog(string method)
	{
		if (string.IsNullOrEmpty(method))
		{
			throw new ArgumentException("Argument 'method' value can't be null or empty !");
		}
		method = method.ToUpper();
		return method switch
		{
			"INVITE" => true, 
			"SUBSCRIBE" => true, 
			"REFER" => true, 
			_ => false, 
		};
	}

	public static string CreateTag()
	{
		return Guid.NewGuid().ToString().Replace("-", "")
			.Substring(8);
	}

	public static bool IsReliableTransport(string transport)
	{
		if (transport == null)
		{
			throw new ArgumentNullException("transport");
		}
		if (transport.ToUpper() == "TCP")
		{
			return true;
		}
		if (transport.ToUpper() == "TLS")
		{
			return true;
		}
		return false;
	}

	public static bool IsToken(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return MIME_Reader.IsToken(value);
	}

	public static string ListToString(List<string> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i == 0)
			{
				stringBuilder.Append(list[i]);
			}
			else
			{
				stringBuilder.Append("," + list[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
