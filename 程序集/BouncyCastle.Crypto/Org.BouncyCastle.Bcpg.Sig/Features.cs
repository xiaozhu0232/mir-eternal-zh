using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.Sig;

public class Features : SignatureSubpacket
{
	public static readonly byte FEATURE_MODIFICATION_DETECTION = 1;

	public bool SupportsModificationDetection => SupportsFeature(FEATURE_MODIFICATION_DETECTION);

	private static byte[] FeatureToByteArray(byte feature)
	{
		return new byte[1] { feature };
	}

	public Features(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.Features, critical, isLongLength, data)
	{
	}

	public Features(bool critical, byte feature)
		: base(SignatureSubpacketTag.Features, critical, isLongLength: false, FeatureToByteArray(feature))
	{
	}

	public bool SupportsFeature(byte feature)
	{
		return Array.IndexOf((Array)data, (object)feature) >= 0;
	}

	private void SetSupportsFeature(byte feature, bool support)
	{
		if (feature == 0)
		{
			throw new ArgumentException("cannot be 0", "feature");
		}
		int num = Array.IndexOf((Array)data, (object)feature);
		if (num >= 0 != support)
		{
			if (support)
			{
				data = Arrays.Append(data, feature);
				return;
			}
			byte[] array = new byte[data.Length - 1];
			Array.Copy(data, 0, array, 0, num);
			Array.Copy(data, num + 1, array, num, array.Length - num);
			data = array;
		}
	}
}
