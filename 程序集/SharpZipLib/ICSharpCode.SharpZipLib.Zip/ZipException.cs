namespace ICSharpCode.SharpZipLib.Zip;

public class ZipException : SharpZipBaseException
{
	public ZipException()
	{
	}

	public ZipException(string msg)
		: base(msg)
	{
	}
}
