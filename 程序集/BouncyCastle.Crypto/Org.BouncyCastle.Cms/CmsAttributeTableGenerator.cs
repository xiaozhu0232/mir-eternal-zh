using System.Collections;
using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Cms;

public interface CmsAttributeTableGenerator
{
	AttributeTable GetAttributes(IDictionary parameters);
}
