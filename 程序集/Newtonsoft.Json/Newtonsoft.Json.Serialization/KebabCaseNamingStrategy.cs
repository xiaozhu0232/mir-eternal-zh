using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class KebabCaseNamingStrategy : NamingStrategy
{
	public KebabCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
	{
		base.ProcessDictionaryKeys = processDictionaryKeys;
		base.OverrideSpecifiedNames = overrideSpecifiedNames;
	}

	public KebabCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
		: this(processDictionaryKeys, overrideSpecifiedNames)
	{
		base.ProcessExtensionDataNames = processExtensionDataNames;
	}

	public KebabCaseNamingStrategy()
	{
	}

	protected override string ResolvePropertyName(string name)
	{
		return StringUtils.ToKebabCase(name);
	}
}
