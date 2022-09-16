using System.Text;

namespace ICSharpCode.SharpZipLib.Zip;

public sealed class ZipConstants
{
	public const int VERSION_MADE_BY = 20;

	public const int VERSION_STRONG_ENCRYPTION = 50;

	public const int LOCHDR = 30;

	public const int LOCSIG = 67324752;

	public const int LOCVER = 4;

	public const int LOCFLG = 6;

	public const int LOCHOW = 8;

	public const int LOCTIM = 10;

	public const int LOCCRC = 14;

	public const int LOCSIZ = 18;

	public const int LOCLEN = 22;

	public const int LOCNAM = 26;

	public const int LOCEXT = 28;

	public const int SPANNINGSIG = 134695760;

	public const int SPANTEMPSIG = 808471376;

	public const int EXTSIG = 134695760;

	public const int EXTHDR = 16;

	public const int EXTCRC = 4;

	public const int EXTSIZ = 8;

	public const int EXTLEN = 12;

	public const int CENSIG = 33639248;

	public const int CENHDR = 46;

	public const int CENVEM = 4;

	public const int CENVER = 6;

	public const int CENFLG = 8;

	public const int CENHOW = 10;

	public const int CENTIM = 12;

	public const int CENCRC = 16;

	public const int CENSIZ = 20;

	public const int CENLEN = 24;

	public const int CENNAM = 28;

	public const int CENEXT = 30;

	public const int CENCOM = 32;

	public const int CENDSK = 34;

	public const int CENATT = 36;

	public const int CENATX = 38;

	public const int CENOFF = 42;

	public const int CENSIG64 = 101075792;

	public const int CENDIGITALSIG = 84233040;

	public const int ENDSIG = 101010256;

	public const int ENDHDR = 22;

	public const int ENDNRD = 4;

	public const int ENDDCD = 6;

	public const int ENDSUB = 8;

	public const int ENDTOT = 10;

	public const int ENDSIZ = 12;

	public const int ENDOFF = 16;

	public const int ENDCOM = 20;

	public const int CRYPTO_HEADER_SIZE = 12;

	private static int defaultCodePage = 0;

	public static int DefaultCodePage
	{
		get
		{
			return defaultCodePage;
		}
		set
		{
			defaultCodePage = value;
		}
	}

	public static string ConvertToString(byte[] data, int length)
	{
		return Encoding.GetEncoding(DefaultCodePage).GetString(data, 0, length);
	}

	public static string ConvertToString(byte[] data)
	{
		return ConvertToString(data, data.Length);
	}

	public static byte[] ConvertToArray(string str)
	{
		return Encoding.GetEncoding(DefaultCodePage).GetBytes(str);
	}
}
