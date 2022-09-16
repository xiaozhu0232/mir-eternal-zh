using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Icao;

public class LdsSecurityObject : Asn1Encodable
{
	public const int UBDataGroups = 16;

	private DerInteger version = new DerInteger(0);

	private AlgorithmIdentifier digestAlgorithmIdentifier;

	private DataGroupHash[] datagroupHash;

	private LdsVersionInfo versionInfo;

	public BigInteger Version => version.Value;

	public AlgorithmIdentifier DigestAlgorithmIdentifier => digestAlgorithmIdentifier;

	public LdsVersionInfo VersionInfo => versionInfo;

	public static LdsSecurityObject GetInstance(object obj)
	{
		if (obj is LdsSecurityObject)
		{
			return (LdsSecurityObject)obj;
		}
		if (obj != null)
		{
			return new LdsSecurityObject(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	private LdsSecurityObject(Asn1Sequence seq)
	{
		if (seq == null || seq.Count == 0)
		{
			throw new ArgumentException("null or empty sequence passed.");
		}
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		version = DerInteger.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		digestAlgorithmIdentifier = AlgorithmIdentifier.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		Asn1Sequence instance = Asn1Sequence.GetInstance(enumerator.Current);
		if (version.Value.Equals(BigInteger.One))
		{
			enumerator.MoveNext();
			versionInfo = LdsVersionInfo.GetInstance(enumerator.Current);
		}
		CheckDatagroupHashSeqSize(instance.Count);
		datagroupHash = new DataGroupHash[instance.Count];
		for (int i = 0; i < instance.Count; i++)
		{
			datagroupHash[i] = DataGroupHash.GetInstance(instance[i]);
		}
	}

	public LdsSecurityObject(AlgorithmIdentifier digestAlgorithmIdentifier, DataGroupHash[] datagroupHash)
	{
		version = new DerInteger(0);
		this.digestAlgorithmIdentifier = digestAlgorithmIdentifier;
		this.datagroupHash = datagroupHash;
		CheckDatagroupHashSeqSize(datagroupHash.Length);
	}

	public LdsSecurityObject(AlgorithmIdentifier digestAlgorithmIdentifier, DataGroupHash[] datagroupHash, LdsVersionInfo versionInfo)
	{
		version = new DerInteger(1);
		this.digestAlgorithmIdentifier = digestAlgorithmIdentifier;
		this.datagroupHash = datagroupHash;
		this.versionInfo = versionInfo;
		CheckDatagroupHashSeqSize(datagroupHash.Length);
	}

	private void CheckDatagroupHashSeqSize(int size)
	{
		if (size < 2 || size > 16)
		{
			throw new ArgumentException("wrong size in DataGroupHashValues : not in (2.." + 16 + ")");
		}
	}

	public DataGroupHash[] GetDatagroupHash()
	{
		return datagroupHash;
	}

	public override Asn1Object ToAsn1Object()
	{
		DerSequence derSequence = new DerSequence(datagroupHash);
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, digestAlgorithmIdentifier, derSequence);
		asn1EncodableVector.AddOptional(versionInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
