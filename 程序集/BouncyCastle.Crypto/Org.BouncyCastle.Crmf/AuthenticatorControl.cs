using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Crmf;

namespace Org.BouncyCastle.Crmf;

public class AuthenticatorControl : IControl
{
	private static readonly DerObjectIdentifier type = CrmfObjectIdentifiers.id_regCtrl_authenticator;

	private readonly DerUtf8String token;

	public DerObjectIdentifier Type => type;

	public Asn1Encodable Value => token;

	public AuthenticatorControl(DerUtf8String token)
	{
		this.token = token;
	}

	public AuthenticatorControl(string token)
	{
		this.token = new DerUtf8String(token);
	}
}
