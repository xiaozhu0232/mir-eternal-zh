using System;
using System.Collections;
using System.IO;
using System.Text;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Utilities.IO.Pem;

public class PemReader
{
	private const string BeginString = "-----BEGIN ";

	private const string EndString = "-----END ";

	private readonly TextReader reader;

	public TextReader Reader => reader;

	public PemReader(TextReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		this.reader = reader;
	}

	public PemObject ReadPemObject()
	{
		string text = reader.ReadLine();
		if (text != null && Platform.StartsWith(text, "-----BEGIN "))
		{
			text = text.Substring("-----BEGIN ".Length);
			int num = text.IndexOf('-');
			if (num > 0 && Platform.EndsWith(text, "-----") && text.Length - num == 5)
			{
				string type = text.Substring(0, num);
				return LoadObject(type);
			}
		}
		return null;
	}

	private PemObject LoadObject(string type)
	{
		string text = "-----END " + type;
		IList list = Platform.CreateArrayList();
		StringBuilder stringBuilder = new StringBuilder();
		string text2;
		while ((text2 = reader.ReadLine()) != null && Platform.IndexOf(text2, text) == -1)
		{
			int num = text2.IndexOf(':');
			if (num == -1)
			{
				stringBuilder.Append(text2.Trim());
				continue;
			}
			string text3 = text2.Substring(0, num).Trim();
			if (Platform.StartsWith(text3, "X-"))
			{
				text3 = text3.Substring(2);
			}
			string val = text2.Substring(num + 1).Trim();
			list.Add(new PemHeader(text3, val));
		}
		if (text2 == null)
		{
			throw new IOException(text + " not found");
		}
		if (stringBuilder.Length % 4 != 0)
		{
			throw new IOException("base64 data appears to be truncated");
		}
		return new PemObject(type, list, Base64.Decode(stringBuilder.ToString()));
	}
}
