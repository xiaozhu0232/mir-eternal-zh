using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Asn1.X500.Style;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Pkix;

public class PkixNameConstraintValidator
{
	private static readonly DerObjectIdentifier SerialNumberOid = new DerObjectIdentifier("2.5.4.5");

	private ISet excludedSubtreesDN = new HashSet();

	private ISet excludedSubtreesDNS = new HashSet();

	private ISet excludedSubtreesEmail = new HashSet();

	private ISet excludedSubtreesURI = new HashSet();

	private ISet excludedSubtreesIP = new HashSet();

	private ISet excludedSubtreesOtherName = new HashSet();

	private ISet permittedSubtreesDN;

	private ISet permittedSubtreesDNS;

	private ISet permittedSubtreesEmail;

	private ISet permittedSubtreesURI;

	private ISet permittedSubtreesIP;

	private ISet permittedSubtreesOtherName;

	private static bool WithinDNSubtree(Asn1Sequence dns, Asn1Sequence subtree)
	{
		if (subtree.Count < 1 || subtree.Count > dns.Count)
		{
			return false;
		}
		int num = 0;
		Rdn instance = Rdn.GetInstance(subtree[0]);
		for (int i = 0; i < dns.Count; i++)
		{
			num = i;
			Rdn instance2 = Rdn.GetInstance(dns[i]);
			if (IetfUtilities.RdnAreEqual(instance, instance2))
			{
				break;
			}
		}
		if (subtree.Count > dns.Count - num)
		{
			return false;
		}
		for (int j = 0; j < subtree.Count; j++)
		{
			Rdn instance3 = Rdn.GetInstance(subtree[j]);
			Rdn instance4 = Rdn.GetInstance(dns[num + j]);
			if (instance3.Count == 1 && instance4.Count == 1 && instance3.GetFirst().GetType().Equals(SerialNumberOid) && instance4.GetFirst().GetType().Equals(SerialNumberOid))
			{
				if (!Platform.StartsWith(instance4.GetFirst().Value.ToString(), instance3.GetFirst().Value.ToString()))
				{
					return false;
				}
			}
			else if (!IetfUtilities.RdnAreEqual(instance3, instance4))
			{
				return false;
			}
		}
		return true;
	}

	public void CheckPermittedDN(Asn1Sequence dn)
	{
		CheckPermittedDirectory(permittedSubtreesDN, dn);
	}

	public void CheckExcludedDN(Asn1Sequence dn)
	{
		CheckExcludedDirectory(excludedSubtreesDN, dn);
	}

	private ISet IntersectDN(ISet permitted, ISet dns)
	{
		ISet set = new HashSet();
		foreach (GeneralSubtree dn in dns)
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(dn.Base.Name);
			if (permitted == null)
			{
				if (instance != null)
				{
					set.Add(instance);
				}
				continue;
			}
			foreach (object item in permitted)
			{
				Asn1Sequence instance2 = Asn1Sequence.GetInstance(item);
				if (WithinDNSubtree(instance, instance2))
				{
					set.Add(instance);
				}
				else if (WithinDNSubtree(instance2, instance))
				{
					set.Add(instance2);
				}
			}
		}
		return set;
	}

	private ISet UnionDN(ISet excluded, Asn1Sequence dn)
	{
		if (excluded.IsEmpty)
		{
			if (dn == null)
			{
				return excluded;
			}
			excluded.Add(dn);
			return excluded;
		}
		ISet set = new HashSet();
		foreach (object item in excluded)
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(item);
			if (WithinDNSubtree(dn, instance))
			{
				set.Add(instance);
				continue;
			}
			if (WithinDNSubtree(instance, dn))
			{
				set.Add(dn);
				continue;
			}
			set.Add(instance);
			set.Add(dn);
		}
		return set;
	}

	private ISet IntersectOtherName(ISet permitted, ISet otherNames)
	{
		ISet set = new HashSet();
		foreach (GeneralSubtree otherName in otherNames)
		{
			OtherName instance = OtherName.GetInstance(otherName.Base.Name);
			if (instance == null)
			{
				continue;
			}
			if (permitted == null)
			{
				set.Add(instance);
				continue;
			}
			foreach (object item in permitted)
			{
				OtherName instance2 = OtherName.GetInstance(item);
				if (instance2 != null)
				{
					IntersectOtherName(instance, instance2, set);
				}
			}
		}
		return set;
	}

	private void IntersectOtherName(OtherName otherName1, OtherName otherName2, ISet intersect)
	{
		if (otherName1.Equals(otherName2))
		{
			intersect.Add(otherName1);
		}
	}

	private ISet UnionOtherName(ISet permitted, OtherName otherName)
	{
		ISet set = ((permitted != null) ? new HashSet(permitted) : new HashSet());
		set.Add(otherName);
		return set;
	}

	private ISet IntersectEmail(ISet permitted, ISet emails)
	{
		ISet set = new HashSet();
		foreach (GeneralSubtree email2 in emails)
		{
			string text = ExtractNameAsString(email2.Base);
			if (permitted == null)
			{
				if (text != null)
				{
					set.Add(text);
				}
				continue;
			}
			foreach (string item in permitted)
			{
				IntersectEmail(text, item, set);
			}
		}
		return set;
	}

	private ISet UnionEmail(ISet excluded, string email)
	{
		if (excluded.IsEmpty)
		{
			if (email == null)
			{
				return excluded;
			}
			excluded.Add(email);
			return excluded;
		}
		ISet set = new HashSet();
		foreach (string item in excluded)
		{
			UnionEmail(item, email, set);
		}
		return set;
	}

	private ISet IntersectIP(ISet permitted, ISet ips)
	{
		ISet set = new HashSet();
		foreach (GeneralSubtree ip in ips)
		{
			byte[] octets = Asn1OctetString.GetInstance(ip.Base.Name).GetOctets();
			if (permitted == null)
			{
				if (octets != null)
				{
					set.Add(octets);
				}
				continue;
			}
			foreach (byte[] item in permitted)
			{
				set.AddAll(IntersectIPRange(item, octets));
			}
		}
		return set;
	}

	private ISet UnionIP(ISet excluded, byte[] ip)
	{
		if (excluded.IsEmpty)
		{
			if (ip == null)
			{
				return excluded;
			}
			excluded.Add(ip);
			return excluded;
		}
		ISet set = new HashSet();
		foreach (byte[] item in excluded)
		{
			set.AddAll(UnionIPRange(item, ip));
		}
		return set;
	}

	private ISet UnionIPRange(byte[] ipWithSubmask1, byte[] ipWithSubmask2)
	{
		ISet set = new HashSet();
		if (Arrays.AreEqual(ipWithSubmask1, ipWithSubmask2))
		{
			set.Add(ipWithSubmask1);
		}
		else
		{
			set.Add(ipWithSubmask1);
			set.Add(ipWithSubmask2);
		}
		return set;
	}

	private ISet IntersectIPRange(byte[] ipWithSubmask1, byte[] ipWithSubmask2)
	{
		if (ipWithSubmask1.Length != ipWithSubmask2.Length)
		{
			return new HashSet();
		}
		byte[][] array = ExtractIPsAndSubnetMasks(ipWithSubmask1, ipWithSubmask2);
		byte[] ip = array[0];
		byte[] array2 = array[1];
		byte[] ip2 = array[2];
		byte[] array3 = array[3];
		byte[][] array4 = MinMaxIPs(ip, array2, ip2, array3);
		byte[] ip3 = Min(array4[1], array4[3]);
		byte[] ip4 = Max(array4[0], array4[2]);
		if (CompareTo(ip4, ip3) == 1)
		{
			return new HashSet();
		}
		byte[] ip5 = Or(array4[0], array4[2]);
		byte[] subnetMask = Or(array2, array3);
		ISet set = new HashSet();
		set.Add(IpWithSubnetMask(ip5, subnetMask));
		return set;
	}

	private byte[] IpWithSubnetMask(byte[] ip, byte[] subnetMask)
	{
		int num = ip.Length;
		byte[] array = new byte[num * 2];
		Array.Copy(ip, 0, array, 0, num);
		Array.Copy(subnetMask, 0, array, num, num);
		return array;
	}

	private byte[][] ExtractIPsAndSubnetMasks(byte[] ipWithSubmask1, byte[] ipWithSubmask2)
	{
		int num = ipWithSubmask1.Length / 2;
		byte[] array = new byte[num];
		byte[] array2 = new byte[num];
		Array.Copy(ipWithSubmask1, 0, array, 0, num);
		Array.Copy(ipWithSubmask1, num, array2, 0, num);
		byte[] array3 = new byte[num];
		byte[] array4 = new byte[num];
		Array.Copy(ipWithSubmask2, 0, array3, 0, num);
		Array.Copy(ipWithSubmask2, num, array4, 0, num);
		return new byte[4][] { array, array2, array3, array4 };
	}

	private byte[][] MinMaxIPs(byte[] ip1, byte[] subnetmask1, byte[] ip2, byte[] subnetmask2)
	{
		int num = ip1.Length;
		byte[] array = new byte[num];
		byte[] array2 = new byte[num];
		byte[] array3 = new byte[num];
		byte[] array4 = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (byte)(ip1[i] & subnetmask1[i]);
			array2[i] = (byte)((ip1[i] & subnetmask1[i]) | ~subnetmask1[i]);
			array3[i] = (byte)(ip2[i] & subnetmask2[i]);
			array4[i] = (byte)((ip2[i] & subnetmask2[i]) | ~subnetmask2[i]);
		}
		return new byte[4][] { array, array2, array3, array4 };
	}

	private bool IsOtherNameConstrained(OtherName constraint, OtherName otherName)
	{
		return constraint.Equals(otherName);
	}

	private bool IsOtherNameConstrained(ISet constraints, OtherName otherName)
	{
		foreach (object constraint in constraints)
		{
			OtherName instance = OtherName.GetInstance(constraint);
			if (IsOtherNameConstrained(instance, otherName))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPermittedOtherName(ISet permitted, OtherName name)
	{
		if (permitted != null && !IsOtherNameConstrained(permitted, name))
		{
			throw new PkixNameConstraintValidatorException("Subject OtherName is not from a permitted subtree.");
		}
	}

	private void CheckExcludedOtherName(ISet excluded, OtherName name)
	{
		if (IsOtherNameConstrained(excluded, name))
		{
			throw new PkixNameConstraintValidatorException("OtherName is from an excluded subtree.");
		}
	}

	private bool IsEmailConstrained(string constraint, string email)
	{
		string text = email.Substring(email.IndexOf('@') + 1);
		if (constraint.IndexOf('@') != -1)
		{
			if (Platform.ToUpperInvariant(email).Equals(Platform.ToUpperInvariant(constraint)))
			{
				return true;
			}
		}
		else if (!constraint[0].Equals((object)'.'))
		{
			if (Platform.ToUpperInvariant(text).Equals(Platform.ToUpperInvariant(constraint)))
			{
				return true;
			}
		}
		else if (WithinDomain(text, constraint))
		{
			return true;
		}
		return false;
	}

	private bool IsEmailConstrained(ISet constraints, string email)
	{
		foreach (string constraint in constraints)
		{
			if (IsEmailConstrained(constraint, email))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPermittedEmail(ISet permitted, string email)
	{
		if (permitted != null && (email.Length != 0 || !permitted.IsEmpty) && !IsEmailConstrained(permitted, email))
		{
			throw new PkixNameConstraintValidatorException("Subject email address is not from a permitted subtree.");
		}
	}

	private void CheckExcludedEmail(ISet excluded, string email)
	{
		if (IsEmailConstrained(excluded, email))
		{
			throw new PkixNameConstraintValidatorException("Email address is from an excluded subtree.");
		}
	}

	private bool IsDnsConstrained(string constraint, string dns)
	{
		if (!WithinDomain(dns, constraint))
		{
			return Platform.EqualsIgnoreCase(dns, constraint);
		}
		return true;
	}

	private bool IsDnsConstrained(ISet constraints, string dns)
	{
		foreach (string constraint in constraints)
		{
			if (IsDnsConstrained(constraint, dns))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPermittedDns(ISet permitted, string dns)
	{
		if (permitted != null && (dns.Length != 0 || !permitted.IsEmpty) && !IsDnsConstrained(permitted, dns))
		{
			throw new PkixNameConstraintValidatorException("DNS is not from a permitted subtree.");
		}
	}

	private void CheckExcludedDns(ISet excluded, string dns)
	{
		if (IsDnsConstrained(excluded, dns))
		{
			throw new PkixNameConstraintValidatorException("DNS is from an excluded subtree.");
		}
	}

	private bool IsDirectoryConstrained(ISet constraints, Asn1Sequence directory)
	{
		foreach (object constraint in constraints)
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(constraint);
			if (WithinDNSubtree(directory, instance))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPermittedDirectory(ISet permitted, Asn1Sequence directory)
	{
		if (permitted != null && (directory.Count != 0 || !permitted.IsEmpty) && !IsDirectoryConstrained(permitted, directory))
		{
			throw new PkixNameConstraintValidatorException("Subject distinguished name is not from a permitted subtree");
		}
	}

	private void CheckExcludedDirectory(ISet excluded, Asn1Sequence directory)
	{
		if (IsDirectoryConstrained(excluded, directory))
		{
			throw new PkixNameConstraintValidatorException("Subject distinguished name is from an excluded subtree");
		}
	}

	private bool IsUriConstrained(string constraint, string uri)
	{
		string text = ExtractHostFromURL(uri);
		if (Platform.StartsWith(constraint, "."))
		{
			return WithinDomain(text, constraint);
		}
		return Platform.EqualsIgnoreCase(text, constraint);
	}

	private bool IsUriConstrained(ISet constraints, string uri)
	{
		foreach (string constraint in constraints)
		{
			if (IsUriConstrained(constraint, uri))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPermittedUri(ISet permitted, string uri)
	{
		if (permitted != null && (uri.Length != 0 || !permitted.IsEmpty) && !IsUriConstrained(permitted, uri))
		{
			throw new PkixNameConstraintValidatorException("URI is not from a permitted subtree.");
		}
	}

	private void CheckExcludedUri(ISet excluded, string uri)
	{
		if (IsUriConstrained(excluded, uri))
		{
			throw new PkixNameConstraintValidatorException("URI is from an excluded subtree.");
		}
	}

	private bool IsIPConstrained(byte[] constraint, byte[] ip)
	{
		int num = ip.Length;
		if (num != constraint.Length / 2)
		{
			return false;
		}
		byte[] array = new byte[num];
		Array.Copy(constraint, num, array, 0, num);
		byte[] array2 = new byte[num];
		byte[] array3 = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = (byte)(constraint[i] & array[i]);
			array3[i] = (byte)(ip[i] & array[i]);
		}
		return Arrays.AreEqual(array2, array3);
	}

	private bool IsIPConstrained(ISet constraints, byte[] ip)
	{
		foreach (byte[] constraint in constraints)
		{
			if (IsIPConstrained(constraint, ip))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPermittedIP(ISet permitted, byte[] ip)
	{
		if (permitted != null && (ip.Length != 0 || !permitted.IsEmpty) && !IsIPConstrained(permitted, ip))
		{
			throw new PkixNameConstraintValidatorException("IP is not from a permitted subtree.");
		}
	}

	private void CheckExcludedIP(ISet excluded, byte[] ip)
	{
		if (IsIPConstrained(excluded, ip))
		{
			throw new PkixNameConstraintValidatorException("IP is from an excluded subtree.");
		}
	}

	private bool WithinDomain(string testDomain, string domain)
	{
		string text = domain;
		if (Platform.StartsWith(text, "."))
		{
			text = text.Substring(1);
		}
		string[] array = text.Split('.');
		string[] array2 = testDomain.Split('.');
		if (array2.Length <= array.Length)
		{
			return false;
		}
		int num = array2.Length - array.Length;
		for (int i = -1; i < array.Length; i++)
		{
			if (i == -1)
			{
				if (array2[i + num].Length < 1)
				{
					return false;
				}
			}
			else if (!Platform.EqualsIgnoreCase(array2[i + num], array[i]))
			{
				return false;
			}
		}
		return true;
	}

	private void UnionEmail(string email1, string email2, ISet union)
	{
		if (email1.IndexOf('@') != -1)
		{
			string text = email1.Substring(email1.IndexOf('@') + 1);
			if (email2.IndexOf('@') != -1)
			{
				if (Platform.EqualsIgnoreCase(email1, email2))
				{
					union.Add(email1);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(text, email2))
				{
					union.Add(email2);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (Platform.EqualsIgnoreCase(text, email2))
			{
				union.Add(email2);
			}
			else
			{
				union.Add(email1);
				union.Add(email2);
			}
		}
		else if (Platform.StartsWith(email1, "."))
		{
			if (email2.IndexOf('@') != -1)
			{
				string testDomain = email2.Substring(email1.IndexOf('@') + 1);
				if (WithinDomain(testDomain, email1))
				{
					union.Add(email1);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(email1, email2) || Platform.EqualsIgnoreCase(email1, email2))
				{
					union.Add(email2);
					return;
				}
				if (WithinDomain(email2, email1))
				{
					union.Add(email1);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (WithinDomain(email2, email1))
			{
				union.Add(email1);
			}
			else
			{
				union.Add(email1);
				union.Add(email2);
			}
		}
		else if (email2.IndexOf('@') != -1)
		{
			string a = email2.Substring(email1.IndexOf('@') + 1);
			if (Platform.EqualsIgnoreCase(a, email1))
			{
				union.Add(email1);
				return;
			}
			union.Add(email1);
			union.Add(email2);
		}
		else if (Platform.StartsWith(email2, "."))
		{
			if (WithinDomain(email1, email2))
			{
				union.Add(email2);
				return;
			}
			union.Add(email1);
			union.Add(email2);
		}
		else if (Platform.EqualsIgnoreCase(email1, email2))
		{
			union.Add(email1);
		}
		else
		{
			union.Add(email1);
			union.Add(email2);
		}
	}

	private void unionURI(string email1, string email2, ISet union)
	{
		if (email1.IndexOf('@') != -1)
		{
			string text = email1.Substring(email1.IndexOf('@') + 1);
			if (email2.IndexOf('@') != -1)
			{
				if (Platform.EqualsIgnoreCase(email1, email2))
				{
					union.Add(email1);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(text, email2))
				{
					union.Add(email2);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (Platform.EqualsIgnoreCase(text, email2))
			{
				union.Add(email2);
			}
			else
			{
				union.Add(email1);
				union.Add(email2);
			}
		}
		else if (Platform.StartsWith(email1, "."))
		{
			if (email2.IndexOf('@') != -1)
			{
				string testDomain = email2.Substring(email1.IndexOf('@') + 1);
				if (WithinDomain(testDomain, email1))
				{
					union.Add(email1);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(email1, email2) || Platform.EqualsIgnoreCase(email1, email2))
				{
					union.Add(email2);
					return;
				}
				if (WithinDomain(email2, email1))
				{
					union.Add(email1);
					return;
				}
				union.Add(email1);
				union.Add(email2);
			}
			else if (WithinDomain(email2, email1))
			{
				union.Add(email1);
			}
			else
			{
				union.Add(email1);
				union.Add(email2);
			}
		}
		else if (email2.IndexOf('@') != -1)
		{
			string a = email2.Substring(email1.IndexOf('@') + 1);
			if (Platform.EqualsIgnoreCase(a, email1))
			{
				union.Add(email1);
				return;
			}
			union.Add(email1);
			union.Add(email2);
		}
		else if (Platform.StartsWith(email2, "."))
		{
			if (WithinDomain(email1, email2))
			{
				union.Add(email2);
				return;
			}
			union.Add(email1);
			union.Add(email2);
		}
		else if (Platform.EqualsIgnoreCase(email1, email2))
		{
			union.Add(email1);
		}
		else
		{
			union.Add(email1);
			union.Add(email2);
		}
	}

	private ISet IntersectDns(ISet permitted, ISet dnss)
	{
		ISet set = new HashSet();
		foreach (GeneralSubtree item in dnss)
		{
			string text = ExtractNameAsString(item.Base);
			if (permitted == null)
			{
				if (text != null)
				{
					set.Add(text);
				}
				continue;
			}
			foreach (string item2 in permitted)
			{
				if (WithinDomain(item2, text))
				{
					set.Add(item2);
				}
				else if (WithinDomain(text, item2))
				{
					set.Add(text);
				}
			}
		}
		return set;
	}

	private ISet UnionDns(ISet excluded, string dns)
	{
		if (excluded.IsEmpty)
		{
			if (dns == null)
			{
				return excluded;
			}
			excluded.Add(dns);
			return excluded;
		}
		ISet set = new HashSet();
		foreach (string item in excluded)
		{
			if (WithinDomain(item, dns))
			{
				set.Add(dns);
				continue;
			}
			if (WithinDomain(dns, item))
			{
				set.Add(item);
				continue;
			}
			set.Add(item);
			set.Add(dns);
		}
		return set;
	}

	private void IntersectEmail(string email1, string email2, ISet intersect)
	{
		if (email1.IndexOf('@') != -1)
		{
			string text = email1.Substring(email1.IndexOf('@') + 1);
			if (email2.IndexOf('@') != -1)
			{
				if (Platform.EqualsIgnoreCase(email1, email2))
				{
					intersect.Add(email1);
				}
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(text, email2))
				{
					intersect.Add(email1);
				}
			}
			else if (Platform.EqualsIgnoreCase(text, email2))
			{
				intersect.Add(email1);
			}
		}
		else if (Platform.StartsWith(email1, "."))
		{
			if (email2.IndexOf('@') != -1)
			{
				string testDomain = email2.Substring(email1.IndexOf('@') + 1);
				if (WithinDomain(testDomain, email1))
				{
					intersect.Add(email2);
				}
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(email1, email2) || Platform.EqualsIgnoreCase(email1, email2))
				{
					intersect.Add(email1);
				}
				else if (WithinDomain(email2, email1))
				{
					intersect.Add(email2);
				}
			}
			else if (WithinDomain(email2, email1))
			{
				intersect.Add(email2);
			}
		}
		else if (email2.IndexOf('@') != -1)
		{
			string a = email2.Substring(email2.IndexOf('@') + 1);
			if (Platform.EqualsIgnoreCase(a, email1))
			{
				intersect.Add(email2);
			}
		}
		else if (Platform.StartsWith(email2, "."))
		{
			if (WithinDomain(email1, email2))
			{
				intersect.Add(email1);
			}
		}
		else if (Platform.EqualsIgnoreCase(email1, email2))
		{
			intersect.Add(email1);
		}
	}

	private ISet IntersectUri(ISet permitted, ISet uris)
	{
		ISet set = new HashSet();
		foreach (GeneralSubtree uri in uris)
		{
			string text = ExtractNameAsString(uri.Base);
			if (permitted == null)
			{
				if (text != null)
				{
					set.Add(text);
				}
				continue;
			}
			foreach (string item in permitted)
			{
				IntersectUri(item, text, set);
			}
		}
		return set;
	}

	private ISet UnionUri(ISet excluded, string uri)
	{
		if (excluded.IsEmpty)
		{
			if (uri == null)
			{
				return excluded;
			}
			excluded.Add(uri);
			return excluded;
		}
		ISet set = new HashSet();
		foreach (string item in excluded)
		{
			unionURI(item, uri, set);
		}
		return set;
	}

	private void IntersectUri(string email1, string email2, ISet intersect)
	{
		if (email1.IndexOf('@') != -1)
		{
			string text = email1.Substring(email1.IndexOf('@') + 1);
			if (email2.IndexOf('@') != -1)
			{
				if (Platform.EqualsIgnoreCase(email1, email2))
				{
					intersect.Add(email1);
				}
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(text, email2))
				{
					intersect.Add(email1);
				}
			}
			else if (Platform.EqualsIgnoreCase(text, email2))
			{
				intersect.Add(email1);
			}
		}
		else if (Platform.StartsWith(email1, "."))
		{
			if (email2.IndexOf('@') != -1)
			{
				string testDomain = email2.Substring(email1.IndexOf('@') + 1);
				if (WithinDomain(testDomain, email1))
				{
					intersect.Add(email2);
				}
			}
			else if (Platform.StartsWith(email2, "."))
			{
				if (WithinDomain(email1, email2) || Platform.EqualsIgnoreCase(email1, email2))
				{
					intersect.Add(email1);
				}
				else if (WithinDomain(email2, email1))
				{
					intersect.Add(email2);
				}
			}
			else if (WithinDomain(email2, email1))
			{
				intersect.Add(email2);
			}
		}
		else if (email2.IndexOf('@') != -1)
		{
			string a = email2.Substring(email2.IndexOf('@') + 1);
			if (Platform.EqualsIgnoreCase(a, email1))
			{
				intersect.Add(email2);
			}
		}
		else if (Platform.StartsWith(email2, "."))
		{
			if (WithinDomain(email1, email2))
			{
				intersect.Add(email1);
			}
		}
		else if (Platform.EqualsIgnoreCase(email1, email2))
		{
			intersect.Add(email1);
		}
	}

	private static string ExtractHostFromURL(string url)
	{
		string text = url.Substring(url.IndexOf(':') + 1);
		int num = Platform.IndexOf(text, "//");
		if (num != -1)
		{
			text = text.Substring(num + 2);
		}
		if (text.LastIndexOf(':') != -1)
		{
			text = text.Substring(0, text.LastIndexOf(':'));
		}
		text = text.Substring(text.IndexOf(':') + 1);
		text = text.Substring(text.IndexOf('@') + 1);
		if (text.IndexOf('/') != -1)
		{
			text = text.Substring(0, text.IndexOf('/'));
		}
		return text;
	}

	public void checkPermitted(GeneralName name)
	{
		switch (name.TagNo)
		{
		case 0:
			CheckPermittedOtherName(permittedSubtreesOtherName, OtherName.GetInstance(name.Name));
			break;
		case 1:
			CheckPermittedEmail(permittedSubtreesEmail, ExtractNameAsString(name));
			break;
		case 2:
			CheckPermittedDns(permittedSubtreesDNS, ExtractNameAsString(name));
			break;
		case 4:
			CheckPermittedDN(Asn1Sequence.GetInstance(name.Name.ToAsn1Object()));
			break;
		case 6:
			CheckPermittedUri(permittedSubtreesURI, ExtractNameAsString(name));
			break;
		case 7:
			CheckPermittedIP(permittedSubtreesIP, Asn1OctetString.GetInstance(name.Name).GetOctets());
			break;
		case 3:
		case 5:
			break;
		}
	}

	public void checkExcluded(GeneralName name)
	{
		switch (name.TagNo)
		{
		case 0:
			CheckExcludedOtherName(excludedSubtreesOtherName, OtherName.GetInstance(name.Name));
			break;
		case 1:
			CheckExcludedEmail(excludedSubtreesEmail, ExtractNameAsString(name));
			break;
		case 2:
			CheckExcludedDns(excludedSubtreesDNS, ExtractNameAsString(name));
			break;
		case 4:
			CheckExcludedDN(Asn1Sequence.GetInstance(name.Name.ToAsn1Object()));
			break;
		case 6:
			CheckExcludedUri(excludedSubtreesURI, ExtractNameAsString(name));
			break;
		case 7:
			CheckExcludedIP(excludedSubtreesIP, Asn1OctetString.GetInstance(name.Name).GetOctets());
			break;
		case 3:
		case 5:
			break;
		}
	}

	public void IntersectPermittedSubtree(Asn1Sequence permitted)
	{
		IDictionary dictionary = Platform.CreateHashtable();
		foreach (object item in permitted)
		{
			GeneralSubtree instance = GeneralSubtree.GetInstance(item);
			int tagNo = instance.Base.TagNo;
			if (dictionary[tagNo] == null)
			{
				dictionary[tagNo] = new HashSet();
			}
			((ISet)dictionary[tagNo]).Add(instance);
		}
		foreach (object item2 in dictionary)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)item2;
			switch ((int)dictionaryEntry.Key)
			{
			case 0:
				permittedSubtreesOtherName = IntersectOtherName(permittedSubtreesOtherName, (ISet)dictionaryEntry.Value);
				break;
			case 1:
				permittedSubtreesEmail = IntersectEmail(permittedSubtreesEmail, (ISet)dictionaryEntry.Value);
				break;
			case 2:
				permittedSubtreesDNS = IntersectDns(permittedSubtreesDNS, (ISet)dictionaryEntry.Value);
				break;
			case 4:
				permittedSubtreesDN = IntersectDN(permittedSubtreesDN, (ISet)dictionaryEntry.Value);
				break;
			case 6:
				permittedSubtreesURI = IntersectUri(permittedSubtreesURI, (ISet)dictionaryEntry.Value);
				break;
			case 7:
				permittedSubtreesIP = IntersectIP(permittedSubtreesIP, (ISet)dictionaryEntry.Value);
				break;
			}
		}
	}

	private string ExtractNameAsString(GeneralName name)
	{
		return DerIA5String.GetInstance(name.Name).GetString();
	}

	public void IntersectEmptyPermittedSubtree(int nameType)
	{
		switch (nameType)
		{
		case 0:
			permittedSubtreesOtherName = new HashSet();
			break;
		case 1:
			permittedSubtreesEmail = new HashSet();
			break;
		case 2:
			permittedSubtreesDNS = new HashSet();
			break;
		case 4:
			permittedSubtreesDN = new HashSet();
			break;
		case 6:
			permittedSubtreesURI = new HashSet();
			break;
		case 7:
			permittedSubtreesIP = new HashSet();
			break;
		case 3:
		case 5:
			break;
		}
	}

	public void AddExcludedSubtree(GeneralSubtree subtree)
	{
		GeneralName @base = subtree.Base;
		switch (@base.TagNo)
		{
		case 0:
			excludedSubtreesOtherName = UnionOtherName(excludedSubtreesOtherName, OtherName.GetInstance(@base.Name));
			break;
		case 1:
			excludedSubtreesEmail = UnionEmail(excludedSubtreesEmail, ExtractNameAsString(@base));
			break;
		case 2:
			excludedSubtreesDNS = UnionDns(excludedSubtreesDNS, ExtractNameAsString(@base));
			break;
		case 4:
			excludedSubtreesDN = UnionDN(excludedSubtreesDN, (Asn1Sequence)@base.Name.ToAsn1Object());
			break;
		case 6:
			excludedSubtreesURI = UnionUri(excludedSubtreesURI, ExtractNameAsString(@base));
			break;
		case 7:
			excludedSubtreesIP = UnionIP(excludedSubtreesIP, Asn1OctetString.GetInstance(@base.Name).GetOctets());
			break;
		case 3:
		case 5:
			break;
		}
	}

	private static byte[] Max(byte[] ip1, byte[] ip2)
	{
		for (int i = 0; i < ip1.Length; i++)
		{
			if ((ip1[i] & 0xFFFF) > (ip2[i] & 0xFFFF))
			{
				return ip1;
			}
		}
		return ip2;
	}

	private static byte[] Min(byte[] ip1, byte[] ip2)
	{
		for (int i = 0; i < ip1.Length; i++)
		{
			if ((ip1[i] & 0xFFFF) < (ip2[i] & 0xFFFF))
			{
				return ip1;
			}
		}
		return ip2;
	}

	private static int CompareTo(byte[] ip1, byte[] ip2)
	{
		if (Arrays.AreEqual(ip1, ip2))
		{
			return 0;
		}
		if (Arrays.AreEqual(Max(ip1, ip2), ip1))
		{
			return 1;
		}
		return -1;
	}

	private static byte[] Or(byte[] ip1, byte[] ip2)
	{
		byte[] array = new byte[ip1.Length];
		for (int i = 0; i < ip1.Length; i++)
		{
			array[i] = (byte)(ip1[i] | ip2[i]);
		}
		return array;
	}

	[Obsolete("Use GetHashCode instead")]
	public int HashCode()
	{
		return GetHashCode();
	}

	public override int GetHashCode()
	{
		return HashCollection(excludedSubtreesDN) + HashCollection(excludedSubtreesDNS) + HashCollection(excludedSubtreesEmail) + HashCollection(excludedSubtreesIP) + HashCollection(excludedSubtreesURI) + HashCollection(excludedSubtreesOtherName) + HashCollection(permittedSubtreesDN) + HashCollection(permittedSubtreesDNS) + HashCollection(permittedSubtreesEmail) + HashCollection(permittedSubtreesIP) + HashCollection(permittedSubtreesURI) + HashCollection(permittedSubtreesOtherName);
	}

	private int HashCollection(ICollection c)
	{
		if (c == null)
		{
			return 0;
		}
		int num = 0;
		foreach (object item in c)
		{
			num = ((!(item is byte[])) ? (num + item.GetHashCode()) : (num + Arrays.GetHashCode((byte[])item)));
		}
		return num;
	}

	public override bool Equals(object o)
	{
		if (!(o is PkixNameConstraintValidator))
		{
			return false;
		}
		PkixNameConstraintValidator pkixNameConstraintValidator = (PkixNameConstraintValidator)o;
		if (CollectionsAreEqual(pkixNameConstraintValidator.excludedSubtreesDN, excludedSubtreesDN) && CollectionsAreEqual(pkixNameConstraintValidator.excludedSubtreesDNS, excludedSubtreesDNS) && CollectionsAreEqual(pkixNameConstraintValidator.excludedSubtreesEmail, excludedSubtreesEmail) && CollectionsAreEqual(pkixNameConstraintValidator.excludedSubtreesIP, excludedSubtreesIP) && CollectionsAreEqual(pkixNameConstraintValidator.excludedSubtreesURI, excludedSubtreesURI) && CollectionsAreEqual(pkixNameConstraintValidator.excludedSubtreesOtherName, excludedSubtreesOtherName) && CollectionsAreEqual(pkixNameConstraintValidator.permittedSubtreesDN, permittedSubtreesDN) && CollectionsAreEqual(pkixNameConstraintValidator.permittedSubtreesDNS, permittedSubtreesDNS) && CollectionsAreEqual(pkixNameConstraintValidator.permittedSubtreesEmail, permittedSubtreesEmail) && CollectionsAreEqual(pkixNameConstraintValidator.permittedSubtreesIP, permittedSubtreesIP) && CollectionsAreEqual(pkixNameConstraintValidator.permittedSubtreesURI, permittedSubtreesURI))
		{
			return CollectionsAreEqual(pkixNameConstraintValidator.permittedSubtreesOtherName, permittedSubtreesOtherName);
		}
		return false;
	}

	private bool CollectionsAreEqual(ICollection coll1, ICollection coll2)
	{
		if (coll1 == coll2)
		{
			return true;
		}
		if (coll1 == null || coll2 == null || coll1.Count != coll2.Count)
		{
			return false;
		}
		foreach (object item in coll1)
		{
			bool flag = false;
			foreach (object item2 in coll2)
			{
				if (SpecialEquals(item, item2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private bool SpecialEquals(object o1, object o2)
	{
		if (o1 == o2)
		{
			return true;
		}
		if (o1 == null || o2 == null)
		{
			return false;
		}
		if (o1 is byte[] && o2 is byte[])
		{
			return Arrays.AreEqual((byte[])o1, (byte[])o2);
		}
		return o1.Equals(o2);
	}

	private string StringifyIP(byte[] ip)
	{
		string text = "";
		for (int i = 0; i < ip.Length / 2; i++)
		{
			text = text + (ip[i] & 0xFF) + ".";
		}
		text = text.Substring(0, text.Length - 1);
		text += "/";
		for (int j = ip.Length / 2; j < ip.Length; j++)
		{
			text = text + (ip[j] & 0xFF) + ".";
		}
		return text.Substring(0, text.Length - 1);
	}

	private string StringifyIPCollection(ISet ips)
	{
		string text = "";
		text += "[";
		foreach (byte[] ip in ips)
		{
			text = text + StringifyIP(ip) + ",";
		}
		if (text.Length > 1)
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text + "]";
	}

	private string StringifyOtherNameCollection(ISet otherNames)
	{
		string text = "";
		text += "[";
		foreach (object otherName in otherNames)
		{
			OtherName instance = OtherName.GetInstance(otherName);
			if (text.Length > 1)
			{
				text += ",";
			}
			text += instance.TypeID.Id;
			text += ":";
			try
			{
				text += Hex.ToHexString(instance.Value.ToAsn1Object().GetEncoded());
			}
			catch (IOException ex)
			{
				text += ex.ToString();
			}
		}
		return text + "]";
	}

	public override string ToString()
	{
		string text = "";
		text += "permitted:\n";
		if (permittedSubtreesDN != null)
		{
			text += "DN:\n";
			text = text + permittedSubtreesDN.ToString() + "\n";
		}
		if (permittedSubtreesDNS != null)
		{
			text += "DNS:\n";
			text = text + permittedSubtreesDNS.ToString() + "\n";
		}
		if (permittedSubtreesEmail != null)
		{
			text += "Email:\n";
			text = text + permittedSubtreesEmail.ToString() + "\n";
		}
		if (permittedSubtreesURI != null)
		{
			text += "URI:\n";
			text = text + permittedSubtreesURI.ToString() + "\n";
		}
		if (permittedSubtreesIP != null)
		{
			text += "IP:\n";
			text = text + StringifyIPCollection(permittedSubtreesIP) + "\n";
		}
		if (permittedSubtreesOtherName != null)
		{
			text += "OtherName:\n";
			text += StringifyOtherNameCollection(permittedSubtreesOtherName);
		}
		text += "excluded:\n";
		if (!excludedSubtreesDN.IsEmpty)
		{
			text += "DN:\n";
			text = text + excludedSubtreesDN.ToString() + "\n";
		}
		if (!excludedSubtreesDNS.IsEmpty)
		{
			text += "DNS:\n";
			text = text + excludedSubtreesDNS.ToString() + "\n";
		}
		if (!excludedSubtreesEmail.IsEmpty)
		{
			text += "Email:\n";
			text = text + excludedSubtreesEmail.ToString() + "\n";
		}
		if (!excludedSubtreesURI.IsEmpty)
		{
			text += "URI:\n";
			text = text + excludedSubtreesURI.ToString() + "\n";
		}
		if (!excludedSubtreesIP.IsEmpty)
		{
			text += "IP:\n";
			text = text + StringifyIPCollection(excludedSubtreesIP) + "\n";
		}
		if (!excludedSubtreesOtherName.IsEmpty)
		{
			text += "OtherName:\n";
			text += StringifyOtherNameCollection(excludedSubtreesOtherName);
		}
		return text;
	}
}
