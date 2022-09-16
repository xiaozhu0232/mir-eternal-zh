using System;
using System.IO;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Utilities.IO.Pem;

public class PemWriter
{
	private const int LineLength = 64;

	private readonly TextWriter writer;

	private readonly int nlLength;

	private char[] buf = new char[64];

	public TextWriter Writer => writer;

	public PemWriter(TextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		this.writer = writer;
		nlLength = Platform.NewLine.Length;
	}

	public int GetOutputSize(PemObject obj)
	{
		int num = 2 * (obj.Type.Length + 10 + nlLength) + 6 + 4;
		if (obj.Headers.Count > 0)
		{
			foreach (PemHeader header in obj.Headers)
			{
				num += header.Name.Length + ": ".Length + header.Value.Length + nlLength;
			}
			num += nlLength;
		}
		int num2 = (obj.Content.Length + 2) / 3 * 4;
		return num + (num2 + (num2 + 64 - 1) / 64 * nlLength);
	}

	public void WriteObject(PemObjectGenerator objGen)
	{
		PemObject pemObject = objGen.Generate();
		WritePreEncapsulationBoundary(pemObject.Type);
		if (pemObject.Headers.Count > 0)
		{
			foreach (PemHeader header in pemObject.Headers)
			{
				writer.Write(header.Name);
				writer.Write(": ");
				writer.WriteLine(header.Value);
			}
			writer.WriteLine();
		}
		WriteEncoded(pemObject.Content);
		WritePostEncapsulationBoundary(pemObject.Type);
	}

	private void WriteEncoded(byte[] bytes)
	{
		bytes = Base64.Encode(bytes);
		for (int i = 0; i < bytes.Length; i += buf.Length)
		{
			int j;
			for (j = 0; j != buf.Length && i + j < bytes.Length; j++)
			{
				buf[j] = (char)bytes[i + j];
			}
			writer.WriteLine(buf, 0, j);
		}
	}

	private void WritePreEncapsulationBoundary(string type)
	{
		writer.WriteLine("-----BEGIN " + type + "-----");
	}

	private void WritePostEncapsulationBoundary(string type)
	{
		writer.WriteLine("-----END " + type + "-----");
	}
}
