namespace LumiSoft.Net.SIP.Proxy;

public class SIP_ProxyHandler
{
	private object m_pTag;

	public virtual bool IsReusable => false;

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	public virtual bool ProcessRequest(SIP_RequestContext requestContext)
	{
		if (requestContext.Request.RequestLine.Uri.Scheme.ToLower() != "tel" && !(requestContext.Request.RequestLine.Uri is SIP_Uri))
		{
			return false;
		}
		SIP_Uri sIP_Uri = (SIP_Uri)requestContext.Request.RequestLine.Uri;
		long result = 0L;
		if (sIP_Uri.User.StartsWith("+") || long.TryParse(sIP_Uri.User, out result))
		{
			if (requestContext.User == null)
			{
				requestContext.ChallengeRequest();
				return true;
			}
			requestContext.ProxyContext.Start();
			return true;
		}
		return false;
	}

	public bool IsLocalUri()
	{
		return false;
	}

	public void GetRegistrarContacts()
	{
	}
}
