using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 游戏服务器.模板类;

public static class 系统数据网关
{
	public static void 加载数据()
	{
		List<Type> 模板列表 = new List<Type>
		{
			typeof(游戏怪物),
			typeof(地图守卫),
			typeof(对话数据),
			typeof(游戏地图),
			typeof(地形数据),
			typeof(地图区域),
			typeof(传送法阵),
			typeof(怪物刷新),
			typeof(守卫刷新),
			typeof(游戏物品),
			typeof(随机属性),
			typeof(装备属性),
			typeof(游戏商店),
			typeof(珍宝商品),
			typeof(游戏称号),
			typeof(铭文技能),
			typeof(游戏技能),
			typeof(技能陷阱),
			typeof(游戏Buff)
		};
		Task.Run(delegate
		{
			foreach (Type item in 模板列表)
			{
				MethodInfo method = item.GetMethod("载入数据", BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(null, null);
				}
				else
				{
					MessageBox.Show(item.Name + " 未能找到加载方法, 加载失败");
				}
				FieldInfo field = item.GetField("数据表", BindingFlags.Static | BindingFlags.Public);
				object obj = ((field != null) ? field.GetValue(null) : null);
				object obj2;
				if (obj != null)
				{
					PropertyInfo property = obj.GetType().GetProperty("Count", BindingFlags.Instance | BindingFlags.Public);
					if (!(property == null))
					{
						obj2 = property.GetValue(obj);
						goto IL_00a2;
					}
				}
				obj2 = null;
				goto IL_00a2;
				IL_00a2:
				int num = (int)obj2;
				if (num != 0)
				{
					主窗口.添加系统日志($"{item.Name}模板已经加载,  数量: {num}");
				}
				else
				{
					主窗口.添加系统日志(item.Name + "模板加载失败, 请注意检查数据目录");
				}
			}
		}).Wait();
	}
}
