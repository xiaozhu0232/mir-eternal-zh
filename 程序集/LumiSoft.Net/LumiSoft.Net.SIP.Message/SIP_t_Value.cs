namespace LumiSoft.Net.SIP.Message;

public abstract class SIP_t_Value
{
	public SIP_t_Value()
	{
	}

	public abstract void Parse(StringReader reader);

	public abstract string ToStringValue();
}
