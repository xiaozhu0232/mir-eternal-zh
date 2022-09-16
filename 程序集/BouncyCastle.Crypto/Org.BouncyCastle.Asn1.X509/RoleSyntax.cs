using System;
using System.Text;

namespace Org.BouncyCastle.Asn1.X509;

public class RoleSyntax : Asn1Encodable
{
	private readonly GeneralNames roleAuthority;

	private readonly GeneralName roleName;

	public GeneralNames RoleAuthority => roleAuthority;

	public GeneralName RoleName => roleName;

	public static RoleSyntax GetInstance(object obj)
	{
		if (obj is RoleSyntax)
		{
			return (RoleSyntax)obj;
		}
		if (obj != null)
		{
			return new RoleSyntax(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public RoleSyntax(GeneralNames roleAuthority, GeneralName roleName)
	{
		if (roleName == null || roleName.TagNo != 6 || ((IAsn1String)roleName.Name).GetString().Equals(""))
		{
			throw new ArgumentException("the role name MUST be non empty and MUST use the URI option of GeneralName");
		}
		this.roleAuthority = roleAuthority;
		this.roleName = roleName;
	}

	public RoleSyntax(GeneralName roleName)
		: this(null, roleName)
	{
	}

	public RoleSyntax(string roleName)
		: this(new GeneralName(6, (roleName == null) ? "" : roleName))
	{
	}

	private RoleSyntax(Asn1Sequence seq)
	{
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		for (int i = 0; i != seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			switch (instance.TagNo)
			{
			case 0:
				roleAuthority = GeneralNames.GetInstance(instance, explicitly: false);
				break;
			case 1:
				roleName = GeneralName.GetInstance(instance, explicitly: true);
				break;
			default:
				throw new ArgumentException("Unknown tag in RoleSyntax");
			}
		}
	}

	public string GetRoleNameAsString()
	{
		return ((IAsn1String)roleName.Name).GetString();
	}

	public string[] GetRoleAuthorityAsString()
	{
		if (roleAuthority == null)
		{
			return new string[0];
		}
		GeneralName[] names = roleAuthority.GetNames();
		string[] array = new string[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			Asn1Encodable name = names[i].Name;
			if (name is IAsn1String)
			{
				array[i] = ((IAsn1String)name).GetString();
			}
			else
			{
				array[i] = name.ToString();
			}
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, roleAuthority);
		asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 1, roleName));
		return new DerSequence(asn1EncodableVector);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("Name: " + GetRoleNameAsString() + " - Auth: ");
		if (roleAuthority == null || roleAuthority.GetNames().Length == 0)
		{
			stringBuilder.Append("N/A");
		}
		else
		{
			string[] roleAuthorityAsString = GetRoleAuthorityAsString();
			stringBuilder.Append('[').Append(roleAuthorityAsString[0]);
			for (int i = 1; i < roleAuthorityAsString.Length; i++)
			{
				stringBuilder.Append(", ").Append(roleAuthorityAsString[i]);
			}
			stringBuilder.Append(']');
		}
		return stringBuilder.ToString();
	}
}
