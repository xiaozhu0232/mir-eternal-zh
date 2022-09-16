using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal static class ImmutableCollectionsUtils
{
	internal class ImmutableCollectionTypeInfo
	{
		public string ContractTypeName { get; set; }

		public string CreatedTypeName { get; set; }

		public string BuilderTypeName { get; set; }

		public ImmutableCollectionTypeInfo(string contractTypeName, string createdTypeName, string builderTypeName)
		{
			ContractTypeName = contractTypeName;
			CreatedTypeName = createdTypeName;
			BuilderTypeName = builderTypeName;
		}
	}

	private const string ImmutableListGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableList`1";

	private const string ImmutableQueueGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableQueue`1";

	private const string ImmutableStackGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableStack`1";

	private const string ImmutableSetGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableSet`1";

	private const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray";

	private const string ImmutableArrayGenericTypeName = "System.Collections.Immutable.ImmutableArray`1";

	private const string ImmutableListTypeName = "System.Collections.Immutable.ImmutableList";

	private const string ImmutableListGenericTypeName = "System.Collections.Immutable.ImmutableList`1";

	private const string ImmutableQueueTypeName = "System.Collections.Immutable.ImmutableQueue";

	private const string ImmutableQueueGenericTypeName = "System.Collections.Immutable.ImmutableQueue`1";

	private const string ImmutableStackTypeName = "System.Collections.Immutable.ImmutableStack";

	private const string ImmutableStackGenericTypeName = "System.Collections.Immutable.ImmutableStack`1";

	private const string ImmutableSortedSetTypeName = "System.Collections.Immutable.ImmutableSortedSet";

	private const string ImmutableSortedSetGenericTypeName = "System.Collections.Immutable.ImmutableSortedSet`1";

	private const string ImmutableHashSetTypeName = "System.Collections.Immutable.ImmutableHashSet";

	private const string ImmutableHashSetGenericTypeName = "System.Collections.Immutable.ImmutableHashSet`1";

	private static readonly IList<ImmutableCollectionTypeInfo> ArrayContractImmutableCollectionDefinitions = new List<ImmutableCollectionTypeInfo>
	{
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.IImmutableList`1", "System.Collections.Immutable.ImmutableList`1", "System.Collections.Immutable.ImmutableList"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableList`1", "System.Collections.Immutable.ImmutableList`1", "System.Collections.Immutable.ImmutableList"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.IImmutableQueue`1", "System.Collections.Immutable.ImmutableQueue`1", "System.Collections.Immutable.ImmutableQueue"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableQueue`1", "System.Collections.Immutable.ImmutableQueue`1", "System.Collections.Immutable.ImmutableQueue"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.IImmutableStack`1", "System.Collections.Immutable.ImmutableStack`1", "System.Collections.Immutable.ImmutableStack"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableStack`1", "System.Collections.Immutable.ImmutableStack`1", "System.Collections.Immutable.ImmutableStack"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.IImmutableSet`1", "System.Collections.Immutable.ImmutableHashSet`1", "System.Collections.Immutable.ImmutableHashSet"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableSortedSet`1", "System.Collections.Immutable.ImmutableSortedSet`1", "System.Collections.Immutable.ImmutableSortedSet"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableHashSet`1", "System.Collections.Immutable.ImmutableHashSet`1", "System.Collections.Immutable.ImmutableHashSet"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableArray`1", "System.Collections.Immutable.ImmutableArray`1", "System.Collections.Immutable.ImmutableArray")
	};

	private const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";

	private const string ImmutableDictionaryTypeName = "System.Collections.Immutable.ImmutableDictionary";

	private const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";

	private const string ImmutableSortedDictionaryTypeName = "System.Collections.Immutable.ImmutableSortedDictionary";

	private const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

	private static readonly IList<ImmutableCollectionTypeInfo> DictionaryContractImmutableCollectionDefinitions = new List<ImmutableCollectionTypeInfo>
	{
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.IImmutableDictionary`2", "System.Collections.Immutable.ImmutableDictionary`2", "System.Collections.Immutable.ImmutableDictionary"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableSortedDictionary`2", "System.Collections.Immutable.ImmutableSortedDictionary`2", "System.Collections.Immutable.ImmutableSortedDictionary"),
		new ImmutableCollectionTypeInfo("System.Collections.Immutable.ImmutableDictionary`2", "System.Collections.Immutable.ImmutableDictionary`2", "System.Collections.Immutable.ImmutableDictionary")
	};

	internal static bool TryBuildImmutableForArrayContract(Type underlyingType, Type collectionItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor<object>? parameterizedCreator)
	{
		if (underlyingType.IsGenericType())
		{
			Type genericTypeDefinition = underlyingType.GetGenericTypeDefinition();
			string name = genericTypeDefinition.FullName;
			ImmutableCollectionTypeInfo immutableCollectionTypeInfo = ArrayContractImmutableCollectionDefinitions.FirstOrDefault((ImmutableCollectionTypeInfo d) => d.ContractTypeName == name);
			if (immutableCollectionTypeInfo != null)
			{
				Type type = genericTypeDefinition.Assembly().GetType(immutableCollectionTypeInfo.CreatedTypeName);
				Type type2 = genericTypeDefinition.Assembly().GetType(immutableCollectionTypeInfo.BuilderTypeName);
				if (type != null && type2 != null)
				{
					MethodInfo methodInfo = type2.GetMethods().FirstOrDefault((MethodInfo m) => m.Name == "CreateRange" && m.GetParameters().Length == 1);
					if (methodInfo != null)
					{
						createdType = type.MakeGenericType(collectionItemType);
						MethodInfo method = methodInfo.MakeGenericMethod(collectionItemType);
						parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(method);
						return true;
					}
				}
			}
		}
		createdType = null;
		parameterizedCreator = null;
		return false;
	}

	internal static bool TryBuildImmutableForDictionaryContract(Type underlyingType, Type keyItemType, Type valueItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor<object>? parameterizedCreator)
	{
		if (underlyingType.IsGenericType())
		{
			Type genericTypeDefinition = underlyingType.GetGenericTypeDefinition();
			string name = genericTypeDefinition.FullName;
			ImmutableCollectionTypeInfo immutableCollectionTypeInfo = DictionaryContractImmutableCollectionDefinitions.FirstOrDefault((ImmutableCollectionTypeInfo d) => d.ContractTypeName == name);
			if (immutableCollectionTypeInfo != null)
			{
				Type type = genericTypeDefinition.Assembly().GetType(immutableCollectionTypeInfo.CreatedTypeName);
				Type type2 = genericTypeDefinition.Assembly().GetType(immutableCollectionTypeInfo.BuilderTypeName);
				if (type != null && type2 != null)
				{
					MethodInfo methodInfo = type2.GetMethods().FirstOrDefault(delegate(MethodInfo m)
					{
						ParameterInfo[] parameters = m.GetParameters();
						return m.Name == "CreateRange" && parameters.Length == 1 && parameters[0].ParameterType.IsGenericType() && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
					});
					if (methodInfo != null)
					{
						createdType = type.MakeGenericType(keyItemType, valueItemType);
						MethodInfo method = methodInfo.MakeGenericMethod(keyItemType, valueItemType);
						parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(method);
						return true;
					}
				}
			}
		}
		createdType = null;
		parameterizedCreator = null;
		return false;
	}
}
