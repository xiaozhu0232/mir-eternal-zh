using System.Collections;

namespace Org.BouncyCastle.X509.Store;

public interface IX509Store
{
	ICollection GetMatches(IX509Selector selector);
}
