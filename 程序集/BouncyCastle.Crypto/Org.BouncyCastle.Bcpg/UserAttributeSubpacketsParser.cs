using System.IO;
using Org.BouncyCastle.Bcpg.Attr;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class UserAttributeSubpacketsParser
{
	private readonly Stream input;

	public UserAttributeSubpacketsParser(Stream input)
	{
		this.input = input;
	}

	public virtual UserAttributeSubpacket ReadPacket()
	{
		int num = input.ReadByte();
		if (num < 0)
		{
			return null;
		}
		int num2 = 0;
		bool forceLongLength = false;
		if (num < 192)
		{
			num2 = num;
		}
		else if (num <= 223)
		{
			num2 = (num - 192 << 8) + input.ReadByte() + 192;
		}
		else
		{
			if (num != 255)
			{
				throw new IOException("unrecognised length reading user attribute sub packet");
			}
			num2 = (input.ReadByte() << 24) | (input.ReadByte() << 16) | (input.ReadByte() << 8) | input.ReadByte();
			forceLongLength = true;
		}
		int num3 = input.ReadByte();
		if (num3 < 0)
		{
			throw new EndOfStreamException("unexpected EOF reading user attribute sub packet");
		}
		byte[] array = new byte[num2 - 1];
		if (Streams.ReadFully(input, array) < array.Length)
		{
			throw new EndOfStreamException();
		}
		UserAttributeSubpacketTag userAttributeSubpacketTag = (UserAttributeSubpacketTag)num3;
		UserAttributeSubpacketTag userAttributeSubpacketTag2 = userAttributeSubpacketTag;
		if (userAttributeSubpacketTag2 == UserAttributeSubpacketTag.ImageAttribute)
		{
			return new ImageAttrib(forceLongLength, array);
		}
		return new UserAttributeSubpacket(userAttributeSubpacketTag, forceLongLength, array);
	}
}
