namespace Org.BouncyCastle.Bcpg.Sig;

public class Exportable : SignatureSubpacket
{
	private static byte[] BooleanToByteArray(bool val)
	{
		byte[] array = new byte[1];
		if (val)
		{
			array[0] = 1;
			return array;
		}
		return array;
	}

	public Exportable(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.Exportable, critical, isLongLength, data)
	{
	}

	public Exportable(bool critical, bool isExportable)
		: base(SignatureSubpacketTag.Exportable, critical, isLongLength: false, BooleanToByteArray(isExportable))
	{
	}

	public bool IsExportable()
	{
		return data[0] != 0;
	}
}
