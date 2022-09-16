namespace LumiSoft.Net.Media.Codec;

public abstract class Codec
{
	public abstract string Name { get; }

	public abstract byte[] Encode(byte[] buffer, int offset, int count);

	public abstract byte[] Decode(byte[] buffer, int offset, int count);
}
