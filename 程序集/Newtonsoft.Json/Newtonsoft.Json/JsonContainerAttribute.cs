using System;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public abstract class JsonContainerAttribute : Attribute
{
	internal bool? _isReference;

	internal bool? _itemIsReference;

	internal ReferenceLoopHandling? _itemReferenceLoopHandling;

	internal TypeNameHandling? _itemTypeNameHandling;

	private Type? _namingStrategyType;

	private object[]? _namingStrategyParameters;

	public string? Id { get; set; }

	public string? Title { get; set; }

	public string? Description { get; set; }

	public Type? ItemConverterType { get; set; }

	public object[]? ItemConverterParameters { get; set; }

	public Type? NamingStrategyType
	{
		get
		{
			return _namingStrategyType;
		}
		set
		{
			_namingStrategyType = value;
			NamingStrategyInstance = null;
		}
	}

	public object[]? NamingStrategyParameters
	{
		get
		{
			return _namingStrategyParameters;
		}
		set
		{
			_namingStrategyParameters = value;
			NamingStrategyInstance = null;
		}
	}

	internal NamingStrategy? NamingStrategyInstance { get; set; }

	public bool IsReference
	{
		get
		{
			return _isReference.GetValueOrDefault();
		}
		set
		{
			_isReference = value;
		}
	}

	public bool ItemIsReference
	{
		get
		{
			return _itemIsReference.GetValueOrDefault();
		}
		set
		{
			_itemIsReference = value;
		}
	}

	public ReferenceLoopHandling ItemReferenceLoopHandling
	{
		get
		{
			return _itemReferenceLoopHandling.GetValueOrDefault();
		}
		set
		{
			_itemReferenceLoopHandling = value;
		}
	}

	public TypeNameHandling ItemTypeNameHandling
	{
		get
		{
			return _itemTypeNameHandling.GetValueOrDefault();
		}
		set
		{
			_itemTypeNameHandling = value;
		}
	}

	protected JsonContainerAttribute()
	{
	}

	protected JsonContainerAttribute(string id)
	{
		Id = id;
	}
}
