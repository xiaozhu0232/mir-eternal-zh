using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Crmf;

public interface IControl
{
	DerObjectIdentifier Type { get; }

	Asn1Encodable Value { get; }
}
