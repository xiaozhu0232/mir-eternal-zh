namespace LumiSoft.Net.AUTH;

public abstract class AUTH_SASL_Client
{
	public abstract bool IsCompleted { get; }

	public abstract string Name { get; }

	public abstract string UserName { get; }

	public virtual bool SupportsInitialResponse => false;

	public AUTH_SASL_Client()
	{
	}

	public abstract byte[] Continue(byte[] serverResponse);
}
