namespace Org.BouncyCastle.Bcpg.Sig;

public class SignerUserId : SignatureSubpacket
{
	private static byte[] UserIdToBytes(string id)
	{
		byte[] array = new byte[id.Length];
		for (int i = 0; i != id.Length; i++)
		{
			array[i] = (byte)id[i];
		}
		return array;
	}

	public SignerUserId(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.SignerUserId, critical, isLongLength, data)
	{
	}

	public SignerUserId(bool critical, string userId)
		: base(SignatureSubpacketTag.SignerUserId, critical, isLongLength: false, UserIdToBytes(userId))
	{
	}

	public string GetId()
	{
		char[] array = new char[data.Length];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = (char)(data[i] & 0xFFu);
		}
		return new string(array);
	}
}
