using System;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Tsp;

public class TimeStampTokenInfo
{
	private TstInfo tstInfo;

	private DateTime genTime;

	public bool IsOrdered => tstInfo.Ordering.IsTrue;

	public Accuracy Accuracy => tstInfo.Accuracy;

	public DateTime GenTime => genTime;

	public GenTimeAccuracy GenTimeAccuracy
	{
		get
		{
			if (Accuracy != null)
			{
				return new GenTimeAccuracy(Accuracy);
			}
			return null;
		}
	}

	public string Policy => tstInfo.Policy.Id;

	public BigInteger SerialNumber => tstInfo.SerialNumber.Value;

	public GeneralName Tsa => tstInfo.Tsa;

	public BigInteger Nonce
	{
		get
		{
			if (tstInfo.Nonce != null)
			{
				return tstInfo.Nonce.Value;
			}
			return null;
		}
	}

	public AlgorithmIdentifier HashAlgorithm => tstInfo.MessageImprint.HashAlgorithm;

	public string MessageImprintAlgOid => tstInfo.MessageImprint.HashAlgorithm.Algorithm.Id;

	public TstInfo TstInfo => tstInfo;

	public TimeStampTokenInfo(TstInfo tstInfo)
	{
		this.tstInfo = tstInfo;
		try
		{
			genTime = tstInfo.GenTime.ToDateTime();
		}
		catch (Exception ex)
		{
			throw new TspException("unable to parse genTime field: " + ex.Message);
		}
	}

	public byte[] GetMessageImprintDigest()
	{
		return tstInfo.MessageImprint.GetHashedMessage();
	}

	public byte[] GetEncoded()
	{
		return tstInfo.GetEncoded();
	}
}
