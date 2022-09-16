using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal static class EnumUtils
{
	private const char EnumSeparatorChar = ',';

	private const string EnumSeparatorString = ", ";

	private static readonly ThreadSafeStore<StructMultiKey<Type, NamingStrategy?>, EnumInfo> ValuesAndNamesPerEnum = new ThreadSafeStore<StructMultiKey<Type, NamingStrategy>, EnumInfo>(InitializeValuesAndNames);

	private static CamelCaseNamingStrategy _camelCaseNamingStrategy = new CamelCaseNamingStrategy();

	private static EnumInfo InitializeValuesAndNames(StructMultiKey<Type, NamingStrategy?> key)
	{
		Type value = key.Value1;
		string[] names = Enum.GetNames(value);
		string[] array = new string[names.Length];
		ulong[] array2 = new ulong[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			FieldInfo field = value.GetField(text, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			array2[i] = ToUInt64(field.GetValue(null));
			string text2 = (from EnumMemberAttribute a in field.GetCustomAttributes(typeof(EnumMemberAttribute), inherit: true)
				select a.Value).SingleOrDefault();
			bool hasSpecifiedName = text2 != null;
			if (text2 == null)
			{
				text2 = text;
			}
			string text3 = text2;
			if (Array.IndexOf(array, text3, 0, i) != -1)
			{
				throw new InvalidOperationException("Enum name '{0}' already exists on enum '{1}'.".FormatWith(CultureInfo.InvariantCulture, text3, value.Name));
			}
			array[i] = ((key.Value2 != null) ? key.Value2!.GetPropertyName(text3, hasSpecifiedName) : text3);
		}
		return new EnumInfo(value.IsDefined(typeof(FlagsAttribute), inherit: false), array2, names, array);
	}

	public static IList<T> GetFlagsValues<T>(T value) where T : struct
	{
		Type typeFromHandle = typeof(T);
		if (!typeFromHandle.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			throw new ArgumentException("Enum type {0} is not a set of flags.".FormatWith(CultureInfo.InvariantCulture, typeFromHandle));
		}
		Type underlyingType = Enum.GetUnderlyingType(value.GetType());
		ulong num = ToUInt64(value);
		EnumInfo enumValuesAndNames = GetEnumValuesAndNames(typeFromHandle);
		IList<T> list = new List<T>();
		for (int i = 0; i < enumValuesAndNames.Values.Length; i++)
		{
			ulong num2 = enumValuesAndNames.Values[i];
			if ((num & num2) == num2 && num2 != 0L)
			{
				list.Add((T)Convert.ChangeType(num2, underlyingType, CultureInfo.CurrentCulture));
			}
		}
		if (list.Count == 0 && enumValuesAndNames.Values.Any((ulong v) => v == 0))
		{
			list.Add(default(T));
		}
		return list;
	}

	public static bool TryToString(Type enumType, object value, bool camelCase, [NotNullWhen(true)] out string? name)
	{
		return TryToString(enumType, value, camelCase ? _camelCaseNamingStrategy : null, out name);
	}

	public static bool TryToString(Type enumType, object value, NamingStrategy? namingStrategy, [NotNullWhen(true)] out string? name)
	{
		EnumInfo enumInfo = ValuesAndNamesPerEnum.Get(new StructMultiKey<Type, NamingStrategy>(enumType, namingStrategy));
		ulong num = ToUInt64(value);
		if (!enumInfo.IsFlags)
		{
			int num2 = Array.BinarySearch(enumInfo.Values, num);
			if (num2 >= 0)
			{
				name = enumInfo.ResolvedNames[num2];
				return true;
			}
			name = null;
			return false;
		}
		name = InternalFlagsFormat(enumInfo, num);
		return name != null;
	}

	private static string? InternalFlagsFormat(EnumInfo entry, ulong result)
	{
		string[] resolvedNames = entry.ResolvedNames;
		ulong[] values = entry.Values;
		int num = values.Length - 1;
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		ulong num2 = result;
		while (num >= 0 && (num != 0 || values[num] != 0L))
		{
			if ((result & values[num]) == values[num])
			{
				result -= values[num];
				if (!flag)
				{
					stringBuilder.Insert(0, ", ");
				}
				string value = resolvedNames[num];
				stringBuilder.Insert(0, value);
				flag = false;
			}
			num--;
		}
		if (result != 0L)
		{
			return null;
		}
		if (num2 == 0L)
		{
			if (values.Length != 0 && values[0] == 0L)
			{
				return resolvedNames[0];
			}
			return null;
		}
		return stringBuilder.ToString();
	}

	public static EnumInfo GetEnumValuesAndNames(Type enumType)
	{
		return ValuesAndNamesPerEnum.Get(new StructMultiKey<Type, NamingStrategy>(enumType, null));
	}

	private static ulong ToUInt64(object value)
	{
		bool isEnum;
		return ConvertUtils.GetTypeCode(value.GetType(), out isEnum) switch
		{
			PrimitiveTypeCode.SByte => (ulong)(sbyte)value, 
			PrimitiveTypeCode.Byte => (byte)value, 
			PrimitiveTypeCode.Boolean => Convert.ToByte((bool)value), 
			PrimitiveTypeCode.Int16 => (ulong)(short)value, 
			PrimitiveTypeCode.UInt16 => (ushort)value, 
			PrimitiveTypeCode.Char => (char)value, 
			PrimitiveTypeCode.UInt32 => (uint)value, 
			PrimitiveTypeCode.Int32 => (ulong)(int)value, 
			PrimitiveTypeCode.UInt64 => (ulong)value, 
			PrimitiveTypeCode.Int64 => (ulong)(long)value, 
			_ => throw new InvalidOperationException("Unknown enum type."), 
		};
	}

	public static object ParseEnum(Type enumType, NamingStrategy? namingStrategy, string value, bool disallowNumber)
	{
		ValidationUtils.ArgumentNotNull(enumType, "enumType");
		ValidationUtils.ArgumentNotNull(value, "value");
		if (!enumType.IsEnum())
		{
			throw new ArgumentException("Type provided must be an Enum.", "enumType");
		}
		EnumInfo enumInfo = ValuesAndNamesPerEnum.Get(new StructMultiKey<Type, NamingStrategy>(enumType, namingStrategy));
		string[] names = enumInfo.Names;
		string[] resolvedNames = enumInfo.ResolvedNames;
		ulong[] values = enumInfo.Values;
		int? num = FindIndexByName(resolvedNames, value, 0, value.Length, StringComparison.Ordinal);
		if (num.HasValue)
		{
			return Enum.ToObject(enumType, values[num.Value]);
		}
		int num2 = -1;
		for (int i = 0; i < value.Length; i++)
		{
			if (!char.IsWhiteSpace(value[i]))
			{
				num2 = i;
				break;
			}
		}
		if (num2 == -1)
		{
			throw new ArgumentException("Must specify valid information for parsing in the string.");
		}
		char c = value[num2];
		if (char.IsDigit(c) || c == '-' || c == '+')
		{
			Type underlyingType = Enum.GetUnderlyingType(enumType);
			value = value.Trim();
			object obj = null;
			try
			{
				obj = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
			}
			if (obj != null)
			{
				if (disallowNumber)
				{
					throw new FormatException("Integer string '{0}' is not allowed.".FormatWith(CultureInfo.InvariantCulture, value));
				}
				return Enum.ToObject(enumType, obj);
			}
		}
		ulong num3 = 0uL;
		int j = num2;
		while (j <= value.Length)
		{
			int num4 = value.IndexOf(',', j);
			if (num4 == -1)
			{
				num4 = value.Length;
			}
			int num5 = num4;
			for (; j < num4 && char.IsWhiteSpace(value[j]); j++)
			{
			}
			while (num5 > j && char.IsWhiteSpace(value[num5 - 1]))
			{
				num5--;
			}
			int valueSubstringLength = num5 - j;
			num = MatchName(value, names, resolvedNames, j, valueSubstringLength, StringComparison.Ordinal);
			if (!num.HasValue)
			{
				num = MatchName(value, names, resolvedNames, j, valueSubstringLength, StringComparison.OrdinalIgnoreCase);
			}
			if (!num.HasValue)
			{
				num = FindIndexByName(resolvedNames, value, 0, value.Length, StringComparison.OrdinalIgnoreCase);
				if (num.HasValue)
				{
					return Enum.ToObject(enumType, values[num.Value]);
				}
				throw new ArgumentException("Requested value '{0}' was not found.".FormatWith(CultureInfo.InvariantCulture, value));
			}
			num3 |= values[num.Value];
			j = num4 + 1;
		}
		return Enum.ToObject(enumType, num3);
	}

	private static int? MatchName(string value, string[] enumNames, string[] resolvedNames, int valueIndex, int valueSubstringLength, StringComparison comparison)
	{
		int? result = FindIndexByName(resolvedNames, value, valueIndex, valueSubstringLength, comparison);
		if (!result.HasValue)
		{
			result = FindIndexByName(enumNames, value, valueIndex, valueSubstringLength, comparison);
		}
		return result;
	}

	private static int? FindIndexByName(string[] enumNames, string value, int valueIndex, int valueSubstringLength, StringComparison comparison)
	{
		for (int i = 0; i < enumNames.Length; i++)
		{
			if (enumNames[i].Length == valueSubstringLength && string.Compare(enumNames[i], 0, value, valueIndex, valueSubstringLength, comparison) == 0)
			{
				return i;
			}
		}
		return null;
	}
}
