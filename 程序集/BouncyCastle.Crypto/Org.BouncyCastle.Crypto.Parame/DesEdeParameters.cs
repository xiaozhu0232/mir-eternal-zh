using System;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DesEdeParameters : DesParameters
{
	public const int DesEdeKeyLength = 24;

	private static byte[] FixKey(byte[] key, int keyOff, int keyLen)
	{
		byte[] array = new byte[24];
		switch (keyLen)
		{
		case 16:
			Array.Copy(key, keyOff, array, 0, 16);
			Array.Copy(key, keyOff, array, 16, 8);
			break;
		case 24:
			Array.Copy(key, keyOff, array, 0, 24);
			break;
		default:
			throw new ArgumentException("Bad length for DESede key: " + keyLen, "keyLen");
		}
		if (IsWeakKey(array))
		{
			throw new ArgumentException("attempt to create weak DESede key");
		}
		return array;
	}

	public DesEdeParameters(byte[] key)
		: base(FixKey(key, 0, key.Length))
	{
	}

	public DesEdeParameters(byte[] key, int keyOff, int keyLen)
		: base(FixKey(key, keyOff, keyLen))
	{
	}

	public static bool IsWeakKey(byte[] key, int offset, int length)
	{
		for (int i = offset; i < length; i += 8)
		{
			if (DesParameters.IsWeakKey(key, i))
			{
				return true;
			}
		}
		return false;
	}

	public new static bool IsWeakKey(byte[] key, int offset)
	{
		return IsWeakKey(key, offset, key.Length - offset);
	}

	public new static bool IsWeakKey(byte[] key)
	{
		return IsWeakKey(key, 0, key.Length);
	}

	public static bool IsRealEdeKey(byte[] key, int offset)
	{
		if (key.Length != 16)
		{
			return IsReal3Key(key, offset);
		}
		return IsReal2Key(key, offset);
	}

	public static bool IsReal2Key(byte[] key, int offset)
	{
		bool flag = false;
		for (int i = offset; i != offset + 8; i++)
		{
			flag |= key[i] != key[i + 8];
		}
		return flag;
	}

	public static bool IsReal3Key(byte[] key, int offset)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = offset; i != offset + 8; i++)
		{
			flag |= key[i] != key[i + 8];
			flag2 |= key[i] != key[i + 16];
			flag3 |= key[i + 8] != key[i + 16];
		}
		if (flag && flag2)
		{
			return flag3;
		}
		return false;
	}
}
