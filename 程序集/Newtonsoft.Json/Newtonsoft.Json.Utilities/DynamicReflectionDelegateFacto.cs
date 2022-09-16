using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal class DynamicReflectionDelegateFactory : ReflectionDelegateFactory
{
	internal static DynamicReflectionDelegateFactory Instance { get; } = new DynamicReflectionDelegateFactory();


	private static DynamicMethod CreateDynamicMethod(string name, Type? returnType, Type[] parameterTypes, Type owner)
	{
		if (owner.IsInterface())
		{
			return new DynamicMethod(name, returnType, parameterTypes, owner.Module, skipVisibility: true);
		}
		return new DynamicMethod(name, returnType, parameterTypes, owner, skipVisibility: true);
	}

	public override ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod(method.ToString(), typeof(object), new Type[1] { typeof(object[]) }, method.DeclaringType);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateMethodCallIL(method, iLGenerator, 0);
		return (ObjectConstructor<object>)dynamicMethod.CreateDelegate(typeof(ObjectConstructor<object>));
	}

	public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod(method.ToString(), typeof(object), new Type[2]
		{
			typeof(object),
			typeof(object[])
		}, method.DeclaringType);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateMethodCallIL(method, iLGenerator, 1);
		return (MethodCall<T, object>)dynamicMethod.CreateDelegate(typeof(MethodCall<T, object>));
	}

	private void GenerateCreateMethodCallIL(MethodBase method, ILGenerator generator, int argsIndex)
	{
		ParameterInfo[] parameters = method.GetParameters();
		Label label = generator.DefineLabel();
		generator.Emit(OpCodes.Ldarg, argsIndex);
		generator.Emit(OpCodes.Ldlen);
		generator.Emit(OpCodes.Ldc_I4, parameters.Length);
		generator.Emit(OpCodes.Beq, label);
		generator.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(ReflectionUtils.EmptyTypes));
		generator.Emit(OpCodes.Throw);
		generator.MarkLabel(label);
		if (!method.IsConstructor && !method.IsStatic)
		{
			generator.PushInstance(method.DeclaringType);
		}
		LocalBuilder local = generator.DeclareLocal(typeof(IConvertible));
		LocalBuilder local2 = generator.DeclareLocal(typeof(object));
		OpCode opcode = ((parameters.Length < 256) ? OpCodes.Ldloca_S : OpCodes.Ldloca);
		OpCode opcode2 = ((parameters.Length < 256) ? OpCodes.Ldloc_S : OpCodes.Ldloc);
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type parameterType = parameterInfo.ParameterType;
			if (parameterType.IsByRef)
			{
				parameterType = parameterType.GetElementType();
				LocalBuilder local3 = generator.DeclareLocal(parameterType);
				if (!parameterInfo.IsOut)
				{
					generator.PushArrayInstance(argsIndex, i);
					if (parameterType.IsValueType())
					{
						Label label2 = generator.DefineLabel();
						Label label3 = generator.DefineLabel();
						generator.Emit(OpCodes.Brtrue_S, label2);
						generator.Emit(opcode, local3);
						generator.Emit(OpCodes.Initobj, parameterType);
						generator.Emit(OpCodes.Br_S, label3);
						generator.MarkLabel(label2);
						generator.PushArrayInstance(argsIndex, i);
						generator.UnboxIfNeeded(parameterType);
						generator.Emit(OpCodes.Stloc_S, local3);
						generator.MarkLabel(label3);
					}
					else
					{
						generator.UnboxIfNeeded(parameterType);
						generator.Emit(OpCodes.Stloc_S, local3);
					}
				}
				generator.Emit(opcode, local3);
			}
			else if (parameterType.IsValueType())
			{
				generator.PushArrayInstance(argsIndex, i);
				generator.Emit(OpCodes.Stloc_S, local2);
				Label label4 = generator.DefineLabel();
				Label label5 = generator.DefineLabel();
				generator.Emit(OpCodes.Ldloc_S, local2);
				generator.Emit(OpCodes.Brtrue_S, label4);
				LocalBuilder local4 = generator.DeclareLocal(parameterType);
				generator.Emit(opcode, local4);
				generator.Emit(OpCodes.Initobj, parameterType);
				generator.Emit(opcode2, local4);
				generator.Emit(OpCodes.Br_S, label5);
				generator.MarkLabel(label4);
				if (parameterType.IsPrimitive())
				{
					MethodInfo method2 = typeof(IConvertible).GetMethod("To" + parameterType.Name, new Type[1] { typeof(IFormatProvider) });
					if (method2 != null)
					{
						Label label6 = generator.DefineLabel();
						generator.Emit(OpCodes.Ldloc_S, local2);
						generator.Emit(OpCodes.Isinst, parameterType);
						generator.Emit(OpCodes.Brtrue_S, label6);
						generator.Emit(OpCodes.Ldloc_S, local2);
						generator.Emit(OpCodes.Isinst, typeof(IConvertible));
						generator.Emit(OpCodes.Stloc_S, local);
						generator.Emit(OpCodes.Ldloc_S, local);
						generator.Emit(OpCodes.Brfalse_S, label6);
						generator.Emit(OpCodes.Ldloc_S, local);
						generator.Emit(OpCodes.Ldnull);
						generator.Emit(OpCodes.Callvirt, method2);
						generator.Emit(OpCodes.Br_S, label5);
						generator.MarkLabel(label6);
					}
				}
				generator.Emit(OpCodes.Ldloc_S, local2);
				generator.UnboxIfNeeded(parameterType);
				generator.MarkLabel(label5);
			}
			else
			{
				generator.PushArrayInstance(argsIndex, i);
				generator.UnboxIfNeeded(parameterType);
			}
		}
		if (method.IsConstructor)
		{
			generator.Emit(OpCodes.Newobj, (ConstructorInfo)method);
		}
		else
		{
			generator.CallMethod((MethodInfo)method);
		}
		Type type = (method.IsConstructor ? method.DeclaringType : ((MethodInfo)method).ReturnType);
		if (type != typeof(void))
		{
			generator.BoxIfNeeded(type);
		}
		else
		{
			generator.Emit(OpCodes.Ldnull);
		}
		generator.Return();
	}

	public override Func<T> CreateDefaultConstructor<T>(Type type)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod("Create" + type.FullName, typeof(T), ReflectionUtils.EmptyTypes, type);
		dynamicMethod.InitLocals = true;
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateDefaultConstructorIL(type, iLGenerator, typeof(T));
		return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
	}

	private void GenerateCreateDefaultConstructorIL(Type type, ILGenerator generator, Type delegateType)
	{
		if (type.IsValueType())
		{
			generator.DeclareLocal(type);
			generator.Emit(OpCodes.Ldloc_0);
			if (type != delegateType)
			{
				generator.Emit(OpCodes.Box, type);
			}
		}
		else
		{
			ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, ReflectionUtils.EmptyTypes, null);
			if (constructor == null)
			{
				throw new ArgumentException("Could not get constructor for {0}.".FormatWith(CultureInfo.InvariantCulture, type));
			}
			generator.Emit(OpCodes.Newobj, constructor);
		}
		generator.Return();
	}

	public override Func<T, object?> CreateGet<T>(PropertyInfo propertyInfo)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + propertyInfo.Name, typeof(object), new Type[1] { typeof(T) }, propertyInfo.DeclaringType);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateGetPropertyIL(propertyInfo, iLGenerator);
		return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
	}

	private void GenerateCreateGetPropertyIL(PropertyInfo propertyInfo, ILGenerator generator)
	{
		MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
		if (getMethod == null)
		{
			throw new ArgumentException("Property '{0}' does not have a getter.".FormatWith(CultureInfo.InvariantCulture, propertyInfo.Name));
		}
		if (!getMethod.IsStatic)
		{
			generator.PushInstance(propertyInfo.DeclaringType);
		}
		generator.CallMethod(getMethod);
		generator.BoxIfNeeded(propertyInfo.PropertyType);
		generator.Return();
	}

	public override Func<T, object?> CreateGet<T>(FieldInfo fieldInfo)
	{
		if (fieldInfo.IsLiteral)
		{
			object constantValue = fieldInfo.GetValue(null);
			return (T o) => constantValue;
		}
		DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + fieldInfo.Name, typeof(T), new Type[1] { typeof(object) }, fieldInfo.DeclaringType);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateGetFieldIL(fieldInfo, iLGenerator);
		return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
	}

	private void GenerateCreateGetFieldIL(FieldInfo fieldInfo, ILGenerator generator)
	{
		if (!fieldInfo.IsStatic)
		{
			generator.PushInstance(fieldInfo.DeclaringType);
			generator.Emit(OpCodes.Ldfld, fieldInfo);
		}
		else
		{
			generator.Emit(OpCodes.Ldsfld, fieldInfo);
		}
		generator.BoxIfNeeded(fieldInfo.FieldType);
		generator.Return();
	}

	public override Action<T, object?> CreateSet<T>(FieldInfo fieldInfo)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + fieldInfo.Name, null, new Type[2]
		{
			typeof(T),
			typeof(object)
		}, fieldInfo.DeclaringType);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateSetFieldIL(fieldInfo, iLGenerator);
		return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
	}

	internal static void GenerateCreateSetFieldIL(FieldInfo fieldInfo, ILGenerator generator)
	{
		if (!fieldInfo.IsStatic)
		{
			generator.PushInstance(fieldInfo.DeclaringType);
		}
		generator.Emit(OpCodes.Ldarg_1);
		generator.UnboxIfNeeded(fieldInfo.FieldType);
		if (!fieldInfo.IsStatic)
		{
			generator.Emit(OpCodes.Stfld, fieldInfo);
		}
		else
		{
			generator.Emit(OpCodes.Stsfld, fieldInfo);
		}
		generator.Return();
	}

	public override Action<T, object?> CreateSet<T>(PropertyInfo propertyInfo)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + propertyInfo.Name, null, new Type[2]
		{
			typeof(T),
			typeof(object)
		}, propertyInfo.DeclaringType);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		GenerateCreateSetPropertyIL(propertyInfo, iLGenerator);
		return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
	}

	internal static void GenerateCreateSetPropertyIL(PropertyInfo propertyInfo, ILGenerator generator)
	{
		MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
		if (!setMethod.IsStatic)
		{
			generator.PushInstance(propertyInfo.DeclaringType);
		}
		generator.Emit(OpCodes.Ldarg_1);
		generator.UnboxIfNeeded(propertyInfo.PropertyType);
		generator.CallMethod(setMethod);
		generator.Return();
	}
}
