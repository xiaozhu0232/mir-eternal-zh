using System;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public sealed class JsonObjectAttribute : JsonContainerAttribute
{
	private MemberSerialization _memberSerialization;

	internal MissingMemberHandling? _missingMemberHandling;

	internal Required? _itemRequired;

	internal NullValueHandling? _itemNullValueHandling;

	public MemberSerialization MemberSerialization
	{
		get
		{
			return _memberSerialization;
		}
		set
		{
			_memberSerialization = value;
		}
	}

	public MissingMemberHandling MissingMemberHandling
	{
		get
		{
			return _missingMemberHandling.GetValueOrDefault();
		}
		set
		{
			_missingMemberHandling = value;
		}
	}

	public NullValueHandling ItemNullValueHandling
	{
		get
		{
			return _itemNullValueHandling.GetValueOrDefault();
		}
		set
		{
			_itemNullValueHandling = value;
		}
	}

	public Required ItemRequired
	{
		get
		{
			return _itemRequired.GetValueOrDefault();
		}
		set
		{
			_itemRequired = value;
		}
	}

	public JsonObjectAttribute()
	{
	}

	public JsonObjectAttribute(MemberSerialization memberSerialization)
	{
		MemberSerialization = memberSerialization;
	}

	public JsonObjectAttribute(string id)
		: base(id)
	{
	}
}
