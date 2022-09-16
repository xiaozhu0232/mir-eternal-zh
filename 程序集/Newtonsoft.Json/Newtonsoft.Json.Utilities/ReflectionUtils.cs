using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal static class ReflectionUtils
{
	public static readonly Type[] EmptyTypes;

	static ReflectionUtils()
	{
		EmptyTypes = Type.EmptyTypes;
	}

	public static bool IsVirtual(this PropertyInfo propertyInfo)
	{
		ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
		MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
		if (getMethod != null && getMethod.IsVirtual)
		{
			return true;
		}
		getMethod = propertyInfo.GetSetMethod(nonPublic: true);
		if (getMethod != null && getMethod.IsVirtual)
		{
			return true;
		}
		return false;
	}

	public static MethodInfo? GetBaseDefinition(this PropertyInfo propertyInfo)
	{
		ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
		MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
		if (getMethod != null)
		{
			return getMethod.GetBaseDefinition();
		}
		return propertyInfo.GetSetMethod(nonPublic: true)?.GetBaseDefinition();
	}

	public static bool IsPublic(PropertyInfo property)
	{
		MethodInfo getMethod = property.GetGetMethod();
		if (getMethod != null && getMethod.IsPublic)
		{
			return true;
		}
		MethodInfo setMethod = property.GetSetMethod();
		if (setMethod != null && setMethod.IsPublic)
		{
			return true;
		}
		return false;
	}

	public static Type? GetObjectType(object? v)
	{
		return v?.GetType();
	}

	public static string GetTypeName(Type t, TypeNameAssemblyFormatHandling assemblyFormat, ISerializationBinder? binder)
	{
		string fullyQualifiedTypeName = GetFullyQualifiedTypeName(t, binder);
		return assemblyFormat switch
		{
			TypeNameAssemblyFormatHandling.Simple => RemoveAssemblyDetails(fullyQualifiedTypeName), 
			TypeNameAssemblyFormatHandling.Full => fullyQualifiedTypeName, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static string GetFullyQualifiedTypeName(Type t, ISerializationBinder? binder)
	{
		if (binder != null)
		{
			binder!.BindToName(t, out var assemblyName, out var typeName);
			return typeName + ((assemblyName == null) ? "" : (", " + assemblyName));
		}
		return t.AssemblyQualifiedName;
	}

	private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		foreach (char c in fullyQualifiedTypeName)
		{
			switch (c)
			{
			case '[':
			case ']':
				flag = false;
				flag2 = false;
				stringBuilder.Append(c);
				break;
			case ',':
				if (!flag)
				{
					flag = true;
					stringBuilder.Append(c);
				}
				else
				{
					flag2 = true;
				}
				break;
			default:
				if (!flag2)
				{
					stringBuilder.Append(c);
				}
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static bool HasDefaultConstructor(Type t, bool nonPublic)
	{
		ValidationUtils.ArgumentNotNull(t, "t");
		if (t.IsValueType())
		{
			return true;
		}
		return GetDefaultConstructor(t, nonPublic) != null;
	}

	public static ConstructorInfo GetDefaultConstructor(Type t)
	{
		return GetDefaultConstructor(t, nonPublic: false);
	}

	public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
	{
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
		if (nonPublic)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}
		return t.GetConstructors(bindingFlags).SingleOrDefault((ConstructorInfo c) => !c.GetParameters().Any());
	}

	public static bool IsNullable(Type t)
	{
		ValidationUtils.ArgumentNotNull(t, "t");
		if (t.IsValueType())
		{
			return IsNullableType(t);
		}
		return true;
	}

	public static bool IsNullableType(Type t)
	{
		ValidationUtils.ArgumentNotNull(t, "t");
		if (t.IsGenericType())
		{
			return t.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	public static Type EnsureNotNullableType(Type t)
	{
		if (!IsNullableType(t))
		{
			return t;
		}
		return Nullable.GetUnderlyingType(t);
	}

	public static Type EnsureNotByRefType(Type t)
	{
		if (!t.IsByRef || !t.HasElementType)
		{
			return t;
		}
		return t.GetElementType();
	}

	public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition)
	{
		if (!type.IsGenericType())
		{
			return false;
		}
		return type.GetGenericTypeDefinition() == genericInterfaceDefinition;
	}

	public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
	{
		Type implementingType;
		return ImplementsGenericDefinition(type, genericInterfaceDefinition, out implementingType);
	}

	public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, [NotNullWhen(true)] out Type? implementingType)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, "genericInterfaceDefinition");
		if (!genericInterfaceDefinition.IsInterface() || !genericInterfaceDefinition.IsGenericTypeDefinition())
		{
			throw new ArgumentNullException("'{0}' is not a generic interface definition.".FormatWith(CultureInfo.InvariantCulture, genericInterfaceDefinition));
		}
		if (type.IsInterface() && type.IsGenericType())
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericInterfaceDefinition == genericTypeDefinition)
			{
				implementingType = type;
				return true;
			}
		}
		Type[] interfaces = type.GetInterfaces();
		foreach (Type type2 in interfaces)
		{
			if (type2.IsGenericType())
			{
				Type genericTypeDefinition2 = type2.GetGenericTypeDefinition();
				if (genericInterfaceDefinition == genericTypeDefinition2)
				{
					implementingType = type2;
					return true;
				}
			}
		}
		implementingType = null;
		return false;
	}

	public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
	{
		Type implementingType;
		return InheritsGenericDefinition(type, genericClassDefinition, out implementingType);
	}

	public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type? implementingType)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		ValidationUtils.ArgumentNotNull(genericClassDefinition, "genericClassDefinition");
		if (!genericClassDefinition.IsClass() || !genericClassDefinition.IsGenericTypeDefinition())
		{
			throw new ArgumentNullException("'{0}' is not a generic class definition.".FormatWith(CultureInfo.InvariantCulture, genericClassDefinition));
		}
		return InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
	}

	private static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type? implementingType)
	{
		do
		{
			if (currentType.IsGenericType() && genericClassDefinition == currentType.GetGenericTypeDefinition())
			{
				implementingType = currentType;
				return true;
			}
			currentType = currentType.BaseType();
		}
		while (currentType != null);
		implementingType = null;
		return false;
	}

	public static Type? GetCollectionItemType(Type type)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		if (type.IsArray)
		{
			return type.GetElementType();
		}
		if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out var implementingType))
		{
			if (implementingType.IsGenericTypeDefinition())
			{
				throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
			}
			return implementingType.GetGenericArguments()[0];
		}
		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			return null;
		}
		throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
	}

	public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type? keyType, out Type? valueType)
	{
		ValidationUtils.ArgumentNotNull(dictionaryType, "dictionaryType");
		if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<, >), out var implementingType))
		{
			if (implementingType.IsGenericTypeDefinition())
			{
				throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
			}
			Type[] genericArguments = implementingType.GetGenericArguments();
			keyType = genericArguments[0];
			valueType = genericArguments[1];
		}
		else
		{
			if (!typeof(IDictionary).IsAssignableFrom(dictionaryType))
			{
				throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
			}
			keyType = null;
			valueType = null;
		}
	}

	public static Type GetMemberUnderlyingType(MemberInfo member)
	{
		ValidationUtils.ArgumentNotNull(member, "member");
		return member.MemberType() switch
		{
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			MemberTypes.Event => ((EventInfo)member).EventHandlerType, 
			MemberTypes.Method => ((MethodInfo)member).ReturnType, 
			_ => throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", "member"), 
		};
	}

	public static bool IsByRefLikeType(Type type)
	{
		if (!type.IsValueType())
		{
			return false;
		}
		Attribute[] attributes = GetAttributes(type, null, inherit: false);
		for (int i = 0; i < attributes.Length; i++)
		{
			if (string.Equals(attributes[i].GetType().FullName, "System.Runtime.CompilerServices.IsByRefLikeAttribute", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsIndexedProperty(PropertyInfo property)
	{
		ValidationUtils.ArgumentNotNull(property, "property");
		return property.GetIndexParameters().Length != 0;
	}

	public static object GetMemberValue(MemberInfo member, object target)
	{
		ValidationUtils.ArgumentNotNull(member, "member");
		ValidationUtils.ArgumentNotNull(target, "target");
		switch (member.MemberType())
		{
		case MemberTypes.Field:
			return ((FieldInfo)member).GetValue(target);
		case MemberTypes.Property:
			try
			{
				return ((PropertyInfo)member).GetValue(target, null);
			}
			catch (TargetParameterCountException innerException)
			{
				throw new ArgumentException("MemberInfo '{0}' has index parameters".FormatWith(CultureInfo.InvariantCulture, member.Name), innerException);
			}
		default:
			throw new ArgumentException("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), "member");
		}
	}

	public static void SetMemberValue(MemberInfo member, object target, object? value)
	{
		ValidationUtils.ArgumentNotNull(member, "member");
		ValidationUtils.ArgumentNotNull(target, "target");
		switch (member.MemberType())
		{
		case MemberTypes.Field:
			((FieldInfo)member).SetValue(target, value);
			break;
		case MemberTypes.Property:
			((PropertyInfo)member).SetValue(target, value, null);
			break;
		default:
			throw new ArgumentException("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), "member");
		}
	}

	public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
	{
		switch (member.MemberType())
		{
		case MemberTypes.Field:
		{
			FieldInfo fieldInfo = (FieldInfo)member;
			if (nonPublic)
			{
				return true;
			}
			if (fieldInfo.IsPublic)
			{
				return true;
			}
			return false;
		}
		case MemberTypes.Property:
		{
			PropertyInfo propertyInfo = (PropertyInfo)member;
			if (!propertyInfo.CanRead)
			{
				return false;
			}
			if (nonPublic)
			{
				return true;
			}
			return propertyInfo.GetGetMethod(nonPublic) != null;
		}
		default:
			return false;
		}
	}

	public static bool CanSetMemberValue(MemberInfo member, bool nonPublic, bool canSetReadOnly)
	{
		switch (member.MemberType())
		{
		case MemberTypes.Field:
		{
			FieldInfo fieldInfo = (FieldInfo)member;
			if (fieldInfo.IsLiteral)
			{
				return false;
			}
			if (fieldInfo.IsInitOnly && !canSetReadOnly)
			{
				return false;
			}
			if (nonPublic)
			{
				return true;
			}
			if (fieldInfo.IsPublic)
			{
				return true;
			}
			return false;
		}
		case MemberTypes.Property:
		{
			PropertyInfo propertyInfo = (PropertyInfo)member;
			if (!propertyInfo.CanWrite)
			{
				return false;
			}
			if (nonPublic)
			{
				return true;
			}
			return propertyInfo.GetSetMethod(nonPublic) != null;
		}
		default:
			return false;
		}
	}

	public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
	{
		List<MemberInfo> list = new List<MemberInfo>();
		list.AddRange(GetFields(type, bindingAttr));
		list.AddRange(GetProperties(type, bindingAttr));
		List<MemberInfo> list2 = new List<MemberInfo>(list.Count);
		foreach (IGrouping<string, MemberInfo> item in from m in list
			group m by m.Name)
		{
			if (item.Count() == 1)
			{
				list2.Add(item.First());
				continue;
			}
			List<MemberInfo> list3 = new List<MemberInfo>();
			foreach (MemberInfo memberInfo in item)
			{
				if (list3.Count == 0)
				{
					list3.Add(memberInfo);
				}
				else if ((!IsOverridenGenericMember(memberInfo, bindingAttr) || memberInfo.Name == "Item") && !list3.Any((MemberInfo m) => m.DeclaringType == memberInfo.DeclaringType))
				{
					list3.Add(memberInfo);
				}
			}
			list2.AddRange(list3);
		}
		return list2;
	}

	private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
	{
		if (memberInfo.MemberType() != MemberTypes.Property)
		{
			return false;
		}
		PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
		if (!propertyInfo.IsVirtual())
		{
			return false;
		}
		Type declaringType = propertyInfo.DeclaringType;
		if (!declaringType.IsGenericType())
		{
			return false;
		}
		Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
		if (genericTypeDefinition == null)
		{
			return false;
		}
		MemberInfo[] member = genericTypeDefinition.GetMember(propertyInfo.Name, bindingAttr);
		if (member.Length == 0)
		{
			return false;
		}
		if (!GetMemberUnderlyingType(member[0]).IsGenericParameter)
		{
			return false;
		}
		return true;
	}

	public static T? GetAttribute<T>(object attributeProvider) where T : Attribute
	{
		return GetAttribute<T>(attributeProvider, inherit: true);
	}

	public static T? GetAttribute<T>(object attributeProvider, bool inherit) where T : Attribute
	{
		T[] attributes = GetAttributes<T>(attributeProvider, inherit);
		if (attributes == null)
		{
			return null;
		}
		return attributes.FirstOrDefault();
	}

	public static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
	{
		Attribute[] attributes = GetAttributes(attributeProvider, typeof(T), inherit);
		if (attributes is T[] result)
		{
			return result;
		}
		return attributes.Cast<T>().ToArray();
	}

	public static Attribute[] GetAttributes(object attributeProvider, Type? attributeType, bool inherit)
	{
		ValidationUtils.ArgumentNotNull(attributeProvider, "attributeProvider");
		if (!(attributeProvider is Type type))
		{
			if (!(attributeProvider is Assembly element))
			{
				if (!(attributeProvider is MemberInfo element2))
				{
					if (!(attributeProvider is Module element3))
					{
						if (attributeProvider is ParameterInfo element4)
						{
							if (!(attributeType != null))
							{
								return Attribute.GetCustomAttributes(element4, inherit);
							}
							return Attribute.GetCustomAttributes(element4, attributeType, inherit);
						}
						ICustomAttributeProvider customAttributeProvider = (ICustomAttributeProvider)attributeProvider;
						return (Attribute[])((attributeType != null) ? customAttributeProvider.GetCustomAttributes(attributeType, inherit) : customAttributeProvider.GetCustomAttributes(inherit));
					}
					if (!(attributeType != null))
					{
						return Attribute.GetCustomAttributes(element3, inherit);
					}
					return Attribute.GetCustomAttributes(element3, attributeType, inherit);
				}
				if (!(attributeType != null))
				{
					return Attribute.GetCustomAttributes(element2, inherit);
				}
				return Attribute.GetCustomAttributes(element2, attributeType, inherit);
			}
			if (!(attributeType != null))
			{
				return Attribute.GetCustomAttributes(element);
			}
			return Attribute.GetCustomAttributes(element, attributeType);
		}
		return ((attributeType != null) ? type.GetCustomAttributes(attributeType, inherit) : type.GetCustomAttributes(inherit)).Cast<Attribute>().ToArray();
	}

	public static StructMultiKey<string?, string> SplitFullyQualifiedTypeName(string fullyQualifiedTypeName)
	{
		int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);
		string v;
		string v2;
		if (assemblyDelimiterIndex.HasValue)
		{
			v = fullyQualifiedTypeName.Trim(0, assemblyDelimiterIndex.GetValueOrDefault());
			v2 = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
		}
		else
		{
			v = fullyQualifiedTypeName;
			v2 = null;
		}
		return new StructMultiKey<string, string>(v2, v);
	}

	private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
	{
		int num = 0;
		for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
		{
			switch (fullyQualifiedTypeName[i])
			{
			case '[':
				num++;
				break;
			case ']':
				num--;
				break;
			case ',':
				if (num == 0)
				{
					return i;
				}
				break;
			}
		}
		return null;
	}

	public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo memberInfo)
	{
		if (memberInfo.MemberType() == MemberTypes.Property)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			Type[] types = (from p in propertyInfo.GetIndexParameters()
				select p.ParameterType).ToArray();
			return targetType.GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, propertyInfo.PropertyType, types, null);
		}
		return targetType.GetMember(memberInfo.Name, memberInfo.MemberType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SingleOrDefault();
	}

	public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
	{
		ValidationUtils.ArgumentNotNull(targetType, "targetType");
		List<MemberInfo> list = new List<MemberInfo>(targetType.GetFields(bindingAttr));
		GetChildPrivateFields(list, targetType, bindingAttr);
		return list.Cast<FieldInfo>();
	}

	private static void GetChildPrivateFields(IList<MemberInfo> initialFields, Type targetType, BindingFlags bindingAttr)
	{
		if ((bindingAttr & BindingFlags.NonPublic) == 0)
		{
			return;
		}
		BindingFlags bindingAttr2 = bindingAttr.RemoveFlag(BindingFlags.Public);
		while ((targetType = targetType.BaseType()) != null)
		{
			IEnumerable<FieldInfo> collection = from f in targetType.GetFields(bindingAttr2)
				where f.IsPrivate
				select f;
			initialFields.AddRange(collection);
		}
	}

	public static IEnumerable<PropertyInfo> GetProperties(Type targetType, BindingFlags bindingAttr)
	{
		ValidationUtils.ArgumentNotNull(targetType, "targetType");
		List<PropertyInfo> list = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));
		if (targetType.IsInterface())
		{
			Type[] interfaces = targetType.GetInterfaces();
			foreach (Type type in interfaces)
			{
				list.AddRange(type.GetProperties(bindingAttr));
			}
		}
		GetChildPrivateProperties(list, targetType, bindingAttr);
		for (int j = 0; j < list.Count; j++)
		{
			PropertyInfo propertyInfo = list[j];
			if (propertyInfo.DeclaringType != targetType)
			{
				PropertyInfo propertyInfo3 = (list[j] = (PropertyInfo)GetMemberInfoFromType(propertyInfo.DeclaringType, propertyInfo));
			}
		}
		return list;
	}

	public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
	{
		if ((bindingAttr & flag) != flag)
		{
			return bindingAttr;
		}
		return bindingAttr ^ flag;
	}

	private static void GetChildPrivateProperties(IList<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
	{
		while ((targetType = targetType.BaseType()) != null)
		{
			PropertyInfo[] properties = targetType.GetProperties(bindingAttr);
			foreach (PropertyInfo propertyInfo in properties)
			{
				PropertyInfo subTypeProperty = propertyInfo;
				if (!subTypeProperty.IsVirtual())
				{
					if (!IsPublic(subTypeProperty))
					{
						int num = initialProperties.IndexOf((PropertyInfo p) => p.Name == subTypeProperty.Name);
						if (num == -1)
						{
							initialProperties.Add(subTypeProperty);
						}
						else if (!IsPublic(initialProperties[num]))
						{
							initialProperties[num] = subTypeProperty;
						}
					}
					else if (initialProperties.IndexOf((PropertyInfo p) => p.Name == subTypeProperty.Name && p.DeclaringType == subTypeProperty.DeclaringType) == -1)
					{
						initialProperties.Add(subTypeProperty);
					}
				}
				else
				{
					Type subTypePropertyDeclaringType = subTypeProperty.GetBaseDefinition()?.DeclaringType ?? subTypeProperty.DeclaringType;
					if (initialProperties.IndexOf((PropertyInfo p) => p.Name == subTypeProperty.Name && p.IsVirtual() && (p.GetBaseDefinition()?.DeclaringType ?? p.DeclaringType).IsAssignableFrom(subTypePropertyDeclaringType)) == -1)
					{
						initialProperties.Add(subTypeProperty);
					}
				}
			}
		}
	}

	public static bool IsMethodOverridden(Type currentType, Type methodDeclaringType, string method)
	{
		string method2 = method;
		Type methodDeclaringType2 = methodDeclaringType;
		return currentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any((MethodInfo info) => info.Name == method2 && info.DeclaringType != methodDeclaringType2 && info.GetBaseDefinition().DeclaringType == methodDeclaringType2);
	}

	public static object? GetDefaultValue(Type type)
	{
		if (!type.IsValueType())
		{
			return null;
		}
		switch (ConvertUtils.GetTypeCode(type))
		{
		case PrimitiveTypeCode.Boolean:
			return false;
		case PrimitiveTypeCode.Char:
		case PrimitiveTypeCode.SByte:
		case PrimitiveTypeCode.Int16:
		case PrimitiveTypeCode.UInt16:
		case PrimitiveTypeCode.Int32:
		case PrimitiveTypeCode.Byte:
		case PrimitiveTypeCode.UInt32:
			return 0;
		case PrimitiveTypeCode.Int64:
		case PrimitiveTypeCode.UInt64:
			return 0L;
		case PrimitiveTypeCode.Single:
			return 0f;
		case PrimitiveTypeCode.Double:
			return 0.0;
		case PrimitiveTypeCode.Decimal:
			return 0m;
		case PrimitiveTypeCode.DateTime:
			return default(DateTime);
		case PrimitiveTypeCode.BigInteger:
			return default(BigInteger);
		case PrimitiveTypeCode.Guid:
			return default(Guid);
		case PrimitiveTypeCode.DateTimeOffset:
			return default(DateTimeOffset);
		default:
			if (IsNullable(type))
			{
				return null;
			}
			return Activator.CreateInstance(type);
		}
	}
}
