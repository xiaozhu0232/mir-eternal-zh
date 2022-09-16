namespace ICSharpCode.SharpZipLib.Tar;

public class InvalidHeaderException : TarException
{
	public InvalidHeaderException()
	{
	}

	public InvalidHeaderException(string msg)
		: base(msg)
	{
	}
}
