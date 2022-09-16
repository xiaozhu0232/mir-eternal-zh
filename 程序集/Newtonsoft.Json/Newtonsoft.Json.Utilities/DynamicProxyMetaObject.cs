using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Newtonsoft.Json.Utilities;

internal sealed class DynamicProxyMetaObject<T> : DynamicMetaObject
{
	private delegate DynamicMetaObject Fallback(DynamicMetaObject? errorSuggestion);

	private sealed class GetBinderAdapter : GetMemberBinder
	{
		internal GetBinderAdapter(InvokeMemberBinder binder)
			: base(binder.Name, binder.IgnoreCase)
		{
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			throw new NotSupportedException();
		}
	}

	private readonly DynamicProxy<T> _proxy;

	private static Expression[] NoArgs => CollectionUtils.ArrayEmpty<Expression>();

	internal DynamicProxyMetaObject(Expression expression, T value, DynamicProxy<T> proxy)
		: base(expression, BindingRestrictions.Empty, value)
	{
		_proxy = proxy;
	}

	private bool IsOverridden(string method)
	{
		return ReflectionUtils.IsMethodOverridden(_proxy.GetType(), typeof(DynamicProxy<T>), method);
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
	{
		GetMemberBinder binder2 = binder;
		if (!IsOverridden("TryGetMember"))
		{
			return base.BindGetMember(binder2);
		}
		return CallMethodWithResult("TryGetMember", binder2, NoArgs, (DynamicMetaObject? e) => binder2.FallbackGetMember(this, e));
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
	{
		SetMemberBinder binder2 = binder;
		DynamicMetaObject value2 = value;
		if (!IsOverridden("TrySetMember"))
		{
			return base.BindSetMember(binder2, value2);
		}
		return CallMethodReturnLast("TrySetMember", binder2, GetArgs(value2), (DynamicMetaObject? e) => binder2.FallbackSetMember(this, value2, e));
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
	{
		DeleteMemberBinder binder2 = binder;
		if (!IsOverridden("TryDeleteMember"))
		{
			return base.BindDeleteMember(binder2);
		}
		return CallMethodNoResult("TryDeleteMember", binder2, NoArgs, (DynamicMetaObject? e) => binder2.FallbackDeleteMember(this, e));
	}

	public override DynamicMetaObject BindConvert(ConvertBinder binder)
	{
		ConvertBinder binder2 = binder;
		if (!IsOverridden("TryConvert"))
		{
			return base.BindConvert(binder2);
		}
		return CallMethodWithResult("TryConvert", binder2, NoArgs, (DynamicMetaObject? e) => binder2.FallbackConvert(this, e));
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		InvokeMemberBinder binder2 = binder;
		DynamicMetaObject[] args2 = args;
		if (!IsOverridden("TryInvokeMember"))
		{
			return base.BindInvokeMember(binder2, args2);
		}
		Fallback fallback = (DynamicMetaObject? e) => binder2.FallbackInvokeMember(this, args2, e);
		return BuildCallMethodWithResult("TryInvokeMember", binder2, GetArgArray(args2), BuildCallMethodWithResult("TryGetMember", new GetBinderAdapter(binder2), NoArgs, fallback(null), (DynamicMetaObject? e) => binder2.FallbackInvoke(e, args2, null)), null);
	}

	public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
	{
		CreateInstanceBinder binder2 = binder;
		DynamicMetaObject[] args2 = args;
		if (!IsOverridden("TryCreateInstance"))
		{
			return base.BindCreateInstance(binder2, args2);
		}
		return CallMethodWithResult("TryCreateInstance", binder2, GetArgArray(args2), (DynamicMetaObject? e) => binder2.FallbackCreateInstance(this, args2, e));
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
	{
		InvokeBinder binder2 = binder;
		DynamicMetaObject[] args2 = args;
		if (!IsOverridden("TryInvoke"))
		{
			return base.BindInvoke(binder2, args2);
		}
		return CallMethodWithResult("TryInvoke", binder2, GetArgArray(args2), (DynamicMetaObject? e) => binder2.FallbackInvoke(this, args2, e));
	}

	public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		BinaryOperationBinder binder2 = binder;
		DynamicMetaObject arg2 = arg;
		if (!IsOverridden("TryBinaryOperation"))
		{
			return base.BindBinaryOperation(binder2, arg2);
		}
		return CallMethodWithResult("TryBinaryOperation", binder2, GetArgs(arg2), (DynamicMetaObject? e) => binder2.FallbackBinaryOperation(this, arg2, e));
	}

	public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
	{
		UnaryOperationBinder binder2 = binder;
		if (!IsOverridden("TryUnaryOperation"))
		{
			return base.BindUnaryOperation(binder2);
		}
		return CallMethodWithResult("TryUnaryOperation", binder2, NoArgs, (DynamicMetaObject? e) => binder2.FallbackUnaryOperation(this, e));
	}

	public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		GetIndexBinder binder2 = binder;
		DynamicMetaObject[] indexes2 = indexes;
		if (!IsOverridden("TryGetIndex"))
		{
			return base.BindGetIndex(binder2, indexes2);
		}
		return CallMethodWithResult("TryGetIndex", binder2, GetArgArray(indexes2), (DynamicMetaObject? e) => binder2.FallbackGetIndex(this, indexes2, e));
	}

	public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		SetIndexBinder binder2 = binder;
		DynamicMetaObject[] indexes2 = indexes;
		DynamicMetaObject value2 = value;
		if (!IsOverridden("TrySetIndex"))
		{
			return base.BindSetIndex(binder2, indexes2, value2);
		}
		return CallMethodReturnLast("TrySetIndex", binder2, GetArgArray(indexes2, value2), (DynamicMetaObject? e) => binder2.FallbackSetIndex(this, indexes2, value2, e));
	}

	public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
	{
		DeleteIndexBinder binder2 = binder;
		DynamicMetaObject[] indexes2 = indexes;
		if (!IsOverridden("TryDeleteIndex"))
		{
			return base.BindDeleteIndex(binder2, indexes2);
		}
		return CallMethodNoResult("TryDeleteIndex", binder2, GetArgArray(indexes2), (DynamicMetaObject? e) => binder2.FallbackDeleteIndex(this, indexes2, e));
	}

	private static IEnumerable<Expression> GetArgs(params DynamicMetaObject[] args)
	{
		return args.Select(delegate(DynamicMetaObject arg)
		{
			Expression expression = arg.Expression;
			return (!expression.Type.IsValueType()) ? expression : Expression.Convert(expression, typeof(object));
		});
	}

	private static Expression[] GetArgArray(DynamicMetaObject[] args)
	{
		return new NewArrayExpression[1] { Expression.NewArrayInit(typeof(object), GetArgs(args)) };
	}

	private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
	{
		Expression expression = value.Expression;
		return new Expression[2]
		{
			Expression.NewArrayInit(typeof(object), GetArgs(args)),
			expression.Type.IsValueType() ? Expression.Convert(expression, typeof(object)) : expression
		};
	}

	private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
	{
		Type type = binder.GetType();
		while (!type.IsVisible())
		{
			type = type.BaseType();
		}
		return Expression.Constant(binder, type);
	}

	private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback, Fallback? fallbackInvoke = null)
	{
		DynamicMetaObject fallbackResult = fallback(null);
		return BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
	}

	private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, DynamicMetaObject fallbackResult, Fallback? fallbackInvoke)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), null);
		IList<Expression> list = new List<Expression>();
		list.Add(Expression.Convert(base.Expression, typeof(T)));
		list.Add(Constant(binder));
		list.AddRange(args);
		list.Add(parameterExpression);
		DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty);
		if (binder.ReturnType != typeof(object))
		{
			dynamicMetaObject = new DynamicMetaObject(Expression.Convert(dynamicMetaObject.Expression, binder.ReturnType), dynamicMetaObject.Restrictions);
		}
		if (fallbackInvoke != null)
		{
			dynamicMetaObject = fallbackInvoke!(dynamicMetaObject);
		}
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Call(Expression.Constant(_proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), dynamicMetaObject.Expression, fallbackResult.Expression, binder.ReturnType)), GetRestrictions().Merge(dynamicMetaObject.Restrictions).Merge(fallbackResult.Restrictions));
	}

	private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback)
	{
		DynamicMetaObject dynamicMetaObject = fallback(null);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), null);
		IList<Expression> list = new List<Expression>();
		list.Add(Expression.Convert(base.Expression, typeof(T)));
		list.Add(Constant(binder));
		list.AddRange(args);
		list[list.Count - 1] = Expression.Assign(parameterExpression, list[list.Count - 1]);
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Call(Expression.Constant(_proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), parameterExpression, dynamicMetaObject.Expression, typeof(object))), GetRestrictions().Merge(dynamicMetaObject.Restrictions));
	}

	private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
	{
		DynamicMetaObject dynamicMetaObject = fallback(null);
		IList<Expression> list = new List<Expression>();
		list.Add(Expression.Convert(base.Expression, typeof(T)));
		list.Add(Constant(binder));
		list.AddRange(args);
		return new DynamicMetaObject(Expression.Condition(Expression.Call(Expression.Constant(_proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), Expression.Empty(), dynamicMetaObject.Expression, typeof(void)), GetRestrictions().Merge(dynamicMetaObject.Restrictions));
	}

	private BindingRestrictions GetRestrictions()
	{
		if (base.Value != null || !base.HasValue)
		{
			return BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType);
		}
		return BindingRestrictions.GetInstanceRestriction(base.Expression, null);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		return _proxy.GetDynamicMemberNames((T)base.Value);
	}
}
