namespace Newtonsoft.Json.Linq.JsonPath;

internal abstract class QueryExpression
{
	internal QueryOperator Operator;

	public QueryExpression(QueryOperator @operator)
	{
		Operator = @operator;
	}

	public bool IsMatch(JToken root, JToken t)
	{
		return IsMatch(root, t, null);
	}

	public abstract bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings);
}
