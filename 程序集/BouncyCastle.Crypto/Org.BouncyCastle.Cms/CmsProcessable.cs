using System;
using System.IO;

namespace Org.BouncyCastle.Cms;

public interface CmsProcessable
{
	void Write(Stream outStream);

	[Obsolete]
	object GetContent();
}
