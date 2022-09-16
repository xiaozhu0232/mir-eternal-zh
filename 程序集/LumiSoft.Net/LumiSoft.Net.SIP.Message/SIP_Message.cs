using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public abstract class SIP_Message
{
	private SIP_HeaderFieldCollection m_pHeader;

	private byte[] m_Data;

	public SIP_HeaderFieldCollection Header => m_pHeader;

	public SIP_MVGroupHFCollection<SIP_t_AcceptRange> Accept => new SIP_MVGroupHFCollection<SIP_t_AcceptRange>(this, "Accept:");

	public SIP_MVGroupHFCollection<SIP_t_ACValue> AcceptContact => new SIP_MVGroupHFCollection<SIP_t_ACValue>(this, "Accept-Contact:");

	public SIP_MVGroupHFCollection<SIP_t_Encoding> AcceptEncoding => new SIP_MVGroupHFCollection<SIP_t_Encoding>(this, "Accept-Encoding:");

	public SIP_MVGroupHFCollection<SIP_t_Language> AcceptLanguage => new SIP_MVGroupHFCollection<SIP_t_Language>(this, "Accept-Language:");

	public SIP_MVGroupHFCollection<SIP_t_RValue> AcceptResourcePriority => new SIP_MVGroupHFCollection<SIP_t_RValue>(this, "Accept-Resource-Priority:");

	public SIP_MVGroupHFCollection<SIP_t_AlertParam> AlertInfo => new SIP_MVGroupHFCollection<SIP_t_AlertParam>(this, "Alert-Info:");

	public SIP_MVGroupHFCollection<SIP_t_Method> Allow => new SIP_MVGroupHFCollection<SIP_t_Method>(this, "Allow:");

	public SIP_MVGroupHFCollection<SIP_t_EventType> AllowEvents => new SIP_MVGroupHFCollection<SIP_t_EventType>(this, "Allow-Events:");

	public SIP_SVGroupHFCollection<SIP_t_AuthenticationInfo> AuthenticationInfo => new SIP_SVGroupHFCollection<SIP_t_AuthenticationInfo>(this, "Authentication-Info:");

	public SIP_SVGroupHFCollection<SIP_t_Credentials> Authorization => new SIP_SVGroupHFCollection<SIP_t_Credentials>(this, "Authorization:");

	public string CallID
	{
		get
		{
			return m_pHeader.GetFirst("Call-ID:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Call-ID:");
			}
			else
			{
				m_pHeader.Set("Call-ID:", value);
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_Info> CallInfo => new SIP_MVGroupHFCollection<SIP_t_Info>(this, "Call-Info:");

	public SIP_MVGroupHFCollection<SIP_t_ContactParam> Contact => new SIP_MVGroupHFCollection<SIP_t_ContactParam>(this, "Contact:");

	public SIP_t_ContentDisposition ContentDisposition
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Content-Disposition:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_ContentDisposition>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Content-Disposition:");
			}
			else
			{
				m_pHeader.Set("Content-Disposition:", value.ToStringValue());
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_ContentCoding> ContentEncoding => new SIP_MVGroupHFCollection<SIP_t_ContentCoding>(this, "Content-Encoding:");

	public SIP_MVGroupHFCollection<SIP_t_LanguageTag> ContentLanguage => new SIP_MVGroupHFCollection<SIP_t_LanguageTag>(this, "Content-Language:");

	public int ContentLength
	{
		get
		{
			if (m_Data == null)
			{
				return 0;
			}
			return m_Data.Length;
		}
	}

	public string ContentType
	{
		get
		{
			return m_pHeader.GetFirst("Content-Type:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Content-Type:");
			}
			else
			{
				m_pHeader.Set("Content-Type:", value);
			}
		}
	}

	public SIP_t_CSeq CSeq
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("CSeq:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_CSeq>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("CSeq:");
			}
			else
			{
				m_pHeader.Set("CSeq:", value.ToStringValue());
			}
		}
	}

	public DateTime Date
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Date:");
			if (first != null)
			{
				return DateTime.ParseExact(first.Value, "r", DateTimeFormatInfo.InvariantInfo);
			}
			return DateTime.MinValue;
		}
		set
		{
			if (value == DateTime.MinValue)
			{
				m_pHeader.RemoveFirst("Date:");
			}
			else
			{
				m_pHeader.Set("Date:", value.ToString("r"));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_ErrorUri> ErrorInfo => new SIP_MVGroupHFCollection<SIP_t_ErrorUri>(this, "Error-Info:");

	public SIP_t_Event Event
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Event:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_Event>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Event:");
			}
			else
			{
				m_pHeader.Set("Event:", value.ToStringValue());
			}
		}
	}

	public int Expires
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Expires:");
			if (first != null)
			{
				return Convert.ToInt32(first.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				m_pHeader.RemoveFirst("Expires:");
			}
			else
			{
				m_pHeader.Set("Expires:", value.ToString());
			}
		}
	}

	public SIP_t_From From
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("From:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_From>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("From:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_From>("From:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_HiEntry> HistoryInfo => new SIP_MVGroupHFCollection<SIP_t_HiEntry>(this, "History-Info:");

	public string Identity
	{
		get
		{
			return m_pHeader.GetFirst("Identity:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Identity:");
			}
			else
			{
				m_pHeader.Set("Identity:", value);
			}
		}
	}

	public SIP_t_IdentityInfo IdentityInfo
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Identity-Info:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_IdentityInfo>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Identity-Info:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_IdentityInfo>("Identity-Info:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_CallID> InReplyTo => new SIP_MVGroupHFCollection<SIP_t_CallID>(this, "In-Reply-To:");

	public SIP_t_Join Join
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Join:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_Join>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Join:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_Join>("Join:", value));
			}
		}
	}

	public int MaxForwards
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Max-Forwards:");
			if (first != null)
			{
				return Convert.ToInt32(first.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				m_pHeader.RemoveFirst("Max-Forwards:");
			}
			else
			{
				m_pHeader.Set("Max-Forwards:", value.ToString());
			}
		}
	}

	public string MimeVersion
	{
		get
		{
			return m_pHeader.GetFirst("Mime-Version:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Mime-Version:");
			}
			else
			{
				m_pHeader.Set("Mime-Version:", value);
			}
		}
	}

	public int MinExpires
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Min-Expires:");
			if (first != null)
			{
				return Convert.ToInt32(first.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				m_pHeader.RemoveFirst("Min-Expires:");
			}
			else
			{
				m_pHeader.Set("Min-Expires:", value.ToString());
			}
		}
	}

	public SIP_t_MinSE MinSE
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Min-SE:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_MinSE>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Min-SE:");
			}
			else
			{
				m_pHeader.Set("Min-SE:", value.ToStringValue());
			}
		}
	}

	public string Organization
	{
		get
		{
			return m_pHeader.GetFirst("Organization:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Organization:");
			}
			else
			{
				m_pHeader.Set("Organization:", value);
			}
		}
	}

	public SIP_SVGroupHFCollection<SIP_t_AddressParam> Path => new SIP_SVGroupHFCollection<SIP_t_AddressParam>(this, "Path:");

	public string Priority
	{
		get
		{
			return m_pHeader.GetFirst("Priority:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Priority:");
			}
			else
			{
				m_pHeader.Set("Priority:", value);
			}
		}
	}

	public SIP_SVGroupHFCollection<SIP_t_Challenge> ProxyAuthenticate => new SIP_SVGroupHFCollection<SIP_t_Challenge>(this, "Proxy-Authenticate:");

	public SIP_SVGroupHFCollection<SIP_t_Credentials> ProxyAuthorization => new SIP_SVGroupHFCollection<SIP_t_Credentials>(this, "Proxy-Authorization:");

	public SIP_MVGroupHFCollection<SIP_t_OptionTag> ProxyRequire => new SIP_MVGroupHFCollection<SIP_t_OptionTag>(this, "Proxy-Require:");

	public SIP_t_RAck RAck
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("RAck:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_RAck>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("RAck:");
			}
			else
			{
				m_pHeader.Set("RAck:", value.ToStringValue());
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_ReasonValue> Reason => new SIP_MVGroupHFCollection<SIP_t_ReasonValue>(this, "Reason:");

	public SIP_MVGroupHFCollection<SIP_t_AddressParam> RecordRoute => new SIP_MVGroupHFCollection<SIP_t_AddressParam>(this, "Record-Route:");

	public SIP_t_ReferSub ReferSub
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Refer-Sub:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_ReferSub>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Refer-Sub:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_ReferSub>("Refer-Sub:", value));
			}
		}
	}

	public SIP_t_AddressParam ReferTo
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Refer-To:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_AddressParam>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Refer-To:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_AddressParam>("Refer-To:", value));
			}
		}
	}

	public SIP_t_ReferredBy ReferredBy
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Referred-By:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_ReferredBy>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Referred-By:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_ReferredBy>("Referred-By:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_RCValue> RejectContact => new SIP_MVGroupHFCollection<SIP_t_RCValue>(this, "Reject-Contact:");

	public SIP_t_Replaces Replaces
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Replaces:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_Replaces>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Replaces:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_Replaces>("Replaces:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_AddressParam> ReplyTo => new SIP_MVGroupHFCollection<SIP_t_AddressParam>(this, "Reply-To:");

	public SIP_MVGroupHFCollection<SIP_t_Directive> RequestDisposition => new SIP_MVGroupHFCollection<SIP_t_Directive>(this, "Request-Disposition:");

	public SIP_MVGroupHFCollection<SIP_t_OptionTag> Require => new SIP_MVGroupHFCollection<SIP_t_OptionTag>(this, "Require:");

	public SIP_MVGroupHFCollection<SIP_t_RValue> ResourcePriority => new SIP_MVGroupHFCollection<SIP_t_RValue>(this, "Resource-Priority:");

	public SIP_t_RetryAfter RetryAfter
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Retry-After:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_RetryAfter>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Retry-After:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_RetryAfter>("Retry-After:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_AddressParam> Route => new SIP_MVGroupHFCollection<SIP_t_AddressParam>(this, "Route:");

	public int RSeq
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("RSeq:");
			if (first != null)
			{
				return Convert.ToInt32(first.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				m_pHeader.RemoveFirst("RSeq:");
			}
			else
			{
				m_pHeader.Set("RSeq:", value.ToString());
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_SecMechanism> SecurityClient => new SIP_MVGroupHFCollection<SIP_t_SecMechanism>(this, "Security-Client:");

	public SIP_MVGroupHFCollection<SIP_t_SecMechanism> SecurityServer => new SIP_MVGroupHFCollection<SIP_t_SecMechanism>(this, "Security-Server:");

	public SIP_MVGroupHFCollection<SIP_t_SecMechanism> SecurityVerify => new SIP_MVGroupHFCollection<SIP_t_SecMechanism>(this, "Security-Verify:");

	public string Server
	{
		get
		{
			return m_pHeader.GetFirst("Server:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Server:");
			}
			else
			{
				m_pHeader.Set("Server:", value);
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_AddressParam> ServiceRoute => new SIP_MVGroupHFCollection<SIP_t_AddressParam>(this, "Service-Route:");

	public SIP_t_SessionExpires SessionExpires
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Session-Expires:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_SessionExpires>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Session-Expires:");
			}
			else
			{
				m_pHeader.Set("Session-Expires:", value.ToStringValue());
			}
		}
	}

	public string SIPETag
	{
		get
		{
			return m_pHeader.GetFirst("SIP-ETag:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("SIP-ETag:");
			}
			else
			{
				m_pHeader.Set("SIP-ETag:", value);
			}
		}
	}

	public string SIPIfMatch
	{
		get
		{
			return m_pHeader.GetFirst("SIP-If-Match:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("SIP-If-Match:");
			}
			else
			{
				m_pHeader.Set("SIP-If-Match:", value);
			}
		}
	}

	public string Subject
	{
		get
		{
			return m_pHeader.GetFirst("Subject:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Subject:");
			}
			else
			{
				m_pHeader.Set("Subject:", value);
			}
		}
	}

	public SIP_t_SubscriptionState SubscriptionState
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Subscription-State:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_SubscriptionState>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Subscription-State:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_SubscriptionState>("Subscription-State:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_OptionTag> Supported => new SIP_MVGroupHFCollection<SIP_t_OptionTag>(this, "Supported:");

	public SIP_t_TargetDialog TargetDialog
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Target-Dialog:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_TargetDialog>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Target-Dialog:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_TargetDialog>("Target-Dialog:", value));
			}
		}
	}

	public SIP_t_Timestamp Timestamp
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("Timestamp:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_Timestamp>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("Timestamp:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_Timestamp>("Timestamp:", value));
			}
		}
	}

	public SIP_t_To To
	{
		get
		{
			SIP_HeaderField first = m_pHeader.GetFirst("To:");
			if (first != null)
			{
				return ((SIP_SingleValueHF<SIP_t_To>)first).ValueX;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("To:");
			}
			else
			{
				m_pHeader.Add(new SIP_SingleValueHF<SIP_t_To>("To:", value));
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_OptionTag> Unsupported => new SIP_MVGroupHFCollection<SIP_t_OptionTag>(this, "Unsupported:");

	public string UserAgent
	{
		get
		{
			return m_pHeader.GetFirst("User-Agent:")?.Value;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.RemoveFirst("User-Agent:");
			}
			else
			{
				m_pHeader.Set("User-Agent:", value);
			}
		}
	}

	public SIP_MVGroupHFCollection<SIP_t_ViaParm> Via => new SIP_MVGroupHFCollection<SIP_t_ViaParm>(this, "Via:");

	public SIP_MVGroupHFCollection<SIP_t_WarningValue> Warning => new SIP_MVGroupHFCollection<SIP_t_WarningValue>(this, "Warning:");

	public SIP_SVGroupHFCollection<SIP_t_Challenge> WWWAuthenticate => new SIP_SVGroupHFCollection<SIP_t_Challenge>(this, "WWW-Authenticate:");

	public byte[] Data
	{
		get
		{
			return m_Data;
		}
		set
		{
			m_Data = value;
		}
	}

	public SIP_Message()
	{
		m_pHeader = new SIP_HeaderFieldCollection();
	}

	protected void InternalParse(byte[] data)
	{
		InternalParse(new MemoryStream(data));
	}

	protected void InternalParse(Stream stream)
	{
		Header.Parse(stream);
		int num = 0;
		try
		{
			num = Convert.ToInt32(m_pHeader.GetFirst("Content-Length:").Value);
		}
		catch
		{
		}
		if (num > 0)
		{
			byte[] array = new byte[num];
			stream.Read(array, 0, array.Length);
			Data = array;
		}
	}

	protected void InternalToStream(Stream stream)
	{
		m_pHeader.RemoveAll("Content-Length:");
		if (m_Data != null)
		{
			m_pHeader.Add("Content-Length:", Convert.ToString(m_Data.Length));
		}
		else
		{
			m_pHeader.Add("Content-Length:", Convert.ToString(0));
		}
		byte[] bytes = Encoding.UTF8.GetBytes(m_pHeader.ToHeaderString());
		stream.Write(bytes, 0, bytes.Length);
		if (m_Data != null && m_Data.Length != 0)
		{
			stream.Write(m_Data, 0, m_Data.Length);
		}
	}
}
