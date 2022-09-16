using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace ICSharpCode.SharpZipLib.Core;

public class NameFilter
{
	private string filter;

	private ArrayList inclusions;

	private ArrayList exclusions;

	public NameFilter(string filter)
	{
		this.filter = filter;
		inclusions = new ArrayList();
		exclusions = new ArrayList();
		Compile();
	}

	public static bool IsValidExpression(string e)
	{
		bool result = true;
		try
		{
			Regex regex = new Regex(e, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public static bool IsValidFilterExpression(string toTest)
	{
		bool result = true;
		try
		{
			string[] array = toTest.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].Length > 0)
				{
					string pattern = ((array[i][0] == '+') ? array[i].Substring(1, array[i].Length - 1) : ((array[i][0] != '-') ? array[i] : array[i].Substring(1, array[i].Length - 1)));
					Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
				}
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	public override string ToString()
	{
		return filter;
	}

	public bool IsIncluded(string testValue)
	{
		bool result = false;
		if (inclusions.Count == 0)
		{
			result = true;
		}
		else
		{
			foreach (Regex inclusion in inclusions)
			{
				if (inclusion.IsMatch(testValue))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public bool IsExcluded(string testValue)
	{
		bool result = false;
		foreach (Regex exclusion in exclusions)
		{
			if (exclusion.IsMatch(testValue))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool IsMatch(string testValue)
	{
		return IsIncluded(testValue) && !IsExcluded(testValue);
	}

	private void Compile()
	{
		if (filter == null)
		{
			return;
		}
		string[] array = filter.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].Length > 0)
			{
				bool flag = array[i][0] != '-';
				string pattern = ((array[i][0] == '+') ? array[i].Substring(1, array[i].Length - 1) : ((array[i][0] != '-') ? array[i] : array[i].Substring(1, array[i].Length - 1)));
				if (flag)
				{
					inclusions.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
				}
				else
				{
					exclusions.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
				}
			}
		}
	}
}
