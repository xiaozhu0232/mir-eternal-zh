using System.IO;

namespace ICSharpCode.SharpZipLib.Core;

public class PathFilter : IScanFilter
{
	private NameFilter nameFilter;

	public PathFilter(string filter)
	{
		nameFilter = new NameFilter(filter);
	}

	public virtual bool IsMatch(string name)
	{
		return nameFilter.IsMatch(Path.GetFullPath(name));
	}
}
