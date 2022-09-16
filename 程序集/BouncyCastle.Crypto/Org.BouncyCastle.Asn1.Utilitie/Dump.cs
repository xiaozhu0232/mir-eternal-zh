using System;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Utilities;

public sealed class Dump
{
	private Dump()
	{
	}

	public static void Main(string[] args)
	{
		FileStream inputStream = File.OpenRead(args[0]);
		Asn1InputStream asn1InputStream = new Asn1InputStream(inputStream);
		Asn1Object obj;
		while ((obj = asn1InputStream.ReadObject()) != null)
		{
			Console.WriteLine(Asn1Dump.DumpAsString(obj));
		}
		Platform.Dispose(asn1InputStream);
	}
}
