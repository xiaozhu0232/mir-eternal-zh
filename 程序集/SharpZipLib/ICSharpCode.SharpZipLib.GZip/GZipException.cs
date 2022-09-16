namespace ICSharpCode.SharpZipLib.GZip;

public class GZipException : SharpZipBaseException
{
	public GZipException()
	{
	}

	public GZipException(string message)
		: base(message)
	{
	}
}
