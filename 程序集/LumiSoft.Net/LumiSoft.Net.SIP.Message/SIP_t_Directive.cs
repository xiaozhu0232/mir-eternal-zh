using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Directive : SIP_t_Value
{
	public enum DirectiveType
	{
		Proxy,
		Redirect,
		Cancel,
		NoCancel,
		Fork,
		NoFork,
		Recurse,
		NoRecurse,
		Parallel,
		Sequential,
		Queue,
		NoQueue
	}

	private DirectiveType m_Directive = DirectiveType.Fork;

	public DirectiveType Directive
	{
		get
		{
			return m_Directive;
		}
		set
		{
			m_Directive = value;
		}
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Parse(new StringReader(value));
	}

	public override void Parse(StringReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("'directive' value is missing !");
		}
		if (text.ToLower() == "proxy")
		{
			m_Directive = DirectiveType.Proxy;
			return;
		}
		if (text.ToLower() == "redirect")
		{
			m_Directive = DirectiveType.Redirect;
			return;
		}
		if (text.ToLower() == "cancel")
		{
			m_Directive = DirectiveType.Cancel;
			return;
		}
		if (text.ToLower() == "no-cancel")
		{
			m_Directive = DirectiveType.NoCancel;
			return;
		}
		if (text.ToLower() == "fork")
		{
			m_Directive = DirectiveType.Fork;
			return;
		}
		if (text.ToLower() == "no-fork")
		{
			m_Directive = DirectiveType.NoFork;
			return;
		}
		if (text.ToLower() == "recurse")
		{
			m_Directive = DirectiveType.Recurse;
			return;
		}
		if (text.ToLower() == "no-recurse")
		{
			m_Directive = DirectiveType.NoRecurse;
			return;
		}
		if (text.ToLower() == "parallel")
		{
			m_Directive = DirectiveType.Parallel;
			return;
		}
		if (text.ToLower() == "sequential")
		{
			m_Directive = DirectiveType.Sequential;
			return;
		}
		if (text.ToLower() == "queue")
		{
			m_Directive = DirectiveType.Queue;
			return;
		}
		if (text.ToLower() == "no-queue")
		{
			m_Directive = DirectiveType.NoQueue;
			return;
		}
		throw new SIP_ParseException("Invalid 'directive' value !");
	}

	public override string ToStringValue()
	{
		if (m_Directive == DirectiveType.Proxy)
		{
			return "proxy";
		}
		if (m_Directive == DirectiveType.Redirect)
		{
			return "redirect";
		}
		if (m_Directive == DirectiveType.Cancel)
		{
			return "cancel";
		}
		if (m_Directive == DirectiveType.NoCancel)
		{
			return "no-cancel";
		}
		if (m_Directive == DirectiveType.Fork)
		{
			return "fork";
		}
		if (m_Directive == DirectiveType.NoFork)
		{
			return "no-fork";
		}
		if (m_Directive == DirectiveType.Recurse)
		{
			return "recurse";
		}
		if (m_Directive == DirectiveType.NoRecurse)
		{
			return "no-recurse";
		}
		if (m_Directive == DirectiveType.Parallel)
		{
			return "parallel";
		}
		if (m_Directive == DirectiveType.Sequential)
		{
			return "sequential";
		}
		if (m_Directive == DirectiveType.Queue)
		{
			return "queue";
		}
		if (m_Directive == DirectiveType.NoQueue)
		{
			return "no-queue";
		}
		throw new ArgumentException("Invalid property Directive value, this should never happen !");
	}
}
