using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class JsonDynamicContract : JsonContainerContract
{
	private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>> _callSiteGetters = new ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>>(CreateCallSiteGetter);

	private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object?, object>>> _callSiteSetters = new ThreadSafeStore<string, CallSite<Func<CallSite, object, object, object>>>(CreateCallSiteSetter);

	public JsonPropertyCollection Properties { get; }

	public Func<string, string>? PropertyNameResolver { get; set; }

	private static CallSite<Func<CallSite, object, object>> CreateCallSiteGetter(string name)
	{
		return CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember((GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils))));
	}

	private static CallSite<Func<CallSite, object, object?, object>> CreateCallSiteSetter(string name)
	{
		return CallSite<Func<CallSite, object, object, object>>.Create(new NoThrowSetBinderMember((SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils))));
	}

	public JsonDynamicContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Dynamic;
		Properties = new JsonPropertyCollection(base.UnderlyingType);
	}

	internal bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, out object? value)
	{
		ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
		CallSite<Func<CallSite, object, object>> callSite = _callSiteGetters.Get(name);
		object obj = callSite.Target(callSite, dynamicProvider);
		if (obj != NoThrowExpressionVisitor.ErrorResult)
		{
			value = obj;
			return true;
		}
		value = null;
		return false;
	}

	internal bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object? value)
	{
		ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
		CallSite<Func<CallSite, object, object, object>> callSite = _callSiteSetters.Get(name);
		return callSite.Target(callSite, dynamicProvider, value) != NoThrowExpressionVisitor.ErrorResult;
	}
}
