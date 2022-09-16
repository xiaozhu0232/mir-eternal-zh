using System;

namespace Org.BouncyCastle.Asn1.X9;

public class X962Parameters : Asn1Encodable, IAsn1Choice
{
	private readonly Asn1Object _params;

	public bool IsNamedCurve => _params is DerObjectIdentifier;

	public bool IsImplicitlyCA => _params is Asn1Null;

	public Asn1Object Parameters => _params;

	public static X962Parameters GetInstance(object obj)
	{
		if (obj == null || obj is X962Parameters)
		{
			return (X962Parameters)obj;
		}
		if (obj is Asn1Object)
		{
			return new X962Parameters((Asn1Object)obj);
		}
		if (obj is byte[])
		{
			try
			{
				return new X962Parameters(Asn1Object.FromByteArray((byte[])obj));
			}
			catch (Exception ex)
			{
				throw new ArgumentException("unable to parse encoded data: " + ex.Message, ex);
			}
		}
		throw new ArgumentException("unknown object in getInstance()");
	}

	public X962Parameters(X9ECParameters ecParameters)
	{
		_params = ecParameters.ToAsn1Object();
	}

	public X962Parameters(DerObjectIdentifier namedCurve)
	{
		_params = namedCurve;
	}

	public X962Parameters(Asn1Null obj)
	{
		_params = obj;
	}

	[Obsolete("Use 'GetInstance' instead")]
	public X962Parameters(Asn1Object obj)
	{
		_params = obj;
	}

	public override Asn1Object ToAsn1Object()
	{
		return _params;
	}
}
