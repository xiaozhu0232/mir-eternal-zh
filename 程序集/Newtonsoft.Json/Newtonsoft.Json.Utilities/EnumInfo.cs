namespace Newtonsoft.Json.Utilities;

internal class EnumInfo
{
	public readonly bool IsFlags;

	public readonly ulong[] Values;

	public readonly string[] Names;

	public readonly string[] ResolvedNames;

	public EnumInfo(bool isFlags, ulong[] values, string[] names, string[] resolvedNames)
	{
		IsFlags = isFlags;
		Values = values;
		Names = names;
		ResolvedNames = resolvedNames;
	}
}
