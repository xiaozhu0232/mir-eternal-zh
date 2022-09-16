using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;

namespace Org.BouncyCastle.Cmp;

public class GeneralPkiMessage
{
	private readonly PkiMessage pkiMessage;

	public PkiHeader Header => pkiMessage.Header;

	public PkiBody Body => pkiMessage.Body;

	public bool HasProtection => pkiMessage.Protection != null;

	private static PkiMessage ParseBytes(byte[] encoding)
	{
		return PkiMessage.GetInstance(Asn1Object.FromByteArray(encoding));
	}

	public GeneralPkiMessage(PkiMessage pkiMessage)
	{
		this.pkiMessage = pkiMessage;
	}

	public GeneralPkiMessage(byte[] encoding)
		: this(ParseBytes(encoding))
	{
	}

	public PkiMessage ToAsn1Structure()
	{
		return pkiMessage;
	}
}
