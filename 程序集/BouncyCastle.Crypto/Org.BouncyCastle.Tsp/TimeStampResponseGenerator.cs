using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Tsp;

public class TimeStampResponseGenerator
{
	private class FailInfo : DerBitString
	{
		internal FailInfo(int failInfoValue)
			: base(failInfoValue)
		{
		}
	}

	private PkiStatus status;

	private Asn1EncodableVector statusStrings;

	private int failInfo;

	private TimeStampTokenGenerator tokenGenerator;

	private IList acceptedAlgorithms;

	private IList acceptedPolicies;

	private IList acceptedExtensions;

	public TimeStampResponseGenerator(TimeStampTokenGenerator tokenGenerator, IList acceptedAlgorithms)
		: this(tokenGenerator, acceptedAlgorithms, null, null)
	{
	}

	public TimeStampResponseGenerator(TimeStampTokenGenerator tokenGenerator, IList acceptedAlgorithms, IList acceptedPolicy)
		: this(tokenGenerator, acceptedAlgorithms, acceptedPolicy, null)
	{
	}

	public TimeStampResponseGenerator(TimeStampTokenGenerator tokenGenerator, IList acceptedAlgorithms, IList acceptedPolicies, IList acceptedExtensions)
	{
		this.tokenGenerator = tokenGenerator;
		this.acceptedAlgorithms = acceptedAlgorithms;
		this.acceptedPolicies = acceptedPolicies;
		this.acceptedExtensions = acceptedExtensions;
		statusStrings = new Asn1EncodableVector();
	}

	private void AddStatusString(string statusString)
	{
		statusStrings.Add(new DerUtf8String(statusString));
	}

	private void SetFailInfoField(int field)
	{
		failInfo |= field;
	}

	private PkiStatusInfo GetPkiStatusInfo()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger((int)status));
		if (statusStrings.Count > 0)
		{
			asn1EncodableVector.Add(new PkiFreeText(new DerSequence(statusStrings)));
		}
		if (failInfo != 0)
		{
			asn1EncodableVector.Add(new FailInfo(failInfo));
		}
		return new PkiStatusInfo(new DerSequence(asn1EncodableVector));
	}

	public TimeStampResponse Generate(TimeStampRequest request, BigInteger serialNumber, DateTime genTime)
	{
		return Generate(request, serialNumber, new DateTimeObject(genTime));
	}

	public TimeStampResponse Generate(TimeStampRequest request, BigInteger serialNumber, DateTimeObject genTime)
	{
		TimeStampResp resp;
		try
		{
			if (genTime == null)
			{
				throw new TspValidationException("The time source is not available.", 512);
			}
			request.Validate(acceptedAlgorithms, acceptedPolicies, acceptedExtensions);
			status = PkiStatus.Granted;
			AddStatusString("Operation Okay");
			PkiStatusInfo pkiStatusInfo = GetPkiStatusInfo();
			ContentInfo instance;
			try
			{
				TimeStampToken timeStampToken = tokenGenerator.Generate(request, serialNumber, genTime.Value);
				byte[] encoded = timeStampToken.ToCmsSignedData().GetEncoded();
				instance = ContentInfo.GetInstance(Asn1Object.FromByteArray(encoded));
			}
			catch (IOException e)
			{
				throw new TspException("Timestamp token received cannot be converted to ContentInfo", e);
			}
			resp = new TimeStampResp(pkiStatusInfo, instance);
		}
		catch (TspValidationException ex)
		{
			status = PkiStatus.Rejection;
			SetFailInfoField(ex.FailureCode);
			AddStatusString(ex.Message);
			PkiStatusInfo pkiStatusInfo2 = GetPkiStatusInfo();
			resp = new TimeStampResp(pkiStatusInfo2, null);
		}
		try
		{
			return new TimeStampResponse(resp);
		}
		catch (IOException e2)
		{
			throw new TspException("created badly formatted response!", e2);
		}
	}

	public TimeStampResponse GenerateGrantedResponse(TimeStampRequest request, BigInteger serialNumber, DateTimeObject genTime, string statusString, X509Extensions additionalExtensions)
	{
		TimeStampResp resp;
		try
		{
			if (genTime == null)
			{
				throw new TspValidationException("The time source is not available.", 512);
			}
			request.Validate(acceptedAlgorithms, acceptedPolicies, acceptedExtensions);
			status = PkiStatus.Granted;
			AddStatusString(statusString);
			PkiStatusInfo pkiStatusInfo = GetPkiStatusInfo();
			ContentInfo instance;
			try
			{
				TimeStampToken timeStampToken = tokenGenerator.Generate(request, serialNumber, genTime.Value, additionalExtensions);
				byte[] encoded = timeStampToken.ToCmsSignedData().GetEncoded();
				instance = ContentInfo.GetInstance(Asn1Object.FromByteArray(encoded));
			}
			catch (IOException e)
			{
				throw new TspException("Timestamp token received cannot be converted to ContentInfo", e);
			}
			resp = new TimeStampResp(pkiStatusInfo, instance);
		}
		catch (TspValidationException ex)
		{
			status = PkiStatus.Rejection;
			SetFailInfoField(ex.FailureCode);
			AddStatusString(ex.Message);
			PkiStatusInfo pkiStatusInfo2 = GetPkiStatusInfo();
			resp = new TimeStampResp(pkiStatusInfo2, null);
		}
		try
		{
			return new TimeStampResponse(resp);
		}
		catch (IOException e2)
		{
			throw new TspException("created badly formatted response!", e2);
		}
	}

	public TimeStampResponse GenerateFailResponse(PkiStatus status, int failInfoField, string statusString)
	{
		this.status = status;
		SetFailInfoField(failInfoField);
		if (statusString != null)
		{
			AddStatusString(statusString);
		}
		PkiStatusInfo pkiStatusInfo = GetPkiStatusInfo();
		TimeStampResp resp = new TimeStampResp(pkiStatusInfo, null);
		try
		{
			return new TimeStampResponse(resp);
		}
		catch (IOException e)
		{
			throw new TspException("created badly formatted response!", e);
		}
	}
}
