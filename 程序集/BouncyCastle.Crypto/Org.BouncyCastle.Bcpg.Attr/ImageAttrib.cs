using System;
using System.IO;

namespace Org.BouncyCastle.Bcpg.Attr;

public class ImageAttrib : UserAttributeSubpacket
{
	public enum Format : byte
	{
		Jpeg = 1
	}

	private static readonly byte[] Zeroes = new byte[12];

	private int hdrLength;

	private int _version;

	private int _encoding;

	private byte[] imageData;

	public virtual int Version => _version;

	public virtual int Encoding => _encoding;

	public ImageAttrib(byte[] data)
		: this(forceLongLength: false, data)
	{
	}

	public ImageAttrib(bool forceLongLength, byte[] data)
		: base(UserAttributeSubpacketTag.ImageAttribute, forceLongLength, data)
	{
		hdrLength = ((data[1] & 0xFF) << 8) | (data[0] & 0xFF);
		_version = data[2] & 0xFF;
		_encoding = data[3] & 0xFF;
		imageData = new byte[data.Length - hdrLength];
		Array.Copy(data, hdrLength, imageData, 0, imageData.Length);
	}

	public ImageAttrib(Format imageType, byte[] imageData)
		: this(ToByteArray(imageType, imageData))
	{
	}

	private static byte[] ToByteArray(Format imageType, byte[] imageData)
	{
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.WriteByte(16);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(1);
		memoryStream.WriteByte((byte)imageType);
		memoryStream.Write(Zeroes, 0, Zeroes.Length);
		memoryStream.Write(imageData, 0, imageData.Length);
		return memoryStream.ToArray();
	}

	public virtual byte[] GetImageData()
	{
		return imageData;
	}
}
