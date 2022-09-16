namespace ICSharpCode.SharpZipLib.Tar;

public class TarException : SharpZipBaseException
{
	public TarException()
	{
	}

	public TarException(string message)
		: base(message)
	{
	}
}
