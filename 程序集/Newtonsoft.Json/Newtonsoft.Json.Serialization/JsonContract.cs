using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public abstract class JsonContract
{
	internal bool IsNullable;

	internal bool IsConvertable;

	internal bool IsEnum;

	internal Type NonNullableUnderlyingType;

	internal ReadType InternalReadType;

	internal JsonContractType ContractType;

	internal bool IsReadOnlyOrFixedSize;

	internal bool IsSealed;

	internal bool IsInstantiable;

	private List<SerializationCallback>? _onDeserializedCallbacks;

	private List<SerializationCallback>? _onDeserializingCallbacks;

	private List<SerializationCallback>? _onSerializedCallbacks;

	private List<SerializationCallback>? _onSerializingCallbacks;

	private List<SerializationErrorCallback>? _onErrorCallbacks;

	private Type _createdType;

	public Type UnderlyingType { get; }

	public Type CreatedType
	{
		get
		{
			return _createdType;
		}
		set
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			_createdType = value;
			IsSealed = _createdType.IsSealed();
			IsInstantiable = !_createdType.IsInterface() && !_createdType.IsAbstract();
		}
	}

	public bool? IsReference { get; set; }

	public JsonConverter? Converter { get; set; }

	public JsonConverter? InternalConverter { get; internal set; }

	public IList<SerializationCallback> OnDeserializedCallbacks
	{
		get
		{
			if (_onDeserializedCallbacks == null)
			{
				_onDeserializedCallbacks = new List<SerializationCallback>();
			}
			return _onDeserializedCallbacks;
		}
	}

	public IList<SerializationCallback> OnDeserializingCallbacks
	{
		get
		{
			if (_onDeserializingCallbacks == null)
			{
				_onDeserializingCallbacks = new List<SerializationCallback>();
			}
			return _onDeserializingCallbacks;
		}
	}

	public IList<SerializationCallback> OnSerializedCallbacks
	{
		get
		{
			if (_onSerializedCallbacks == null)
			{
				_onSerializedCallbacks = new List<SerializationCallback>();
			}
			return _onSerializedCallbacks;
		}
	}

	public IList<SerializationCallback> OnSerializingCallbacks
	{
		get
		{
			if (_onSerializingCallbacks == null)
			{
				_onSerializingCallbacks = new List<SerializationCallback>();
			}
			return _onSerializingCallbacks;
		}
	}

	public IList<SerializationErrorCallback> OnErrorCallbacks
	{
		get
		{
			if (_onErrorCallbacks == null)
			{
				_onErrorCallbacks = new List<SerializationErrorCallback>();
			}
			return _onErrorCallbacks;
		}
	}

	public Func<object>? DefaultCreator { get; set; }

	public bool DefaultCreatorNonPublic { get; set; }

	internal JsonContract(Type underlyingType)
	{
		ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");
		UnderlyingType = underlyingType;
		underlyingType = ReflectionUtils.EnsureNotByRefType(underlyingType);
		IsNullable = ReflectionUtils.IsNullable(underlyingType);
		NonNullableUnderlyingType = ((IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType);
		_createdType = (CreatedType = NonNullableUnderlyingType);
		IsConvertable = ConvertUtils.IsConvertible(NonNullableUnderlyingType);
		IsEnum = NonNullableUnderlyingType.IsEnum();
		InternalReadType = ReadType.Read;
	}

	internal void InvokeOnSerializing(object o, StreamingContext context)
	{
		if (_onSerializingCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback item in _onSerializingCallbacks!)
		{
			item(o, context);
		}
	}

	internal void InvokeOnSerialized(object o, StreamingContext context)
	{
		if (_onSerializedCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback item in _onSerializedCallbacks!)
		{
			item(o, context);
		}
	}

	internal void InvokeOnDeserializing(object o, StreamingContext context)
	{
		if (_onDeserializingCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback item in _onDeserializingCallbacks!)
		{
			item(o, context);
		}
	}

	internal void InvokeOnDeserialized(object o, StreamingContext context)
	{
		if (_onDeserializedCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback item in _onDeserializedCallbacks!)
		{
			item(o, context);
		}
	}

	internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
	{
		if (_onErrorCallbacks == null)
		{
			return;
		}
		foreach (SerializationErrorCallback item in _onErrorCallbacks!)
		{
			item(o, context, errorContext);
		}
	}

	internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
	{
		MethodInfo callbackMethodInfo2 = callbackMethodInfo;
		return delegate(object o, StreamingContext context)
		{
			callbackMethodInfo2.Invoke(o, new object[1] { context });
		};
	}

	internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
	{
		MethodInfo callbackMethodInfo2 = callbackMethodInfo;
		return delegate(object o, StreamingContext context, ErrorContext econtext)
		{
			callbackMethodInfo2.Invoke(o, new object[2] { context, econtext });
		};
	}
}
