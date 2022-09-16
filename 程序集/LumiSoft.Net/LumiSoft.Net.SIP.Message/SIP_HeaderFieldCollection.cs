using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_HeaderFieldCollection : IEnumerable
{
	private List<SIP_HeaderField> m_pHeaderFields;

	public SIP_HeaderField this[int index] => m_pHeaderFields[index];

	public int Count => m_pHeaderFields.Count;

	public SIP_HeaderFieldCollection()
	{
		m_pHeaderFields = new List<SIP_HeaderField>();
	}

	public void Add(string fieldName, string value)
	{
		Add(GetheaderField(fieldName, value));
	}

	public void Add(SIP_HeaderField headerField)
	{
		m_pHeaderFields.Add(headerField);
	}

	public void Insert(int index, string fieldName, string value)
	{
		m_pHeaderFields.Insert(index, GetheaderField(fieldName, value));
	}

	public void Set(string fieldName, string value)
	{
		SIP_HeaderField first = GetFirst(fieldName);
		if (first != null)
		{
			first.Value = value;
		}
		else
		{
			Add(GetheaderField(fieldName, value));
		}
	}

	public void Remove(int index)
	{
		m_pHeaderFields.RemoveAt(index);
	}

	public void Remove(SIP_HeaderField field)
	{
		m_pHeaderFields.Remove(field);
	}

	public void RemoveFirst(string name)
	{
		foreach (SIP_HeaderField pHeaderField in m_pHeaderFields)
		{
			if (pHeaderField.Name.ToLower() == name.ToLower())
			{
				m_pHeaderFields.Remove(pHeaderField);
				break;
			}
		}
	}

	public void RemoveAll(string fieldName)
	{
		for (int i = 0; i < m_pHeaderFields.Count; i++)
		{
			SIP_HeaderField sIP_HeaderField = m_pHeaderFields[i];
			if (sIP_HeaderField.Name.ToLower() == fieldName.ToLower())
			{
				m_pHeaderFields.Remove(sIP_HeaderField);
				i--;
			}
		}
	}

	public void Clear()
	{
		m_pHeaderFields.Clear();
	}

	public bool Contains(string fieldName)
	{
		foreach (SIP_HeaderField pHeaderField in m_pHeaderFields)
		{
			if (pHeaderField.Name.ToLower() == fieldName.ToLower())
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(SIP_HeaderField headerField)
	{
		return m_pHeaderFields.Contains(headerField);
	}

	public SIP_HeaderField GetFirst(string fieldName)
	{
		foreach (SIP_HeaderField pHeaderField in m_pHeaderFields)
		{
			if (pHeaderField.Name.ToLower() == fieldName.ToLower())
			{
				return pHeaderField;
			}
		}
		return null;
	}

	public SIP_HeaderField[] Get(string fieldName)
	{
		List<SIP_HeaderField> list = new List<SIP_HeaderField>();
		foreach (SIP_HeaderField pHeaderField in m_pHeaderFields)
		{
			if (pHeaderField.Name.ToLower() == fieldName.ToLower())
			{
				list.Add(pHeaderField);
			}
		}
		return list.ToArray();
	}

	public void Parse(string headerString)
	{
		Parse(new MemoryStream(Encoding.Default.GetBytes(headerString)));
	}

	public void Parse(Stream stream)
	{
		m_pHeaderFields.Clear();
		StreamLineReader streamLineReader = new StreamLineReader(stream);
		streamLineReader.CRLF_LinesOnly = false;
		string text = streamLineReader.ReadLineString();
		while (text != null && !(text == ""))
		{
			string text2 = text;
			text = streamLineReader.ReadLineString();
			while (text != null && (text.StartsWith("\t") || text.StartsWith(" ")))
			{
				text2 += text;
				text = streamLineReader.ReadLineString();
			}
			string[] array = text2.Split(new char[1] { ':' }, 2);
			if (array.Length == 2)
			{
				Add(array[0] + ":", array[1].Trim());
			}
		}
	}

	public string ToHeaderString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				SIP_HeaderField sIP_HeaderField = (SIP_HeaderField)enumerator.Current;
				stringBuilder.Append(sIP_HeaderField.Name + " " + sIP_HeaderField.Value + "\r\n");
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}

	private SIP_HeaderField GetheaderField(string name, string value)
	{
		name = name.Replace(":", "").Trim();
		if (string.Equals(name, "i", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Call-ID";
		}
		else if (string.Equals(name, "m", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Contact";
		}
		else if (string.Equals(name, "e", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Content-Encoding";
		}
		else if (string.Equals(name, "l", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Content-Length";
		}
		else if (string.Equals(name, "c", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Content-Type";
		}
		else if (string.Equals(name, "f", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "From";
		}
		else if (string.Equals(name, "s", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Subject";
		}
		else if (string.Equals(name, "k", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Supported";
		}
		else if (string.Equals(name, "t", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "To";
		}
		else if (string.Equals(name, "v", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Via";
		}
		else if (string.Equals(name, "u", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "AllowEevents";
		}
		else if (string.Equals(name, "r", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Refer-To";
		}
		else if (string.Equals(name, "d", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Request-Disposition";
		}
		else if (string.Equals(name, "x", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Session-Expires";
		}
		else if (string.Equals(name, "o", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Event";
		}
		else if (string.Equals(name, "b", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Referred-By";
		}
		else if (string.Equals(name, "a", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Accept-Contact";
		}
		else if (string.Equals(name, "y", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Identity";
		}
		else if (string.Equals(name, "n", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Identity-Info";
		}
		else if (string.Equals(name, "j", StringComparison.InvariantCultureIgnoreCase))
		{
			name = "Reject-Contact";
		}
		if (string.Equals(name, "accept", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AcceptRange>("Accept:", value);
		}
		if (string.Equals(name, "accept-contact", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_ACValue>("Accept-Contact:", value);
		}
		if (string.Equals(name, "accept-encoding", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_Encoding>("Accept-Encoding:", value);
		}
		if (string.Equals(name, "accept-language", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_Language>("Accept-Language:", value);
		}
		if (string.Equals(name, "accept-resource-priority", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_RValue>("Accept-Resource-Priority:", value);
		}
		if (string.Equals(name, "alert-info", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AlertParam>("Alert-Info:", value);
		}
		if (string.Equals(name, "allow", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_Method>("Allow:", value);
		}
		if (string.Equals(name, "allow-events", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_EventType>("Allow-Events:", value);
		}
		if (string.Equals(name, "authentication-info", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_AuthenticationInfo>("Authentication-Info:", new SIP_t_AuthenticationInfo(value));
		}
		if (string.Equals(name, "authorization", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Credentials>("Authorization:", new SIP_t_Credentials(value));
		}
		if (string.Equals(name, "contact", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_ContactParam>("Contact:", value);
		}
		if (string.Equals(name, "Content-Disposition", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_ContentDisposition>("Content-Disposition:", new SIP_t_ContentDisposition(value));
		}
		if (string.Equals(name, "cseq", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_CSeq>("CSeq:", new SIP_t_CSeq(value));
		}
		if (string.Equals(name, "content-encoding", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_ContentCoding>("Content-Encoding:", value);
		}
		if (string.Equals(name, "content-language", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_LanguageTag>("Content-Language:", value);
		}
		if (string.Equals(name, "error-info", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_ErrorUri>("Error-Info:", value);
		}
		if (string.Equals(name, "event", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Event>("Event:", new SIP_t_Event(value));
		}
		if (string.Equals(name, "from", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_From>("From:", new SIP_t_From(value));
		}
		if (string.Equals(name, "history-info", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_HiEntry>("History-Info:", value);
		}
		if (string.Equals(name, "identity-info", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_IdentityInfo>("Identity-Info:", new SIP_t_IdentityInfo(value));
		}
		if (string.Equals(name, "in-replay-to", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_CallID>("In-Reply-To:", value);
		}
		if (string.Equals(name, "join", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Join>("Join:", new SIP_t_Join(value));
		}
		if (string.Equals(name, "min-se", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_MinSE>("Min-SE:", new SIP_t_MinSE(value));
		}
		if (string.Equals(name, "path", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AddressParam>("Path:", value);
		}
		if (string.Equals(name, "proxy-authenticate", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Challenge>("Proxy-Authenticate:", new SIP_t_Challenge(value));
		}
		if (string.Equals(name, "proxy-authorization", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Credentials>("Proxy-Authorization:", new SIP_t_Credentials(value));
		}
		if (string.Equals(name, "proxy-require", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_OptionTag>("Proxy-Require:", value);
		}
		if (string.Equals(name, "rack", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_RAck>("RAck:", new SIP_t_RAck(value));
		}
		if (string.Equals(name, "reason", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_ReasonValue>("Reason:", value);
		}
		if (string.Equals(name, "record-route", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AddressParam>("Record-Route:", value);
		}
		if (string.Equals(name, "refer-sub", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_ReferSub>("Refer-Sub:", new SIP_t_ReferSub(value));
		}
		if (string.Equals(name, "refer-to", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_AddressParam>("Refer-To:", new SIP_t_AddressParam(value));
		}
		if (string.Equals(name, "referred-by", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_ReferredBy>("Referred-By:", new SIP_t_ReferredBy(value));
		}
		if (string.Equals(name, "reject-contact", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_RCValue>("Reject-Contact:", value);
		}
		if (string.Equals(name, "replaces", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_SessionExpires>("Replaces:", new SIP_t_SessionExpires(value));
		}
		if (string.Equals(name, "reply-to", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AddressParam>("Reply-To:", value);
		}
		if (string.Equals(name, "request-disposition", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_Directive>("Request-Disposition:", value);
		}
		if (string.Equals(name, "require", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_OptionTag>("Require:", value);
		}
		if (string.Equals(name, "resource-priority", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_RValue>("Resource-Priority:", value);
		}
		if (string.Equals(name, "retry-after", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_RetryAfter>("Retry-After:", new SIP_t_RetryAfter(value));
		}
		if (string.Equals(name, "route", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AddressParam>("Route:", value);
		}
		if (string.Equals(name, "security-client", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_SecMechanism>("Security-Client:", value);
		}
		if (string.Equals(name, "security-server", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_SecMechanism>("Security-Server:", value);
		}
		if (string.Equals(name, "security-verify", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_SecMechanism>("Security-Verify:", value);
		}
		if (string.Equals(name, "service-route", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_AddressParam>("Service-Route:", value);
		}
		if (string.Equals(name, "session-expires", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_SessionExpires>("Session-Expires:", new SIP_t_SessionExpires(value));
		}
		if (string.Equals(name, "subscription-state", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_SubscriptionState>("Subscription-State:", new SIP_t_SubscriptionState(value));
		}
		if (string.Equals(name, "supported", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_OptionTag>("Supported:", value);
		}
		if (string.Equals(name, "target-dialog", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_TargetDialog>("Target-Dialog:", new SIP_t_TargetDialog(value));
		}
		if (string.Equals(name, "timestamp", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Timestamp>("Timestamp:", new SIP_t_Timestamp(value));
		}
		if (string.Equals(name, "to", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_To>("To:", new SIP_t_To(value));
		}
		if (string.Equals(name, "unsupported", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_OptionTag>("Unsupported:", value);
		}
		if (string.Equals(name, "via", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_ViaParm>("Via:", value);
		}
		if (string.Equals(name, "warning", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_MultiValueHF<SIP_t_WarningValue>("Warning:", value);
		}
		if (string.Equals(name, "www-authenticate", StringComparison.InvariantCultureIgnoreCase))
		{
			return new SIP_SingleValueHF<SIP_t_Challenge>("WWW-Authenticate:", new SIP_t_Challenge(value));
		}
		return new SIP_HeaderField(name + ":", value);
	}

	public IEnumerator GetEnumerator()
	{
		return m_pHeaderFields.GetEnumerator();
	}
}
