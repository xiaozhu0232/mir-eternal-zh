using System.Collections;

namespace Org.BouncyCastle.Utilities.IO.Pem;

public class PemObject : PemObjectGenerator
{
	private string type;

	private IList headers;

	private byte[] content;

	public string Type => type;

	public IList Headers => headers;

	public byte[] Content => content;

	public PemObject(string type, byte[] content)
		: this(type, Platform.CreateArrayList(), content)
	{
	}

	public PemObject(string type, IList headers, byte[] content)
	{
		this.type = type;
		this.headers = Platform.CreateArrayList(headers);
		this.content = content;
	}

	public PemObject Generate()
	{
		return this;
	}
}
