using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Cms;

public class CmsCompressedDataStreamGenerator
{
	private class CmsCompressedOutputStream : BaseOutputStream
	{
		private ZOutputStream _out;

		private BerSequenceGenerator _sGen;

		private BerSequenceGenerator _cGen;

		private BerSequenceGenerator _eiGen;

		internal CmsCompressedOutputStream(ZOutputStream outStream, BerSequenceGenerator sGen, BerSequenceGenerator cGen, BerSequenceGenerator eiGen)
		{
			_out = outStream;
			_sGen = sGen;
			_cGen = cGen;
			_eiGen = eiGen;
		}

		public override void WriteByte(byte b)
		{
			_out.WriteByte(b);
		}

		public override void Write(byte[] bytes, int off, int len)
		{
			_out.Write(bytes, off, len);
		}

		public override void Close()
		{
			Platform.Dispose(_out);
			_eiGen.Close();
			_cGen.Close();
			_sGen.Close();
			base.Close();
		}
	}

	public const string ZLib = "1.2.840.113549.1.9.16.3.8";

	private int _bufferSize;

	public void SetBufferSize(int bufferSize)
	{
		_bufferSize = bufferSize;
	}

	public Stream Open(Stream outStream, string compressionOID)
	{
		return Open(outStream, CmsObjectIdentifiers.Data.Id, compressionOID);
	}

	public Stream Open(Stream outStream, string contentOID, string compressionOID)
	{
		BerSequenceGenerator berSequenceGenerator = new BerSequenceGenerator(outStream);
		berSequenceGenerator.AddObject(CmsObjectIdentifiers.CompressedData);
		BerSequenceGenerator berSequenceGenerator2 = new BerSequenceGenerator(berSequenceGenerator.GetRawOutputStream(), 0, isExplicit: true);
		berSequenceGenerator2.AddObject(new DerInteger(0));
		berSequenceGenerator2.AddObject(new AlgorithmIdentifier(new DerObjectIdentifier("1.2.840.113549.1.9.16.3.8")));
		BerSequenceGenerator berSequenceGenerator3 = new BerSequenceGenerator(berSequenceGenerator2.GetRawOutputStream());
		berSequenceGenerator3.AddObject(new DerObjectIdentifier(contentOID));
		Stream output = CmsUtilities.CreateBerOctetOutputStream(berSequenceGenerator3.GetRawOutputStream(), 0, isExplicit: true, _bufferSize);
		return new CmsCompressedOutputStream(new ZOutputStream(output, -1), berSequenceGenerator, berSequenceGenerator2, berSequenceGenerator3);
	}
}
