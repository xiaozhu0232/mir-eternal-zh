namespace Newtonsoft.Json.Serialization;

public abstract class NamingStrategy
{
	public bool ProcessDictionaryKeys { get; set; }

	public bool ProcessExtensionDataNames { get; set; }

	public bool OverrideSpecifiedNames { get; set; }

	public virtual string GetPropertyName(string name, bool hasSpecifiedName)
	{
		if (hasSpecifiedName && !OverrideSpecifiedNames)
		{
			return name;
		}
		return ResolvePropertyName(name);
	}

	public virtual string GetExtensionDataName(string name)
	{
		if (!ProcessExtensionDataNames)
		{
			return name;
		}
		return ResolvePropertyName(name);
	}

	public virtual string GetDictionaryKey(string key)
	{
		if (!ProcessDictionaryKeys)
		{
			return key;
		}
		return ResolvePropertyName(key);
	}

	protected abstract string ResolvePropertyName(string name);

	public override int GetHashCode()
	{
		return (((((GetType().GetHashCode() * 397) ^ ProcessDictionaryKeys.GetHashCode()) * 397) ^ ProcessExtensionDataNames.GetHashCode()) * 397) ^ OverrideSpecifiedNames.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as NamingStrategy);
	}

	protected bool Equals(NamingStrategy? other)
	{
		if (other == null)
		{
			return false;
		}
		if (GetType() == other!.GetType() && ProcessDictionaryKeys == other!.ProcessDictionaryKeys && ProcessExtensionDataNames == other!.ProcessExtensionDataNames)
		{
			return OverrideSpecifiedNames == other!.OverrideSpecifiedNames;
		}
		return false;
	}
}
