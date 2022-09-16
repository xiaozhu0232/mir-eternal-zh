using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class CamelCaseNamingStrategy : NamingStrategy
{
	public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
	{
		base.ProcessDictionaryKeys = processDictionaryKeys;
		base.OverrideSpecifiedNames = overrideSpecifiedNames;
	}

	public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
		: this(processDictionaryKeys, overrideSpecifiedNames)
	{
		base.ProcessExtensionDataNames = processExtensionDataNames;
	}

	public CamelCaseNamingStrategy()
	{
	}

	protected override string ResolvePropertyName(string name)
	{
		return StringUtils.ToCamelCase(name);
	}
}
