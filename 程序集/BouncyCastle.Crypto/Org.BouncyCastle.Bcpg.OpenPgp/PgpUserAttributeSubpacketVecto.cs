using System;
using System.Collections;
using Org.BouncyCastle.Bcpg.Attr;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpUserAttributeSubpacketVector
{
	private readonly UserAttributeSubpacket[] packets;

	internal PgpUserAttributeSubpacketVector(UserAttributeSubpacket[] packets)
	{
		this.packets = packets;
	}

	public UserAttributeSubpacket GetSubpacket(UserAttributeSubpacketTag type)
	{
		for (int i = 0; i != packets.Length; i++)
		{
			if (packets[i].SubpacketType == type)
			{
				return packets[i];
			}
		}
		return null;
	}

	public ImageAttrib GetImageAttribute()
	{
		UserAttributeSubpacket subpacket = GetSubpacket(UserAttributeSubpacketTag.ImageAttribute);
		if (subpacket != null)
		{
			return (ImageAttrib)subpacket;
		}
		return null;
	}

	internal UserAttributeSubpacket[] ToSubpacketArray()
	{
		return packets;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is PgpUserAttributeSubpacketVector pgpUserAttributeSubpacketVector))
		{
			return false;
		}
		if (pgpUserAttributeSubpacketVector.packets.Length != packets.Length)
		{
			return false;
		}
		for (int i = 0; i != packets.Length; i++)
		{
			if (!pgpUserAttributeSubpacketVector.packets[i].Equals(packets[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = 0;
		UserAttributeSubpacket[] array = packets;
		foreach (object obj in array)
		{
			num ^= obj.GetHashCode();
		}
		return num;
	}
}
public class PgpUserAttributeSubpacketVectorGenerator
{
	private IList list = Platform.CreateArrayList();

	public virtual void SetImageAttribute(ImageAttrib.Format imageType, byte[] imageData)
	{
		if (imageData == null)
		{
			throw new ArgumentException("attempt to set null image", "imageData");
		}
		list.Add(new ImageAttrib(imageType, imageData));
	}

	public virtual PgpUserAttributeSubpacketVector Generate()
	{
		UserAttributeSubpacket[] array = new UserAttributeSubpacket[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = (UserAttributeSubpacket)list[i];
		}
		return new PgpUserAttributeSubpacketVector(array);
	}
}
