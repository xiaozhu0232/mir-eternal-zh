using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class QueryScanFilter : PathFilter
{
	internal QueryExpression Expression;

	public QueryScanFilter(QueryExpression expression)
	{
		Expression = expression;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
	{
		foreach (JToken item in current)
		{
			if (item is JContainer jContainer)
			{
				foreach (JToken item2 in jContainer.DescendantsAndSelf())
				{
					if (Expression.IsMatch(root, item2, settings))
					{
						yield return item2;
					}
				}
			}
			else if (Expression.IsMatch(root, item, settings))
			{
				yield return item;
			}
		}
	}
}
