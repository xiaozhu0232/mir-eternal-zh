using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crmf;

internal class PKMacFactory : IMacFactory
{
	protected readonly PbmParameter parameters;

	private readonly byte[] key;

	public virtual object AlgorithmDetails => new AlgorithmIdentifier(CmpObjectIdentifiers.passwordBasedMac, parameters);

	public PKMacFactory(byte[] key, PbmParameter parameters)
	{
		this.key = Arrays.Clone(key);
		this.parameters = parameters;
	}

	public virtual IStreamCalculator CreateCalculator()
	{
		IMac mac = MacUtilities.GetMac(parameters.Mac.Algorithm);
		mac.Init(new KeyParameter(key));
		return new PKMacStreamCalculator(mac);
	}
}
