using System.Collections;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkix;

public abstract class PkixAttrCertChecker
{
	public abstract ISet GetSupportedExtensions();

	public abstract void Check(IX509AttributeCertificate attrCert, PkixCertPath certPath, PkixCertPath holderCertPath, ICollection unresolvedCritExts);

	public abstract PkixAttrCertChecker Clone();
}
