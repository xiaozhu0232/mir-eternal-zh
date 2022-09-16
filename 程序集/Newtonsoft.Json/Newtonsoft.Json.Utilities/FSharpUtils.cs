using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal class FSharpUtils
{
	private static readonly object Lock = new object();

	private static FSharpUtils? _instance;

	private MethodInfo _ofSeq;

	private Type _mapType;

	public const string FSharpSetTypeName = "FSharpSet`1";

	public const string FSharpListTypeName = "FSharpList`1";

	public const string FSharpMapTypeName = "FSharpMap`2";

	public static FSharpUtils Instance => _instance;

	public Assembly FSharpCoreAssembly { get; private set; }

	public MethodCall<object?, object> IsUnion { get; private set; }

	public MethodCall<object?, object> GetUnionCases { get; private set; }

	public MethodCall<object?, object> PreComputeUnionTagReader { get; private set; }

	public MethodCall<object?, object> PreComputeUnionReader { get; private set; }

	public MethodCall<object?, object> PreComputeUnionConstructor { get; private set; }

	public Func<object, object> GetUnionCaseInfoDeclaringType { get; private set; }

	public Func<object, object> GetUnionCaseInfoName { get; private set; }

	public Func<object, object> GetUnionCaseInfoTag { get; private set; }

	public MethodCall<object, object?> GetUnionCaseInfoFields { get; private set; }

	private FSharpUtils(Assembly fsharpCoreAssembly)
	{
		FSharpCoreAssembly = fsharpCoreAssembly;
		Type type = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpType");
		MethodInfo methodWithNonPublicFallback = GetMethodWithNonPublicFallback(type, "IsUnion", BindingFlags.Static | BindingFlags.Public);
		IsUnion = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(methodWithNonPublicFallback);
		MethodInfo methodWithNonPublicFallback2 = GetMethodWithNonPublicFallback(type, "GetUnionCases", BindingFlags.Static | BindingFlags.Public);
		GetUnionCases = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(methodWithNonPublicFallback2);
		Type type2 = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpValue");
		PreComputeUnionTagReader = CreateFSharpFuncCall(type2, "PreComputeUnionTagReader");
		PreComputeUnionReader = CreateFSharpFuncCall(type2, "PreComputeUnionReader");
		PreComputeUnionConstructor = CreateFSharpFuncCall(type2, "PreComputeUnionConstructor");
		Type type3 = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.UnionCaseInfo");
		GetUnionCaseInfoName = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(type3.GetProperty("Name"));
		GetUnionCaseInfoTag = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(type3.GetProperty("Tag"));
		GetUnionCaseInfoDeclaringType = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(type3.GetProperty("DeclaringType"));
		GetUnionCaseInfoFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(type3.GetMethod("GetFields"));
		Type type4 = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.ListModule");
		_ofSeq = type4.GetMethod("OfSeq");
		_mapType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.FSharpMap`2");
	}

	public static void EnsureInitialized(Assembly fsharpCoreAssembly)
	{
		if (_instance != null)
		{
			return;
		}
		lock (Lock)
		{
			if (_instance == null)
			{
				_instance = new FSharpUtils(fsharpCoreAssembly);
			}
		}
	}

	private static MethodInfo GetMethodWithNonPublicFallback(Type type, string methodName, BindingFlags bindingFlags)
	{
		MethodInfo method = type.GetMethod(methodName, bindingFlags);
		if (method == null && (bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic)
		{
			method = type.GetMethod(methodName, bindingFlags | BindingFlags.NonPublic);
		}
		return method;
	}

	private static MethodCall<object?, object> CreateFSharpFuncCall(Type type, string methodName)
	{
		MethodInfo methodWithNonPublicFallback = GetMethodWithNonPublicFallback(type, methodName, BindingFlags.Static | BindingFlags.Public);
		MethodInfo method = methodWithNonPublicFallback.ReturnType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
		MethodCall<object?, object?> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(methodWithNonPublicFallback);
		MethodCall<object?, object> invoke = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
		return (object? target, object?[] args) => new FSharpFunction(call(target, args), invoke);
	}

	public ObjectConstructor<object> CreateSeq(Type t)
	{
		MethodInfo method = _ofSeq.MakeGenericMethod(t);
		return JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(method);
	}

	public ObjectConstructor<object> CreateMap(Type keyType, Type valueType)
	{
		return (ObjectConstructor<object>)typeof(FSharpUtils).GetMethod("BuildMapCreator").MakeGenericMethod(keyType, valueType).Invoke(this, null);
	}

	public ObjectConstructor<object> BuildMapCreator<TKey, TValue>()
	{
		ConstructorInfo constructor = _mapType.MakeGenericType(typeof(TKey), typeof(TValue)).GetConstructor(new Type[1] { typeof(IEnumerable<Tuple<TKey, TValue>>) });
		ObjectConstructor<object> ctorDelegate = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
		return delegate(object?[] args)
		{
			IEnumerable<Tuple<TKey, TValue>> enumerable = ((IEnumerable<KeyValuePair<TKey, TValue>>)args[0]).Select((KeyValuePair<TKey, TValue> kv) => new Tuple<TKey, TValue>(kv.Key, kv.Value));
			return ctorDelegate(enumerable);
		};
	}
}
