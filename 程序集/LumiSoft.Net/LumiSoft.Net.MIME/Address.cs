using System;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public abstract class Address
{
	private bool m_GroupAddress;

	private object m_pOwner;

	public bool IsGroupAddress => m_GroupAddress;

	internal object Owner
	{
		get
		{
			return m_pOwner;
		}
		set
		{
			m_pOwner = value;
		}
	}

	public Address(bool groupAddress)
	{
		m_GroupAddress = groupAddress;
	}
}
