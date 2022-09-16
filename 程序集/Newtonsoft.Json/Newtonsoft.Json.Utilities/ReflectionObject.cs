using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal class ReflectionObject
{
	public ObjectConstructor<object>? Creator { get; }

	public IDictionary<string, ReflectionMember> Members { get; }

	private ReflectionObject(ObjectConstructor<object>? creator)
	{
		Members = new Dictionary<string, ReflectionMember>();
		Creator = creator;
	}

	public object? GetValue(object target, string member)
	{
		return Members[member].Getter!(target);
	}

	public void SetValue(object target, string member, object? value)
	{
		Members[member].Setter!(target, value);
	}

	public Type GetType(string member)
	{
		return Members[member].MemberType;
	}

	public static ReflectionObject Create(Type t, params string[] memberNames)
	{
		return Create(t, null, memberNames);
	}

	public static ReflectionObject Create(Type t, MethodBase? creator, params string[] memberNames)
	{
		ReflectionDelegateFactory reflectionDelegateFactory = JsonTypeReflector.ReflectionDelegateFactory;
		ObjectConstructor<object> creator2 = null;
		if (creator != null)
		{
			creator2 = reflectionDelegateFactory.CreateParameterizedConstructor(creator);
		}
		else if (ReflectionUtils.HasDefaultConstructor(t, nonPublic: false))
		{
			Func<object> ctor = reflectionDelegateFactory.CreateDefaultConstructor<object>(t);
			creator2 = (object?[] args) => ctor();
		}
		ReflectionObject reflectionObject = new ReflectionObject(creator2);
		MethodCall<object, object?> call2;
		MethodCall<object, object?> call;
		foreach (string text in memberNames)
		{
			MemberInfo[] member = t.GetMember(text, BindingFlags.Instance | BindingFlags.Public);
			if (member.Length != 1)
			{
				throw new ArgumentException("Expected a single member with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, text));
			}
			MemberInfo memberInfo = member.Single();
			ReflectionMember reflectionMember = new ReflectionMember();
			switch (memberInfo.MemberType())
			{
			case MemberTypes.Field:
			case MemberTypes.Property:
				if (ReflectionUtils.CanReadMemberValue(memberInfo, nonPublic: false))
				{
					reflectionMember.Getter = reflectionDelegateFactory.CreateGet<object>(memberInfo);
				}
				if (ReflectionUtils.CanSetMemberValue(memberInfo, nonPublic: false, canSetReadOnly: false))
				{
					reflectionMember.Setter = reflectionDelegateFactory.CreateSet<object>(memberInfo);
				}
				break;
			case MemberTypes.Method:
			{
				MethodInfo methodInfo = (MethodInfo)memberInfo;
				if (!methodInfo.IsPublic)
				{
					break;
				}
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 0 && methodInfo.ReturnType != typeof(void))
				{
					call2 = reflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
					reflectionMember.Getter = (object target) => call2(target);
				}
				else if (parameters.Length == 1 && methodInfo.ReturnType == typeof(void))
				{
					call = reflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
					reflectionMember.Setter = delegate(object target, object? arg)
					{
						call(target, arg);
					};
				}
				break;
			}
			default:
				throw new ArgumentException("Unexpected member type '{0}' for member '{1}'.".FormatWith(CultureInfo.InvariantCulture, memberInfo.MemberType(), memberInfo.Name));
			}
			reflectionMember.MemberType = ReflectionUtils.GetMemberUnderlyingType(memberInfo);
			reflectionObject.Members[text] = reflectionMember;
		}
		return reflectionObject;
	}
}
