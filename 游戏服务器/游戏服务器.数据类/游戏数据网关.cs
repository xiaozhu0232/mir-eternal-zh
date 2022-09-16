using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 游戏服务器.数据类;

public static class 游戏数据网关
{
	private static bool 数据修改;

	private static byte[] 表头描述;

	public static 数据表实例<账号数据> 账号数据表;

	public static 数据表实例<角色数据> 角色数据表;

	public static 数据表实例<宠物数据> 宠物数据表;

	public static 数据表实例<物品数据> 物品数据表;

	public static 数据表实例<装备数据> 装备数据表;

	public static 数据表实例<技能数据> 技能数据表;

	public static 数据表实例<Buff数据> Buff数据表;

	public static 数据表实例<队伍数据> 队伍数据表;

	public static 数据表实例<行会数据> 行会数据表;

	public static 数据表实例<师门数据> 师门数据表;

	public static 数据表实例<邮件数据> 邮件数据表;

	public static Dictionary<Type, 数据表基类> 数据类型表;

	public static bool 已经修改
	{
		get
		{
			return 数据修改;
		}
		set
		{
			数据修改 = value;
			if (数据修改 && !主程.已经启动 && (主程.主线程 == null || !主程.主线程.IsAlive))
			{
				主窗口.主界面.BeginInvoke((MethodInvoker)delegate
				{
					主窗口.主界面.保存按钮.Enabled = true;
				});
			}
		}
	}

	public static string 数据目录 => 自定义类.游戏数据目录 + "\\User";

	public static string 备份目录 => 自定义类.数据备份目录;

	public static string 数据文件 => 自定义类.游戏数据目录 + "\\User\\Data.db";

	public static string 缓存文件 => 自定义类.游戏数据目录 + "\\User\\Temp.db";

	public static string 备份文件 => $"{自定义类.数据备份目录}\\User-{主程.当前时间:yyyy-MM-dd-HH-mm-ss-ffff}.db.gz";

	public static void 加载数据()
	{
		数据类型表 = new Dictionary<Type, 数据表基类>();
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (type.IsSubclassOf(typeof(游戏数据)))
			{
				数据类型表[type] = (数据表基类)Activator.CreateInstance(typeof(数据表实例<>).MakeGenericType(type));
			}
		}
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(数据类型表.Count);
			foreach (KeyValuePair<Type, 数据表基类> item in 数据类型表)
			{
				item.Value.当前映射.保存映射描述(binaryWriter);
			}
			表头描述 = memoryStream.ToArray();
		}
		if (File.Exists(数据文件))
		{
			using BinaryReader binaryReader = new BinaryReader(File.OpenRead(数据文件));
			List<数据映射> list = new List<数据映射>();
			int num = binaryReader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				list.Add(new 数据映射(binaryReader));
			}
			List<Task> list2 = new List<Task>();
			foreach (数据映射 当前历史映射 in list)
			{
				byte[] 历史映射数据 = binaryReader.ReadBytes(binaryReader.ReadInt32());
				if (!(当前历史映射.数据类型 == null) && 数据类型表.TryGetValue(当前历史映射.数据类型, out var 存表实例))
				{
					list2.Add(Task.Run(delegate
					{
						存表实例.加载数据(历史映射数据, 当前历史映射);
					}));
				}
			}
			if (list2.Count > 0)
			{
				Task.WaitAll(list2.ToArray());
			}
		}
		if (数据类型表[typeof(系统数据)].数据表.Count == 0)
		{
			new 系统数据(1);
		}
		账号数据表 = 数据类型表[typeof(账号数据)] as 数据表实例<账号数据>;
		角色数据表 = 数据类型表[typeof(角色数据)] as 数据表实例<角色数据>;
		宠物数据表 = 数据类型表[typeof(宠物数据)] as 数据表实例<宠物数据>;
		物品数据表 = 数据类型表[typeof(物品数据)] as 数据表实例<物品数据>;
		装备数据表 = 数据类型表[typeof(装备数据)] as 数据表实例<装备数据>;
		技能数据表 = 数据类型表[typeof(技能数据)] as 数据表实例<技能数据>;
		Buff数据表 = 数据类型表[typeof(Buff数据)] as 数据表实例<Buff数据>;
		队伍数据表 = 数据类型表[typeof(队伍数据)] as 数据表实例<队伍数据>;
		行会数据表 = 数据类型表[typeof(行会数据)] as 数据表实例<行会数据>;
		师门数据表 = 数据类型表[typeof(师门数据)] as 数据表实例<师门数据>;
		邮件数据表 = 数据类型表[typeof(邮件数据)] as 数据表实例<邮件数据>;
		数据关联表.处理任务();
		foreach (KeyValuePair<int, 游戏数据> item2 in 角色数据表.数据表)
		{
			item2.Value.加载完成();
		}
		系统数据.数据.加载完成();
	}

	public static void 保存数据()
	{
		Parallel.ForEach(数据类型表.Values, delegate(数据表基类 x)
		{
			x.保存数据();
		});
	}

	public static void 强制保存()
	{
		Parallel.ForEach(数据类型表.Values, delegate(数据表基类 x)
		{
			x.强制保存();
		});
	}

	public static void 导出数据()
	{
		if (!Directory.Exists(数据目录))
		{
			Directory.CreateDirectory(数据目录);
		}
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Create(缓存文件)))
		{
			binaryWriter.Write(表头描述);
			foreach (KeyValuePair<Type, 数据表基类> item in 数据类型表)
			{
				byte[] array = item.Value.存表数据();
				binaryWriter.Write(array.Length);
				binaryWriter.Write(array);
			}
		}
		if (!Directory.Exists(自定义类.数据备份目录))
		{
			Directory.CreateDirectory(自定义类.数据备份目录);
		}
		if (File.Exists(数据文件))
		{
			using (FileStream fileStream = File.OpenRead(数据文件))
			{
				using FileStream stream = File.Create(备份文件);
				using GZipStream destination = new GZipStream(stream, CompressionMode.Compress);
				fileStream.CopyTo(destination);
			}
			File.Delete(数据文件);
		}
		File.Move(缓存文件, 数据文件);
		已经修改 = false;
	}

	public static void 整理数据(bool 保存数据)
	{
		int num = 0;
		foreach (KeyValuePair<Type, 数据表基类> item in 数据类型表)
		{
			int num2 = 0;
			if (item.Value.数据类型 == typeof(行会数据))
			{
				num2 = 1610612736;
			}
			if (item.Value.数据类型 == typeof(队伍数据))
			{
				num2 = 1879048192;
			}
			List<游戏数据> list = item.Value.数据表.Values.OrderBy((游戏数据 O) => O.数据索引.V).ToList();
			int num3 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				int num4 = num2 + i + 1;
				游戏数据 游戏数据2 = list[i];
				if (游戏数据2.数据索引.V == num4)
				{
					continue;
				}
				if (!(游戏数据2 is 角色数据))
				{
					if (游戏数据2 is 行会数据)
					{
						foreach (KeyValuePair<int, 游戏数据> item2 in 行会数据表.数据表)
						{
							foreach (行会事记 item3 in ((行会数据)item2.Value).行会事记)
							{
								事记类型 事记类型2 = item3.事记类型;
								if ((uint)(事记类型2 - 9) <= 1u || (uint)(事记类型2 - 21) <= 1u)
								{
									if (item3.第一参数 == 游戏数据2.数据索引.V)
									{
										item3.第一参数 = num4;
									}
									if (item3.第二参数 == 游戏数据2.数据索引.V)
									{
										item3.第二参数 = num4;
									}
								}
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<int, 游戏数据> item4 in 行会数据表.数据表)
					{
						foreach (行会事记 item5 in ((行会数据)item4.Value).行会事记)
						{
							switch (item5.事记类型)
							{
							case 事记类型.创建公会:
							case 事记类型.加入公会:
							case 事记类型.离开公会:
								if (item5.第一参数 == 游戏数据2.数据索引.V)
								{
									item5.第一参数 = num4;
								}
								break;
							case 事记类型.逐出公会:
							case 事记类型.变更职位:
							case 事记类型.会长传位:
								if (item5.第一参数 == 游戏数据2.数据索引.V)
								{
									item5.第一参数 = num4;
								}
								if (item5.第二参数 == 游戏数据2.数据索引.V)
								{
									item5.第二参数 = num4;
								}
								break;
							}
						}
					}
				}
				游戏数据2.数据索引.V = num4;
				num3++;
			}
			item.Value.当前索引 = list.Count + num2;
			num += num3;
			item.Value.数据表 = item.Value.数据表.ToDictionary((KeyValuePair<int, 游戏数据> x) => x.Value.数据索引.V, (KeyValuePair<int, 游戏数据> x) => x.Value);
			主窗口.添加命令日志($"{item.Key.Name}已经整理完毕, 整理数量:{num3}");
		}
		主窗口.添加命令日志($"客户数据已经整理完毕, 整理总数:{num}");
		if (num > 0 && 保存数据)
		{
			主窗口.添加命令日志("正在重新保存整理后的客户数据, 可能花费较长时间, 请稍后...");
			强制保存();
			导出数据();
			主窗口.添加命令日志("数据已经保存到磁盘");
			MessageBox.Show("客户数据已经整理完毕, 应用程序需要重启");
			Environment.Exit(0);
		}
	}

	public static void 清理角色(int 限制等级, int 限制天数)
	{
		主窗口.添加命令日志("开始清理角色数据...");
		DateTime dateTime = DateTime.Now.AddDays(-限制天数);
		int num = 0;
		foreach (游戏数据 item in 角色数据表.数据表.Values.ToList())
		{
			if (!(item is 角色数据 角色数据2) || 角色数据2.当前等级.V >= 限制等级 || 角色数据2.离线日期.V > dateTime)
			{
				continue;
			}
			if (角色数据2.当前排名.Count <= 0)
			{
				if (角色数据2.元宝数量 <= 0)
				{
					if (角色数据2.当前行会?.会长数据 == 角色数据2)
					{
						主窗口.添加命令日志($"[{角色数据2}]({角色数据2.当前等级}/{(int)(DateTime.Now - 角色数据2.离线日期.V).TotalDays}) 是行会的会长, 已跳过清理");
						continue;
					}
					主窗口.添加命令日志($"开始清理[{角色数据2}]({角色数据2.当前等级}/{(int)(DateTime.Now - 角色数据2.离线日期.V).TotalDays})...");
					角色数据2.删除数据();
					num++;
					主窗口.移除角色数据(角色数据2);
				}
				else
				{
					主窗口.添加命令日志($"[{角色数据2}]({角色数据2.当前等级}/{(int)(DateTime.Now - 角色数据2.离线日期.V).TotalDays}) 有未消费元宝, 已跳过清理");
				}
			}
			else
			{
				主窗口.添加命令日志($"[{角色数据2}]({角色数据2.当前等级}/{(int)(DateTime.Now - 角色数据2.离线日期.V).TotalDays}) 在排行榜单上, 已跳过清理");
			}
		}
		主窗口.添加命令日志($"角色数据已经清理完成, 清理总数:{num}");
		if (num > 0)
		{
			主窗口.添加命令日志("正在重新保存清理后的客户数据, 可能花费较长时间, 请稍后...");
			保存数据();
			导出数据();
			加载数据();
			主窗口.添加命令日志("数据已经保存到磁盘");
		}
	}

	public static void 合并数据(string 数据文件)
	{
		byte[] 历史映射数据 = null;
		数据表基类 存表实例 = null;
		主窗口.主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主窗口.主界面.下方控件页.Enabled = false;
			主窗口.主界面.设置页面.Enabled = false;
			主窗口.主界面.主选项卡.SelectedIndex = 0;
			主窗口.主界面.日志选项卡.SelectedIndex = 2;
			主窗口.添加命令日志("开始整理当前客户数据...");
			整理数据(保存数据: false);
			Dictionary<Type, 数据表基类> dictionary = 数据类型表;
			主窗口.添加命令日志("开始加载指定客户数据...");
			数据类型表 = new Dictionary<Type, 数据表基类>();
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (type.IsSubclassOf(typeof(游戏数据)))
				{
					数据类型表[type] = (数据表基类)Activator.CreateInstance(typeof(数据表实例<>).MakeGenericType(type));
				}
			}
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(数据类型表.Count);
				foreach (KeyValuePair<Type, 数据表基类> item in 数据类型表)
				{
					item.Value.当前映射.保存映射描述(binaryWriter);
				}
				表头描述 = memoryStream.ToArray();
			}
			if (File.Exists(数据文件))
			{
				using BinaryReader binaryReader = new BinaryReader(File.OpenRead(数据文件));
				List<数据映射> list = new List<数据映射>();
				int num = binaryReader.ReadInt32();
				for (int j = 0; j < num; j++)
				{
					list.Add(new 数据映射(binaryReader));
				}
				List<Task> list2 = new List<Task>();
				foreach (数据映射 当前历史映射 in list)
				{
					历史映射数据 = binaryReader.ReadBytes(binaryReader.ReadInt32());
					if (!(当前历史映射.数据类型 == null) && 数据类型表.TryGetValue(当前历史映射.数据类型, out 存表实例))
					{
						list2.Add(Task.Run(delegate
						{
							存表实例.加载数据(历史映射数据, 当前历史映射);
						}));
					}
				}
				if (list2.Count > 0)
				{
					Task.WaitAll(list2.ToArray());
				}
			}
			主窗口.添加命令日志("开始整理指定客户数据...");
			数据关联表.处理任务();
			整理数据(保存数据: false);
			Dictionary<Type, 数据表基类> dictionary2 = 数据类型表;
			foreach (KeyValuePair<Type, 数据表基类> item2 in dictionary)
			{
				if (dictionary2.ContainsKey(item2.Key))
				{
					if (!(item2.Key == typeof(账号数据)))
					{
						if (!(item2.Key == typeof(角色数据)))
						{
							if (!(item2.Key == typeof(行会数据)))
							{
								foreach (KeyValuePair<int, 游戏数据> item3 in dictionary2[item2.Key].数据表)
								{
									item2.Value.添加数据(item3.Value, 分配索引: true);
								}
							}
							else
							{
								数据表实例<行会数据> 数据表实例2 = dictionary[item2.Key] as 数据表实例<行会数据>;
								foreach (KeyValuePair<int, 游戏数据> item4 in (dictionary2[item2.Key] as 数据表实例<行会数据>).数据表)
								{
									行会数据 行会数据2 = item4.Value as 行会数据;
									if (数据表实例2.检索表.TryGetValue(行会数据2.行会名字.V, out var value) && value is 行会数据 行会数据3)
									{
										if (行会数据3.创建日期.V > 行会数据2.创建日期.V)
										{
											行会数据3.行会名字.V += "_";
										}
										else
										{
											行会数据2.行会名字.V += "_";
										}
									}
									item2.Value.添加数据(行会数据2, 分配索引: true);
								}
							}
						}
						else
						{
							数据表实例<角色数据> 数据表实例3 = dictionary[item2.Key] as 数据表实例<角色数据>;
							foreach (KeyValuePair<int, 游戏数据> item5 in (dictionary2[item2.Key] as 数据表实例<角色数据>).数据表)
							{
								角色数据 角色数据2 = item5.Value as 角色数据;
								if (数据表实例3.检索表.TryGetValue(角色数据2.角色名字.V, out var value2) && value2 is 角色数据 角色数据3)
								{
									if (角色数据3.创建日期.V > 角色数据2.创建日期.V)
									{
										角色数据3.角色名字.V += "_";
									}
									else
									{
										角色数据2.角色名字.V += "_";
									}
								}
								item2.Value.添加数据(角色数据2, 分配索引: true);
							}
						}
					}
					else
					{
						数据表实例<账号数据> 数据表实例4 = dictionary[item2.Key] as 数据表实例<账号数据>;
						foreach (KeyValuePair<int, 游戏数据> item6 in (dictionary2[item2.Key] as 数据表实例<账号数据>).数据表)
						{
							账号数据 账号数据2 = item6.Value as 账号数据;
							if (数据表实例4.检索表.TryGetValue(账号数据2.账号名字.V, out var value3) && value3 is 账号数据 账号数据3)
							{
								foreach (角色数据 item7 in 账号数据2.角色列表)
								{
									账号数据3.角色列表.Add(item7);
									item7.所属账号.V = 账号数据3;
								}
								foreach (角色数据 item8 in 账号数据2.冻结列表)
								{
									账号数据3.冻结列表.Add(item8);
									item8.所属账号.V = 账号数据3;
								}
								foreach (角色数据 item9 in 账号数据2.删除列表)
								{
									账号数据3.删除列表.Add(item9);
									item9.所属账号.V = 账号数据3;
								}
								账号数据3.封禁日期.V = ((账号数据3.封禁日期.V <= 账号数据2.封禁日期.V) ? 账号数据3.封禁日期.V : 账号数据2.封禁日期.V);
								账号数据3.删除日期.V = default(DateTime);
							}
							else
							{
								item2.Value.添加数据(账号数据2, 分配索引: true);
							}
						}
					}
				}
			}
			dictionary2[typeof(系统数据)].数据表.Clear();
			dictionary[typeof(系统数据)].数据表.Clear();
			dictionary[typeof(系统数据)].数据表[1] = new 系统数据(1);
			foreach (KeyValuePair<int, 游戏数据> item10 in dictionary[typeof(行会数据)].数据表)
			{
				((行会数据)item10.Value).行会排名.V = 0;
			}
			foreach (KeyValuePair<int, 游戏数据> item11 in dictionary[typeof(角色数据)].数据表)
			{
				((角色数据)item11.Value).历史排名.Clear();
				((角色数据)item11.Value).当前排名.Clear();
			}
			数据类型表 = dictionary;
			强制保存();
			导出数据();
			主窗口.添加命令日志("客户数据已经合并完成");
			MessageBox.Show("客户数据已经合并完毕, 应用程序需要重启");
			Environment.Exit(0);
		});
	}

	static 游戏数据网关()
	{
	}
}
