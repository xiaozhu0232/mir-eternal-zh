namespace Org.BouncyCastle.Crypto.Tls;

public class TlsFatalAlertReceived : TlsException
{
	private readonly byte alertDescription;

	public virtual byte AlertDescription => alertDescription;

	public TlsFatalAlertReceived(byte alertDescription)
		: base(Org.BouncyCastle.Crypto.Tls.AlertDescription.GetText(alertDescription), null)
	{
		this.alertDescription = alertDescription;
	}
}
