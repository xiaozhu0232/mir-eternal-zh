using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkix;

public abstract class PkixCertPathChecker
{
	public abstract void Init(bool forward);

	public abstract bool IsForwardCheckingSupported();

	public abstract ISet GetSupportedExtensions();

	public abstract void Check(X509Certificate cert, ISet unresolvedCritExts);

	public virtual object Clone()
	{
		return MemberwiseClone();
	}
}
