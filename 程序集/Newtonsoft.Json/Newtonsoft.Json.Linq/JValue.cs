using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

public class JValue : JToken, IEquatable<JValue>, IFormattable, IComparable, IComparable<JValue>, IConvertible
{
	private class JValueDynamicProxy : DynamicProxy<JValue>
	{
		public override bool TryConvert(JValue instance, ConvertBinder binder, [NotNullWhen(true)] out object? result)
		{
			if (binder.Type == typeof(JValue) || binder.Type == typeof(JToken))
			{
				result = instance;
				return true;
			}
			object value = instance.Value;
			if (value == null)
			{
				result = null;
				return ReflectionUtils.IsNullable(binder.Type);
			}
			result = ConvertUtils.Convert(value, CultureInfo.InvariantCulture, binder.Type);
			return true;
		}

		public override bool TryBinaryOperation(JValue instance, BinaryOperationBinder binder, object arg, [NotNullWhen(true)] out object? result)
		{
			object objB = ((arg is JValue jValue) ? jValue.Value : arg);
			switch (binder.Operation)
			{
			case ExpressionType.Equal:
				result = Compare(instance.Type, instance.Value, objB) == 0;
				return true;
			case ExpressionType.NotEqual:
				result = Compare(instance.Type, instance.Value, objB) != 0;
				return true;
			case ExpressionType.GreaterThan:
				result = Compare(instance.Type, instance.Value, objB) > 0;
				return true;
			case ExpressionType.GreaterThanOrEqual:
				result = Compare(instance.Type, instance.Value, objB) >= 0;
				return true;
			case ExpressionType.LessThan:
				result = Compare(instance.Type, instance.Value, objB) < 0;
				return true;
			case ExpressionType.LessThanOrEqual:
				result = Compare(instance.Type, instance.Value, objB) <= 0;
				return true;
			case ExpressionType.Add:
			case ExpressionType.Divide:
			case ExpressionType.Multiply:
			case ExpressionType.Subtract:
			case ExpressionType.AddAssign:
			case ExpressionType.DivideAssign:
			case ExpressionType.MultiplyAssign:
			case ExpressionType.SubtractAssign:
				if (Operation(binder.Operation, instance.Value, objB, out result))
				{
					result = new JValue(result);
					return true;
				}
				break;
			}
			result = null;
			return false;
		}
	}

	private JTokenType _valueType;

	private object? _value;

	public override bool HasValues => false;

	public override JTokenType Type => _valueType;

	public new object? Value
	{
		get
		{
			return _value;
		}
		set
		{
			Type obj = _value?.GetType();
			Type type = value?.GetType();
			if (obj != type)
			{
				_valueType = GetValueType(_valueType, value);
			}
			_value = value;
		}
	}

	public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		if (converters != null && converters.Length != 0 && _value != null)
		{
			JsonConverter matchingConverter = JsonSerializer.GetMatchingConverter(converters, _value!.GetType());
			if (matchingConverter != null && matchingConverter.CanWrite)
			{
				matchingConverter.WriteJson(writer, _value, JsonSerializer.CreateDefault());
				return AsyncUtils.CompletedTask;
			}
		}
		switch (_valueType)
		{
		case JTokenType.Comment:
			return writer.WriteCommentAsync(_value?.ToString(), cancellationToken);
		case JTokenType.Raw:
			return writer.WriteRawValueAsync(_value?.ToString(), cancellationToken);
		case JTokenType.Null:
			return writer.WriteNullAsync(cancellationToken);
		case JTokenType.Undefined:
			return writer.WriteUndefinedAsync(cancellationToken);
		case JTokenType.Integer:
			if (_value is int value5)
			{
				return writer.WriteValueAsync(value5, cancellationToken);
			}
			if (_value is long value6)
			{
				return writer.WriteValueAsync(value6, cancellationToken);
			}
			if (_value is ulong value7)
			{
				return writer.WriteValueAsync(value7, cancellationToken);
			}
			if (_value is BigInteger bigInteger)
			{
				return writer.WriteValueAsync(bigInteger, cancellationToken);
			}
			return writer.WriteValueAsync(Convert.ToInt64(_value, CultureInfo.InvariantCulture), cancellationToken);
		case JTokenType.Float:
			if (_value is decimal value)
			{
				return writer.WriteValueAsync(value, cancellationToken);
			}
			if (_value is double value2)
			{
				return writer.WriteValueAsync(value2, cancellationToken);
			}
			if (_value is float value3)
			{
				return writer.WriteValueAsync(value3, cancellationToken);
			}
			return writer.WriteValueAsync(Convert.ToDouble(_value, CultureInfo.InvariantCulture), cancellationToken);
		case JTokenType.String:
			return writer.WriteValueAsync(_value?.ToString(), cancellationToken);
		case JTokenType.Boolean:
			return writer.WriteValueAsync(Convert.ToBoolean(_value, CultureInfo.InvariantCulture), cancellationToken);
		case JTokenType.Date:
			if (_value is DateTimeOffset value4)
			{
				return writer.WriteValueAsync(value4, cancellationToken);
			}
			return writer.WriteValueAsync(Convert.ToDateTime(_value, CultureInfo.InvariantCulture), cancellationToken);
		case JTokenType.Bytes:
			return writer.WriteValueAsync((byte[])_value, cancellationToken);
		case JTokenType.Guid:
			return writer.WriteValueAsync((_value != null) ? ((Guid?)_value) : null, cancellationToken);
		case JTokenType.TimeSpan:
			return writer.WriteValueAsync((_value != null) ? ((TimeSpan?)_value) : null, cancellationToken);
		case JTokenType.Uri:
			return writer.WriteValueAsync((Uri)_value, cancellationToken);
		default:
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", _valueType, "Unexpected token type.");
		}
	}

	internal JValue(object? value, JTokenType type)
	{
		_value = value;
		_valueType = type;
	}

	public JValue(JValue other)
		: this(other.Value, other.Type)
	{
		CopyAnnotations(this, other);
	}

	public JValue(long value)
		: this(value, JTokenType.Integer)
	{
	}

	public JValue(decimal value)
		: this(value, JTokenType.Float)
	{
	}

	public JValue(char value)
		: this(value, JTokenType.String)
	{
	}

	[CLSCompliant(false)]
	public JValue(ulong value)
		: this(value, JTokenType.Integer)
	{
	}

	public JValue(double value)
		: this(value, JTokenType.Float)
	{
	}

	public JValue(float value)
		: this(value, JTokenType.Float)
	{
	}

	public JValue(DateTime value)
		: this(value, JTokenType.Date)
	{
	}

	public JValue(DateTimeOffset value)
		: this(value, JTokenType.Date)
	{
	}

	public JValue(bool value)
		: this(value, JTokenType.Boolean)
	{
	}

	public JValue(string? value)
		: this(value, JTokenType.String)
	{
	}

	public JValue(Guid value)
		: this(value, JTokenType.Guid)
	{
	}

	public JValue(Uri? value)
		: this(value, (value != null) ? JTokenType.Uri : JTokenType.Null)
	{
	}

	public JValue(TimeSpan value)
		: this(value, JTokenType.TimeSpan)
	{
	}

	public JValue(object? value)
		: this(value, GetValueType(null, value))
	{
	}

	internal override bool DeepEquals(JToken node)
	{
		if (!(node is JValue jValue))
		{
			return false;
		}
		if (jValue == this)
		{
			return true;
		}
		return ValuesEquals(this, jValue);
	}

	private static int CompareBigInteger(BigInteger i1, object i2)
	{
		int num = i1.CompareTo(ConvertUtils.ToBigInteger(i2));
		if (num != 0)
		{
			return num;
		}
		if (i2 is decimal num2)
		{
			return 0m.CompareTo(Math.Abs(num2 - Math.Truncate(num2)));
		}
		if (i2 is double || i2 is float)
		{
			double num3 = Convert.ToDouble(i2, CultureInfo.InvariantCulture);
			return 0.0.CompareTo(Math.Abs(num3 - Math.Truncate(num3)));
		}
		return num;
	}

	internal static int Compare(JTokenType valueType, object? objA, object? objB)
	{
		if (objA == objB)
		{
			return 0;
		}
		if (objB == null)
		{
			return 1;
		}
		if (objA == null)
		{
			return -1;
		}
		switch (valueType)
		{
		case JTokenType.Integer:
			if (objA is BigInteger i3)
			{
				return CompareBigInteger(i3, objB);
			}
			if (objB is BigInteger i4)
			{
				return -CompareBigInteger(i4, objA);
			}
			if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
			{
				return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
			}
			if (objA is float || objB is float || objA is double || objB is double)
			{
				return CompareFloat(objA, objB);
			}
			return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
		case JTokenType.Float:
			if (objA is BigInteger i)
			{
				return CompareBigInteger(i, objB);
			}
			if (objB is BigInteger i2)
			{
				return -CompareBigInteger(i2, objA);
			}
			if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
			{
				return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
			}
			return CompareFloat(objA, objB);
		case JTokenType.Comment:
		case JTokenType.String:
		case JTokenType.Raw:
		{
			string strA = Convert.ToString(objA, CultureInfo.InvariantCulture);
			string strB = Convert.ToString(objB, CultureInfo.InvariantCulture);
			return string.CompareOrdinal(strA, strB);
		}
		case JTokenType.Boolean:
		{
			bool flag = Convert.ToBoolean(objA, CultureInfo.InvariantCulture);
			bool value4 = Convert.ToBoolean(objB, CultureInfo.InvariantCulture);
			return flag.CompareTo(value4);
		}
		case JTokenType.Date:
		{
			if (objA is DateTime dateTime)
			{
				DateTime value2 = ((!(objB is DateTimeOffset dateTimeOffset)) ? Convert.ToDateTime(objB, CultureInfo.InvariantCulture) : dateTimeOffset.DateTime);
				return dateTime.CompareTo(value2);
			}
			DateTimeOffset dateTimeOffset2 = (DateTimeOffset)objA;
			DateTimeOffset other = ((!(objB is DateTimeOffset)) ? new DateTimeOffset(Convert.ToDateTime(objB, CultureInfo.InvariantCulture)) : ((DateTimeOffset)objB));
			return dateTimeOffset2.CompareTo(other);
		}
		case JTokenType.Bytes:
			if (!(objB is byte[] a))
			{
				throw new ArgumentException("Object must be of type byte[].");
			}
			return MiscellaneousUtils.ByteArrayCompare(objA as byte[], a);
		case JTokenType.Guid:
		{
			if (!(objB is Guid))
			{
				throw new ArgumentException("Object must be of type Guid.");
			}
			Guid guid = (Guid)objA;
			Guid value3 = (Guid)objB;
			return guid.CompareTo(value3);
		}
		case JTokenType.Uri:
		{
			Uri uri = objB as Uri;
			if (uri == null)
			{
				throw new ArgumentException("Object must be of type Uri.");
			}
			Uri uri2 = (Uri)objA;
			return Comparer<string>.Default.Compare(uri2.ToString(), uri.ToString());
		}
		case JTokenType.TimeSpan:
		{
			if (!(objB is TimeSpan))
			{
				throw new ArgumentException("Object must be of type TimeSpan.");
			}
			TimeSpan timeSpan = (TimeSpan)objA;
			TimeSpan value = (TimeSpan)objB;
			return timeSpan.CompareTo(value);
		}
		default:
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("valueType", valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, valueType));
		}
	}

	private static int CompareFloat(object objA, object objB)
	{
		double d = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
		double num = Convert.ToDouble(objB, CultureInfo.InvariantCulture);
		if (MathUtils.ApproxEquals(d, num))
		{
			return 0;
		}
		return d.CompareTo(num);
	}

	private static bool Operation(ExpressionType operation, object? objA, object? objB, out object? result)
	{
		if ((objA is string || objB is string) && (operation == ExpressionType.Add || operation == ExpressionType.AddAssign))
		{
			result = objA?.ToString() + objB;
			return true;
		}
		if (objA is BigInteger || objB is BigInteger)
		{
			if (objA == null || objB == null)
			{
				result = null;
				return true;
			}
			BigInteger bigInteger = ConvertUtils.ToBigInteger(objA);
			BigInteger bigInteger2 = ConvertUtils.ToBigInteger(objB);
			switch (operation)
			{
			case ExpressionType.Add:
			case ExpressionType.AddAssign:
				result = bigInteger + bigInteger2;
				return true;
			case ExpressionType.Subtract:
			case ExpressionType.SubtractAssign:
				result = bigInteger - bigInteger2;
				return true;
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyAssign:
				result = bigInteger * bigInteger2;
				return true;
			case ExpressionType.Divide:
			case ExpressionType.DivideAssign:
				result = bigInteger / bigInteger2;
				return true;
			}
		}
		else if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
		{
			if (objA == null || objB == null)
			{
				result = null;
				return true;
			}
			decimal num = Convert.ToDecimal(objA, CultureInfo.InvariantCulture);
			decimal num2 = Convert.ToDecimal(objB, CultureInfo.InvariantCulture);
			switch (operation)
			{
			case ExpressionType.Add:
			case ExpressionType.AddAssign:
				result = num + num2;
				return true;
			case ExpressionType.Subtract:
			case ExpressionType.SubtractAssign:
				result = num - num2;
				return true;
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyAssign:
				result = num * num2;
				return true;
			case ExpressionType.Divide:
			case ExpressionType.DivideAssign:
				result = num / num2;
				return true;
			}
		}
		else if (objA is float || objB is float || objA is double || objB is double)
		{
			if (objA == null || objB == null)
			{
				result = null;
				return true;
			}
			double num3 = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
			double num4 = Convert.ToDouble(objB, CultureInfo.InvariantCulture);
			switch (operation)
			{
			case ExpressionType.Add:
			case ExpressionType.AddAssign:
				result = num3 + num4;
				return true;
			case ExpressionType.Subtract:
			case ExpressionType.SubtractAssign:
				result = num3 - num4;
				return true;
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyAssign:
				result = num3 * num4;
				return true;
			case ExpressionType.Divide:
			case ExpressionType.DivideAssign:
				result = num3 / num4;
				return true;
			}
		}
		else if (objA is int || objA is uint || objA is long || objA is short || objA is ushort || objA is sbyte || objA is byte || objB is int || objB is uint || objB is long || objB is short || objB is ushort || objB is sbyte || objB is byte)
		{
			if (objA == null || objB == null)
			{
				result = null;
				return true;
			}
			long num5 = Convert.ToInt64(objA, CultureInfo.InvariantCulture);
			long num6 = Convert.ToInt64(objB, CultureInfo.InvariantCulture);
			switch (operation)
			{
			case ExpressionType.Add:
			case ExpressionType.AddAssign:
				result = num5 + num6;
				return true;
			case ExpressionType.Subtract:
			case ExpressionType.SubtractAssign:
				result = num5 - num6;
				return true;
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyAssign:
				result = num5 * num6;
				return true;
			case ExpressionType.Divide:
			case ExpressionType.DivideAssign:
				result = num5 / num6;
				return true;
			}
		}
		result = null;
		return false;
	}

	internal override JToken CloneToken()
	{
		return new JValue(this);
	}

	public static JValue CreateComment(string? value)
	{
		return new JValue(value, JTokenType.Comment);
	}

	public static JValue CreateString(string? value)
	{
		return new JValue(value, JTokenType.String);
	}

	public static JValue CreateNull()
	{
		return new JValue(null, JTokenType.Null);
	}

	public static JValue CreateUndefined()
	{
		return new JValue(null, JTokenType.Undefined);
	}

	private static JTokenType GetValueType(JTokenType? current, object? value)
	{
		if (value == null)
		{
			return JTokenType.Null;
		}
		if (value == DBNull.Value)
		{
			return JTokenType.Null;
		}
		if (value is string)
		{
			return GetStringValueType(current);
		}
		if (value is long || value is int || value is short || value is sbyte || value is ulong || value is uint || value is ushort || value is byte)
		{
			return JTokenType.Integer;
		}
		if (value is Enum)
		{
			return JTokenType.Integer;
		}
		if (value is BigInteger)
		{
			return JTokenType.Integer;
		}
		if (value is double || value is float || value is decimal)
		{
			return JTokenType.Float;
		}
		if (value is DateTime)
		{
			return JTokenType.Date;
		}
		if (value is DateTimeOffset)
		{
			return JTokenType.Date;
		}
		if (value is byte[])
		{
			return JTokenType.Bytes;
		}
		if (value is bool)
		{
			return JTokenType.Boolean;
		}
		if (value is Guid)
		{
			return JTokenType.Guid;
		}
		if (value is Uri)
		{
			return JTokenType.Uri;
		}
		if (value is TimeSpan)
		{
			return JTokenType.TimeSpan;
		}
		throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value!.GetType()));
	}

	private static JTokenType GetStringValueType(JTokenType? current)
	{
		if (!current.HasValue)
		{
			return JTokenType.String;
		}
		JTokenType valueOrDefault = current.GetValueOrDefault();
		if (valueOrDefault == JTokenType.Comment || valueOrDefault == JTokenType.String || valueOrDefault == JTokenType.Raw)
		{
			return current.GetValueOrDefault();
		}
		return JTokenType.String;
	}

	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		if (converters != null && converters.Length != 0 && _value != null)
		{
			JsonConverter matchingConverter = JsonSerializer.GetMatchingConverter(converters, _value!.GetType());
			if (matchingConverter != null && matchingConverter.CanWrite)
			{
				matchingConverter.WriteJson(writer, _value, JsonSerializer.CreateDefault());
				return;
			}
		}
		switch (_valueType)
		{
		case JTokenType.Comment:
			writer.WriteComment(_value?.ToString());
			break;
		case JTokenType.Raw:
			writer.WriteRawValue(_value?.ToString());
			break;
		case JTokenType.Null:
			writer.WriteNull();
			break;
		case JTokenType.Undefined:
			writer.WriteUndefined();
			break;
		case JTokenType.Integer:
			if (_value is int value5)
			{
				writer.WriteValue(value5);
			}
			else if (_value is long value6)
			{
				writer.WriteValue(value6);
			}
			else if (_value is ulong value7)
			{
				writer.WriteValue(value7);
			}
			else if (_value is BigInteger bigInteger)
			{
				writer.WriteValue(bigInteger);
			}
			else
			{
				writer.WriteValue(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
			}
			break;
		case JTokenType.Float:
			if (_value is decimal value)
			{
				writer.WriteValue(value);
			}
			else if (_value is double value2)
			{
				writer.WriteValue(value2);
			}
			else if (_value is float value3)
			{
				writer.WriteValue(value3);
			}
			else
			{
				writer.WriteValue(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
			}
			break;
		case JTokenType.String:
			writer.WriteValue(_value?.ToString());
			break;
		case JTokenType.Boolean:
			writer.WriteValue(Convert.ToBoolean(_value, CultureInfo.InvariantCulture));
			break;
		case JTokenType.Date:
			if (_value is DateTimeOffset value4)
			{
				writer.WriteValue(value4);
			}
			else
			{
				writer.WriteValue(Convert.ToDateTime(_value, CultureInfo.InvariantCulture));
			}
			break;
		case JTokenType.Bytes:
			writer.WriteValue((byte[])_value);
			break;
		case JTokenType.Guid:
			writer.WriteValue((_value != null) ? ((Guid?)_value) : null);
			break;
		case JTokenType.TimeSpan:
			writer.WriteValue((_value != null) ? ((TimeSpan?)_value) : null);
			break;
		case JTokenType.Uri:
			writer.WriteValue((Uri)_value);
			break;
		default:
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", _valueType, "Unexpected token type.");
		}
	}

	internal override int GetDeepHashCode()
	{
		int num = ((_value != null) ? _value!.GetHashCode() : 0);
		int valueType = (int)_valueType;
		return valueType.GetHashCode() ^ num;
	}

	private static bool ValuesEquals(JValue v1, JValue v2)
	{
		if (v1 != v2)
		{
			if (v1._valueType == v2._valueType)
			{
				return Compare(v1._valueType, v1._value, v2._value) == 0;
			}
			return false;
		}
		return true;
	}

	public bool Equals(JValue? other)
	{
		if (other == null)
		{
			return false;
		}
		return ValuesEquals(this, other);
	}

	public override bool Equals(object obj)
	{
		if (obj is JValue other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_value == null)
		{
			return 0;
		}
		return _value!.GetHashCode();
	}

	public override string ToString()
	{
		if (_value == null)
		{
			return string.Empty;
		}
		return _value!.ToString();
	}

	public string ToString(string format)
	{
		return ToString(format, CultureInfo.CurrentCulture);
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return ToString(null, formatProvider);
	}

	public string ToString(string? format, IFormatProvider formatProvider)
	{
		if (_value == null)
		{
			return string.Empty;
		}
		if (_value is IFormattable formattable)
		{
			return formattable.ToString(format, formatProvider);
		}
		return _value!.ToString();
	}

	protected override DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new DynamicProxyMetaObject<JValue>(parameter, this, new JValueDynamicProxy());
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		object objB;
		JTokenType valueType;
		if (obj is JValue jValue)
		{
			objB = jValue.Value;
			valueType = ((_valueType == JTokenType.String && _valueType != jValue._valueType) ? jValue._valueType : _valueType);
		}
		else
		{
			objB = obj;
			valueType = _valueType;
		}
		return Compare(valueType, _value, objB);
	}

	public int CompareTo(JValue obj)
	{
		if (obj == null)
		{
			return 1;
		}
		return Compare((_valueType == JTokenType.String && _valueType != obj._valueType) ? obj._valueType : _valueType, _value, obj._value);
	}

	TypeCode IConvertible.GetTypeCode()
	{
		if (_value == null)
		{
			return TypeCode.Empty;
		}
		if (_value is IConvertible convertible)
		{
			return convertible.GetTypeCode();
		}
		return TypeCode.Object;
	}

	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return (bool)(JToken)this;
	}

	char IConvertible.ToChar(IFormatProvider provider)
	{
		return (char)(JToken)this;
	}

	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return (sbyte)(JToken)this;
	}

	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return (byte)(JToken)this;
	}

	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return (short)(JToken)this;
	}

	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return (ushort)(JToken)this;
	}

	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return (int)(JToken)this;
	}

	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return (uint)(JToken)this;
	}

	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return (long)(JToken)this;
	}

	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return (ulong)(JToken)this;
	}

	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return (float)(JToken)this;
	}

	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return (double)(JToken)this;
	}

	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return (decimal)(JToken)this;
	}

	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		return (DateTime)(JToken)this;
	}

	object? IConvertible.ToType(Type conversionType, IFormatProvider provider)
	{
		return ToObject(conversionType);
	}
}
