namespace Org.BouncyCastle.Utilities.IO;

internal class NullOutputStream : BaseOutputStream
{
	public override void WriteByte(byte b)
	{
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
	}
}
