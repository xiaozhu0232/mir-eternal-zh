using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Iana;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crmf;

public class PKMacBuilder
{
	private AlgorithmIdentifier owf;

	private AlgorithmIdentifier mac;

	private IPKMacPrimitivesProvider provider;

	private SecureRandom random;

	private PbmParameter parameters;

	private int iterationCount;

	private int saltLength = 20;

	private int maxIterations;

	public PKMacBuilder()
		: this(new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1), 1000, new AlgorithmIdentifier(IanaObjectIdentifiers.HmacSha1, DerNull.Instance), new DefaultPKMacPrimitivesProvider())
	{
	}

	public PKMacBuilder(IPKMacPrimitivesProvider provider)
		: this(new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1), 1000, new AlgorithmIdentifier(IanaObjectIdentifiers.HmacSha1, DerNull.Instance), provider)
	{
	}

	public PKMacBuilder(IPKMacPrimitivesProvider provider, AlgorithmIdentifier digestAlgorithmIdentifier, AlgorithmIdentifier macAlgorithmIdentifier)
		: this(digestAlgorithmIdentifier, 1000, macAlgorithmIdentifier, provider)
	{
	}

	public PKMacBuilder(IPKMacPrimitivesProvider provider, int maxIterations)
	{
		this.provider = provider;
		this.maxIterations = maxIterations;
	}

	private PKMacBuilder(AlgorithmIdentifier digestAlgorithmIdentifier, int iterationCount, AlgorithmIdentifier macAlgorithmIdentifier, IPKMacPrimitivesProvider provider)
	{
		this.iterationCount = iterationCount;
		mac = macAlgorithmIdentifier;
		owf = digestAlgorithmIdentifier;
		this.provider = provider;
	}

	public PKMacBuilder SetSaltLength(int saltLength)
	{
		if (saltLength < 8)
		{
			throw new ArgumentException("salt length must be at least 8 bytes");
		}
		this.saltLength = saltLength;
		return this;
	}

	public PKMacBuilder SetIterationCount(int iterationCount)
	{
		if (iterationCount < 100)
		{
			throw new ArgumentException("iteration count must be at least 100");
		}
		CheckIterationCountCeiling(iterationCount);
		this.iterationCount = iterationCount;
		return this;
	}

	public PKMacBuilder SetParameters(PbmParameter parameters)
	{
		CheckIterationCountCeiling(parameters.IterationCount.IntValueExact);
		this.parameters = parameters;
		return this;
	}

	public PKMacBuilder SetSecureRandom(SecureRandom random)
	{
		this.random = random;
		return this;
	}

	public IMacFactory Build(char[] password)
	{
		if (parameters != null)
		{
			return GenCalculator(parameters, password);
		}
		byte[] array = new byte[saltLength];
		if (random == null)
		{
			random = new SecureRandom();
		}
		random.NextBytes(array);
		return GenCalculator(new PbmParameter(array, owf, iterationCount, mac), password);
	}

	private void CheckIterationCountCeiling(int iterationCount)
	{
		if (maxIterations > 0 && iterationCount > maxIterations)
		{
			throw new ArgumentException("iteration count exceeds limit (" + iterationCount + " > " + maxIterations + ")");
		}
	}

	private IMacFactory GenCalculator(PbmParameter parameters, char[] password)
	{
		byte[] array = Strings.ToUtf8ByteArray(password);
		byte[] octets = parameters.Salt.GetOctets();
		byte[] array2 = new byte[array.Length + octets.Length];
		Array.Copy(array, 0, array2, 0, array.Length);
		Array.Copy(octets, 0, array2, array.Length, octets.Length);
		IDigest digest = provider.CreateDigest(parameters.Owf);
		int num = parameters.IterationCount.IntValueExact;
		digest.BlockUpdate(array2, 0, array2.Length);
		array2 = new byte[digest.GetDigestSize()];
		digest.DoFinal(array2, 0);
		while (--num > 0)
		{
			digest.BlockUpdate(array2, 0, array2.Length);
			digest.DoFinal(array2, 0);
		}
		byte[] key = array2;
		return new PKMacFactory(key, parameters);
	}
}
