using System;

namespace Org.BouncyCastle.Utilities;

public class MemoableResetException : InvalidCastException
{
	public MemoableResetException(string msg)
		: base(msg)
	{
	}
}
