using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class SimulatedTlsSrpIdentityManager : TlsSrpIdentityManager
{
	private static readonly byte[] PREFIX_PASSWORD = Strings.ToByteArray("password");

	private static readonly byte[] PREFIX_SALT = Strings.ToByteArray("salt");

	protected readonly Srp6GroupParameters mGroup;

	protected readonly Srp6VerifierGenerator mVerifierGenerator;

	protected readonly IMac mMac;

	public static SimulatedTlsSrpIdentityManager GetRfc5054Default(Srp6GroupParameters group, byte[] seedKey)
	{
		Srp6VerifierGenerator srp6VerifierGenerator = new Srp6VerifierGenerator();
		srp6VerifierGenerator.Init(group, TlsUtilities.CreateHash(2));
		HMac hMac = new HMac(TlsUtilities.CreateHash(2));
		hMac.Init(new KeyParameter(seedKey));
		return new SimulatedTlsSrpIdentityManager(group, srp6VerifierGenerator, hMac);
	}

	public SimulatedTlsSrpIdentityManager(Srp6GroupParameters group, Srp6VerifierGenerator verifierGenerator, IMac mac)
	{
		mGroup = group;
		mVerifierGenerator = verifierGenerator;
		mMac = mac;
	}

	public virtual TlsSrpLoginParameters GetLoginParameters(byte[] identity)
	{
		mMac.BlockUpdate(PREFIX_SALT, 0, PREFIX_SALT.Length);
		mMac.BlockUpdate(identity, 0, identity.Length);
		byte[] array = new byte[mMac.GetMacSize()];
		mMac.DoFinal(array, 0);
		mMac.BlockUpdate(PREFIX_PASSWORD, 0, PREFIX_PASSWORD.Length);
		mMac.BlockUpdate(identity, 0, identity.Length);
		byte[] array2 = new byte[mMac.GetMacSize()];
		mMac.DoFinal(array2, 0);
		BigInteger verifier = mVerifierGenerator.GenerateVerifier(array, identity, array2);
		return new TlsSrpLoginParameters(mGroup, verifier, array);
	}
}
