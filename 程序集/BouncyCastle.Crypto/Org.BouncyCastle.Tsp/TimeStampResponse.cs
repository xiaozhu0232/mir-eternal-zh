using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Tsp;

public class TimeStampResponse
{
	private TimeStampResp resp;

	private TimeStampToken timeStampToken;

	public int Status => resp.Status.Status.IntValue;

	public TimeStampToken TimeStampToken => timeStampToken;

	public TimeStampResponse(TimeStampResp resp)
	{
		this.resp = resp;
		if (resp.TimeStampToken != null)
		{
			timeStampToken = new TimeStampToken(resp.TimeStampToken);
		}
	}

	public TimeStampResponse(byte[] resp)
		: this(readTimeStampResp(new Asn1InputStream(resp)))
	{
	}

	public TimeStampResponse(Stream input)
		: this(readTimeStampResp(new Asn1InputStream(input)))
	{
	}

	private static TimeStampResp readTimeStampResp(Asn1InputStream input)
	{
		try
		{
			return TimeStampResp.GetInstance(input.ReadObject());
		}
		catch (ArgumentException ex)
		{
			throw new TspException("malformed timestamp response: " + ex, ex);
		}
		catch (InvalidCastException ex2)
		{
			throw new TspException("malformed timestamp response: " + ex2, ex2);
		}
	}

	public string GetStatusString()
	{
		if (resp.Status.StatusString == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		PkiFreeText statusString = resp.Status.StatusString;
		for (int i = 0; i != statusString.Count; i++)
		{
			stringBuilder.Append(statusString[i].GetString());
		}
		return stringBuilder.ToString();
	}

	public PkiFailureInfo GetFailInfo()
	{
		if (resp.Status.FailInfo == null)
		{
			return null;
		}
		return new PkiFailureInfo(resp.Status.FailInfo);
	}

	public void Validate(TimeStampRequest request)
	{
		TimeStampToken timeStampToken = TimeStampToken;
		if (timeStampToken != null)
		{
			TimeStampTokenInfo timeStampInfo = timeStampToken.TimeStampInfo;
			if (request.Nonce != null && !request.Nonce.Equals(timeStampInfo.Nonce))
			{
				throw new TspValidationException("response contains wrong nonce value.");
			}
			if (Status != 0 && Status != 1)
			{
				throw new TspValidationException("time stamp token found in failed request.");
			}
			if (!Arrays.ConstantTimeAreEqual(request.GetMessageImprintDigest(), timeStampInfo.GetMessageImprintDigest()))
			{
				throw new TspValidationException("response for different message imprint digest.");
			}
			if (!timeStampInfo.MessageImprintAlgOid.Equals(request.MessageImprintAlgOid))
			{
				throw new TspValidationException("response for different message imprint algorithm.");
			}
			Org.BouncyCastle.Asn1.Cms.Attribute attribute = timeStampToken.SignedAttributes[PkcsObjectIdentifiers.IdAASigningCertificate];
			Org.BouncyCastle.Asn1.Cms.Attribute attribute2 = timeStampToken.SignedAttributes[PkcsObjectIdentifiers.IdAASigningCertificateV2];
			if (attribute == null && attribute2 == null)
			{
				throw new TspValidationException("no signing certificate attribute present.");
			}
			if (attribute != null)
			{
			}
			if (request.ReqPolicy != null && !request.ReqPolicy.Equals(timeStampInfo.Policy))
			{
				throw new TspValidationException("TSA policy wrong for request.");
			}
		}
		else if (Status == 0 || Status == 1)
		{
			throw new TspValidationException("no time stamp token found and one expected.");
		}
	}

	public byte[] GetEncoded()
	{
		return resp.GetEncoded();
	}
}
