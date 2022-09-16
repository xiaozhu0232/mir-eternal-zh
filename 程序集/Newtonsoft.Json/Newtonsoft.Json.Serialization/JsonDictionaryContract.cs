using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class JsonDictionaryContract : JsonContainerContract
{
	private readonly Type? _genericCollectionDefinitionType;

	private Type? _genericWrapperType;

	private ObjectConstructor<object>? _genericWrapperCreator;

	private Func<object>? _genericTemporaryDictionaryCreator;

	private readonly ConstructorInfo? _parameterizedConstructor;

	private ObjectConstructor<object>? _overrideCreator;

	private ObjectConstructor<object>? _parameterizedCreator;

	public Func<string, string>? DictionaryKeyResolver { get; set; }

	public Type? DictionaryKeyType { get; }

	public Type? DictionaryValueType { get; }

	internal JsonContract? KeyContract { get; set; }

	internal bool ShouldCreateWrapper { get; }

	internal ObjectConstructor<object>? ParameterizedCreator
	{
		get
		{
			if (_parameterizedCreator == null && _parameterizedConstructor != null)
			{
				_parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(_parameterizedConstructor);
			}
			return _parameterizedCreator;
		}
	}

	public ObjectConstructor<object>? OverrideCreator
	{
		get
		{
			return _overrideCreator;
		}
		set
		{
			_overrideCreator = value;
		}
	}

	public bool HasParameterizedCreator { get; set; }

	internal bool HasParameterizedCreatorInternal
	{
		get
		{
			if (!HasParameterizedCreator && _parameterizedCreator == null)
			{
				return _parameterizedConstructor != null;
			}
			return true;
		}
	}

	public JsonDictionaryContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Dictionary;
		Type keyType;
		Type valueType;
		if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IDictionary<, >), out _genericCollectionDefinitionType))
		{
			keyType = _genericCollectionDefinitionType!.GetGenericArguments()[0];
			valueType = _genericCollectionDefinitionType!.GetGenericArguments()[1];
			if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IDictionary<, >)))
			{
				base.CreatedType = typeof(Dictionary<, >).MakeGenericType(keyType, valueType);
			}
			else if (NonNullableUnderlyingType.IsGenericType() && NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Collections.Concurrent.ConcurrentDictionary`2")
			{
				ShouldCreateWrapper = true;
			}
			IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(NonNullableUnderlyingType, typeof(ReadOnlyDictionary<, >));
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyDictionary<, >), out _genericCollectionDefinitionType))
		{
			keyType = _genericCollectionDefinitionType!.GetGenericArguments()[0];
			valueType = _genericCollectionDefinitionType!.GetGenericArguments()[1];
			if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyDictionary<, >)))
			{
				base.CreatedType = typeof(ReadOnlyDictionary<, >).MakeGenericType(keyType, valueType);
			}
			IsReadOnlyOrFixedSize = true;
		}
		else
		{
			ReflectionUtils.GetDictionaryKeyValueTypes(NonNullableUnderlyingType, out keyType, out valueType);
			if (NonNullableUnderlyingType == typeof(IDictionary))
			{
				base.CreatedType = typeof(Dictionary<object, object>);
			}
		}
		if (keyType != null && valueType != null)
		{
			_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.CreatedType, typeof(KeyValuePair<, >).MakeGenericType(keyType, valueType), typeof(IDictionary<, >).MakeGenericType(keyType, valueType));
			if (!HasParameterizedCreatorInternal && NonNullableUnderlyingType.Name == "FSharpMap`2")
			{
				FSharpUtils.EnsureInitialized(NonNullableUnderlyingType.Assembly());
				_parameterizedCreator = FSharpUtils.Instance.CreateMap(keyType, valueType);
			}
		}
		if (!typeof(IDictionary).IsAssignableFrom(base.CreatedType))
		{
			ShouldCreateWrapper = true;
		}
		DictionaryKeyType = keyType;
		DictionaryValueType = valueType;
		if (DictionaryKeyType != null && DictionaryValueType != null && ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(NonNullableUnderlyingType, DictionaryKeyType, DictionaryValueType, out var createdType, out var parameterizedCreator))
		{
			base.CreatedType = createdType;
			_parameterizedCreator = parameterizedCreator;
			IsReadOnlyOrFixedSize = true;
		}
	}

	internal IWrappedDictionary CreateWrapper(object dictionary)
	{
		if (_genericWrapperCreator == null)
		{
			_genericWrapperType = typeof(DictionaryWrapper<, >).MakeGenericType(DictionaryKeyType, DictionaryValueType);
			ConstructorInfo constructor = _genericWrapperType!.GetConstructor(new Type[1] { _genericCollectionDefinitionType });
			_genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
		}
		return (IWrappedDictionary)_genericWrapperCreator!(dictionary);
	}

	internal IDictionary CreateTemporaryDictionary()
	{
		if (_genericTemporaryDictionaryCreator == null)
		{
			Type type = typeof(Dictionary<, >).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));
			_genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type);
		}
		return (IDictionary)_genericTemporaryDictionaryCreator!();
	}
}
