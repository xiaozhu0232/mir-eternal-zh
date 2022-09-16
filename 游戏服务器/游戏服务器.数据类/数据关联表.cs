using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace 游戏服务器.数据类;

public static class 数据关联表
{
	private struct 数据关联参数
	{
		public 游戏数据 数据;

		public 数据字段 字段;

		public object 监视器;

		public Type 数据类型;

		public int 数据索引;

		public 数据关联参数(游戏数据 数据, 数据字段 字段, object 监视器, Type 数据类型, int 数据索引)
		{
			this.数据 = 数据;
			this.字段 = 字段;
			this.监视器 = 监视器;
			this.数据类型 = 数据类型;
			this.数据索引 = 数据索引;
		}

		static 数据关联参数()
		{
		}
	}

	private struct 列表关联参数
	{
		public 游戏数据 数据;

		public 数据字段 字段;

		public IList 内部列表;

		public Type 数据类型;

		public int 数据索引;

		public 列表关联参数(游戏数据 数据, 数据字段 字段, IList 内部列表, Type 数据类型, int 数据索引)
		{
			this.数据 = 数据;
			this.字段 = 字段;
			this.内部列表 = 内部列表;
			this.数据类型 = 数据类型;
			this.数据索引 = 数据索引;
		}

		static 列表关联参数()
		{
		}
	}

	private struct 哈希关联参数<T>
	{
		public 游戏数据 数据;

		public 数据字段 字段;

		public ISet<T> 内部列表;

		public int 数据索引;

		public 哈希关联参数(游戏数据 数据, 数据字段 字段, ISet<T> 内部列表, int 数据索引)
		{
			this.数据 = 数据;
			this.字段 = 字段;
			this.内部列表 = 内部列表;
			this.数据索引 = 数据索引;
		}

		static 哈希关联参数()
		{
		}
	}

	private struct 字典关联参数
	{
		public 游戏数据 数据;

		public 数据字段 字段;

		public Type 键类型;

		public Type 值类型;

		public int 键索引;

		public int 值索引;

		public object 字典键;

		public object 字典值;

		public IDictionary 内部字典;

		public 字典关联参数(游戏数据 数据, 数据字段 字段, IDictionary 内部字典, object 字典键, object 字典值, Type 键类型, Type 值类型, int 键索引, int 值索引)
		{
			this.数据 = 数据;
			this.字段 = 字段;
			this.内部字典 = 内部字典;
			this.字典键 = 字典键;
			this.字典值 = 字典值;
			this.键类型 = 键类型;
			this.值类型 = 值类型;
			this.键索引 = 键索引;
			this.值索引 = 值索引;
		}

		static 字典关联参数()
		{
		}
	}

	private static readonly ConcurrentQueue<数据关联参数> 数据任务表;

	private static readonly ConcurrentQueue<列表关联参数> 列表任务表;

	private static readonly ConcurrentQueue<字典关联参数> 字典任务表;

	private static readonly ConcurrentQueue<哈希关联参数<宠物数据>> 哈希宠物表;

	private static readonly ConcurrentQueue<哈希关联参数<角色数据>> 哈希角色表;

	private static readonly ConcurrentQueue<哈希关联参数<邮件数据>> 哈希邮件表;

	public static void 添加任务(游戏数据 数据, 数据字段 字段, object 监视器, Type 数据类型, int 数据索引)
	{
		数据任务表.Enqueue(new 数据关联参数(数据, 字段, 监视器, 数据类型, 数据索引));
	}

	public static void 添加任务(游戏数据 数据, 数据字段 字段, IList 内部列表, Type 数据类型, int 数据索引)
	{
		列表任务表.Enqueue(new 列表关联参数(数据, 字段, 内部列表, 数据类型, 数据索引));
	}

	public static void 添加任务<T>(游戏数据 数据, 数据字段 字段, ISet<T> 内部列表, int 数据索引)
	{
		if (内部列表 is ISet<宠物数据> 内部列表2)
		{
			哈希宠物表.Enqueue(new 哈希关联参数<宠物数据>(数据, 字段, 内部列表2, 数据索引));
		}
		else if (内部列表 is ISet<角色数据> 内部列表3)
		{
			哈希角色表.Enqueue(new 哈希关联参数<角色数据>(数据, 字段, 内部列表3, 数据索引));
		}
		else if (内部列表 is ISet<邮件数据> 内部列表4)
		{
			哈希邮件表.Enqueue(new 哈希关联参数<邮件数据>(数据, 字段, 内部列表4, 数据索引));
		}
		else
		{
			MessageBox.Show("添加哈希关联任务失败");
		}
	}

	public static void 添加任务(游戏数据 数据, 数据字段 字段, IDictionary 内部字典, object 字典键, object 字典值, Type 键类型, Type 值类型, int 键索引, int 值索引)
	{
		字典任务表.Enqueue(new 字典关联参数(数据, 字段, 内部字典, 字典键, 字典值, 键类型, 值类型, 键索引, 值索引));
	}

	public static void 处理任务()
	{
		int num = 0;
		Dictionary<Type, Dictionary<string, int>> dictionary = new Dictionary<Type, Dictionary<string, int>>();
		主窗口.添加系统日志("开始处理数据关联任务...");
		while (!数据任务表.IsEmpty)
		{
			if (!数据任务表.TryDequeue(out var result) || result.数据索引 == 0)
			{
				continue;
			}
			游戏数据 游戏数据2 = 游戏数据网关.数据类型表[result.数据类型][result.数据索引];
			if (游戏数据2 != null)
			{
				result.监视器.GetType().GetField("v", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(result.监视器, 游戏数据2);
				num++;
				continue;
			}
			if (!dictionary.ContainsKey(result.数据.数据类型))
			{
				dictionary[result.数据.数据类型] = new Dictionary<string, int>();
			}
			if (!dictionary[result.数据.数据类型].ContainsKey(result.字段.字段名字))
			{
				dictionary[result.数据.数据类型][result.字段.字段名字] = 0;
			}
			dictionary[result.数据.数据类型][result.字段.字段名字]++;
		}
		while (!列表任务表.IsEmpty)
		{
			if (!列表任务表.TryDequeue(out var result2) || result2.数据索引 == 0)
			{
				continue;
			}
			游戏数据 游戏数据3 = 游戏数据网关.数据类型表[result2.数据类型][result2.数据索引];
			if (游戏数据3 == null)
			{
				if (!dictionary.ContainsKey(result2.数据.数据类型))
				{
					dictionary[result2.数据.数据类型] = new Dictionary<string, int>();
				}
				if (!dictionary[result2.数据.数据类型].ContainsKey(result2.字段.字段名字))
				{
					dictionary[result2.数据.数据类型][result2.字段.字段名字] = 0;
				}
				dictionary[result2.数据.数据类型][result2.字段.字段名字]++;
			}
			else
			{
				result2.内部列表.Add(游戏数据3);
				num++;
			}
		}
		while (!字典任务表.IsEmpty)
		{
			if (!字典任务表.TryDequeue(out var result3) || (result3.字典键 == null && result3.键索引 == 0) || (result3.字典值 == null && result3.值索引 == 0))
			{
				continue;
			}
			object obj = result3.字典键 ?? 游戏数据网关.数据类型表[result3.键类型][result3.键索引];
			object obj2 = result3.字典值 ?? 游戏数据网关.数据类型表[result3.值类型][result3.值索引];
			if (obj == null || obj2 == null)
			{
				if (!dictionary.ContainsKey(result3.数据.数据类型))
				{
					dictionary[result3.数据.数据类型] = new Dictionary<string, int>();
				}
				if (!dictionary[result3.数据.数据类型].ContainsKey(result3.字段.字段名字))
				{
					dictionary[result3.数据.数据类型][result3.字段.字段名字] = 0;
				}
				dictionary[result3.数据.数据类型][result3.字段.字段名字]++;
			}
			else
			{
				result3.内部字典[obj] = obj2;
				num++;
			}
		}
		while (!哈希宠物表.IsEmpty)
		{
			if (!哈希宠物表.TryDequeue(out var result4) || result4.数据索引 == 0)
			{
				continue;
			}
			if (游戏数据网关.数据类型表[typeof(宠物数据)][result4.数据索引] is 宠物数据 item)
			{
				result4.内部列表.Add(item);
				num++;
				continue;
			}
			if (!dictionary.ContainsKey(result4.数据.数据类型))
			{
				dictionary[result4.数据.数据类型] = new Dictionary<string, int>();
			}
			if (!dictionary[result4.数据.数据类型].ContainsKey(result4.字段.字段名字))
			{
				dictionary[result4.数据.数据类型][result4.字段.字段名字] = 0;
			}
			dictionary[result4.数据.数据类型][result4.字段.字段名字]++;
		}
		while (!哈希角色表.IsEmpty)
		{
			if (!哈希角色表.TryDequeue(out var result5) || result5.数据索引 == 0)
			{
				continue;
			}
			if (游戏数据网关.数据类型表[typeof(角色数据)][result5.数据索引] is 角色数据 item2)
			{
				result5.内部列表.Add(item2);
				num++;
				continue;
			}
			if (!dictionary.ContainsKey(result5.数据.数据类型))
			{
				dictionary[result5.数据.数据类型] = new Dictionary<string, int>();
			}
			if (!dictionary[result5.数据.数据类型].ContainsKey(result5.字段.字段名字))
			{
				dictionary[result5.数据.数据类型][result5.字段.字段名字] = 0;
			}
			dictionary[result5.数据.数据类型][result5.字段.字段名字]++;
		}
		while (!哈希邮件表.IsEmpty)
		{
			if (!哈希邮件表.TryDequeue(out var result6) || result6.数据索引 == 0)
			{
				continue;
			}
			if (!(游戏数据网关.数据类型表[typeof(邮件数据)][result6.数据索引] is 邮件数据 item3))
			{
				if (!dictionary.ContainsKey(result6.数据.数据类型))
				{
					dictionary[result6.数据.数据类型] = new Dictionary<string, int>();
				}
				if (!dictionary[result6.数据.数据类型].ContainsKey(result6.字段.字段名字))
				{
					dictionary[result6.数据.数据类型][result6.字段.字段名字] = 0;
				}
				dictionary[result6.数据.数据类型][result6.字段.字段名字]++;
			}
			else
			{
				result6.内部列表.Add(item3);
				num++;
			}
		}
		主窗口.添加系统日志($"数据关联任务处理完成, 任务总数:{num}");
		dictionary.Sum((KeyValuePair<Type, Dictionary<string, int>> x) => x.Value.Sum((KeyValuePair<string, int> o) => o.Value));
		foreach (KeyValuePair<Type, Dictionary<string, int>> item4 in dictionary)
		{
			foreach (KeyValuePair<string, int> item5 in item4.Value)
			{
				主窗口.添加系统日志($"数据类型:[{item4.Key.Name}], 内部字段:[{item5.Key}], 共[{item5.Value}]条数据关联失败");
			}
		}
	}

	static 数据关联表()
	{
		数据任务表 = new ConcurrentQueue<数据关联参数>();
		列表任务表 = new ConcurrentQueue<列表关联参数>();
		字典任务表 = new ConcurrentQueue<字典关联参数>();
		哈希宠物表 = new ConcurrentQueue<哈希关联参数<宠物数据>>();
		哈希角色表 = new ConcurrentQueue<哈希关联参数<角色数据>>();
		哈希邮件表 = new ConcurrentQueue<哈希关联参数<邮件数据>>();
	}
}
