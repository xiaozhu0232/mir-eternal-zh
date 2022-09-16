using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

public class JObject : JContainer, IDictionary<string, JToken?>, ICollection<KeyValuePair<string, JToken?>>, IEnumerable<KeyValuePair<string, JToken?>>, IEnumerable, INotifyPropertyChanged, ICustomTypeDescriptor, INotifyPropertyChanging
{
	private class JObjectDynamicProxy : DynamicProxy<JObject>
	{
		public override bool TryGetMember(JObject instance, GetMemberBinder binder, out object? result)
		{
			result = instance[binder.Name];
			return true;
		}

		public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
		{
			JToken jToken = value as JToken;
			if (jToken == null)
			{
				jToken = new JValue(value);
			}
			instance[binder.Name] = jToken;
			return true;
		}

		public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
		{
			return from p in instance.Properties()
				select p.Name;
		}
	}

	private readonly JPropertyKeyedCollection _properties = new JPropertyKeyedCollection();

	protected override IList<JToken> ChildrenTokens => _properties;

	public override JTokenType Type => JTokenType.Object;

	public override JToken? this[object key]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is string propertyName))
			{
				throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			return this[propertyName];
		}
		set
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is string propertyName))
			{
				throw new ArgumentException("Set JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			this[propertyName] = value;
		}
	}

	public JToken? this[string propertyName]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(propertyName, "propertyName");
			return Property(propertyName, StringComparison.Ordinal)?.Value;
		}
		set
		{
			JProperty jProperty = Property(propertyName, StringComparison.Ordinal);
			if (jProperty != null)
			{
				jProperty.Value = value;
				return;
			}
			OnPropertyChanging(propertyName);
			Add(propertyName, value);
			OnPropertyChanged(propertyName);
		}
	}

	ICollection<string> IDictionary<string, JToken>.Keys => _properties.Keys;

	ICollection<JToken?> IDictionary<string, JToken>.Values
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	bool ICollection<KeyValuePair<string, JToken>>.IsReadOnly => false;

	public event PropertyChangedEventHandler? PropertyChanged;

	public event PropertyChangingEventHandler? PropertyChanging;

	public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		Task task2 = writer.WriteStartObjectAsync(cancellationToken);
		if (!task2.IsCompletedSucessfully())
		{
			return AwaitProperties(task2, 0, writer, cancellationToken, converters);
		}
		for (int j = 0; j < _properties.Count; j++)
		{
			task2 = _properties[j].WriteToAsync(writer, cancellationToken, converters);
			if (!task2.IsCompletedSucessfully())
			{
				return AwaitProperties(task2, j + 1, writer, cancellationToken, converters);
			}
		}
		return writer.WriteEndObjectAsync(cancellationToken);
		async Task AwaitProperties(Task task, int i, JsonWriter Writer, CancellationToken CancellationToken, JsonConverter[] Converters)
		{
			await task.ConfigureAwait(continueOnCapturedContext: false);
			while (i < _properties.Count)
			{
				await _properties[i].WriteToAsync(Writer, CancellationToken, Converters).ConfigureAwait(continueOnCapturedContext: false);
				i++;
			}
			await Writer.WriteEndObjectAsync(CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public new static Task<JObject> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return LoadAsync(reader, null, cancellationToken);
	}

	public new static async Task<JObject> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.StartObject)
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JObject o = new JObject();
		o.SetLineInfo(reader as IJsonLineInfo, settings);
		await o.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return o;
	}

	public JObject()
	{
	}

	public JObject(JObject other)
		: base(other)
	{
	}

	public JObject(params object[] content)
		: this((object)content)
	{
	}

	public JObject(object content)
	{
		Add(content);
	}

	internal override bool DeepEquals(JToken node)
	{
		if (!(node is JObject jObject))
		{
			return false;
		}
		return _properties.Compare(jObject._properties);
	}

	internal override int IndexOfItem(JToken? item)
	{
		if (item == null)
		{
			return -1;
		}
		return _properties.IndexOfReference(item);
	}

	internal override bool InsertItem(int index, JToken? item, bool skipParentCheck)
	{
		if (item != null && item!.Type == JTokenType.Comment)
		{
			return false;
		}
		return base.InsertItem(index, item, skipParentCheck);
	}

	internal override void ValidateToken(JToken o, JToken? existing)
	{
		ValidationUtils.ArgumentNotNull(o, "o");
		if (o.Type != JTokenType.Property)
		{
			throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
		}
		JProperty jProperty = (JProperty)o;
		if (existing != null)
		{
			JProperty jProperty2 = (JProperty)existing;
			if (jProperty.Name == jProperty2.Name)
			{
				return;
			}
		}
		if (_properties.TryGetValue(jProperty.Name, out existing))
		{
			throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, jProperty.Name, GetType()));
		}
	}

	internal override void MergeItem(object content, JsonMergeSettings? settings)
	{
		if (!(content is JObject jObject))
		{
			return;
		}
		foreach (KeyValuePair<string, JToken> item in jObject)
		{
			JProperty jProperty = Property(item.Key, settings?.PropertyNameComparison ?? StringComparison.Ordinal);
			if (jProperty == null)
			{
				Add(item.Key, item.Value);
			}
			else
			{
				if (item.Value == null)
				{
					continue;
				}
				if (!(jProperty.Value is JContainer jContainer) || jContainer.Type != item.Value.Type)
				{
					if (!IsNull(item.Value) || (settings != null && settings!.MergeNullValueHandling == MergeNullValueHandling.Merge))
					{
						jProperty.Value = item.Value;
					}
				}
				else
				{
					jContainer.Merge(item.Value, settings);
				}
			}
		}
	}

	private static bool IsNull(JToken token)
	{
		if (token.Type == JTokenType.Null)
		{
			return true;
		}
		if (token is JValue jValue && jValue.Value == null)
		{
			return true;
		}
		return false;
	}

	internal void InternalPropertyChanged(JProperty childProperty)
	{
		OnPropertyChanged(childProperty.Name);
		if (_listChanged != null)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, IndexOfItem(childProperty)));
		}
		if (_collectionChanged != null)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, childProperty, childProperty, IndexOfItem(childProperty)));
		}
	}

	internal void InternalPropertyChanging(JProperty childProperty)
	{
		OnPropertyChanging(childProperty.Name);
	}

	internal override JToken CloneToken()
	{
		return new JObject(this);
	}

	public IEnumerable<JProperty> Properties()
	{
		return _properties.Cast<JProperty>();
	}

	public JProperty? Property(string name)
	{
		return Property(name, StringComparison.Ordinal);
	}

	public JProperty? Property(string name, StringComparison comparison)
	{
		if (name == null)
		{
			return null;
		}
		if (_properties.TryGetValue(name, out var value))
		{
			return (JProperty)value;
		}
		if (comparison != StringComparison.Ordinal)
		{
			for (int i = 0; i < _properties.Count; i++)
			{
				JProperty jProperty = (JProperty)_properties[i];
				if (string.Equals(jProperty.Name, name, comparison))
				{
					return jProperty;
				}
			}
		}
		return null;
	}

	public JEnumerable<JToken> PropertyValues()
	{
		return new JEnumerable<JToken>(from p in Properties()
			select p.Value);
	}

	public new static JObject Load(JsonReader reader)
	{
		return Load(reader, null);
	}

	public new static JObject Load(JsonReader reader, JsonLoadSettings? settings)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.StartObject)
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JObject jObject = new JObject();
		jObject.SetLineInfo(reader as IJsonLineInfo, settings);
		jObject.ReadTokenFrom(reader, settings);
		return jObject;
	}

	public new static JObject Parse(string json)
	{
		return Parse(json, null);
	}

	public new static JObject Parse(string json, JsonLoadSettings? settings)
	{
		using JsonReader jsonReader = new JsonTextReader(new StringReader(json));
		JObject result = Load(jsonReader, settings);
		while (jsonReader.Read())
		{
		}
		return result;
	}

	public new static JObject FromObject(object o)
	{
		return FromObject(o, JsonSerializer.CreateDefault());
	}

	public new static JObject FromObject(object o, JsonSerializer jsonSerializer)
	{
		JToken jToken = JToken.FromObjectInternal(o, jsonSerializer);
		if (jToken.Type != JTokenType.Object)
		{
			throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(CultureInfo.InvariantCulture, jToken.Type));
		}
		return (JObject)jToken;
	}

	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WriteStartObject();
		for (int i = 0; i < _properties.Count; i++)
		{
			_properties[i].WriteTo(writer, converters);
		}
		writer.WriteEndObject();
	}

	public JToken? GetValue(string? propertyName)
	{
		return GetValue(propertyName, StringComparison.Ordinal);
	}

	public JToken? GetValue(string? propertyName, StringComparison comparison)
	{
		if (propertyName == null)
		{
			return null;
		}
		return Property(propertyName, comparison)?.Value;
	}

	public bool TryGetValue(string propertyName, StringComparison comparison, [NotNullWhen(true)] out JToken? value)
	{
		value = GetValue(propertyName, comparison);
		return value != null;
	}

	public void Add(string propertyName, JToken? value)
	{
		Add(new JProperty(propertyName, value));
	}

	public bool ContainsKey(string propertyName)
	{
		ValidationUtils.ArgumentNotNull(propertyName, "propertyName");
		return _properties.Contains(propertyName);
	}

	public bool Remove(string propertyName)
	{
		JProperty jProperty = Property(propertyName, StringComparison.Ordinal);
		if (jProperty == null)
		{
			return false;
		}
		jProperty.Remove();
		return true;
	}

	public bool TryGetValue(string propertyName, [NotNullWhen(true)] out JToken? value)
	{
		JProperty jProperty = Property(propertyName, StringComparison.Ordinal);
		if (jProperty == null)
		{
			value = null;
			return false;
		}
		value = jProperty.Value;
		return true;
	}

	void ICollection<KeyValuePair<string, JToken>>.Add(KeyValuePair<string, JToken?> item)
	{
		Add(new JProperty(item.Key, item.Value));
	}

	void ICollection<KeyValuePair<string, JToken>>.Clear()
	{
		RemoveAll();
	}

	bool ICollection<KeyValuePair<string, JToken>>.Contains(KeyValuePair<string, JToken?> item)
	{
		JProperty jProperty = Property(item.Key, StringComparison.Ordinal);
		if (jProperty == null)
		{
			return false;
		}
		return jProperty.Value == item.Value;
	}

	void ICollection<KeyValuePair<string, JToken>>.CopyTo(KeyValuePair<string, JToken?>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
		}
		if (arrayIndex >= array.Length && arrayIndex != 0)
		{
			throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
		}
		if (base.Count > array.Length - arrayIndex)
		{
			throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
		}
		int num = 0;
		foreach (JProperty property in _properties)
		{
			array[arrayIndex + num] = new KeyValuePair<string, JToken>(property.Name, property.Value);
			num++;
		}
	}

	bool ICollection<KeyValuePair<string, JToken>>.Remove(KeyValuePair<string, JToken?> item)
	{
		if (!((ICollection<KeyValuePair<string, JToken>>)this).Contains(item))
		{
			return false;
		}
		((IDictionary<string, JToken>)this).Remove(item.Key);
		return true;
	}

	internal override int GetDeepHashCode()
	{
		return ContentsHashCode();
	}

	public IEnumerator<KeyValuePair<string, JToken?>> GetEnumerator()
	{
		foreach (JProperty property in _properties)
		{
			yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
		}
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanging(string propertyName)
	{
		this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return ((ICustomTypeDescriptor)this).GetProperties((Attribute[])null);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		PropertyDescriptor[] array = new PropertyDescriptor[base.Count];
		int num = 0;
		using (IEnumerator<KeyValuePair<string, JToken>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				array[num] = new JPropertyDescriptor(enumerator.Current.Key);
				num++;
			}
		}
		return new PropertyDescriptorCollection(array);
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return AttributeCollection.Empty;
	}

	string? ICustomTypeDescriptor.GetClassName()
	{
		return null;
	}

	string? ICustomTypeDescriptor.GetComponentName()
	{
		return null;
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return new TypeConverter();
	}

	EventDescriptor? ICustomTypeDescriptor.GetDefaultEvent()
	{
		return null;
	}

	PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty()
	{
		return null;
	}

	object? ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return null;
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return EventDescriptorCollection.Empty;
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return EventDescriptorCollection.Empty;
	}

	object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		if (pd is JPropertyDescriptor)
		{
			return this;
		}
		return null;
	}

	protected override DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new DynamicProxyMetaObject<JObject>(parameter, this, new JObjectDynamicProxy());
	}
}
