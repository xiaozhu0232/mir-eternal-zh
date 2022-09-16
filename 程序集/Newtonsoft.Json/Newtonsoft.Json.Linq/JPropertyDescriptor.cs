using System;
using System.ComponentModel;

namespace Newtonsoft.Json.Linq;

public class JPropertyDescriptor : PropertyDescriptor
{
	public override Type ComponentType => typeof(JObject);

	public override bool IsReadOnly => false;

	public override Type PropertyType => typeof(object);

	protected override int NameHashCode => base.NameHashCode;

	public JPropertyDescriptor(string name)
		: base(name, null)
	{
	}

	private static JObject CastInstance(object instance)
	{
		return (JObject)instance;
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override object? GetValue(object component)
	{
		return (component as JObject)?[Name];
	}

	public override void ResetValue(object component)
	{
	}

	public override void SetValue(object component, object value)
	{
		if (component is JObject jObject)
		{
			JToken value2 = (value as JToken) ?? new JValue(value);
			jObject[Name] = value2;
		}
	}

	public override bool ShouldSerializeValue(object component)
	{
		return false;
	}
}
