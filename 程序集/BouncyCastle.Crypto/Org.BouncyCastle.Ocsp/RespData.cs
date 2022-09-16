using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Ocsp;

public class RespData : X509ExtensionBase
{
	internal readonly ResponseData data;

	public int Version => data.Version.IntValueExact + 1;

	public DateTime ProducedAt => data.ProducedAt.ToDateTime();

	public X509Extensions ResponseExtensions => data.ResponseExtensions;

	public RespData(ResponseData data)
	{
		this.data = data;
	}

	public RespID GetResponderId()
	{
		return new RespID(data.ResponderID);
	}

	public SingleResp[] GetResponses()
	{
		Asn1Sequence responses = data.Responses;
		SingleResp[] array = new SingleResp[responses.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = new SingleResp(SingleResponse.GetInstance(responses[i]));
		}
		return array;
	}

	protected override X509Extensions GetX509Extensions()
	{
		return ResponseExtensions;
	}
}
