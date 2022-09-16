using System.Dynamic;

namespace Newtonsoft.Json.Utilities;

internal class NoThrowGetBinderMember : GetMemberBinder
{
	private readonly GetMemberBinder _innerBinder;

	public NoThrowGetBinderMember(GetMemberBinder innerBinder)
		: base(innerBinder.Name, innerBinder.IgnoreCase)
	{
		_innerBinder = innerBinder;
	}

	public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
	{
		DynamicMetaObject dynamicMetaObject = _innerBinder.Bind(target, CollectionUtils.ArrayEmpty<DynamicMetaObject>());
		return new DynamicMetaObject(new NoThrowExpressionVisitor().Visit(dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
	}
}
