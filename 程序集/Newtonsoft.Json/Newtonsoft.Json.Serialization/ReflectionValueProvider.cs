using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class ReflectionValueProvider : IValueProvider
{
	private readonly MemberInfo _memberInfo;

	public ReflectionValueProvider(MemberInfo memberInfo)
	{
		ValidationUtils.ArgumentNotNull(memberInfo, "memberInfo");
		_memberInfo = memberInfo;
	}

	public void SetValue(object target, object? value)
	{
		try
		{
			ReflectionUtils.SetMemberValue(_memberInfo, target, value);
		}
		catch (Exception innerException)
		{
			throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), innerException);
		}
	}

	public object? GetValue(object target)
	{
		try
		{
			if (_memberInfo is PropertyInfo propertyInfo && propertyInfo.PropertyType.IsByRef)
			{
				throw new InvalidOperationException("Could not create getter for {0}. ByRef return values are not supported.".FormatWith(CultureInfo.InvariantCulture, propertyInfo));
			}
			return ReflectionUtils.GetMemberValue(_memberInfo, target);
		}
		catch (Exception innerException)
		{
			throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), innerException);
		}
	}
}
