using System.Security.Cryptography;

namespace LumiSoft.Net;

internal abstract class _MD4 : HashAlgorithm
{
	protected _MD4()
	{
		HashSizeValue = 128;
	}

	public new static _MD4 Create()
	{
		return Create("MD4");
	}

	public new static _MD4 Create(string hashName)
	{
		object obj = CryptoConfig.CreateFromName(hashName);
		if (obj == null)
		{
			obj = new MD4Managed();
		}
		return (_MD4)obj;
	}
}
