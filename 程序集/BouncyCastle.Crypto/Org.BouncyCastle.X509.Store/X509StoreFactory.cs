using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509.Store;

public sealed class X509StoreFactory
{
	private X509StoreFactory()
	{
	}

	public static IX509Store Create(string type, IX509StoreParameters parameters)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		string[] array = Platform.ToUpperInvariant(type).Split('/');
		if (array.Length < 2)
		{
			throw new ArgumentException("type");
		}
		if (array[1] != "COLLECTION")
		{
			throw new NoSuchStoreException("X.509 store type '" + type + "' not available.");
		}
		X509CollectionStoreParameters x509CollectionStoreParameters = (X509CollectionStoreParameters)parameters;
		ICollection collection = x509CollectionStoreParameters.GetCollection();
		switch (array[0])
		{
		case "ATTRIBUTECERTIFICATE":
			checkCorrectType(collection, typeof(IX509AttributeCertificate));
			break;
		case "CERTIFICATE":
			checkCorrectType(collection, typeof(X509Certificate));
			break;
		case "CERTIFICATEPAIR":
			checkCorrectType(collection, typeof(X509CertificatePair));
			break;
		case "CRL":
			checkCorrectType(collection, typeof(X509Crl));
			break;
		default:
			throw new NoSuchStoreException("X.509 store type '" + type + "' not available.");
		}
		return new X509CollectionStore(collection);
	}

	private static void checkCorrectType(ICollection coll, Type t)
	{
		foreach (object item in coll)
		{
			if (!t.IsInstanceOfType(item))
			{
				throw new InvalidCastException("Can't cast object to type: " + t.FullName);
			}
		}
	}
}
