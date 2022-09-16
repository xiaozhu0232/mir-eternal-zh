using System;

namespace LumiSoft.Net;

public class EventArgs<T> : EventArgs
{
	private T m_pValue;

	public T Value => m_pValue;

	public EventArgs(T value)
	{
		m_pValue = value;
	}
}
