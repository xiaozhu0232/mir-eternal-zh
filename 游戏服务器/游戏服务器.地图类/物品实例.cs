using System;
using System.Collections.Generic;
using System.Drawing;
using 游戏服务器.模板类;
using 游戏服务器.数据类;

namespace 游戏服务器.地图类;

public sealed class 物品实例 : 地图对象
{
	public 物品数据 物品数据;

	public 游戏物品 物品模板;

	public int 堆叠数量;

	public bool 物品绑定;

	public DateTime 消失时间;

	public DateTime 归属时间;

	public HashSet<角色数据> 物品归属;

	public override 地图实例 当前地图
	{
		get
		{
			return base.当前地图;
		}
		set
		{
			if (当前地图 != value)
			{
				base.当前地图?.移除对象(this);
				base.当前地图 = value;
				base.当前地图.添加对象(this);
			}
		}
	}

	public override int 处理间隔 => 100;

	public override bool 对象死亡 => false;

	public override bool 阻塞网格 => false;

	public override bool 能被命中 => false;

	public override 游戏对象类型 对象类型 => 游戏对象类型.物品;

	public override 技能范围类型 对象体型 => 技能范围类型.单体1x1;

	public 物品持久分类 持久类型 => 物品模板.持久类型;

	public int 默认持久 => 物品模板.物品持久;

	public int 物品编号 => 物品模板?.物品编号 ?? 0;

	public int 物品重量
	{
		get
		{
			if (物品模板.持久类型 == 物品持久分类.堆叠)
			{
				return 物品模板.物品重量 * 堆叠数量;
			}
			return 物品模板.物品重量;
		}
	}

	public bool 允许堆叠 => 物品模板.持久类型 == 物品持久分类.堆叠;

	public 物品实例(游戏物品 物品模板, 物品数据 物品数据, 地图实例 掉落地图, Point 掉落坐标, HashSet<角色数据> 物品归属, int 堆叠数量 = 0, bool 物品绑定 = false)
	{
		this.物品归属 = 物品归属;
		this.物品模板 = 物品模板;
		this.物品数据 = 物品数据;
		当前地图 = 掉落地图;
		this.物品数据 = 物品数据;
		this.堆叠数量 = 堆叠数量;
		this.物品绑定 = 物品模板.是否绑定 || 物品绑定;
		Point point = 掉落坐标;
		int num = int.MaxValue;
		for (int i = 0; i <= 120; i++)
		{
			int num2 = 0;
			Point point2 = 计算类.螺旋坐标(掉落坐标, i);
			if (掉落地图.地形阻塞(point2))
			{
				continue;
			}
			foreach (地图对象 item in 掉落地图[point2])
			{
				if (!item.对象死亡)
				{
					switch (item.对象类型)
					{
					case 游戏对象类型.物品:
						num2 += 100;
						break;
					case 游戏对象类型.玩家:
						num2 += 10000;
						break;
					case 游戏对象类型.宠物:
					case 游戏对象类型.怪物:
					case 游戏对象类型.Npcc:
						num2 += 1000;
						break;
					}
				}
			}
			if (num2 != 0)
			{
				if (num2 < num)
				{
					point = point2;
					num = num2;
				}
				continue;
			}
			point = point2;
			break;
		}
		当前坐标 = point;
		消失时间 = 主程.当前时间.AddMinutes((int)自定义类.物品清理时间);
		归属时间 = 主程.当前时间.AddMinutes((int)自定义类.物品归属时间);
		地图编号 = ++地图处理网关.物品编号;
		绑定网格();
		更新邻居时处理();
		地图处理网关.添加地图对象(this);
		次要对象 = true;
		地图处理网关.添加次要对象(this);
	}

	public override void 处理对象数据()
	{
		if (主程.当前时间 > 消失时间)
		{
			物品消失处理();
		}
	}

	public void 物品消失处理()
	{
		物品数据?.删除数据();
		删除对象();
	}

	public void 物品转移处理()
	{
		删除对象();
	}

	static 物品实例()
	{
	}
}
