using System.IO;

namespace Org.BouncyCastle.Bcpg;

public abstract class BcpgObject
{
	public virtual byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.WriteObject(this);
		return memoryStream.ToArray();
	}

	public abstract void Encode(BcpgOutputStream bcpgOut);
}
