namespace ICSharpCode.SharpZipLib.GZip;

public class GZipConstants
{
	public const int FTEXT = 1;

	public const int FHCRC = 2;

	public const int FEXTRA = 4;

	public const int FNAME = 8;

	public const int FCOMMENT = 16;

	public static readonly int GZIP_MAGIC = 8075;

	private GZipConstants()
	{
	}
}
