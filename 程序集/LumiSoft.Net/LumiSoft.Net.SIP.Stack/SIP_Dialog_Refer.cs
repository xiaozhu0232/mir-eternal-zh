using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Dialog_Refer : SIP_Dialog
{
	public event EventHandler<SIP_RequestReceivedEventArgs> Notify;

	internal SIP_Dialog_Refer()
	{
	}

	private void CreateNotify(string statusLine)
	{
	}

	protected internal override bool ProcessRequest(SIP_RequestReceivedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (base.ProcessRequest(e))
		{
			return true;
		}
		if (e.Request.RequestLine.Method == "NOTIFY")
		{
			OnNotify(e);
			return true;
		}
		return false;
	}

	private void OnNotify(SIP_RequestReceivedEventArgs e)
	{
		if (this.Notify != null)
		{
			this.Notify(this, e);
		}
	}
}
