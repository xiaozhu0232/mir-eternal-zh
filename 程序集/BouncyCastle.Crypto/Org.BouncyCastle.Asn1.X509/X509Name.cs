using System;
using System.Collections;
using System.IO;
using System.Text;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.X509;

public class X509Name : Asn1Encodable
{
	public static readonly DerObjectIdentifier C;

	public static readonly DerObjectIdentifier O;

	public static readonly DerObjectIdentifier OU;

	public static readonly DerObjectIdentifier T;

	public static readonly DerObjectIdentifier CN;

	public static readonly DerObjectIdentifier Street;

	public static readonly DerObjectIdentifier SerialNumber;

	public static readonly DerObjectIdentifier L;

	public static readonly DerObjectIdentifier ST;

	public static readonly DerObjectIdentifier Surname;

	public static readonly DerObjectIdentifier GivenName;

	public static readonly DerObjectIdentifier Initials;

	public static readonly DerObjectIdentifier Generation;

	public static readonly DerObjectIdentifier UniqueIdentifier;

	public static readonly DerObjectIdentifier BusinessCategory;

	public static readonly DerObjectIdentifier PostalCode;

	public static readonly DerObjectIdentifier DnQualifier;

	public static readonly DerObjectIdentifier Pseudonym;

	public static readonly DerObjectIdentifier DateOfBirth;

	public static readonly DerObjectIdentifier PlaceOfBirth;

	public static readonly DerObjectIdentifier Gender;

	public static readonly DerObjectIdentifier CountryOfCitizenship;

	public static readonly DerObjectIdentifier CountryOfResidence;

	public static readonly DerObjectIdentifier NameAtBirth;

	public static readonly DerObjectIdentifier PostalAddress;

	public static readonly DerObjectIdentifier DmdName;

	public static readonly DerObjectIdentifier TelephoneNumber;

	public static readonly DerObjectIdentifier OrganizationIdentifier;

	public static readonly DerObjectIdentifier Name;

	public static readonly DerObjectIdentifier EmailAddress;

	public static readonly DerObjectIdentifier UnstructuredName;

	public static readonly DerObjectIdentifier UnstructuredAddress;

	public static readonly DerObjectIdentifier E;

	public static readonly DerObjectIdentifier DC;

	public static readonly DerObjectIdentifier UID;

	private static readonly bool[] defaultReverse;

	public static readonly Hashtable DefaultSymbols;

	public static readonly Hashtable RFC2253Symbols;

	public static readonly Hashtable RFC1779Symbols;

	public static readonly Hashtable DefaultLookup;

	private readonly IList ordering = Platform.CreateArrayList();

	private readonly X509NameEntryConverter converter;

	private IList values = Platform.CreateArrayList();

	private IList added = Platform.CreateArrayList();

	private Asn1Sequence seq;

	public static bool DefaultReverse
	{
		get
		{
			return defaultReverse[0];
		}
		set
		{
			defaultReverse[0] = value;
		}
	}

	static X509Name()
	{
		C = new DerObjectIdentifier("2.5.4.6");
		O = new DerObjectIdentifier("2.5.4.10");
		OU = new DerObjectIdentifier("2.5.4.11");
		T = new DerObjectIdentifier("2.5.4.12");
		CN = new DerObjectIdentifier("2.5.4.3");
		Street = new DerObjectIdentifier("2.5.4.9");
		SerialNumber = new DerObjectIdentifier("2.5.4.5");
		L = new DerObjectIdentifier("2.5.4.7");
		ST = new DerObjectIdentifier("2.5.4.8");
		Surname = new DerObjectIdentifier("2.5.4.4");
		GivenName = new DerObjectIdentifier("2.5.4.42");
		Initials = new DerObjectIdentifier("2.5.4.43");
		Generation = new DerObjectIdentifier("2.5.4.44");
		UniqueIdentifier = new DerObjectIdentifier("2.5.4.45");
		BusinessCategory = new DerObjectIdentifier("2.5.4.15");
		PostalCode = new DerObjectIdentifier("2.5.4.17");
		DnQualifier = new DerObjectIdentifier("2.5.4.46");
		Pseudonym = new DerObjectIdentifier("2.5.4.65");
		DateOfBirth = new DerObjectIdentifier("1.3.6.1.5.5.7.9.1");
		PlaceOfBirth = new DerObjectIdentifier("1.3.6.1.5.5.7.9.2");
		Gender = new DerObjectIdentifier("1.3.6.1.5.5.7.9.3");
		CountryOfCitizenship = new DerObjectIdentifier("1.3.6.1.5.5.7.9.4");
		CountryOfResidence = new DerObjectIdentifier("1.3.6.1.5.5.7.9.5");
		NameAtBirth = new DerObjectIdentifier("1.3.36.8.3.14");
		PostalAddress = new DerObjectIdentifier("2.5.4.16");
		DmdName = new DerObjectIdentifier("2.5.4.54");
		TelephoneNumber = X509ObjectIdentifiers.id_at_telephoneNumber;
		OrganizationIdentifier = X509ObjectIdentifiers.id_at_organizationIdentifier;
		Name = X509ObjectIdentifiers.id_at_name;
		EmailAddress = PkcsObjectIdentifiers.Pkcs9AtEmailAddress;
		UnstructuredName = PkcsObjectIdentifiers.Pkcs9AtUnstructuredName;
		UnstructuredAddress = PkcsObjectIdentifiers.Pkcs9AtUnstructuredAddress;
		E = EmailAddress;
		DC = new DerObjectIdentifier("0.9.2342.19200300.100.1.25");
		UID = new DerObjectIdentifier("0.9.2342.19200300.100.1.1");
		bool[] array = (defaultReverse = new bool[1]);
		DefaultSymbols = new Hashtable();
		RFC2253Symbols = new Hashtable();
		RFC1779Symbols = new Hashtable();
		DefaultLookup = new Hashtable();
		DefaultSymbols.Add(C, "C");
		DefaultSymbols.Add(O, "O");
		DefaultSymbols.Add(T, "T");
		DefaultSymbols.Add(OU, "OU");
		DefaultSymbols.Add(CN, "CN");
		DefaultSymbols.Add(L, "L");
		DefaultSymbols.Add(ST, "ST");
		DefaultSymbols.Add(SerialNumber, "SERIALNUMBER");
		DefaultSymbols.Add(EmailAddress, "E");
		DefaultSymbols.Add(DC, "DC");
		DefaultSymbols.Add(UID, "UID");
		DefaultSymbols.Add(Street, "STREET");
		DefaultSymbols.Add(Surname, "SURNAME");
		DefaultSymbols.Add(GivenName, "GIVENNAME");
		DefaultSymbols.Add(Initials, "INITIALS");
		DefaultSymbols.Add(Generation, "GENERATION");
		DefaultSymbols.Add(UnstructuredAddress, "unstructuredAddress");
		DefaultSymbols.Add(UnstructuredName, "unstructuredName");
		DefaultSymbols.Add(UniqueIdentifier, "UniqueIdentifier");
		DefaultSymbols.Add(DnQualifier, "DN");
		DefaultSymbols.Add(Pseudonym, "Pseudonym");
		DefaultSymbols.Add(PostalAddress, "PostalAddress");
		DefaultSymbols.Add(NameAtBirth, "NameAtBirth");
		DefaultSymbols.Add(CountryOfCitizenship, "CountryOfCitizenship");
		DefaultSymbols.Add(CountryOfResidence, "CountryOfResidence");
		DefaultSymbols.Add(Gender, "Gender");
		DefaultSymbols.Add(PlaceOfBirth, "PlaceOfBirth");
		DefaultSymbols.Add(DateOfBirth, "DateOfBirth");
		DefaultSymbols.Add(PostalCode, "PostalCode");
		DefaultSymbols.Add(BusinessCategory, "BusinessCategory");
		DefaultSymbols.Add(TelephoneNumber, "TelephoneNumber");
		RFC2253Symbols.Add(C, "C");
		RFC2253Symbols.Add(O, "O");
		RFC2253Symbols.Add(OU, "OU");
		RFC2253Symbols.Add(CN, "CN");
		RFC2253Symbols.Add(L, "L");
		RFC2253Symbols.Add(ST, "ST");
		RFC2253Symbols.Add(Street, "STREET");
		RFC2253Symbols.Add(DC, "DC");
		RFC2253Symbols.Add(UID, "UID");
		RFC1779Symbols.Add(C, "C");
		RFC1779Symbols.Add(O, "O");
		RFC1779Symbols.Add(OU, "OU");
		RFC1779Symbols.Add(CN, "CN");
		RFC1779Symbols.Add(L, "L");
		RFC1779Symbols.Add(ST, "ST");
		RFC1779Symbols.Add(Street, "STREET");
		DefaultLookup.Add("c", C);
		DefaultLookup.Add("o", O);
		DefaultLookup.Add("t", T);
		DefaultLookup.Add("ou", OU);
		DefaultLookup.Add("cn", CN);
		DefaultLookup.Add("l", L);
		DefaultLookup.Add("st", ST);
		DefaultLookup.Add("serialnumber", SerialNumber);
		DefaultLookup.Add("street", Street);
		DefaultLookup.Add("emailaddress", E);
		DefaultLookup.Add("dc", DC);
		DefaultLookup.Add("e", E);
		DefaultLookup.Add("uid", UID);
		DefaultLookup.Add("surname", Surname);
		DefaultLookup.Add("givenname", GivenName);
		DefaultLookup.Add("initials", Initials);
		DefaultLookup.Add("generation", Generation);
		DefaultLookup.Add("unstructuredaddress", UnstructuredAddress);
		DefaultLookup.Add("unstructuredname", UnstructuredName);
		DefaultLookup.Add("uniqueidentifier", UniqueIdentifier);
		DefaultLookup.Add("dn", DnQualifier);
		DefaultLookup.Add("pseudonym", Pseudonym);
		DefaultLookup.Add("postaladdress", PostalAddress);
		DefaultLookup.Add("nameofbirth", NameAtBirth);
		DefaultLookup.Add("countryofcitizenship", CountryOfCitizenship);
		DefaultLookup.Add("countryofresidence", CountryOfResidence);
		DefaultLookup.Add("gender", Gender);
		DefaultLookup.Add("placeofbirth", PlaceOfBirth);
		DefaultLookup.Add("dateofbirth", DateOfBirth);
		DefaultLookup.Add("postalcode", PostalCode);
		DefaultLookup.Add("businesscategory", BusinessCategory);
		DefaultLookup.Add("telephonenumber", TelephoneNumber);
	}

	public static X509Name GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static X509Name GetInstance(object obj)
	{
		if (obj is X509Name)
		{
			return (X509Name)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new X509Name(Asn1Sequence.GetInstance(obj));
	}

	protected X509Name()
	{
	}

	protected X509Name(Asn1Sequence seq)
	{
		this.seq = seq;
		foreach (Asn1Encodable item in seq)
		{
			Asn1Set instance = Asn1Set.GetInstance(item.ToAsn1Object());
			for (int i = 0; i < instance.Count; i++)
			{
				Asn1Sequence instance2 = Asn1Sequence.GetInstance(instance[i].ToAsn1Object());
				if (instance2.Count != 2)
				{
					throw new ArgumentException("badly sized pair");
				}
				ordering.Add(DerObjectIdentifier.GetInstance(instance2[0].ToAsn1Object()));
				Asn1Object asn1Object = instance2[1].ToAsn1Object();
				if (asn1Object is IAsn1String && !(asn1Object is DerUniversalString))
				{
					string text = ((IAsn1String)asn1Object).GetString();
					if (Platform.StartsWith(text, "#"))
					{
						text = "\\" + text;
					}
					values.Add(text);
				}
				else
				{
					values.Add("#" + Hex.ToHexString(asn1Object.GetEncoded()));
				}
				added.Add(i != 0);
			}
		}
	}

	public X509Name(IList ordering, IDictionary attributes)
		: this(ordering, attributes, new X509DefaultEntryConverter())
	{
	}

	public X509Name(IList ordering, IDictionary attributes, X509NameEntryConverter converter)
	{
		this.converter = converter;
		foreach (DerObjectIdentifier item in ordering)
		{
			object obj = attributes[item];
			if (obj == null)
			{
				throw new ArgumentException(string.Concat("No attribute for object id - ", item, " - passed to distinguished name"));
			}
			this.ordering.Add(item);
			added.Add(false);
			values.Add(obj);
		}
	}

	public X509Name(IList oids, IList values)
		: this(oids, values, new X509DefaultEntryConverter())
	{
	}

	public X509Name(IList oids, IList values, X509NameEntryConverter converter)
	{
		this.converter = converter;
		if (oids.Count != values.Count)
		{
			throw new ArgumentException("'oids' must be same length as 'values'.");
		}
		for (int i = 0; i < oids.Count; i++)
		{
			ordering.Add(oids[i]);
			this.values.Add(values[i]);
			added.Add(false);
		}
	}

	public X509Name(string dirName)
		: this(DefaultReverse, DefaultLookup, dirName)
	{
	}

	public X509Name(string dirName, X509NameEntryConverter converter)
		: this(DefaultReverse, DefaultLookup, dirName, converter)
	{
	}

	public X509Name(bool reverse, string dirName)
		: this(reverse, DefaultLookup, dirName)
	{
	}

	public X509Name(bool reverse, string dirName, X509NameEntryConverter converter)
		: this(reverse, DefaultLookup, dirName, converter)
	{
	}

	public X509Name(bool reverse, IDictionary lookUp, string dirName)
		: this(reverse, lookUp, dirName, new X509DefaultEntryConverter())
	{
	}

	private DerObjectIdentifier DecodeOid(string name, IDictionary lookUp)
	{
		if (Platform.StartsWith(Platform.ToUpperInvariant(name), "OID."))
		{
			return new DerObjectIdentifier(name.Substring(4));
		}
		if (name[0] >= '0' && name[0] <= '9')
		{
			return new DerObjectIdentifier(name);
		}
		DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)lookUp[Platform.ToLowerInvariant(name)];
		if (derObjectIdentifier == null)
		{
			throw new ArgumentException("Unknown object id - " + name + " - passed to distinguished name");
		}
		return derObjectIdentifier;
	}

	public X509Name(bool reverse, IDictionary lookUp, string dirName, X509NameEntryConverter converter)
	{
		this.converter = converter;
		X509NameTokenizer x509NameTokenizer = new X509NameTokenizer(dirName);
		while (x509NameTokenizer.HasMoreTokens())
		{
			string text = x509NameTokenizer.NextToken();
			int num = text.IndexOf('=');
			if (num == -1)
			{
				throw new ArgumentException("badly formated directory string");
			}
			string name = text.Substring(0, num);
			string text2 = text.Substring(num + 1);
			DerObjectIdentifier value = DecodeOid(name, lookUp);
			if (text2.IndexOf('+') > 0)
			{
				X509NameTokenizer x509NameTokenizer2 = new X509NameTokenizer(text2, '+');
				string value2 = x509NameTokenizer2.NextToken();
				ordering.Add(value);
				values.Add(value2);
				added.Add(false);
				while (x509NameTokenizer2.HasMoreTokens())
				{
					string text3 = x509NameTokenizer2.NextToken();
					int num2 = text3.IndexOf('=');
					string name2 = text3.Substring(0, num2);
					string value3 = text3.Substring(num2 + 1);
					ordering.Add(DecodeOid(name2, lookUp));
					values.Add(value3);
					added.Add(true);
				}
			}
			else
			{
				ordering.Add(value);
				values.Add(text2);
				added.Add(false);
			}
		}
		if (!reverse)
		{
			return;
		}
		IList list = Platform.CreateArrayList();
		IList list2 = Platform.CreateArrayList();
		IList list3 = Platform.CreateArrayList();
		int num3 = 1;
		for (int i = 0; i < ordering.Count; i++)
		{
			if (!(bool)added[i])
			{
				num3 = 0;
			}
			int index = num3++;
			list.Insert(index, ordering[i]);
			list2.Insert(index, values[i]);
			list3.Insert(index, added[i]);
		}
		ordering = list;
		values = list2;
		added = list3;
	}

	public IList GetOidList()
	{
		return Platform.CreateArrayList(ordering);
	}

	public IList GetValueList()
	{
		return GetValueList(null);
	}

	public IList GetValueList(DerObjectIdentifier oid)
	{
		IList list = Platform.CreateArrayList();
		for (int i = 0; i != values.Count; i++)
		{
			if (oid == null || oid.Equals(ordering[i]))
			{
				string text = (string)values[i];
				if (Platform.StartsWith(text, "\\#"))
				{
					text = text.Substring(1);
				}
				list.Add(text);
			}
		}
		return list;
	}

	public override Asn1Object ToAsn1Object()
	{
		if (seq == null)
		{
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
			Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
			DerObjectIdentifier derObjectIdentifier = null;
			for (int i = 0; i != ordering.Count; i++)
			{
				DerObjectIdentifier derObjectIdentifier2 = (DerObjectIdentifier)ordering[i];
				string value = (string)values[i];
				if (derObjectIdentifier != null && !(bool)added[i])
				{
					asn1EncodableVector.Add(new DerSet(asn1EncodableVector2));
					asn1EncodableVector2 = new Asn1EncodableVector();
				}
				asn1EncodableVector2.Add(new DerSequence(derObjectIdentifier2, converter.GetConvertedValue(derObjectIdentifier2, value)));
				derObjectIdentifier = derObjectIdentifier2;
			}
			asn1EncodableVector.Add(new DerSet(asn1EncodableVector2));
			seq = new DerSequence(asn1EncodableVector);
		}
		return seq;
	}

	public bool Equivalent(X509Name other, bool inOrder)
	{
		if (!inOrder)
		{
			return Equivalent(other);
		}
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		int count = ordering.Count;
		if (count != other.ordering.Count)
		{
			return false;
		}
		for (int i = 0; i < count; i++)
		{
			DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)ordering[i];
			DerObjectIdentifier obj = (DerObjectIdentifier)other.ordering[i];
			if (!derObjectIdentifier.Equals(obj))
			{
				return false;
			}
			string s = (string)values[i];
			string s2 = (string)other.values[i];
			if (!equivalentStrings(s, s2))
			{
				return false;
			}
		}
		return true;
	}

	public bool Equivalent(X509Name other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		int count = ordering.Count;
		if (count != other.ordering.Count)
		{
			return false;
		}
		bool[] array = new bool[count];
		int num;
		int num2;
		int num3;
		if (ordering[0].Equals(other.ordering[0]))
		{
			num = 0;
			num2 = count;
			num3 = 1;
		}
		else
		{
			num = count - 1;
			num2 = -1;
			num3 = -1;
		}
		for (int i = num; i != num2; i += num3)
		{
			bool flag = false;
			DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)ordering[i];
			string s = (string)values[i];
			for (int j = 0; j < count; j++)
			{
				if (array[j])
				{
					continue;
				}
				DerObjectIdentifier obj = (DerObjectIdentifier)other.ordering[j];
				if (derObjectIdentifier.Equals(obj))
				{
					string s2 = (string)other.values[j];
					if (equivalentStrings(s, s2))
					{
						array[j] = true;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private static bool equivalentStrings(string s1, string s2)
	{
		string text = canonicalize(s1);
		string text2 = canonicalize(s2);
		if (!text.Equals(text2))
		{
			text = stripInternalSpaces(text);
			text2 = stripInternalSpaces(text2);
			if (!text.Equals(text2))
			{
				return false;
			}
		}
		return true;
	}

	private static string canonicalize(string s)
	{
		string text = Platform.ToLowerInvariant(s).Trim();
		if (Platform.StartsWith(text, "#"))
		{
			Asn1Object asn1Object = decodeObject(text);
			if (asn1Object is IAsn1String)
			{
				text = Platform.ToLowerInvariant(((IAsn1String)asn1Object).GetString()).Trim();
			}
		}
		return text;
	}

	private static Asn1Object decodeObject(string v)
	{
		try
		{
			return Asn1Object.FromByteArray(Hex.DecodeStrict(v, 1, v.Length - 1));
		}
		catch (IOException ex)
		{
			throw new InvalidOperationException("unknown encoding in name: " + ex.Message, ex);
		}
	}

	private static string stripInternalSpaces(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (str.Length != 0)
		{
			char c = str[0];
			stringBuilder.Append(c);
			for (int i = 1; i < str.Length; i++)
			{
				char c2 = str[i];
				if (c != ' ' || c2 != ' ')
				{
					stringBuilder.Append(c2);
				}
				c = c2;
			}
		}
		return stringBuilder.ToString();
	}

	private void AppendValue(StringBuilder buf, IDictionary oidSymbols, DerObjectIdentifier oid, string val)
	{
		string text = (string)oidSymbols[oid];
		if (text != null)
		{
			buf.Append(text);
		}
		else
		{
			buf.Append(oid.Id);
		}
		buf.Append('=');
		int i = buf.Length;
		buf.Append(val);
		int num = buf.Length;
		if (Platform.StartsWith(val, "\\#"))
		{
			i += 2;
		}
		for (; i != num; i++)
		{
			if (buf[i] == ',' || buf[i] == '"' || buf[i] == '\\' || buf[i] == '+' || buf[i] == '=' || buf[i] == '<' || buf[i] == '>' || buf[i] == ';')
			{
				buf.Insert(i++, "\\");
				num++;
			}
		}
	}

	public string ToString(bool reverse, IDictionary oidSymbols)
	{
		ArrayList arrayList = new ArrayList();
		StringBuilder stringBuilder = null;
		for (int i = 0; i < ordering.Count; i++)
		{
			if ((bool)added[i])
			{
				stringBuilder.Append('+');
				AppendValue(stringBuilder, oidSymbols, (DerObjectIdentifier)ordering[i], (string)values[i]);
			}
			else
			{
				stringBuilder = new StringBuilder();
				AppendValue(stringBuilder, oidSymbols, (DerObjectIdentifier)ordering[i], (string)values[i]);
				arrayList.Add(stringBuilder);
			}
		}
		if (reverse)
		{
			arrayList.Reverse();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		if (arrayList.Count > 0)
		{
			stringBuilder2.Append(arrayList[0].ToString());
			for (int j = 1; j < arrayList.Count; j++)
			{
				stringBuilder2.Append(',');
				stringBuilder2.Append(arrayList[j].ToString());
			}
		}
		return stringBuilder2.ToString();
	}

	public override string ToString()
	{
		return ToString(DefaultReverse, DefaultSymbols);
	}
}
