using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal class ExpressionReflectionDelegateFactory : ReflectionDelegateFactory
{
	private class ByRefParameter
	{
		public Expression Value;

		public ParameterExpression Variable;

		public bool IsOut;

		public ByRefParameter(Expression value, ParameterExpression variable, bool isOut)
		{
			Value = value;
			Variable = variable;
			IsOut = isOut;
		}
	}

	private static readonly ExpressionReflectionDelegateFactory _instance = new ExpressionReflectionDelegateFactory();

	internal static ReflectionDelegateFactory Instance => _instance;

	public override ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
	{
		ValidationUtils.ArgumentNotNull(method, "method");
		Type typeFromHandle = typeof(object);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");
		Expression body = BuildMethodCall(method, typeFromHandle, null, parameterExpression);
		return (ObjectConstructor<object>)Expression.Lambda(typeof(ObjectConstructor<object>), body, parameterExpression).Compile();
	}

	public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
	{
		ValidationUtils.ArgumentNotNull(method, "method");
		Type typeFromHandle = typeof(object);
		ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle, "target");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "args");
		Expression body = BuildMethodCall(method, typeFromHandle, parameterExpression, parameterExpression2);
		return (MethodCall<T, object>)Expression.Lambda(typeof(MethodCall<T, object>), body, parameterExpression, parameterExpression2).Compile();
	}

	private Expression BuildMethodCall(MethodBase method, Type type, ParameterExpression? targetParameterExpression, ParameterExpression argsParameterExpression)
	{
		ParameterInfo[] parameters = method.GetParameters();
		Expression[] array;
		IList<ByRefParameter> list;
		if (parameters.Length == 0)
		{
			array = CollectionUtils.ArrayEmpty<Expression>();
			list = CollectionUtils.ArrayEmpty<ByRefParameter>();
		}
		else
		{
			array = new Expression[parameters.Length];
			list = new List<ByRefParameter>();
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				Type type2 = parameterInfo.ParameterType;
				bool flag = false;
				if (type2.IsByRef)
				{
					type2 = type2.GetElementType();
					flag = true;
				}
				Expression index = Expression.Constant(i);
				Expression expression = Expression.ArrayIndex(argsParameterExpression, index);
				Expression expression2 = EnsureCastExpression(expression, type2, !flag);
				if (flag)
				{
					ParameterExpression parameterExpression = Expression.Variable(type2);
					list.Add(new ByRefParameter(expression2, parameterExpression, parameterInfo.IsOut));
					expression2 = parameterExpression;
				}
				array[i] = expression2;
			}
		}
		Expression expression3 = (method.IsConstructor ? ((Expression)Expression.New((ConstructorInfo)method, array)) : ((Expression)((!method.IsStatic) ? Expression.Call(EnsureCastExpression(targetParameterExpression, method.DeclaringType), (MethodInfo)method, array) : Expression.Call((MethodInfo)method, array))));
		expression3 = ((!(method is MethodInfo methodInfo)) ? EnsureCastExpression(expression3, type) : ((!(methodInfo.ReturnType != typeof(void))) ? Expression.Block(expression3, Expression.Constant(null)) : EnsureCastExpression(expression3, type)));
		if (list.Count > 0)
		{
			IList<ParameterExpression> list2 = new List<ParameterExpression>();
			IList<Expression> list3 = new List<Expression>();
			foreach (ByRefParameter item in list)
			{
				if (!item.IsOut)
				{
					list3.Add(Expression.Assign(item.Variable, item.Value));
				}
				list2.Add(item.Variable);
			}
			list3.Add(expression3);
			expression3 = Expression.Block(list2, list3);
		}
		return expression3;
	}

	public override Func<T> CreateDefaultConstructor<T>(Type type)
	{
		Type type2 = type;
		ValidationUtils.ArgumentNotNull(type2, "type");
		if (type2.IsAbstract())
		{
			return () => (T)Activator.CreateInstance(type2);
		}
		try
		{
			Type typeFromHandle = typeof(T);
			Expression expression = Expression.New(type2);
			expression = EnsureCastExpression(expression, typeFromHandle);
			return (Func<T>)Expression.Lambda(typeof(Func<T>), expression).Compile();
		}
		catch
		{
			return () => (T)Activator.CreateInstance(type2);
		}
	}

	public override Func<T, object?> CreateGet<T>(PropertyInfo propertyInfo)
	{
		ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
		Type typeFromHandle = typeof(T);
		Type typeFromHandle2 = typeof(object);
		ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle, "instance");
		MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
		if (getMethod == null)
		{
			throw new ArgumentException("Property does not have a getter.");
		}
		Expression expression = ((!getMethod.IsStatic) ? Expression.MakeMemberAccess(EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType), propertyInfo) : Expression.MakeMemberAccess(null, propertyInfo));
		expression = EnsureCastExpression(expression, typeFromHandle2);
		return (Func<T, object>)Expression.Lambda(typeof(Func<T, object>), expression, parameterExpression).Compile();
	}

	public override Func<T, object?> CreateGet<T>(FieldInfo fieldInfo)
	{
		ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "source");
		Expression expression = ((!fieldInfo.IsStatic) ? Expression.Field(EnsureCastExpression(parameterExpression, fieldInfo.DeclaringType), fieldInfo) : Expression.Field(null, fieldInfo));
		expression = EnsureCastExpression(expression, typeof(object));
		return Expression.Lambda<Func<T, object>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
	}

	public override Action<T, object?> CreateSet<T>(FieldInfo fieldInfo)
	{
		ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");
		if (fieldInfo.DeclaringType.IsValueType() || fieldInfo.IsInitOnly)
		{
			return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(fieldInfo);
		}
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "source");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
		Expression expression = ((!fieldInfo.IsStatic) ? Expression.Field(EnsureCastExpression(parameterExpression, fieldInfo.DeclaringType), fieldInfo) : Expression.Field(null, fieldInfo));
		Expression right = EnsureCastExpression(parameterExpression2, expression.Type);
		BinaryExpression body = Expression.Assign(expression, right);
		return (Action<T, object>)Expression.Lambda(typeof(Action<T, object>), body, parameterExpression, parameterExpression2).Compile();
	}

	public override Action<T, object?> CreateSet<T>(PropertyInfo propertyInfo)
	{
		ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
		if (propertyInfo.DeclaringType.IsValueType())
		{
			return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(propertyInfo);
		}
		Type typeFromHandle = typeof(T);
		Type typeFromHandle2 = typeof(object);
		ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle, "instance");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeFromHandle2, "value");
		Expression expression = EnsureCastExpression(parameterExpression2, propertyInfo.PropertyType);
		MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
		if (setMethod == null)
		{
			throw new ArgumentException("Property does not have a setter.");
		}
		return (Action<T, object>)Expression.Lambda(body: (!setMethod.IsStatic) ? Expression.Call(EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType), setMethod, expression) : Expression.Call(setMethod, expression), delegateType: typeof(Action<T, object>), parameters: new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	private Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
	{
		Type type = expression.Type;
		if (type == targetType || (!type.IsValueType() && targetType.IsAssignableFrom(type)))
		{
			return expression;
		}
		if (targetType.IsValueType())
		{
			Expression expression2 = Expression.Unbox(expression, targetType);
			if (allowWidening && targetType.IsPrimitive())
			{
				MethodInfo method = typeof(Convert).GetMethod("To" + targetType.Name, new Type[1] { typeof(object) });
				if (method != null)
				{
					expression2 = Expression.Condition(Expression.TypeIs(expression, targetType), expression2, Expression.Call(method, expression));
				}
			}
			return Expression.Condition(Expression.Equal(expression, Expression.Constant(null, typeof(object))), Expression.Default(targetType), expression2);
		}
		return Expression.Convert(expression, targetType);
	}
}
