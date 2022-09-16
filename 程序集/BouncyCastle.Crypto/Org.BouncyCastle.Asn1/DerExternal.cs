using System;
using System.IO;

namespace Org.BouncyCastle.Asn1;

public class DerExternal : Asn1Object
{
	private DerObjectIdentifier directReference;

	private DerInteger indirectReference;

	private Asn1Object dataValueDescriptor;

	private int encoding;

	private Asn1Object externalContent;

	public Asn1Object DataValueDescriptor
	{
		get
		{
			return dataValueDescriptor;
		}
		set
		{
			dataValueDescriptor = value;
		}
	}

	public DerObjectIdentifier DirectReference
	{
		get
		{
			return directReference;
		}
		set
		{
			directReference = value;
		}
	}

	public int Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			if (encoding < 0 || encoding > 2)
			{
				throw new InvalidOperationException("invalid encoding value: " + encoding);
			}
			encoding = value;
		}
	}

	public Asn1Object ExternalContent
	{
		get
		{
			return externalContent;
		}
		set
		{
			externalContent = value;
		}
	}

	public DerInteger IndirectReference
	{
		get
		{
			return indirectReference;
		}
		set
		{
			indirectReference = value;
		}
	}

	public DerExternal(Asn1EncodableVector vector)
	{
		int num = 0;
		Asn1Object objFromVector = GetObjFromVector(vector, num);
		if (objFromVector is DerObjectIdentifier)
		{
			directReference = (DerObjectIdentifier)objFromVector;
			num++;
			objFromVector = GetObjFromVector(vector, num);
		}
		if (objFromVector is DerInteger)
		{
			indirectReference = (DerInteger)objFromVector;
			num++;
			objFromVector = GetObjFromVector(vector, num);
		}
		if (!(objFromVector is Asn1TaggedObject))
		{
			dataValueDescriptor = objFromVector;
			num++;
			objFromVector = GetObjFromVector(vector, num);
		}
		if (vector.Count != num + 1)
		{
			throw new ArgumentException("input vector too large", "vector");
		}
		if (!(objFromVector is Asn1TaggedObject))
		{
			throw new ArgumentException("No tagged object found in vector. Structure doesn't seem to be of type External", "vector");
		}
		Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)objFromVector;
		Encoding = asn1TaggedObject.TagNo;
		if (encoding < 0 || encoding > 2)
		{
			throw new InvalidOperationException("invalid encoding value");
		}
		externalContent = asn1TaggedObject.GetObject();
	}

	public DerExternal(DerObjectIdentifier directReference, DerInteger indirectReference, Asn1Object dataValueDescriptor, DerTaggedObject externalData)
		: this(directReference, indirectReference, dataValueDescriptor, externalData.TagNo, externalData.ToAsn1Object())
	{
	}

	public DerExternal(DerObjectIdentifier directReference, DerInteger indirectReference, Asn1Object dataValueDescriptor, int encoding, Asn1Object externalData)
	{
		DirectReference = directReference;
		IndirectReference = indirectReference;
		DataValueDescriptor = dataValueDescriptor;
		Encoding = encoding;
		ExternalContent = externalData.ToAsn1Object();
	}

	internal override void Encode(DerOutputStream derOut)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteEncodable(memoryStream, directReference);
		WriteEncodable(memoryStream, indirectReference);
		WriteEncodable(memoryStream, dataValueDescriptor);
		WriteEncodable(memoryStream, new DerTaggedObject(8, externalContent));
		derOut.WriteEncoded(32, 8, memoryStream.ToArray());
	}

	protected override int Asn1GetHashCode()
	{
		int num = externalContent.GetHashCode();
		if (directReference != null)
		{
			num ^= directReference.GetHashCode();
		}
		if (indirectReference != null)
		{
			num ^= indirectReference.GetHashCode();
		}
		if (dataValueDescriptor != null)
		{
			num ^= dataValueDescriptor.GetHashCode();
		}
		return num;
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (this == asn1Object)
		{
			return true;
		}
		if (!(asn1Object is DerExternal derExternal))
		{
			return false;
		}
		if (object.Equals(directReference, derExternal.directReference) && object.Equals(indirectReference, derExternal.indirectReference) && object.Equals(dataValueDescriptor, derExternal.dataValueDescriptor))
		{
			return externalContent.Equals(derExternal.externalContent);
		}
		return false;
	}

	private static Asn1Object GetObjFromVector(Asn1EncodableVector v, int index)
	{
		if (v.Count <= index)
		{
			throw new ArgumentException("too few objects in input vector", "v");
		}
		return v[index].ToAsn1Object();
	}

	private static void WriteEncodable(MemoryStream ms, Asn1Encodable e)
	{
		if (e != null)
		{
			byte[] derEncoded = e.GetDerEncoded();
			ms.Write(derEncoded, 0, derEncoded.Length);
		}
	}
}
