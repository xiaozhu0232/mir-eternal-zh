using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal static class ConvertUtils
{
	internal enum ConvertResult
	{
		Success,
		CannotConvertNull,
		NotInstantiableType,
		NoValidConversion
	}

	private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap = new Dictionary<Type, PrimitiveTypeCode>
	{
		{
			typeof(char),
			PrimitiveTypeCode.Char
		},
		{
			typeof(char?),
			PrimitiveTypeCode.CharNullable
		},
		{
			typeof(bool),
			PrimitiveTypeCode.Boolean
		},
		{
			typeof(bool?),
			PrimitiveTypeCode.BooleanNullable
		},
		{
			typeof(sbyte),
			PrimitiveTypeCode.SByte
		},
		{
			typeof(sbyte?),
			PrimitiveTypeCode.SByteNullable
		},
		{
			typeof(short),
			PrimitiveTypeCode.Int16
		},
		{
			typeof(short?),
			PrimitiveTypeCode.Int16Nullable
		},
		{
			typeof(ushort),
			PrimitiveTypeCode.UInt16
		},
		{
			typeof(ushort?),
			PrimitiveTypeCode.UInt16Nullable
		},
		{
			typeof(int),
			PrimitiveTypeCode.Int32
		},
		{
			typeof(int?),
			PrimitiveTypeCode.Int32Nullable
		},
		{
			typeof(byte),
			PrimitiveTypeCode.Byte
		},
		{
			typeof(byte?),
			PrimitiveTypeCode.ByteNullable
		},
		{
			typeof(uint),
			PrimitiveTypeCode.UInt32
		},
		{
			typeof(uint?),
			PrimitiveTypeCode.UInt32Nullable
		},
		{
			typeof(long),
			PrimitiveTypeCode.Int64
		},
		{
			typeof(long?),
			PrimitiveTypeCode.Int64Nullable
		},
		{
			typeof(ulong),
			PrimitiveTypeCode.UInt64
		},
		{
			typeof(ulong?),
			PrimitiveTypeCode.UInt64Nullable
		},
		{
			typeof(float),
			PrimitiveTypeCode.Single
		},
		{
			typeof(float?),
			PrimitiveTypeCode.SingleNullable
		},
		{
			typeof(double),
			PrimitiveTypeCode.Double
		},
		{
			typeof(double?),
			PrimitiveTypeCode.DoubleNullable
		},
		{
			typeof(DateTime),
			PrimitiveTypeCode.DateTime
		},
		{
			typeof(DateTime?),
			PrimitiveTypeCode.DateTimeNullable
		},
		{
			typeof(DateTimeOffset),
			PrimitiveTypeCode.DateTimeOffset
		},
		{
			typeof(DateTimeOffset?),
			PrimitiveTypeCode.DateTimeOffsetNullable
		},
		{
			typeof(decimal),
			PrimitiveTypeCode.Decimal
		},
		{
			typeof(decimal?),
			PrimitiveTypeCode.DecimalNullable
		},
		{
			typeof(Guid),
			PrimitiveTypeCode.Guid
		},
		{
			typeof(Guid?),
			PrimitiveTypeCode.GuidNullable
		},
		{
			typeof(TimeSpan),
			PrimitiveTypeCode.TimeSpan
		},
		{
			typeof(TimeSpan?),
			PrimitiveTypeCode.TimeSpanNullable
		},
		{
			typeof(BigInteger),
			PrimitiveTypeCode.BigInteger
		},
		{
			typeof(BigInteger?),
			PrimitiveTypeCode.BigIntegerNullable
		},
		{
			typeof(Uri),
			PrimitiveTypeCode.Uri
		},
		{
			typeof(string),
			PrimitiveTypeCode.String
		},
		{
			typeof(byte[]),
			PrimitiveTypeCode.Bytes
		},
		{
			typeof(DBNull),
			PrimitiveTypeCode.DBNull
		}
	};

	private static readonly TypeInformation[] PrimitiveTypeCodes = new TypeInformation[19]
	{
		new TypeInformation(typeof(object), PrimitiveTypeCode.Empty),
		new TypeInformation(typeof(object), PrimitiveTypeCode.Object),
		new TypeInformation(typeof(object), PrimitiveTypeCode.DBNull),
		new TypeInformation(typeof(bool), PrimitiveTypeCode.Boolean),
		new TypeInformation(typeof(char), PrimitiveTypeCode.Char),
		new TypeInformation(typeof(sbyte), PrimitiveTypeCode.SByte),
		new TypeInformation(typeof(byte), PrimitiveTypeCode.Byte),
		new TypeInformation(typeof(short), PrimitiveTypeCode.Int16),
		new TypeInformation(typeof(ushort), PrimitiveTypeCode.UInt16),
		new TypeInformation(typeof(int), PrimitiveTypeCode.Int32),
		new TypeInformation(typeof(uint), PrimitiveTypeCode.UInt32),
		new TypeInformation(typeof(long), PrimitiveTypeCode.Int64),
		new TypeInformation(typeof(ulong), PrimitiveTypeCode.UInt64),
		new TypeInformation(typeof(float), PrimitiveTypeCode.Single),
		new TypeInformation(typeof(double), PrimitiveTypeCode.Double),
		new TypeInformation(typeof(decimal), PrimitiveTypeCode.Decimal),
		new TypeInformation(typeof(DateTime), PrimitiveTypeCode.DateTime),
		new TypeInformation(typeof(object), PrimitiveTypeCode.Empty),
		new TypeInformation(typeof(string), PrimitiveTypeCode.String)
	};

	private static readonly ThreadSafeStore<StructMultiKey<Type, Type>, Func<object?, object?>?> CastConverters = new ThreadSafeStore<StructMultiKey<Type, Type>, Func<object, object>>(CreateCastConverter);

	public static PrimitiveTypeCode GetTypeCode(Type t)
	{
		bool isEnum;
		return GetTypeCode(t, out isEnum);
	}

	public static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
	{
		if (TypeCodeMap.TryGetValue(t, out var value))
		{
			isEnum = false;
			return value;
		}
		if (t.IsEnum())
		{
			isEnum = true;
			return GetTypeCode(Enum.GetUnderlyingType(t));
		}
		if (ReflectionUtils.IsNullableType(t))
		{
			Type underlyingType = Nullable.GetUnderlyingType(t);
			if (underlyingType.IsEnum())
			{
				Type t2 = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(underlyingType));
				isEnum = true;
				return GetTypeCode(t2);
			}
		}
		isEnum = false;
		return PrimitiveTypeCode.Object;
	}

	public static TypeInformation GetTypeInformation(IConvertible convertable)
	{
		return PrimitiveTypeCodes[(int)convertable.GetTypeCode()];
	}

	public static bool IsConvertible(Type t)
	{
		return typeof(IConvertible).IsAssignableFrom(t);
	}

	public static TimeSpan ParseTimeSpan(string input)
	{
		return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
	}

	private static Func<object?, object?>? CreateCastConverter(StructMultiKey<Type, Type> t)
	{
		Type value = t.Value1;
		Type value2 = t.Value2;
		MethodInfo methodInfo = value2.GetMethod("op_Implicit", new Type[1] { value }) ?? value2.GetMethod("op_Explicit", new Type[1] { value });
		if (methodInfo == null)
		{
			return null;
		}
		MethodCall<object?, object?> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
		return (object? o) => call(null, o);
	}

	internal static BigInteger ToBigInteger(object value)
	{
		if (value is BigInteger)
		{
			return (BigInteger)value;
		}
		if (value is string value2)
		{
			return BigInteger.Parse(value2, CultureInfo.InvariantCulture);
		}
		if (value is float value3)
		{
			return new BigInteger(value3);
		}
		if (value is double value4)
		{
			return new BigInteger(value4);
		}
		if (value is decimal value5)
		{
			return new BigInteger(value5);
		}
		if (value is int value6)
		{
			return new BigInteger(value6);
		}
		if (value is long value7)
		{
			return new BigInteger(value7);
		}
		if (value is uint value8)
		{
			return new BigInteger(value8);
		}
		if (value is ulong value9)
		{
			return new BigInteger(value9);
		}
		if (value is byte[] value10)
		{
			return new BigInteger(value10);
		}
		throw new InvalidCastException("Cannot convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
	}

	public static object FromBigInteger(BigInteger i, Type targetType)
	{
		if (targetType == typeof(decimal))
		{
			return (decimal)i;
		}
		if (targetType == typeof(double))
		{
			return (double)i;
		}
		if (targetType == typeof(float))
		{
			return (float)i;
		}
		if (targetType == typeof(ulong))
		{
			return (ulong)i;
		}
		if (targetType == typeof(bool))
		{
			return i != 0L;
		}
		try
		{
			return System.Convert.ChangeType((long)i, targetType, CultureInfo.InvariantCulture);
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Can not convert from BigInteger to {0}.".FormatWith(CultureInfo.InvariantCulture, targetType), innerException);
		}
	}

	public static object Convert(object initialValue, CultureInfo culture, Type targetType)
	{
		object value;
		return TryConvertInternal(initialValue, culture, targetType, out value) switch
		{
			ConvertResult.Success => value, 
			ConvertResult.CannotConvertNull => throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType)), 
			ConvertResult.NotInstantiableType => throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, targetType), "targetType"), 
			ConvertResult.NoValidConversion => throw new InvalidOperationException("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType)), 
			_ => throw new InvalidOperationException("Unexpected conversion result."), 
		};
	}

	private static bool TryConvert(object? initialValue, CultureInfo culture, Type targetType, out object? value)
	{
		try
		{
			if (TryConvertInternal(initialValue, culture, targetType, out value) == ConvertResult.Success)
			{
				return true;
			}
			value = null;
			return false;
		}
		catch
		{
			value = null;
			return false;
		}
	}

	private static ConvertResult TryConvertInternal(object? initialValue, CultureInfo culture, Type targetType, out object? value)
	{
		if (initialValue == null)
		{
			throw new ArgumentNullException("initialValue");
		}
		if (ReflectionUtils.IsNullableType(targetType))
		{
			targetType = Nullable.GetUnderlyingType(targetType);
		}
		Type type = initialValue!.GetType();
		if (targetType == type)
		{
			value = initialValue;
			return ConvertResult.Success;
		}
		if (IsConvertible(initialValue!.GetType()) && IsConvertible(targetType))
		{
			if (targetType.IsEnum())
			{
				if (initialValue is string)
				{
					value = Enum.Parse(targetType, initialValue!.ToString(), ignoreCase: true);
					return ConvertResult.Success;
				}
				if (IsInteger(initialValue))
				{
					value = Enum.ToObject(targetType, initialValue);
					return ConvertResult.Success;
				}
			}
			value = System.Convert.ChangeType(initialValue, targetType, culture);
			return ConvertResult.Success;
		}
		if (initialValue is DateTime dateTime && targetType == typeof(DateTimeOffset))
		{
			value = new DateTimeOffset(dateTime);
			return ConvertResult.Success;
		}
		if (initialValue is byte[] b && targetType == typeof(Guid))
		{
			value = new Guid(b);
			return ConvertResult.Success;
		}
		if (initialValue is Guid guid && targetType == typeof(byte[]))
		{
			value = guid.ToByteArray();
			return ConvertResult.Success;
		}
		if (initialValue is string text)
		{
			if (targetType == typeof(Guid))
			{
				value = new Guid(text);
				return ConvertResult.Success;
			}
			if (targetType == typeof(Uri))
			{
				value = new Uri(text, UriKind.RelativeOrAbsolute);
				return ConvertResult.Success;
			}
			if (targetType == typeof(TimeSpan))
			{
				value = ParseTimeSpan(text);
				return ConvertResult.Success;
			}
			if (targetType == typeof(byte[]))
			{
				value = System.Convert.FromBase64String(text);
				return ConvertResult.Success;
			}
			if (targetType == typeof(Version))
			{
				if (VersionTryParse(text, out var result))
				{
					value = result;
					return ConvertResult.Success;
				}
				value = null;
				return ConvertResult.NoValidConversion;
			}
			if (typeof(Type).IsAssignableFrom(targetType))
			{
				value = Type.GetType(text, throwOnError: true);
				return ConvertResult.Success;
			}
		}
		if (targetType == typeof(BigInteger))
		{
			value = ToBigInteger(initialValue);
			return ConvertResult.Success;
		}
		if (initialValue is BigInteger i)
		{
			value = FromBigInteger(i, targetType);
			return ConvertResult.Success;
		}
		TypeConverter converter = TypeDescriptor.GetConverter(type);
		if (converter != null && converter.CanConvertTo(targetType))
		{
			value = converter.ConvertTo(null, culture, initialValue, targetType);
			return ConvertResult.Success;
		}
		TypeConverter converter2 = TypeDescriptor.GetConverter(targetType);
		if (converter2 != null && converter2.CanConvertFrom(type))
		{
			value = converter2.ConvertFrom(null, culture, initialValue);
			return ConvertResult.Success;
		}
		if (initialValue == DBNull.Value)
		{
			if (ReflectionUtils.IsNullable(targetType))
			{
				value = EnsureTypeAssignable(null, type, targetType);
				return ConvertResult.Success;
			}
			value = null;
			return ConvertResult.CannotConvertNull;
		}
		if (targetType.IsInterface() || targetType.IsGenericTypeDefinition() || targetType.IsAbstract())
		{
			value = null;
			return ConvertResult.NotInstantiableType;
		}
		value = null;
		return ConvertResult.NoValidConversion;
	}

	public static object? ConvertOrCast(object? initialValue, CultureInfo culture, Type targetType)
	{
		if (targetType == typeof(object))
		{
			return initialValue;
		}
		if (initialValue == null && ReflectionUtils.IsNullable(targetType))
		{
			return null;
		}
		if (TryConvert(initialValue, culture, targetType, out var value))
		{
			return value;
		}
		return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
	}

	private static object? EnsureTypeAssignable(object? value, Type initialType, Type targetType)
	{
		if (value != null)
		{
			Type type = value!.GetType();
			if (targetType.IsAssignableFrom(type))
			{
				return value;
			}
			Func<object, object> func = CastConverters.Get(new StructMultiKey<Type, Type>(type, targetType));
			if (func != null)
			{
				return func(value);
			}
		}
		else if (ReflectionUtils.IsNullable(targetType))
		{
			return null;
		}
		throw new ArgumentException("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialType?.ToString() ?? "{null}", targetType));
	}

	public static bool VersionTryParse(string input, [NotNullWhen(true)] out Version? result)
	{
		return Version.TryParse(input, out result);
	}

	public static bool IsInteger(object value)
	{
		switch (GetTypeCode(value.GetType()))
		{
		case PrimitiveTypeCode.SByte:
		case PrimitiveTypeCode.Int16:
		case PrimitiveTypeCode.UInt16:
		case PrimitiveTypeCode.Int32:
		case PrimitiveTypeCode.Byte:
		case PrimitiveTypeCode.UInt32:
		case PrimitiveTypeCode.Int64:
		case PrimitiveTypeCode.UInt64:
			return true;
		default:
			return false;
		}
	}

	public static ParseResult Int32TryParse(char[] chars, int start, int length, out int value)
	{
		value = 0;
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int num = start + length;
		if (length > 10 || (length == 10 && chars[start] - 48 > 2))
		{
			for (int i = start; i < num; i++)
			{
				int num2 = chars[i] - 48;
				if (num2 < 0 || num2 > 9)
				{
					return ParseResult.Invalid;
				}
			}
			return ParseResult.Overflow;
		}
		for (int j = start; j < num; j++)
		{
			int num3 = chars[j] - 48;
			if (num3 < 0 || num3 > 9)
			{
				return ParseResult.Invalid;
			}
			int num4 = 10 * value - num3;
			if (num4 > value)
			{
				for (j++; j < num; j++)
				{
					num3 = chars[j] - 48;
					if (num3 < 0 || num3 > 9)
					{
						return ParseResult.Invalid;
					}
				}
				return ParseResult.Overflow;
			}
			value = num4;
		}
		if (!flag)
		{
			if (value == int.MinValue)
			{
				return ParseResult.Overflow;
			}
			value = -value;
		}
		return ParseResult.Success;
	}

	public static ParseResult Int64TryParse(char[] chars, int start, int length, out long value)
	{
		value = 0L;
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int num = start + length;
		if (length > 19)
		{
			for (int i = start; i < num; i++)
			{
				int num2 = chars[i] - 48;
				if (num2 < 0 || num2 > 9)
				{
					return ParseResult.Invalid;
				}
			}
			return ParseResult.Overflow;
		}
		for (int j = start; j < num; j++)
		{
			int num3 = chars[j] - 48;
			if (num3 < 0 || num3 > 9)
			{
				return ParseResult.Invalid;
			}
			long num4 = 10 * value - num3;
			if (num4 > value)
			{
				for (j++; j < num; j++)
				{
					num3 = chars[j] - 48;
					if (num3 < 0 || num3 > 9)
					{
						return ParseResult.Invalid;
					}
				}
				return ParseResult.Overflow;
			}
			value = num4;
		}
		if (!flag)
		{
			if (value == long.MinValue)
			{
				return ParseResult.Overflow;
			}
			value = -value;
		}
		return ParseResult.Success;
	}

	public static ParseResult DecimalTryParse(char[] chars, int start, int length, out decimal value)
	{
		value = default(decimal);
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int i = start;
		int num = start + length;
		int num2 = num;
		int num3 = num;
		int num4 = 0;
		ulong num5 = 0uL;
		ulong num6 = 0uL;
		int num7 = 0;
		int num8 = 0;
		char? c = null;
		bool? flag2 = null;
		for (; i < num; i++)
		{
			char c2 = chars[i];
			if (c2 == '.')
			{
				goto IL_0074;
			}
			if (c2 == 'E' || c2 == 'e')
			{
				goto IL_0091;
			}
			if (c2 < '0' || c2 > '9')
			{
				return ParseResult.Invalid;
			}
			if (i == start && c2 == '0')
			{
				i++;
				if (i != num)
				{
					c2 = chars[i];
					if (c2 == '.')
					{
						goto IL_0074;
					}
					if (c2 != 'e' && c2 != 'E')
					{
						return ParseResult.Invalid;
					}
					goto IL_0091;
				}
			}
			if (num7 < 29)
			{
				if (num7 == 28)
				{
					bool? flag3 = flag2;
					bool valueOrDefault;
					if (!flag3.HasValue)
					{
						flag2 = num5 > 7922816251426433759L || (num5 == 7922816251426433759L && (num6 > 354395033 || (num6 == 354395033 && c2 > '5')));
						bool? flag4 = flag2;
						valueOrDefault = flag4.GetValueOrDefault();
					}
					else
					{
						valueOrDefault = flag3.GetValueOrDefault();
					}
					if (valueOrDefault)
					{
						goto IL_01ff;
					}
				}
				if (num7 < 19)
				{
					num5 = num5 * 10 + (ulong)(c2 - 48);
				}
				else
				{
					num6 = num6 * 10 + (ulong)(c2 - 48);
				}
				num7++;
				continue;
			}
			goto IL_01ff;
			IL_0074:
			if (i == start)
			{
				return ParseResult.Invalid;
			}
			if (i + 1 == num)
			{
				return ParseResult.Invalid;
			}
			if (num2 != num)
			{
				return ParseResult.Invalid;
			}
			num2 = i + 1;
			continue;
			IL_01ff:
			if (!c.HasValue)
			{
				c = c2;
			}
			num8++;
			continue;
			IL_0091:
			if (i == start)
			{
				return ParseResult.Invalid;
			}
			if (i == num2)
			{
				return ParseResult.Invalid;
			}
			i++;
			if (i == num)
			{
				return ParseResult.Invalid;
			}
			if (num2 < num)
			{
				num3 = i - 1;
			}
			c2 = chars[i];
			bool flag5 = false;
			switch (c2)
			{
			case '-':
				flag5 = true;
				i++;
				break;
			case '+':
				i++;
				break;
			}
			for (; i < num; i++)
			{
				c2 = chars[i];
				if (c2 < '0' || c2 > '9')
				{
					return ParseResult.Invalid;
				}
				int num9 = 10 * num4 + (c2 - 48);
				if (num4 < num9)
				{
					num4 = num9;
				}
			}
			if (flag5)
			{
				num4 = -num4;
			}
		}
		num4 += num8;
		num4 -= num3 - num2;
		if (num7 <= 19)
		{
			value = num5;
		}
		else
		{
			value = (decimal)num5 / new decimal(1, 0, 0, isNegative: false, (byte)(num7 - 19)) + (decimal)num6;
		}
		if (num4 > 0)
		{
			num7 += num4;
			if (num7 > 29)
			{
				return ParseResult.Overflow;
			}
			if (num7 == 29)
			{
				if (num4 > 1)
				{
					value /= new decimal(1, 0, 0, isNegative: false, (byte)(num4 - 1));
					if (value > 7922816251426433759354395033m)
					{
						return ParseResult.Overflow;
					}
				}
				else if (value == 7922816251426433759354395033m && c > '5')
				{
					return ParseResult.Overflow;
				}
				value *= 10m;
			}
			else
			{
				value /= new decimal(1, 0, 0, isNegative: false, (byte)num4);
			}
		}
		else
		{
			if (c >= '5' && num4 >= -28)
			{
				++value;
			}
			if (num4 < 0)
			{
				if (num7 + num4 + 28 <= 0)
				{
					value = (flag ? 0m : 0m);
					return ParseResult.Success;
				}
				if (num4 >= -28)
				{
					value *= new decimal(1, 0, 0, isNegative: false, (byte)(-num4));
				}
				else
				{
					value /= 10000000000000000000000000000m;
					value *= new decimal(1, 0, 0, isNegative: false, (byte)(-num4 - 28));
				}
			}
		}
		if (flag)
		{
			value = -value;
		}
		return ParseResult.Success;
	}

	public static bool TryConvertGuid(string s, out Guid g)
	{
		return Guid.TryParseExact(s, "D", out g);
	}

	public static bool TryHexTextToInt(char[] text, int start, int end, out int value)
	{
		value = 0;
		for (int i = start; i < end; i++)
		{
			char c = text[i];
			int num;
			if (c <= '9' && c >= '0')
			{
				num = c - 48;
			}
			else if (c <= 'F' && c >= 'A')
			{
				num = c - 55;
			}
			else
			{
				if (c > 'f' || c < 'a')
				{
					value = 0;
					return false;
				}
				num = c - 87;
			}
			value += num << (end - 1 - i) * 4;
		}
		return true;
	}
}
