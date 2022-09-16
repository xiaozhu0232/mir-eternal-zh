using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal class LateBoundReflectionDelegateFactory : ReflectionDelegateFactory
{
	private static readonly LateBoundReflectionDelegateFactory _instance = new LateBoundReflectionDelegateFactory();

	internal static ReflectionDelegateFactory Instance => _instance;

	public override ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
	{
		MethodBase method2 = method;
		ValidationUtils.ArgumentNotNull(method2, "method");
		ConstructorInfo c = method2 as ConstructorInfo;
		if ((object)c != null)
		{
			return (object?[] a) => c.Invoke(a);
		}
		return (object?[] a) => method2.Invoke(null, a);
	}

	public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
	{
		MethodBase method2 = method;
		ValidationUtils.ArgumentNotNull(method2, "method");
		ConstructorInfo c = method2 as ConstructorInfo;
		if ((object)c != null)
		{
			return (T o, object?[] a) => c.Invoke(a);
		}
		return (T o, object?[] a) => method2.Invoke(o, a);
	}

	public override Func<T> CreateDefaultConstructor<T>(Type type)
	{
		Type type2 = type;
		ValidationUtils.ArgumentNotNull(type2, "type");
		if (type2.IsValueType())
		{
			return () => (T)Activator.CreateInstance(type2);
		}
		ConstructorInfo constructorInfo = ReflectionUtils.GetDefaultConstructor(type2, nonPublic: true);
		return () => (T)constructorInfo.Invoke(null);
	}

	public override Func<T, object?> CreateGet<T>(PropertyInfo propertyInfo)
	{
		PropertyInfo propertyInfo2 = propertyInfo;
		ValidationUtils.ArgumentNotNull(propertyInfo2, "propertyInfo");
		return (T o) => propertyInfo2.GetValue(o, null);
	}

	public override Func<T, object?> CreateGet<T>(FieldInfo fieldInfo)
	{
		FieldInfo fieldInfo2 = fieldInfo;
		ValidationUtils.ArgumentNotNull(fieldInfo2, "fieldInfo");
		return (T o) => fieldInfo2.GetValue(o);
	}

	public override Action<T, object?> CreateSet<T>(FieldInfo fieldInfo)
	{
		FieldInfo fieldInfo2 = fieldInfo;
		ValidationUtils.ArgumentNotNull(fieldInfo2, "fieldInfo");
		return delegate(T o, object? v)
		{
			fieldInfo2.SetValue(o, v);
		};
	}

	public override Action<T, object?> CreateSet<T>(PropertyInfo propertyInfo)
	{
		PropertyInfo propertyInfo2 = propertyInfo;
		ValidationUtils.ArgumentNotNull(propertyInfo2, "propertyInfo");
		return delegate(T o, object? v)
		{
			propertyInfo2.SetValue(o, v, null);
		};
	}
}
