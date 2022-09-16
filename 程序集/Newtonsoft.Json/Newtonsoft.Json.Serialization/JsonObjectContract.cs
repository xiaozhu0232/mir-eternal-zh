using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class JsonObjectContract : JsonContainerContract
{
	internal bool ExtensionDataIsJToken;

	private bool? _hasRequiredOrDefaultValueProperties;

	private ObjectConstructor<object>? _overrideCreator;

	private ObjectConstructor<object>? _parameterizedCreator;

	private JsonPropertyCollection? _creatorParameters;

	private Type? _extensionDataValueType;

	public MemberSerialization MemberSerialization { get; set; }

	public MissingMemberHandling? MissingMemberHandling { get; set; }

	public Required? ItemRequired { get; set; }

	public NullValueHandling? ItemNullValueHandling { get; set; }

	public JsonPropertyCollection Properties { get; }

	public JsonPropertyCollection CreatorParameters
	{
		get
		{
			if (_creatorParameters == null)
			{
				_creatorParameters = new JsonPropertyCollection(base.UnderlyingType);
			}
			return _creatorParameters;
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

	internal ObjectConstructor<object>? ParameterizedCreator
	{
		get
		{
			return _parameterizedCreator;
		}
		set
		{
			_parameterizedCreator = value;
		}
	}

	public ExtensionDataSetter? ExtensionDataSetter { get; set; }

	public ExtensionDataGetter? ExtensionDataGetter { get; set; }

	public Type? ExtensionDataValueType
	{
		get
		{
			return _extensionDataValueType;
		}
		set
		{
			_extensionDataValueType = value;
			ExtensionDataIsJToken = value != null && typeof(JToken).IsAssignableFrom(value);
		}
	}

	public Func<string, string>? ExtensionDataNameResolver { get; set; }

	internal bool HasRequiredOrDefaultValueProperties
	{
		get
		{
			if (!_hasRequiredOrDefaultValueProperties.HasValue)
			{
				_hasRequiredOrDefaultValueProperties = false;
				if (ItemRequired.GetValueOrDefault(Required.Default) != 0)
				{
					_hasRequiredOrDefaultValueProperties = true;
				}
				else
				{
					foreach (JsonProperty property in Properties)
					{
						if (property.Required != 0 || ((uint?)property.DefaultValueHandling & 2u) == 2)
						{
							_hasRequiredOrDefaultValueProperties = true;
							break;
						}
					}
				}
			}
			return _hasRequiredOrDefaultValueProperties.GetValueOrDefault();
		}
	}

	public JsonObjectContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Object;
		Properties = new JsonPropertyCollection(base.UnderlyingType);
	}

	[SecuritySafeCritical]
	internal object GetUninitializedObject()
	{
		if (!JsonTypeReflector.FullyTrusted)
		{
			throw new JsonException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, NonNullableUnderlyingType));
		}
		return FormatterServices.GetUninitializedObject(NonNullableUnderlyingType);
	}
}
