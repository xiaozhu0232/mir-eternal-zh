using System;

namespace Org.BouncyCastle.Pkix;

[Serializable]
public class PkixNameConstraintValidatorException : Exception
{
	public PkixNameConstraintValidatorException(string msg)
		: base(msg)
	{
	}
}
