using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto;

public abstract class PbeParametersGenerator
{
	protected byte[] mPassword;

	protected byte[] mSalt;

	protected int mIterationCount;

	public virtual byte[] Password => Arrays.Clone(mPassword);

	public virtual byte[] Salt => Arrays.Clone(mSalt);

	public virtual int IterationCount => mIterationCount;

	public virtual void Init(byte[] password, byte[] salt, int iterationCount)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (salt == null)
		{
			throw new ArgumentNullException("salt");
		}
		mPassword = Arrays.Clone(password);
		mSalt = Arrays.Clone(salt);
		mIterationCount = iterationCount;
	}

	[Obsolete("Use 'Password' property")]
	public byte[] GetPassword()
	{
		return Password;
	}

	[Obsolete("Use 'Salt' property")]
	public byte[] GetSalt()
	{
		return Salt;
	}

	[Obsolete("Use version with 'algorithm' parameter")]
	public abstract ICipherParameters GenerateDerivedParameters(int keySize);

	public abstract ICipherParameters GenerateDerivedParameters(string algorithm, int keySize);

	[Obsolete("Use version with 'algorithm' parameter")]
	public abstract ICipherParameters GenerateDerivedParameters(int keySize, int ivSize);

	public abstract ICipherParameters GenerateDerivedParameters(string algorithm, int keySize, int ivSize);

	public abstract ICipherParameters GenerateDerivedMacParameters(int keySize);

	public static byte[] Pkcs5PasswordToBytes(char[] password)
	{
		if (password == null)
		{
			return new byte[0];
		}
		return Strings.ToByteArray(password);
	}

	[Obsolete("Use version taking 'char[]' instead")]
	public static byte[] Pkcs5PasswordToBytes(string password)
	{
		if (password == null)
		{
			return new byte[0];
		}
		return Strings.ToByteArray(password);
	}

	public static byte[] Pkcs5PasswordToUtf8Bytes(char[] password)
	{
		if (password == null)
		{
			return new byte[0];
		}
		return Encoding.UTF8.GetBytes(password);
	}

	[Obsolete("Use version taking 'char[]' instead")]
	public static byte[] Pkcs5PasswordToUtf8Bytes(string password)
	{
		if (password == null)
		{
			return new byte[0];
		}
		return Encoding.UTF8.GetBytes(password);
	}

	public static byte[] Pkcs12PasswordToBytes(char[] password)
	{
		return Pkcs12PasswordToBytes(password, wrongPkcs12Zero: false);
	}

	public static byte[] Pkcs12PasswordToBytes(char[] password, bool wrongPkcs12Zero)
	{
		if (password == null || password.Length < 1)
		{
			return new byte[wrongPkcs12Zero ? 2 : 0];
		}
		byte[] array = new byte[(password.Length + 1) * 2];
		Encoding.BigEndianUnicode.GetBytes(password, 0, password.Length, array, 0);
		return array;
	}
}
