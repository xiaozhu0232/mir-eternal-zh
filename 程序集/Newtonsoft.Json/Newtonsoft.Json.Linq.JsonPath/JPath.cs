using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class JPath
{
	private static readonly char[] FloatCharacters = new char[3] { '.', 'E', 'e' };

	private readonly string _expression;

	private int _currentIndex;

	public List<PathFilter> Filters { get; }

	public JPath(string expression)
	{
		ValidationUtils.ArgumentNotNull(expression, "expression");
		_expression = expression;
		Filters = new List<PathFilter>();
		ParseMain();
	}

	private void ParseMain()
	{
		int currentIndex = _currentIndex;
		EatWhitespace();
		if (_expression.Length == _currentIndex)
		{
			return;
		}
		if (_expression[_currentIndex] == '$')
		{
			if (_expression.Length == 1)
			{
				return;
			}
			char c = _expression[_currentIndex + 1];
			if (c == '.' || c == '[')
			{
				_currentIndex++;
				currentIndex = _currentIndex;
			}
		}
		if (!ParsePath(Filters, currentIndex, query: false))
		{
			int currentIndex2 = _currentIndex;
			EatWhitespace();
			if (_currentIndex < _expression.Length)
			{
				throw new JsonException("Unexpected character while parsing path: " + _expression[currentIndex2]);
			}
		}
	}

	private bool ParsePath(List<PathFilter> filters, int currentPartStartIndex, bool query)
	{
		bool scan = false;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (_currentIndex < _expression.Length && !flag3)
		{
			char c = _expression[_currentIndex];
			switch (c)
			{
			case '(':
			case '[':
				if (_currentIndex > currentPartStartIndex)
				{
					string text = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
					if (text == "*")
					{
						text = null;
					}
					filters.Add(CreatePathFilter(text, scan));
					scan = false;
				}
				filters.Add(ParseIndexer(c, scan));
				scan = false;
				_currentIndex++;
				currentPartStartIndex = _currentIndex;
				flag = true;
				flag2 = false;
				break;
			case ')':
			case ']':
				flag3 = true;
				break;
			case ' ':
				if (_currentIndex < _expression.Length)
				{
					flag3 = true;
				}
				break;
			case '.':
				if (_currentIndex > currentPartStartIndex)
				{
					string text2 = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
					if (text2 == "*")
					{
						text2 = null;
					}
					filters.Add(CreatePathFilter(text2, scan));
					scan = false;
				}
				if (_currentIndex + 1 < _expression.Length && _expression[_currentIndex + 1] == '.')
				{
					scan = true;
					_currentIndex++;
				}
				_currentIndex++;
				currentPartStartIndex = _currentIndex;
				flag = false;
				flag2 = true;
				break;
			default:
				if (query && (c == '=' || c == '<' || c == '!' || c == '>' || c == '|' || c == '&'))
				{
					flag3 = true;
					break;
				}
				if (flag)
				{
					throw new JsonException("Unexpected character following indexer: " + c);
				}
				_currentIndex++;
				break;
			}
		}
		bool flag4 = _currentIndex == _expression.Length;
		if (_currentIndex > currentPartStartIndex)
		{
			string text3 = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex).TrimEnd();
			if (text3 == "*")
			{
				text3 = null;
			}
			filters.Add(CreatePathFilter(text3, scan));
		}
		else if (flag2 && (flag4 || query))
		{
			throw new JsonException("Unexpected end while parsing path.");
		}
		return flag4;
	}

	private static PathFilter CreatePathFilter(string? member, bool scan)
	{
		if (!scan)
		{
			return new FieldFilter(member);
		}
		return new ScanFilter(member);
	}

	private PathFilter ParseIndexer(char indexerOpenChar, bool scan)
	{
		_currentIndex++;
		char indexerCloseChar = ((indexerOpenChar == '[') ? ']' : ')');
		EnsureLength("Path ended with open indexer.");
		EatWhitespace();
		if (_expression[_currentIndex] == '\'')
		{
			return ParseQuotedField(indexerCloseChar, scan);
		}
		if (_expression[_currentIndex] == '?')
		{
			return ParseQuery(indexerCloseChar, scan);
		}
		return ParseArrayIndexer(indexerCloseChar);
	}

	private PathFilter ParseArrayIndexer(char indexerCloseChar)
	{
		int currentIndex = _currentIndex;
		int? num = null;
		List<int> list = null;
		int num2 = 0;
		int? start = null;
		int? end = null;
		int? step = null;
		while (_currentIndex < _expression.Length)
		{
			char c = _expression[_currentIndex];
			if (c == ' ')
			{
				num = _currentIndex;
				EatWhitespace();
				continue;
			}
			if (c == indexerCloseChar)
			{
				int num3 = (num ?? _currentIndex) - currentIndex;
				if (list != null)
				{
					if (num3 == 0)
					{
						throw new JsonException("Array index expected.");
					}
					int item = Convert.ToInt32(_expression.Substring(currentIndex, num3), CultureInfo.InvariantCulture);
					list.Add(item);
					return new ArrayMultipleIndexFilter(list);
				}
				if (num2 > 0)
				{
					if (num3 > 0)
					{
						int value = Convert.ToInt32(_expression.Substring(currentIndex, num3), CultureInfo.InvariantCulture);
						if (num2 == 1)
						{
							end = value;
						}
						else
						{
							step = value;
						}
					}
					return new ArraySliceFilter
					{
						Start = start,
						End = end,
						Step = step
					};
				}
				if (num3 == 0)
				{
					throw new JsonException("Array index expected.");
				}
				int value2 = Convert.ToInt32(_expression.Substring(currentIndex, num3), CultureInfo.InvariantCulture);
				return new ArrayIndexFilter
				{
					Index = value2
				};
			}
			switch (c)
			{
			case ',':
			{
				int num5 = (num ?? _currentIndex) - currentIndex;
				if (num5 == 0)
				{
					throw new JsonException("Array index expected.");
				}
				if (list == null)
				{
					list = new List<int>();
				}
				string value4 = _expression.Substring(currentIndex, num5);
				list.Add(Convert.ToInt32(value4, CultureInfo.InvariantCulture));
				_currentIndex++;
				EatWhitespace();
				currentIndex = _currentIndex;
				num = null;
				break;
			}
			case '*':
				_currentIndex++;
				EnsureLength("Path ended with open indexer.");
				EatWhitespace();
				if (_expression[_currentIndex] != indexerCloseChar)
				{
					throw new JsonException("Unexpected character while parsing path indexer: " + c);
				}
				return new ArrayIndexFilter();
			case ':':
			{
				int num4 = (num ?? _currentIndex) - currentIndex;
				if (num4 > 0)
				{
					int value3 = Convert.ToInt32(_expression.Substring(currentIndex, num4), CultureInfo.InvariantCulture);
					switch (num2)
					{
					case 0:
						start = value3;
						break;
					case 1:
						end = value3;
						break;
					default:
						step = value3;
						break;
					}
				}
				num2++;
				_currentIndex++;
				EatWhitespace();
				currentIndex = _currentIndex;
				num = null;
				break;
			}
			default:
				if (!char.IsDigit(c) && c != '-')
				{
					throw new JsonException("Unexpected character while parsing path indexer: " + c);
				}
				if (num.HasValue)
				{
					throw new JsonException("Unexpected character while parsing path indexer: " + c);
				}
				_currentIndex++;
				break;
			}
		}
		throw new JsonException("Path ended with open indexer.");
	}

	private void EatWhitespace()
	{
		while (_currentIndex < _expression.Length && _expression[_currentIndex] == ' ')
		{
			_currentIndex++;
		}
	}

	private PathFilter ParseQuery(char indexerCloseChar, bool scan)
	{
		_currentIndex++;
		EnsureLength("Path ended with open indexer.");
		if (_expression[_currentIndex] != '(')
		{
			throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
		}
		_currentIndex++;
		QueryExpression expression = ParseExpression();
		_currentIndex++;
		EnsureLength("Path ended with open indexer.");
		EatWhitespace();
		if (_expression[_currentIndex] != indexerCloseChar)
		{
			throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
		}
		if (!scan)
		{
			return new QueryFilter(expression);
		}
		return new QueryScanFilter(expression);
	}

	private bool TryParseExpression(out List<PathFilter>? expressionPath)
	{
		if (_expression[_currentIndex] == '$')
		{
			expressionPath = new List<PathFilter> { RootFilter.Instance };
		}
		else
		{
			if (_expression[_currentIndex] != '@')
			{
				expressionPath = null;
				return false;
			}
			expressionPath = new List<PathFilter>();
		}
		_currentIndex++;
		if (ParsePath(expressionPath, _currentIndex, query: true))
		{
			throw new JsonException("Path ended with open query.");
		}
		return true;
	}

	private JsonException CreateUnexpectedCharacterException()
	{
		return new JsonException("Unexpected character while parsing path query: " + _expression[_currentIndex]);
	}

	private object ParseSide()
	{
		EatWhitespace();
		if (TryParseExpression(out var expressionPath))
		{
			EatWhitespace();
			EnsureLength("Path ended with open query.");
			return expressionPath;
		}
		if (TryParseValue(out var value))
		{
			EatWhitespace();
			EnsureLength("Path ended with open query.");
			return new JValue(value);
		}
		throw CreateUnexpectedCharacterException();
	}

	private QueryExpression ParseExpression()
	{
		QueryExpression queryExpression = null;
		CompositeExpression compositeExpression = null;
		while (_currentIndex < _expression.Length)
		{
			object left = ParseSide();
			object right = null;
			QueryOperator @operator;
			if (_expression[_currentIndex] == ')' || _expression[_currentIndex] == '|' || _expression[_currentIndex] == '&')
			{
				@operator = QueryOperator.Exists;
			}
			else
			{
				@operator = ParseOperator();
				right = ParseSide();
			}
			BooleanQueryExpression booleanQueryExpression = new BooleanQueryExpression(@operator, left, right);
			if (_expression[_currentIndex] == ')')
			{
				if (compositeExpression != null)
				{
					compositeExpression.Expressions.Add(booleanQueryExpression);
					return queryExpression;
				}
				return booleanQueryExpression;
			}
			if (_expression[_currentIndex] == '&')
			{
				if (!Match("&&"))
				{
					throw CreateUnexpectedCharacterException();
				}
				if (compositeExpression == null || compositeExpression.Operator != QueryOperator.And)
				{
					CompositeExpression compositeExpression2 = new CompositeExpression(QueryOperator.And);
					compositeExpression?.Expressions.Add(compositeExpression2);
					compositeExpression = compositeExpression2;
					if (queryExpression == null)
					{
						queryExpression = compositeExpression;
					}
				}
				compositeExpression.Expressions.Add(booleanQueryExpression);
			}
			if (_expression[_currentIndex] != '|')
			{
				continue;
			}
			if (!Match("||"))
			{
				throw CreateUnexpectedCharacterException();
			}
			if (compositeExpression == null || compositeExpression.Operator != QueryOperator.Or)
			{
				CompositeExpression compositeExpression3 = new CompositeExpression(QueryOperator.Or);
				compositeExpression?.Expressions.Add(compositeExpression3);
				compositeExpression = compositeExpression3;
				if (queryExpression == null)
				{
					queryExpression = compositeExpression;
				}
			}
			compositeExpression.Expressions.Add(booleanQueryExpression);
		}
		throw new JsonException("Path ended with open query.");
	}

	private bool TryParseValue(out object? value)
	{
		char c = _expression[_currentIndex];
		if (c == '\'')
		{
			value = ReadQuotedString();
			return true;
		}
		if (char.IsDigit(c) || c == '-')
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(c);
			_currentIndex++;
			while (_currentIndex < _expression.Length)
			{
				c = _expression[_currentIndex];
				if (c == ' ' || c == ')')
				{
					string text = stringBuilder.ToString();
					if (text.IndexOfAny(FloatCharacters) != -1)
					{
						double result;
						bool result2 = double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
						value = result;
						return result2;
					}
					long result3;
					bool result4 = long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result3);
					value = result3;
					return result4;
				}
				stringBuilder.Append(c);
				_currentIndex++;
			}
		}
		else
		{
			switch (c)
			{
			case 't':
				if (Match("true"))
				{
					value = true;
					return true;
				}
				break;
			case 'f':
				if (Match("false"))
				{
					value = false;
					return true;
				}
				break;
			case 'n':
				if (Match("null"))
				{
					value = null;
					return true;
				}
				break;
			case '/':
				value = ReadRegexString();
				return true;
			}
		}
		value = null;
		return false;
	}

	private string ReadQuotedString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		_currentIndex++;
		while (_currentIndex < _expression.Length)
		{
			char c = _expression[_currentIndex];
			if (c == '\\' && _currentIndex + 1 < _expression.Length)
			{
				_currentIndex++;
				c = _expression[_currentIndex];
				char value;
				switch (c)
				{
				case 'b':
					value = '\b';
					break;
				case 't':
					value = '\t';
					break;
				case 'n':
					value = '\n';
					break;
				case 'f':
					value = '\f';
					break;
				case 'r':
					value = '\r';
					break;
				case '"':
				case '\'':
				case '/':
				case '\\':
					value = c;
					break;
				default:
					throw new JsonException("Unknown escape character: \\" + c);
				}
				stringBuilder.Append(value);
				_currentIndex++;
			}
			else
			{
				if (c == '\'')
				{
					_currentIndex++;
					return stringBuilder.ToString();
				}
				_currentIndex++;
				stringBuilder.Append(c);
			}
		}
		throw new JsonException("Path ended with an open string.");
	}

	private string ReadRegexString()
	{
		int currentIndex = _currentIndex;
		_currentIndex++;
		while (_currentIndex < _expression.Length)
		{
			char c = _expression[_currentIndex];
			if (c == '\\' && _currentIndex + 1 < _expression.Length)
			{
				_currentIndex += 2;
				continue;
			}
			if (c == '/')
			{
				_currentIndex++;
				while (_currentIndex < _expression.Length)
				{
					c = _expression[_currentIndex];
					if (!char.IsLetter(c))
					{
						break;
					}
					_currentIndex++;
				}
				return _expression.Substring(currentIndex, _currentIndex - currentIndex);
			}
			_currentIndex++;
		}
		throw new JsonException("Path ended with an open regex.");
	}

	private bool Match(string s)
	{
		int num = _currentIndex;
		for (int i = 0; i < s.Length; i++)
		{
			if (num < _expression.Length && _expression[num] == s[i])
			{
				num++;
				continue;
			}
			return false;
		}
		_currentIndex = num;
		return true;
	}

	private QueryOperator ParseOperator()
	{
		if (_currentIndex + 1 >= _expression.Length)
		{
			throw new JsonException("Path ended with open query.");
		}
		if (Match("==="))
		{
			return QueryOperator.StrictEquals;
		}
		if (Match("=="))
		{
			return QueryOperator.Equals;
		}
		if (Match("=~"))
		{
			return QueryOperator.RegexEquals;
		}
		if (Match("!=="))
		{
			return QueryOperator.StrictNotEquals;
		}
		if (Match("!=") || Match("<>"))
		{
			return QueryOperator.NotEquals;
		}
		if (Match("<="))
		{
			return QueryOperator.LessThanOrEquals;
		}
		if (Match("<"))
		{
			return QueryOperator.LessThan;
		}
		if (Match(">="))
		{
			return QueryOperator.GreaterThanOrEquals;
		}
		if (Match(">"))
		{
			return QueryOperator.GreaterThan;
		}
		throw new JsonException("Could not read query operator.");
	}

	private PathFilter ParseQuotedField(char indexerCloseChar, bool scan)
	{
		List<string> list = null;
		while (_currentIndex < _expression.Length)
		{
			string text = ReadQuotedString();
			EatWhitespace();
			EnsureLength("Path ended with open indexer.");
			if (_expression[_currentIndex] == indexerCloseChar)
			{
				if (list != null)
				{
					list.Add(text);
					if (!scan)
					{
						return new FieldMultipleFilter(list);
					}
					return new ScanMultipleFilter(list);
				}
				return CreatePathFilter(text, scan);
			}
			if (_expression[_currentIndex] == ',')
			{
				_currentIndex++;
				EatWhitespace();
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(text);
				continue;
			}
			throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
		}
		throw new JsonException("Path ended with open indexer.");
	}

	private void EnsureLength(string message)
	{
		if (_currentIndex >= _expression.Length)
		{
			throw new JsonException(message);
		}
	}

	internal IEnumerable<JToken> Evaluate(JToken root, JToken t, JsonSelectSettings? settings)
	{
		return Evaluate(Filters, root, t, settings);
	}

	internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken root, JToken t, JsonSelectSettings? settings)
	{
		IEnumerable<JToken> enumerable = new JToken[1] { t };
		foreach (PathFilter filter in filters)
		{
			enumerable = filter.ExecuteFilter(root, enumerable, settings);
		}
		return enumerable;
	}
}
