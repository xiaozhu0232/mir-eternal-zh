using System;

namespace Newtonsoft.Json.Linq;

public class JsonMergeSettings
{
	private MergeArrayHandling _mergeArrayHandling;

	private MergeNullValueHandling _mergeNullValueHandling;

	private StringComparison _propertyNameComparison;

	public MergeArrayHandling MergeArrayHandling
	{
		get
		{
			return _mergeArrayHandling;
		}
		set
		{
			if (value < MergeArrayHandling.Concat || value > MergeArrayHandling.Merge)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_mergeArrayHandling = value;
		}
	}

	public MergeNullValueHandling MergeNullValueHandling
	{
		get
		{
			return _mergeNullValueHandling;
		}
		set
		{
			if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_mergeNullValueHandling = value;
		}
	}

	public StringComparison PropertyNameComparison
	{
		get
		{
			return _propertyNameComparison;
		}
		set
		{
			if (value < StringComparison.CurrentCulture || value > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_propertyNameComparison = value;
		}
	}

	public JsonMergeSettings()
	{
		_propertyNameComparison = StringComparison.Ordinal;
	}
}
