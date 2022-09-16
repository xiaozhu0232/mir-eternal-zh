using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace 游戏服务器;

public abstract class GM命令
{
	private static readonly Dictionary<string, Type> 命令字典;

	private static readonly Dictionary<string, FieldInfo[]> 字段列表;

	private static readonly Dictionary<Type, Func<string, object>> 字段写入方法表;

	public static readonly Dictionary<string, string> 命令格式;

	public abstract 执行方式 执行方式 { get; }

	static GM命令()
	{
		命令字典 = new Dictionary<string, Type>();
		命令格式 = new Dictionary<string, string>();
		字段列表 = new Dictionary<string, FieldInfo[]>();
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsSubclassOf(typeof(GM命令)))
			{
				continue;
			}
			Dictionary<FieldInfo, int> 字段集合 = new Dictionary<FieldInfo, int>();
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				字段描述 customAttribute = fieldInfo.GetCustomAttribute<字段描述>();
				if (customAttribute != null)
				{
					字段集合.Add(fieldInfo, customAttribute.排序);
				}
			}
			命令字典[type.Name] = type;
			字段列表[type.Name] = 字段集合.Keys.OrderBy((FieldInfo x) => 字段集合[x]).ToArray();
			命令格式[type.Name] = "@" + type.Name;
			fields = 字段列表[type.Name];
			foreach (FieldInfo fieldInfo2 in fields)
			{
				Dictionary<string, string> dictionary = 命令格式;
				string name = type.Name;
				dictionary[name] = dictionary[name] + " " + fieldInfo2.Name;
			}
		}
		字段写入方法表 = new Dictionary<Type, Func<string, object>>
		{
			[typeof(string)] = (string s) => s,
			[typeof(int)] = (string s) => Convert.ToInt32(s),
			[typeof(uint)] = (string s) => Convert.ToUInt32(s),
			[typeof(byte)] = (string s) => Convert.ToByte(s),
			[typeof(bool)] = (string s) => Convert.ToBoolean(s),
			[typeof(float)] = (string s) => Convert.ToSingle(s),
			[typeof(decimal)] = (string s) => Convert.ToDecimal(s)
		};
	}

	public static bool 解析命令(string 文本, out GM命令 命令)
	{
		string[] array = 文本.Trim('@').Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (!命令字典.TryGetValue(array[0], out var value) || !字段列表.TryGetValue(array[0], out var value2))
		{
			主窗口.添加命令日志("<= @命令解析错误, '" + array[0] + "' 不是支持的GM命令");
			命令 = null;
			return false;
		}
		if (array.Length > 字段列表[array[0]].Length)
		{
			GM命令 gM命令 = Activator.CreateInstance(value) as GM命令;
			for (int i = 0; i < value2.Length; i++)
			{
				try
				{
					value2[i].SetValue(gM命令, 字段写入方法表[value2[i].FieldType](array[i + 1]));
				}
				catch
				{
					主窗口.添加命令日志("<= @参数转换错误. 不能将字符串 '" + array[i + 1] + "' 转换为参数 '" + value2[i].Name + "' 所需要的数据类型");
					命令 = null;
					return false;
				}
			}
			命令 = gM命令;
			return true;
		}
		主窗口.添加命令日志("<= @参数长度错误, 请参照格式: " + 命令格式[array[0]]);
		命令 = null;
		return false;
	}

	public abstract void 执行命令();
}
