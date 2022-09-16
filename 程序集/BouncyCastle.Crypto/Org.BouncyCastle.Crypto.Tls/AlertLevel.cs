namespace Org.BouncyCastle.Crypto.Tls;

public abstract class AlertLevel
{
	public const byte warning = 1;

	public const byte fatal = 2;

	public static string GetName(byte alertDescription)
	{
		return alertDescription switch
		{
			1 => "warning", 
			2 => "fatal", 
			_ => "UNKNOWN", 
		};
	}

	public static string GetText(byte alertDescription)
	{
		return GetName(alertDescription) + "(" + alertDescription + ")";
	}
}
