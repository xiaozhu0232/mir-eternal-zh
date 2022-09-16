namespace Org.BouncyCastle.Asn1.Icao;

public abstract class IcaoObjectIdentifiers
{
	public static readonly DerObjectIdentifier IdIcao = new DerObjectIdentifier("2.23.136");

	public static readonly DerObjectIdentifier IdIcaoMrtd = IdIcao.Branch("1");

	public static readonly DerObjectIdentifier IdIcaoMrtdSecurity = IdIcaoMrtd.Branch("1");

	public static readonly DerObjectIdentifier IdIcaoLdsSecurityObject = IdIcaoMrtdSecurity.Branch("1");

	public static readonly DerObjectIdentifier IdIcaoCscaMasterList = IdIcaoMrtdSecurity.Branch("2");

	public static readonly DerObjectIdentifier IdIcaoCscaMasterListSigningKey = IdIcaoMrtdSecurity.Branch("3");

	public static readonly DerObjectIdentifier IdIcaoDocumentTypeList = IdIcaoMrtdSecurity.Branch("4");

	public static readonly DerObjectIdentifier IdIcaoAAProtocolObject = IdIcaoMrtdSecurity.Branch("5");

	public static readonly DerObjectIdentifier IdIcaoExtensions = IdIcaoMrtdSecurity.Branch("6");

	public static readonly DerObjectIdentifier IdIcaoExtensionsNamechangekeyrollover = IdIcaoExtensions.Branch("1");
}
