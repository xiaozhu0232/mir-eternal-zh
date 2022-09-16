namespace ICSharpCode.SharpZipLib.Core;

public class DirectoryEventArgs : ScanEventArgs
{
	private bool hasMatchingFiles;

	public bool HasMatchingFiles => hasMatchingFiles;

	public DirectoryEventArgs(string name, bool hasMatchingFiles)
		: base(name)
	{
		this.hasMatchingFiles = hasMatchingFiles;
	}
}
