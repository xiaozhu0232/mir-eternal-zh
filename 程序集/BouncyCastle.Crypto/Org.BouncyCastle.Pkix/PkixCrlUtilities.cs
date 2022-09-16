using System;
using System.Collections;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix;

public class PkixCrlUtilities
{
	public virtual ISet FindCrls(X509CrlStoreSelector crlselect, PkixParameters paramsPkix, DateTime currentDate)
	{
		ISet set = new HashSet();
		try
		{
			set.AddAll(FindCrls(crlselect, paramsPkix.GetAdditionalStores()));
			set.AddAll(FindCrls(crlselect, paramsPkix.GetStores()));
		}
		catch (Exception innerException)
		{
			throw new Exception("Exception obtaining complete CRLs.", innerException);
		}
		ISet set2 = new HashSet();
		DateTime dateTime = currentDate;
		if (paramsPkix.Date != null)
		{
			dateTime = paramsPkix.Date.Value;
		}
		foreach (X509Crl item in set)
		{
			if (item.NextUpdate.Value.CompareTo((object)dateTime) <= 0)
			{
				continue;
			}
			X509Certificate certificateChecking = crlselect.CertificateChecking;
			if (certificateChecking != null)
			{
				if (item.ThisUpdate.CompareTo((object)certificateChecking.NotAfter) < 0)
				{
					set2.Add(item);
				}
			}
			else
			{
				set2.Add(item);
			}
		}
		return set2;
	}

	public virtual ISet FindCrls(X509CrlStoreSelector crlselect, PkixParameters paramsPkix)
	{
		ISet set = new HashSet();
		try
		{
			set.AddAll(FindCrls(crlselect, paramsPkix.GetStores()));
			return set;
		}
		catch (Exception innerException)
		{
			throw new Exception("Exception obtaining complete CRLs.", innerException);
		}
	}

	private ICollection FindCrls(X509CrlStoreSelector crlSelect, IList crlStores)
	{
		ISet set = new HashSet();
		Exception ex = null;
		bool flag = false;
		foreach (IX509Store crlStore in crlStores)
		{
			try
			{
				set.AddAll(crlStore.GetMatches(crlSelect));
				flag = true;
			}
			catch (X509StoreException innerException)
			{
				ex = new Exception("Exception searching in X.509 CRL store.", innerException);
			}
		}
		if (!flag && ex != null)
		{
			throw ex;
		}
		return set;
	}
}
