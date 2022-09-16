namespace ICSharpCode.SharpZipLib.Core;

public interface IScanFilter
{
	bool IsMatch(string name);
}
