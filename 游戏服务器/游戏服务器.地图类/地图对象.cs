using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public abstract class 地图对象
{
	public bool 次要对象;

	public bool 激活对象;

	public HashSet<地图对象> 重要邻居;

	public HashSet<地图对象> 潜行邻居;

	public HashSet<地图对象> 邻居列表;

	public HashSet<技能实例> 技能任务;

	public HashSet<陷阱实例> 陷阱列表;

	public Dictionary<object, Dictionary<游戏对象属性, int>> 属性加成;

	public DateTime 恢复时间 { get; set; }

	public DateTime 治疗时间 { get; set; }

	public DateTime 脱战时间 { get; set; }

	public DateTime 处理计时 { get; set; }

	public DateTime 预约时间 { get; set; }

	public virtual int 处理间隔 { get; }

	public int 治疗次数 { get; set; }

	public int 治疗基数 { get; set; }

	public byte 动作编号 { get; set; }

	public bool 战斗姿态 { get; set; }

	public abstract 游戏对象类型 对象类型 { get; }

	public abstract 技能范围类型 对象体型 { get; }

	public ushort 行走速度 => (ushort)this[游戏对象属性.行走速度];

	public ushort 奔跑速度 => (ushort)this[游戏对象属性.奔跑速度];

	public virtual int 行走耗时 => 行走速度 * 60;

	public virtual int 奔跑耗时 => 奔跑速度 * 60;

	public virtual int 地图编号 { get; set; }

	public virtual int 当前体力 { get; set; }

	public virtual int 当前魔力 { get; set; }

	public virtual byte 当前等级 { get; set; }

	public virtual bool 对象死亡 { get; set; }

	public virtual bool 阻塞网格 { get; set; }

	public virtual bool 能被命中 => !对象死亡;

	public virtual string 对象名字 { get; set; }

	public virtual 游戏方向 当前方向 { get; set; }

	public virtual 地图实例 当前地图 { get; set; }

	public virtual Point 当前坐标 { get; set; }

	public virtual ushort 当前高度 => 当前地图.地形高度(当前坐标);

	public virtual DateTime 忙碌时间 { get; set; }

	public virtual DateTime 硬直时间 { get; set; }

	public virtual DateTime 行走时间 { get; set; }

	public virtual DateTime 奔跑时间 { get; set; }

	public virtual int this[游戏对象属性 属性]
	{
		get
		{
			if (当前属性.ContainsKey(属性))
			{
				return 当前属性[属性];
			}
			return 0;
		}
		set
		{
			当前属性[属性] = value;
			switch (属性)
			{
			case 游戏对象属性.最大魔力:
				当前魔力 = Math.Min(当前魔力, value);
				break;
			case 游戏对象属性.最大体力:
				当前体力 = Math.Min(当前体力, value);
				break;
			}
		}
	}

	public virtual Dictionary<游戏对象属性, int> 当前属性 { get; }

	public virtual 字典监视器<int, DateTime> 冷却记录 { get; }

	public virtual 字典监视器<ushort, Buff数据> Buff列表 { get; }

	public override string ToString()
	{
		return 对象名字;
	}

	public virtual void 更新对象属性()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (object value2 in Enum.GetValues(typeof(游戏对象属性)))
		{
			int num5 = 0;
			游戏对象属性 游戏对象属性 = (游戏对象属性)value2;
			foreach (KeyValuePair<object, Dictionary<游戏对象属性, int>> item in 属性加成)
			{
				if (item.Value == null || !item.Value.TryGetValue(游戏对象属性, out var value) || value == 0)
				{
					continue;
				}
				if (!(item.Key is Buff数据))
				{
					num5 += value;
					continue;
				}
				switch (游戏对象属性)
				{
				default:
					num5 += value;
					break;
				case 游戏对象属性.奔跑速度:
					num4 = Math.Max(num4, value);
					num3 = Math.Min(num3, value);
					break;
				case 游戏对象属性.行走速度:
					num2 = Math.Max(num2, value);
					num = Math.Min(num, value);
					break;
				}
			}
			switch (游戏对象属性)
			{
			default:
				this[游戏对象属性] = Math.Max(0, num5);
				break;
			case 游戏对象属性.幸运等级:
				this[游戏对象属性] = num5;
				break;
			case 游戏对象属性.奔跑速度:
				this[游戏对象属性] = Math.Max(1, num5 + num3 + num4);
				break;
			case 游戏对象属性.行走速度:
				this[游戏对象属性] = Math.Max(1, num5 + num + num2);
				break;
			}
		}
		if (!(this is 玩家实例 玩家实例2))
		{
			return;
		}
		foreach (宠物实例 item2 in 玩家实例2.宠物列表)
		{
			if (item2.对象模板.继承属性 != null)
			{
				Dictionary<游戏对象属性, int> dictionary = new Dictionary<游戏对象属性, int>();
				属性继承[] 继承属性 = item2.对象模板.继承属性;
				for (int i = 0; i < 继承属性.Length; i++)
				{
					属性继承 属性继承 = 继承属性[i];
					dictionary[属性继承.转换属性] = (int)((float)this[属性继承.继承属性] * 属性继承.继承比例);
				}
				item2.属性加成[玩家实例2.角色数据] = dictionary;
				item2.更新对象属性();
			}
		}
	}

	public virtual void 处理对象数据()
	{
		处理计时 = 主程.当前时间;
		预约时间 = 主程.当前时间.AddMilliseconds(处理间隔);
	}

	public virtual void 自身死亡处理(地图对象 对象, bool 技能击杀)
	{
		发送封包(new 对象角色死亡
		{
			对象编号 = 地图编号
		});
		技能任务.Clear();
		对象死亡 = true;
		阻塞网格 = false;
		foreach (地图对象 item in 邻居列表.ToList())
		{
			item.对象死亡时处理(this);
		}
	}

	public 地图对象()
	{
		处理计时 = 主程.当前时间;
		技能任务 = new HashSet<技能实例>();
		陷阱列表 = new HashSet<陷阱实例>();
		重要邻居 = new HashSet<地图对象>();
		潜行邻居 = new HashSet<地图对象>();
		邻居列表 = new HashSet<地图对象>();
		当前属性 = new Dictionary<游戏对象属性, int>();
		冷却记录 = new 字典监视器<int, DateTime>(null);
		Buff列表 = new 字典监视器<ushort, Buff数据>(null);
		属性加成 = new Dictionary<object, Dictionary<游戏对象属性, int>>();
		预约时间 = 主程.当前时间.AddMilliseconds(主程.随机数.Next(处理间隔));
	}

	public void 解绑网格()
	{
		Point[] array = 计算类.技能范围(当前坐标, 当前方向, 对象体型);
		foreach (Point 坐标 in array)
		{
			当前地图[坐标].Remove(this);
		}
	}

	public void 绑定网格()
	{
		Point[] array = 计算类.技能范围(当前坐标, 当前方向, 对象体型);
		foreach (Point 坐标 in array)
		{
			当前地图[坐标].Add(this);
		}
	}

	public void 删除对象()
	{
		清空邻居时处理();
		解绑网格();
		次要对象 = false;
		地图处理网关.移除地图对象(this);
		激活对象 = false;
		地图处理网关.移除激活对象(this);
	}

	public int 网格距离(Point 坐标)
	{
		return 计算类.网格距离(当前坐标, 坐标);
	}

	public int 网格距离(地图对象 对象)
	{
		return 计算类.网格距离(当前坐标, 对象.当前坐标);
	}

	public void 发送封包(游戏封包 封包)
	{
		switch (封包.封包类型.Name)
		{
		case "触发技能正常":
		case "对象移除状态":
		case "陷阱移动位置":
		case "接收聊天信息":
		case "触发命中特效":
		case "角色等级提升":
		case "同步对象体力":
		case "触发状态效果":
		case "触发技能扩展":
		case "技能释放中断":
		case "对象角色死亡":
		case "同步宠物等级":
		case "同步对象行会":
		case "对象添加状态":
		case "对象变换类型":
		case "开始释放技能":
		case "对象转动方向":
		case "对象被动位移":
		case "对象状态变动":
		case "同步角色外形":
		case "变更摊位名字":
		case "对象角色走动":
		case "同步对象惩罚":
		case "同步装配称号":
		case "对象角色跑动":
		case "摆摊状态改变":
		case "玩家名字变灰":
			foreach (地图对象 item in 邻居列表)
			{
				if (item is 玩家实例 玩家实例2 && !玩家实例2.潜行邻居.Contains(this))
				{
					玩家实例2?.网络连接.发送封包(封包);
				}
			}
			break;
		}
		if (this is 玩家实例 玩家实例3)
		{
			玩家实例3.网络连接?.发送封包(封包);
		}
	}

	public bool 在视线内(地图对象 对象)
	{
		if (Math.Abs(当前坐标.X - 对象.当前坐标.X) > 20 || Math.Abs(当前坐标.Y - 对象.当前坐标.Y) > 20)
		{
			return false;
		}
		return true;
	}

	public bool 主动攻击(地图对象 对象)
	{
		if (!对象.对象死亡)
		{
			if (!(this is 怪物实例 怪物实例2))
			{
				if (!(this is 守卫实例 守卫实例2))
				{
					if (this is 宠物实例)
					{
						if (对象 is 怪物实例 怪物实例3)
						{
							return 怪物实例3.主动攻击目标;
						}
						return false;
					}
				}
				else
				{
					if (!守卫实例2.主动攻击目标)
					{
						return false;
					}
					if (对象 is 怪物实例 怪物实例4)
					{
						return 怪物实例4.主动攻击目标;
					}
					if (对象 is 玩家实例 玩家实例2)
					{
						return 玩家实例2.红名玩家;
					}
					if (对象 is 宠物实例)
					{
						return 守卫实例2.模板编号 == 6734;
					}
				}
			}
			else if (怪物实例2.主动攻击目标 && (对象 is 玩家实例 || 对象 is 宠物实例 || (对象 is 守卫实例 守卫实例3 && 守卫实例3.能否受伤)))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool 邻居类型(地图对象 对象)
	{
		switch (对象类型)
		{
		case 游戏对象类型.Npcc:
		{
			游戏对象类型 游戏对象类型 = 对象.对象类型;
			if ((uint)(游戏对象类型 - 1) > 1u && 游戏对象类型 != 游戏对象类型.怪物 && 游戏对象类型 != 游戏对象类型.陷阱)
			{
				return false;
			}
			return true;
		}
		case 游戏对象类型.玩家:
			return true;
		case 游戏对象类型.宠物:
		case 游戏对象类型.怪物:
			switch (对象.对象类型)
			{
			default:
				return false;
			case 游戏对象类型.玩家:
			case 游戏对象类型.宠物:
			case 游戏对象类型.怪物:
			case 游戏对象类型.Npcc:
			case 游戏对象类型.陷阱:
				return true;
			}
		default:
			return false;
		case 游戏对象类型.陷阱:
		{
			游戏对象类型 游戏对象类型2 = 对象.对象类型;
			if ((uint)(游戏对象类型2 - 1) > 1u && 游戏对象类型2 != 游戏对象类型.怪物 && 游戏对象类型2 != 游戏对象类型.Npcc)
			{
				return false;
			}
			return true;
		}
		case 游戏对象类型.物品:
			if (对象.对象类型 != 游戏对象类型.玩家)
			{
				return false;
			}
			return true;
		}
	}

	public 游戏对象关系 对象关系(地图对象 对象)
	{
		if (对象 is 陷阱实例 陷阱实例2)
		{
			对象 = 陷阱实例2.陷阱来源;
		}
		if (this != 对象)
		{
			if (!(this is 怪物实例))
			{
				if (!(this is 守卫实例))
				{
					if (!(this is 玩家实例 玩家实例2))
					{
						if (this is 宠物实例 宠物实例2)
						{
							if (宠物实例2.宠物主人 != 对象)
							{
								return 宠物实例2.宠物主人.对象关系(对象);
							}
							return 游戏对象关系.友方;
						}
						if (this is 陷阱实例 陷阱实例3)
						{
							return 陷阱实例3.陷阱来源.对象关系(对象);
						}
					}
					else
					{
						if (对象 is 怪物实例)
						{
							return 游戏对象关系.敌对;
						}
						if (对象 is 守卫实例)
						{
							if (玩家实例2.攻击模式 != 攻击模式.全体 || 当前地图.地图编号 == 80)
							{
								return 游戏对象关系.友方;
							}
							return 游戏对象关系.敌对;
						}
						if (!(对象 is 玩家实例 玩家实例3))
						{
							if (对象 is 宠物实例 宠物实例3)
							{
								if (宠物实例3.宠物主人 == 玩家实例2)
								{
									if (玩家实例2.攻击模式 != 攻击模式.全体)
									{
										return 游戏对象关系.友方;
									}
									return 游戏对象关系.友方 | 游戏对象关系.敌对;
								}
								if (玩家实例2.攻击模式 == 攻击模式.和平)
								{
									return 游戏对象关系.友方;
								}
								if (玩家实例2.攻击模式 == 攻击模式.行会)
								{
									if (玩家实例2.所属行会 != null && 宠物实例3.宠物主人.所属行会 != null && (玩家实例2.所属行会 == 宠物实例3.宠物主人.所属行会 || 玩家实例2.所属行会.结盟行会.ContainsKey(宠物实例3.宠物主人.所属行会)))
									{
										return 游戏对象关系.友方;
									}
									return 游戏对象关系.敌对;
								}
								if (玩家实例2.攻击模式 == 攻击模式.组队)
								{
									if (玩家实例2.所属队伍 != null && 宠物实例3.宠物主人.所属队伍 != null && 玩家实例2.所属队伍 == 宠物实例3.宠物主人.所属队伍)
									{
										return 游戏对象关系.友方;
									}
									return 游戏对象关系.敌对;
								}
								if (玩家实例2.攻击模式 == 攻击模式.全体)
								{
									return 游戏对象关系.敌对;
								}
								if (玩家实例2.攻击模式 == 攻击模式.善恶)
								{
									if (宠物实例3.宠物主人.红名玩家 || 宠物实例3.宠物主人.灰名玩家)
									{
										return 游戏对象关系.敌对;
									}
									return 游戏对象关系.友方;
								}
								if (玩家实例2.攻击模式 == 攻击模式.敌对)
								{
									if (玩家实例2.所属行会 != null && 宠物实例3.宠物主人.所属行会 != null && 玩家实例2.所属行会.敌对行会.ContainsKey(宠物实例3.宠物主人.所属行会))
									{
										return 游戏对象关系.敌对;
									}
									return 游戏对象关系.友方;
								}
							}
						}
						else
						{
							if (玩家实例2.攻击模式 == 攻击模式.和平)
							{
								return 游戏对象关系.友方;
							}
							if (玩家实例2.攻击模式 == 攻击模式.行会)
							{
								if (玩家实例2.所属行会 != null && 玩家实例3.所属行会 != null && (玩家实例2.所属行会 == 玩家实例3.所属行会 || 玩家实例2.所属行会.结盟行会.ContainsKey(玩家实例3.所属行会)))
								{
									return 游戏对象关系.友方;
								}
								return 游戏对象关系.敌对;
							}
							if (玩家实例2.攻击模式 == 攻击模式.组队)
							{
								if (玩家实例2.所属队伍 == null || 玩家实例3.所属队伍 == null || 玩家实例2.所属队伍 != 玩家实例3.所属队伍)
								{
									return 游戏对象关系.敌对;
								}
								return 游戏对象关系.友方;
							}
							if (玩家实例2.攻击模式 == 攻击模式.全体)
							{
								return 游戏对象关系.敌对;
							}
							if (玩家实例2.攻击模式 == 攻击模式.善恶)
							{
								if (玩家实例3.红名玩家 || 玩家实例3.灰名玩家)
								{
									return 游戏对象关系.敌对;
								}
								return 游戏对象关系.友方;
							}
							if (玩家实例2.攻击模式 == 攻击模式.敌对)
							{
								if (玩家实例2.所属行会 != null && 玩家实例3.所属行会 != null && 玩家实例2.所属行会.敌对行会.ContainsKey(玩家实例3.所属行会))
								{
									return 游戏对象关系.敌对;
								}
								return 游戏对象关系.友方;
							}
						}
					}
				}
				else if (对象 is 怪物实例 || 对象 is 宠物实例 || 对象 is 玩家实例)
				{
					return 游戏对象关系.敌对;
				}
				return 游戏对象关系.自身;
			}
			if (对象 is 怪物实例)
			{
				return 游戏对象关系.友方;
			}
			return 游戏对象关系.敌对;
		}
		return 游戏对象关系.自身;
	}

	public bool 特定类型(地图对象 来源, 指定目标类型 类型)
	{
		try
		{
			地图对象 地图对象2 = ((来源 is 陷阱实例 陷阱实例2) ? 陷阱实例2.陷阱来源 : 来源);
			if (!(this is 怪物实例 怪物实例2))
			{
				if (this is 守卫实例)
				{
					if (类型 == 指定目标类型.无)
					{
						return true;
					}
					if ((类型 & 指定目标类型.低级目标) == 指定目标类型.低级目标 && 当前等级 < 地图对象2.当前等级)
					{
						return true;
					}
					if ((类型 & 指定目标类型.背刺目标) == 指定目标类型.背刺目标)
					{
						游戏方向 游戏方向 = 计算类.计算方向(来源.当前坐标, 当前坐标);
						switch (当前方向)
						{
						case 游戏方向.上方:
							if (游戏方向 == 游戏方向.左上 || 游戏方向 == 游戏方向.上方 || 游戏方向 == 游戏方向.右上)
							{
								return true;
							}
							break;
						case 游戏方向.左上:
							if (游戏方向 == 游戏方向.左方 || 游戏方向 == 游戏方向.左上 || 游戏方向 == 游戏方向.上方)
							{
								return true;
							}
							break;
						case 游戏方向.左方:
							if (游戏方向 == 游戏方向.左方 || 游戏方向 == 游戏方向.左上 || 游戏方向 == 游戏方向.左下)
							{
								return true;
							}
							break;
						case 游戏方向.右方:
							if (游戏方向 == 游戏方向.右上 || 游戏方向 == 游戏方向.右方 || 游戏方向 == 游戏方向.右下)
							{
								return true;
							}
							break;
						case 游戏方向.右上:
							if (游戏方向 == 游戏方向.上方 || 游戏方向 == 游戏方向.右上 || 游戏方向 == 游戏方向.右方)
							{
								return true;
							}
							break;
						default:
							if (游戏方向 == 游戏方向.右方 || 游戏方向 == 游戏方向.右下 || 游戏方向 == 游戏方向.下方)
							{
								return true;
							}
							break;
						case 游戏方向.左下:
							if (游戏方向 == 游戏方向.左方 || 游戏方向 == 游戏方向.下方 || 游戏方向 == 游戏方向.左下)
							{
								return true;
							}
							break;
						case 游戏方向.下方:
							if (游戏方向 == 游戏方向.右下 || 游戏方向 == 游戏方向.下方 || 游戏方向 == 游戏方向.左下)
							{
								return true;
							}
							break;
						}
					}
				}
				else if (this is 宠物实例 宠物实例2)
				{
					if (类型 == 指定目标类型.无)
					{
						return true;
					}
					if ((类型 & 指定目标类型.低级目标) == 指定目标类型.低级目标 && 当前等级 < 地图对象2.当前等级)
					{
						return true;
					}
					if ((类型 & 指定目标类型.不死生物) == 指定目标类型.不死生物 && 宠物实例2.宠物种族 == 怪物种族分类.不死生物)
					{
						return true;
					}
					if ((类型 & 指定目标类型.虫族生物) == 指定目标类型.虫族生物 && 宠物实例2.宠物种族 == 怪物种族分类.虫族生物)
					{
						return true;
					}
					if ((类型 & 指定目标类型.所有宠物) == 指定目标类型.所有宠物)
					{
						return true;
					}
					if ((类型 & 指定目标类型.背刺目标) == 指定目标类型.背刺目标)
					{
						游戏方向 游戏方向2 = 计算类.计算方向(来源.当前坐标, 当前坐标);
						switch (当前方向)
						{
						case 游戏方向.左方:
							if (游戏方向2 == 游戏方向.左方 || 游戏方向2 == 游戏方向.左上 || 游戏方向2 == 游戏方向.左下)
							{
								return true;
							}
							break;
						case 游戏方向.上方:
							if (游戏方向2 == 游戏方向.左上 || 游戏方向2 == 游戏方向.上方 || 游戏方向2 == 游戏方向.右上)
							{
								return true;
							}
							break;
						case 游戏方向.左上:
							if (游戏方向2 == 游戏方向.左方 || 游戏方向2 == 游戏方向.左上 || 游戏方向2 == 游戏方向.上方)
							{
								return true;
							}
							break;
						case 游戏方向.右方:
							if (游戏方向2 == 游戏方向.右上 || 游戏方向2 == 游戏方向.右方 || 游戏方向2 == 游戏方向.右下)
							{
								return true;
							}
							break;
						case 游戏方向.右上:
							if (游戏方向2 == 游戏方向.上方 || 游戏方向2 == 游戏方向.右上 || 游戏方向2 == 游戏方向.右方)
							{
								return true;
							}
							break;
						default:
							if (游戏方向2 == 游戏方向.右方 || 游戏方向2 == 游戏方向.右下 || 游戏方向2 == 游戏方向.下方)
							{
								return true;
							}
							break;
						case 游戏方向.左下:
							if (游戏方向2 == 游戏方向.左方 || 游戏方向2 == 游戏方向.下方 || 游戏方向2 == 游戏方向.左下)
							{
								return true;
							}
							break;
						case 游戏方向.下方:
							if (游戏方向2 == 游戏方向.右下 || 游戏方向2 == 游戏方向.下方 || 游戏方向2 == 游戏方向.左下)
							{
								return true;
							}
							break;
						}
					}
				}
				else if (this is 玩家实例 玩家实例2)
				{
					if (类型 == 指定目标类型.无)
					{
						return true;
					}
					if ((类型 & 指定目标类型.低级目标) == 指定目标类型.低级目标 && 当前等级 < 地图对象2.当前等级)
					{
						return true;
					}
					if ((类型 & 指定目标类型.带盾法师) == 指定目标类型.带盾法师 && 玩家实例2.角色职业 == 游戏对象职业.法师 && 玩家实例2.Buff列表.ContainsKey(25350))
					{
						return true;
					}
					if ((类型 & 指定目标类型.背刺目标) == 指定目标类型.背刺目标)
					{
						游戏方向 游戏方向3 = 计算类.计算方向(来源.当前坐标, 当前坐标);
						switch (当前方向)
						{
						case 游戏方向.左方:
							if (游戏方向3 == 游戏方向.左方 || 游戏方向3 == 游戏方向.左上 || 游戏方向3 == 游戏方向.左下)
							{
								return true;
							}
							break;
						case 游戏方向.上方:
							if (游戏方向3 == 游戏方向.左上 || 游戏方向3 == 游戏方向.上方 || 游戏方向3 == 游戏方向.右上)
							{
								return true;
							}
							break;
						case 游戏方向.左上:
							if (游戏方向3 == 游戏方向.左方 || 游戏方向3 == 游戏方向.左上 || 游戏方向3 == 游戏方向.上方)
							{
								return true;
							}
							break;
						case 游戏方向.右方:
							if (游戏方向3 == 游戏方向.右上 || 游戏方向3 == 游戏方向.右方 || 游戏方向3 == 游戏方向.右下)
							{
								return true;
							}
							break;
						case 游戏方向.右上:
							if (游戏方向3 == 游戏方向.上方 || 游戏方向3 == 游戏方向.右上 || 游戏方向3 == 游戏方向.右方)
							{
								return true;
							}
							break;
						default:
							if (游戏方向3 == 游戏方向.右方 || 游戏方向3 == 游戏方向.右下 || 游戏方向3 == 游戏方向.下方)
							{
								return true;
							}
							break;
						case 游戏方向.左下:
							if (游戏方向3 == 游戏方向.左方 || 游戏方向3 == 游戏方向.下方 || 游戏方向3 == 游戏方向.左下)
							{
								return true;
							}
							break;
						case 游戏方向.下方:
							if (游戏方向3 == 游戏方向.右下 || 游戏方向3 == 游戏方向.下方 || 游戏方向3 == 游戏方向.左下)
							{
								return true;
							}
							break;
						}
					}
					if ((类型 & 指定目标类型.所有玩家) == 指定目标类型.所有玩家)
					{
						return true;
					}
				}
			}
			else
			{
				if (类型 == 指定目标类型.无)
				{
					return true;
				}
				if ((类型 & 指定目标类型.低级目标) == 指定目标类型.低级目标 && 当前等级 < 地图对象2.当前等级)
				{
					return true;
				}
				if ((类型 & 指定目标类型.所有怪物) == 指定目标类型.所有怪物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.低级怪物) == 指定目标类型.低级怪物 && 当前等级 < 地图对象2.当前等级)
				{
					return true;
				}
				if ((类型 & 指定目标类型.低血怪物) == 指定目标类型.低血怪物 && (float)当前体力 / (float)this[游戏对象属性.最大体力] < 0.4f)
				{
					return true;
				}
				if ((类型 & 指定目标类型.普通怪物) == 指定目标类型.普通怪物 && 怪物实例2.怪物级别 == 怪物级别分类.普通怪物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.不死生物) == 指定目标类型.不死生物 && 怪物实例2.怪物种族 == 怪物种族分类.不死生物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.虫族生物) == 指定目标类型.虫族生物 && 怪物实例2.怪物种族 == 怪物种族分类.虫族生物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.沃玛怪物) == 指定目标类型.沃玛怪物 && 怪物实例2.怪物种族 == 怪物种族分类.沃玛怪物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.猪类怪物) == 指定目标类型.猪类怪物 && 怪物实例2.怪物种族 == 怪物种族分类.猪类怪物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.祖玛怪物) == 指定目标类型.祖玛怪物 && 怪物实例2.怪物种族 == 怪物种族分类.祖玛怪物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.魔龙怪物) == 指定目标类型.魔龙怪物 && 怪物实例2.怪物种族 == 怪物种族分类.魔龙怪物)
				{
					return true;
				}
				if ((类型 & 指定目标类型.精英怪物) == 指定目标类型.精英怪物 && (怪物实例2.怪物级别 == 怪物级别分类.精英干将 || 怪物实例2.怪物级别 == 怪物级别分类.头目首领))
				{
					return true;
				}
				if ((类型 & 指定目标类型.背刺目标) == 指定目标类型.背刺目标)
				{
					游戏方向 游戏方向4 = 计算类.计算方向(来源.当前坐标, 当前坐标);
					switch (当前方向)
					{
					case 游戏方向.上方:
						if (游戏方向4 == 游戏方向.左上 || 游戏方向4 == 游戏方向.上方 || 游戏方向4 == 游戏方向.右上)
						{
							return true;
						}
						break;
					case 游戏方向.左上:
						if (游戏方向4 == 游戏方向.左方 || 游戏方向4 == 游戏方向.左上 || 游戏方向4 == 游戏方向.上方)
						{
							return true;
						}
						break;
					case 游戏方向.左方:
						if (游戏方向4 == 游戏方向.左方 || 游戏方向4 == 游戏方向.左上 || 游戏方向4 == 游戏方向.左下)
						{
							return true;
						}
						break;
					case 游戏方向.右方:
						if (游戏方向4 == 游戏方向.右上 || 游戏方向4 == 游戏方向.右方 || 游戏方向4 == 游戏方向.右下)
						{
							return true;
						}
						break;
					case 游戏方向.右上:
						if (游戏方向4 == 游戏方向.上方 || 游戏方向4 == 游戏方向.右上 || 游戏方向4 == 游戏方向.右方)
						{
							return true;
						}
						break;
					default:
						if (游戏方向4 == 游戏方向.右方 || 游戏方向4 == 游戏方向.右下 || 游戏方向4 == 游戏方向.下方)
						{
							return true;
						}
						break;
					case 游戏方向.左下:
						if (游戏方向4 == 游戏方向.左方 || 游戏方向4 == 游戏方向.下方 || 游戏方向4 == 游戏方向.左下)
						{
							return true;
						}
						break;
					case 游戏方向.下方:
						if (游戏方向4 == 游戏方向.右下 || 游戏方向4 == 游戏方向.下方 || 游戏方向4 == 游戏方向.左下)
						{
							return true;
						}
						break;
					}
				}
			}
			return false;
		}
		catch
		{
		}
		return false;
	}

	public virtual bool 能否走动()
	{
		if (对象死亡)
		{
			return false;
		}
		if (主程.当前时间 < 忙碌时间)
		{
			return false;
		}
		if (主程.当前时间 < 行走时间)
		{
			return false;
		}
		if (!检查状态(游戏对象状态.忙绿状态 | 游戏对象状态.定身状态 | 游戏对象状态.麻痹状态 | 游戏对象状态.失神状态))
		{
			return true;
		}
		return false;
	}

	public virtual bool 能否跑动()
	{
		if (对象死亡)
		{
			return false;
		}
		if (主程.当前时间 < 忙碌时间)
		{
			return false;
		}
		if (!(主程.当前时间 < 奔跑时间))
		{
			if (!检查状态(游戏对象状态.忙绿状态 | 游戏对象状态.残废状态 | 游戏对象状态.定身状态 | 游戏对象状态.麻痹状态 | 游戏对象状态.失神状态))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public virtual bool 能否转动()
	{
		if (对象死亡)
		{
			return false;
		}
		if (!(主程.当前时间 < 忙碌时间))
		{
			if (!(主程.当前时间 < 行走时间))
			{
				if (检查状态(游戏对象状态.忙绿状态 | 游戏对象状态.麻痹状态 | 游戏对象状态.失神状态))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public virtual bool 能被推动(地图对象 来源)
	{
		if (this == 来源)
		{
			return true;
		}
		if (!(this is 守卫实例))
		{
			if (当前等级 < 来源.当前等级)
			{
				if (this is 怪物实例 怪物实例2 && !怪物实例2.可被技能推动)
				{
					return false;
				}
				if (来源.对象关系(this) == 游戏对象关系.敌对)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public virtual bool 能否位移(地图对象 来源, Point 锚点, int 距离, int 数量, bool 穿墙, out Point 终点, out 地图对象[] 目标)
	{
		终点 = 当前坐标;
		目标 = null;
		if (!(当前坐标 == 锚点) && 能被推动(来源))
		{
			List<地图对象> list = new List<地图对象>();
			for (int i = 1; i <= 距离; i++)
			{
				if (穿墙)
				{
					Point point = 计算类.前方坐标(当前坐标, 锚点, i);
					if (当前地图.能否通行(point))
					{
						终点 = point;
					}
					continue;
				}
				游戏方向 方向 = 计算类.计算方向(当前坐标, 锚点);
				Point point2 = 计算类.前方坐标(当前坐标, 锚点, i);
				if (当前地图.地形阻塞(point2))
				{
					break;
				}
				bool flag = false;
				if (当前地图.空间阻塞(point2))
				{
					foreach (地图对象 item in 当前地图[point2].Where((地图对象 O) => O.阻塞网格))
					{
						if (list.Count < 数量)
						{
							if (item.能否位移(来源, 计算类.前方坐标(item.当前坐标, 方向, 1), 1, 数量 - list.Count - 1, 穿墙: false, out var _, out var 目标2))
							{
								list.Add(item);
								list.AddRange(目标2);
								continue;
							}
							flag = true;
							break;
						}
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				终点 = point2;
			}
			目标 = list.ToArray();
			return 终点 != 当前坐标;
		}
		return false;
	}

	public virtual bool 检查状态(游戏对象状态 待检状态)
	{
		foreach (Buff数据 value in Buff列表.Values)
		{
			if ((value.Buff效果 & Buff效果类型.状态标志) != 0 && (value.Buff模板.角色所处状态 & 待检状态) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public void 添加Buff时处理(ushort 编号, 地图对象 来源)
	{
		if (this is 物品实例 || this is 陷阱实例 || (this is 守卫实例 守卫实例2 && !守卫实例2.能否受伤))
		{
			return;
		}
		if (来源 is 陷阱实例 陷阱实例2)
		{
			来源 = 陷阱实例2.陷阱来源;
		}
		if (!游戏Buff.数据表.TryGetValue(编号, out var value))
		{
			return;
		}
		if ((value.Buff效果 & Buff效果类型.状态标志) != 0)
		{
			if (((value.角色所处状态 & 游戏对象状态.隐身状态) != 0 || (value.角色所处状态 & 游戏对象状态.潜行状态) != 0) && 检查状态(游戏对象状态.暴露状态))
			{
				return;
			}
			if ((value.角色所处状态 & 游戏对象状态.暴露状态) != 0)
			{
				foreach (Buff数据 item in Buff列表.Values.ToList())
				{
					if ((item.Buff模板.角色所处状态 & 游戏对象状态.隐身状态) != 0 || (item.Buff模板.角色所处状态 & 游戏对象状态.潜行状态) != 0)
					{
						移除Buff时处理(item.Buff编号.V);
					}
				}
			}
		}
		if ((value.Buff效果 & Buff效果类型.造成伤害) != 0 && value.Buff伤害类型 == 技能伤害类型.灼烧 && Buff列表.ContainsKey(25352))
		{
			return;
		}
		ushort 分组编号 = ((value.分组编号 != 0) ? value.分组编号 : value.Buff编号);
		Buff数据 buff数据 = null;
		switch (value.叠加类型)
		{
		case Buff叠加类型.禁止叠加:
			if (Buff列表.Values.FirstOrDefault((Buff数据 O) => O.Buff分组 == 分组编号) == null)
			{
				Buff数据 buff数据3 = (Buff列表[value.Buff编号] = new Buff数据(来源, this, value.Buff编号));
				buff数据 = buff数据3;
			}
			break;
		case Buff叠加类型.同类替换:
		{
			foreach (Buff数据 item2 in Buff列表.Values.Where((Buff数据 O) => O.Buff分组 == 分组编号).ToList())
			{
				移除Buff时处理(item2.Buff编号.V);
			}
			Buff数据 buff数据3 = (Buff列表[value.Buff编号] = new Buff数据(来源, this, value.Buff编号));
			buff数据 = buff数据3;
			break;
		}
		case Buff叠加类型.同类叠加:
		{
			if (!Buff列表.TryGetValue(编号, out var v2))
			{
				Buff数据 buff数据3 = (Buff列表[value.Buff编号] = new Buff数据(来源, this, value.Buff编号));
				buff数据 = buff数据3;
				break;
			}
			v2.当前层数.V = Math.Min((byte)(v2.当前层数.V + 1), v2.最大层数);
			if (value.Buff允许合成 && v2.当前层数.V >= value.Buff合成层数 && 游戏Buff.数据表.TryGetValue(value.Buff合成编号, out var _))
			{
				移除Buff时处理(v2.Buff编号.V);
				添加Buff时处理(value.Buff合成编号, 来源);
				break;
			}
			v2.剩余时间.V = v2.持续时间.V;
			if (v2.Buff同步)
			{
				发送封包(new 对象状态变动
				{
					对象编号 = 地图编号,
					Buff编号 = v2.Buff编号.V,
					Buff索引 = v2.Buff编号.V,
					当前层数 = v2.当前层数.V,
					剩余时间 = (int)v2.剩余时间.V.TotalMilliseconds,
					持续时间 = (int)v2.持续时间.V.TotalMilliseconds
				});
			}
			break;
		}
		case Buff叠加类型.同类延时:
		{
			if (Buff列表.TryGetValue(编号, out var v))
			{
				v.剩余时间.V += v.持续时间.V;
				if (v.Buff同步)
				{
					发送封包(new 对象状态变动
					{
						对象编号 = 地图编号,
						Buff编号 = v.Buff编号.V,
						Buff索引 = v.Buff编号.V,
						当前层数 = v.当前层数.V,
						剩余时间 = (int)v.剩余时间.V.TotalMilliseconds,
						持续时间 = (int)v.持续时间.V.TotalMilliseconds
					});
				}
			}
			else
			{
				Buff数据 buff数据3 = (Buff列表[value.Buff编号] = new Buff数据(来源, this, value.Buff编号));
				buff数据 = buff数据3;
			}
			break;
		}
		}
		if (buff数据 == null)
		{
			return;
		}
		if (buff数据.Buff同步)
		{
			发送封包(new 对象添加状态
			{
				对象编号 = 地图编号,
				Buff来源 = 来源.地图编号,
				Buff编号 = buff数据.Buff编号.V,
				Buff索引 = buff数据.Buff编号.V,
				Buff层数 = buff数据.当前层数.V,
				持续时间 = (int)buff数据.持续时间.V.TotalMilliseconds
			});
		}
		if ((value.Buff效果 & Buff效果类型.属性增减) != 0)
		{
			属性加成.Add(buff数据, buff数据.属性加成);
			更新对象属性();
		}
		if ((value.Buff效果 & Buff效果类型.状态标志) != 0)
		{
			if ((value.角色所处状态 & 游戏对象状态.隐身状态) != 0)
			{
				foreach (地图对象 item3 in 邻居列表.ToList())
				{
					item3.对象隐身时处理(this);
				}
			}
			if ((value.角色所处状态 & 游戏对象状态.潜行状态) != 0)
			{
				foreach (地图对象 item4 in 邻居列表.ToList())
				{
					item4.对象潜行时处理(this);
				}
			}
		}
		if (value.连带Buff编号 != 0)
		{
			添加Buff时处理(value.连带Buff编号, 来源);
		}
	}

	public void 移除Buff时处理(ushort 编号)
	{
		if (!Buff列表.TryGetValue(编号, out var v))
		{
			return;
		}
		if (v.Buff模板.后接Buff编号 != 0 && v.Buff来源 != null && 地图处理网关.地图对象表.TryGetValue(v.Buff来源.地图编号, out var value) && value == v.Buff来源)
		{
			添加Buff时处理(v.Buff模板.后接Buff编号, v.Buff来源);
		}
		if (v.依存列表 != null)
		{
			ushort[] 依存列表 = v.依存列表;
			foreach (ushort 编号2 in 依存列表)
			{
				删除Buff时处理(编号2);
			}
		}
		if (v.添加冷却 && v.绑定技能 != 0 && v.冷却时间 != 0 && this is 玩家实例 玩家实例2 && 玩家实例2.主体技能表.ContainsKey(v.绑定技能))
		{
			DateTime dateTime = 主程.当前时间.AddMilliseconds((int)v.冷却时间);
			DateTime dateTime2 = ((!冷却记录.ContainsKey(v.绑定技能 | 0x1000000)) ? default(DateTime) : 冷却记录[v.绑定技能 | 0x1000000]);
			if (dateTime > dateTime2)
			{
				冷却记录[v.绑定技能 | 0x1000000] = dateTime;
				发送封包(new 添加技能冷却
				{
					冷却编号 = (v.绑定技能 | 0x1000000),
					冷却时间 = v.冷却时间
				});
			}
		}
		Buff列表.Remove(编号);
		v.删除数据();
		if (v.Buff同步)
		{
			发送封包(new 对象移除状态
			{
				对象编号 = 地图编号,
				Buff索引 = 编号
			});
		}
		if ((v.Buff效果 & Buff效果类型.属性增减) != 0)
		{
			属性加成.Remove(v);
			更新对象属性();
		}
		if ((v.Buff效果 & Buff效果类型.状态标志) == 0)
		{
			return;
		}
		if ((v.Buff模板.角色所处状态 & 游戏对象状态.隐身状态) != 0)
		{
			foreach (地图对象 item in 邻居列表.ToList())
			{
				item.对象显隐时处理(this);
			}
		}
		if ((v.Buff模板.角色所处状态 & 游戏对象状态.潜行状态) == 0)
		{
			return;
		}
		foreach (地图对象 item2 in 邻居列表.ToList())
		{
			item2.对象显行时处理(this);
		}
	}

	public void 删除Buff时处理(ushort 编号)
	{
		if (!Buff列表.TryGetValue(编号, out var v))
		{
			return;
		}
		if (v.依存列表 != null)
		{
			ushort[] 依存列表 = v.依存列表;
			foreach (ushort 编号2 in 依存列表)
			{
				删除Buff时处理(编号2);
			}
		}
		Buff列表.Remove(编号);
		v.删除数据();
		if (v.Buff同步)
		{
			发送封包(new 对象移除状态
			{
				对象编号 = 地图编号,
				Buff索引 = 编号
			});
		}
		if ((v.Buff效果 & Buff效果类型.属性增减) != 0)
		{
			属性加成.Remove(v);
			更新对象属性();
		}
		if ((v.Buff效果 & Buff效果类型.状态标志) == 0)
		{
			return;
		}
		if ((v.Buff模板.角色所处状态 & 游戏对象状态.隐身状态) != 0)
		{
			foreach (地图对象 item in 邻居列表.ToList())
			{
				item.对象显隐时处理(this);
			}
		}
		if ((v.Buff模板.角色所处状态 & 游戏对象状态.潜行状态) == 0)
		{
			return;
		}
		foreach (地图对象 item2 in 邻居列表.ToList())
		{
			item2.对象显行时处理(this);
		}
	}

	public void 轮询Buff时处理(Buff数据 数据)
	{
		if (数据.到期消失 && (数据.剩余时间.V -= 主程.当前时间 - 处理计时) < TimeSpan.Zero)
		{
			移除Buff时处理(数据.Buff编号.V);
		}
		else if ((数据.处理计时.V -= 主程.当前时间 - 处理计时) < TimeSpan.Zero)
		{
			数据.处理计时.V += TimeSpan.FromMilliseconds(数据.处理间隔);
			if ((数据.Buff效果 & Buff效果类型.造成伤害) != 0)
			{
				被动受伤时处理(数据);
			}
			if ((数据.Buff效果 & Buff效果类型.生命回复) != 0)
			{
				被动回复时处理(数据);
			}
		}
	}

	public void 被技能命中处理(技能实例 技能, C_01_计算命中目标 参数)
	{
		地图对象 地图对象2 = ((!(技能.技能来源 is 陷阱实例 陷阱实例2)) ? 技能.技能来源 : 陷阱实例2.陷阱来源);
		if (技能.命中列表.ContainsKey(地图编号) || !能被命中 || (this != 地图对象2 && !邻居列表.Contains(地图对象2)) || 技能.命中列表.Count >= 参数.限定命中数量 || (参数.限定目标关系 & 地图对象2.对象关系(this)) == 0 || (参数.限定目标类型 & 对象类型) == 0 || !特定类型(技能.技能来源, 参数.限定特定类型) || ((参数.限定目标关系 & 游戏对象关系.敌对) != 0 && (检查状态(游戏对象状态.无敌状态) || ((this is 玩家实例 || this is 宠物实例) && (地图对象2 is 玩家实例 || 地图对象2 is 宠物实例) && (当前地图.安全区内(当前坐标) || 地图对象2.当前地图.安全区内(地图对象2.当前坐标))) || (地图对象2 is 怪物实例 && 当前地图.安全区内(当前坐标)))) || (this is 怪物实例 怪物实例2 && (怪物实例2.模板编号 == 8618 || 怪物实例2.模板编号 == 8621) && ((地图对象2 is 玩家实例 玩家实例2 && 玩家实例2.所属行会 != null && 玩家实例2.所属行会 == 系统数据.数据.占领行会.V) || (地图对象2 is 宠物实例 宠物实例2 && 宠物实例2.宠物主人 != null && 宠物实例2.宠物主人.所属行会 != null && 宠物实例2.宠物主人.所属行会 == 系统数据.数据.占领行会.V))))
		{
			return;
		}
		int num = 0;
		float num2 = 0f;
		int num3 = 0;
		float num4 = 0f;
		switch (参数.技能闪避方式)
		{
		case 技能闪避类型.技能无法闪避:
			num = 1;
			break;
		case 技能闪避类型.可被物理闪避:
			num3 = this[游戏对象属性.物理敏捷];
			num = 地图对象2[游戏对象属性.物理准确];
			if (this is 怪物实例)
			{
				num2 += (float)地图对象2[游戏对象属性.怪物命中] / 10000f;
			}
			if (地图对象2 is 怪物实例)
			{
				num4 += (float)this[游戏对象属性.怪物闪避] / 10000f;
			}
			break;
		case 技能闪避类型.可被魔法闪避:
			num4 = (float)this[游戏对象属性.魔法闪避] / 10000f;
			if (this is 怪物实例)
			{
				num2 += (float)地图对象2[游戏对象属性.怪物命中] / 10000f;
			}
			if (地图对象2 is 怪物实例)
			{
				num4 += (float)this[游戏对象属性.怪物闪避] / 10000f;
			}
			break;
		case 技能闪避类型.可被中毒闪避:
			num4 = (float)this[游戏对象属性.中毒躲避] / 10000f;
			break;
		case 技能闪避类型.非怪物可闪避:
			if (!(this is 怪物实例))
			{
				num3 = this[游戏对象属性.物理敏捷];
				num = 地图对象2[游戏对象属性.物理准确];
			}
			else
			{
				num = 1;
			}
			break;
		}
		命中详情 value = new 命中详情(this)
		{
			技能反馈 = (计算类.计算命中(num, num3, num2, num4) ? 参数.技能命中反馈 : 技能命中反馈.闪避)
		};
		技能.命中列表.Add(地图编号, value);
	}

	public void 被动受伤时处理(技能实例 技能, C_02_计算目标伤害 参数, 命中详情 详情, float 伤害系数)
	{
		地图对象 地图对象2 = ((!(技能.技能来源 is 陷阱实例 陷阱实例2)) ? 技能.技能来源 : 陷阱实例2.陷阱来源);
		if (!对象死亡)
		{
			if (!邻居列表.Contains(地图对象2))
			{
				详情.技能反馈 = 技能命中反馈.丢失;
			}
			else if ((地图对象2.对象关系(this) & 游戏对象关系.敌对) != 0)
			{
				if (this is 怪物实例 怪物实例2 && (怪物实例2.模板编号 == 8618 || 怪物实例2.模板编号 == 8621) && 网格距离(地图对象2) >= 4)
				{
					详情.技能反馈 = 技能命中反馈.丢失;
				}
			}
			else
			{
				详情.技能反馈 = 技能命中反馈.丢失;
			}
		}
		else
		{
			详情.技能反馈 = 技能命中反馈.丢失;
		}
		if ((详情.技能反馈 & 技能命中反馈.免疫) != 0 || (详情.技能反馈 & 技能命中反馈.丢失) != 0)
		{
			return;
		}
		if ((详情.技能反馈 & 技能命中反馈.闪避) == 0)
		{
			if (参数.技能斩杀类型 == 指定目标类型.无 || !计算类.计算概率(参数.技能斩杀概率) || !特定类型(地图对象2, 参数.技能斩杀类型))
			{
				int num = ((参数.技能伤害基数?.Length > 技能.技能等级) ? 参数.技能伤害基数[技能.技能等级] : 0);
				float num2 = ((!(参数.技能伤害系数?.Length > 技能.技能等级)) ? 0f : 参数.技能伤害系数[技能.技能等级]);
				if (this is 怪物实例)
				{
					num += 地图对象2[游戏对象属性.怪物伤害];
				}
				int num3 = 0;
				float num4 = 0f;
				if (参数.技能增伤类型 != 0 && 特定类型(地图对象2, 参数.技能增伤类型))
				{
					num3 = 参数.技能增伤基数;
					num4 = 参数.技能增伤系数;
				}
				int num5 = 0;
				float num6 = 0f;
				if (参数.技能破防概率 > 0f && 计算类.计算概率(参数.技能破防概率))
				{
					num5 = 参数.技能破防基数;
					num6 = 参数.技能破防系数;
				}
				int num7 = 0;
				int num8 = 0;
				switch (参数.技能伤害类型)
				{
				case 技能伤害类型.攻击:
					num8 = 计算类.计算防御(this[游戏对象属性.最小防御], this[游戏对象属性.最大防御]);
					num7 = 计算类.计算攻击(地图对象2[游戏对象属性.最小攻击], 地图对象2[游戏对象属性.最大攻击], 地图对象2[游戏对象属性.幸运等级]);
					break;
				case 技能伤害类型.魔法:
					num8 = 计算类.计算防御(this[游戏对象属性.最小魔防], this[游戏对象属性.最大魔防]);
					num7 = 计算类.计算攻击(地图对象2[游戏对象属性.最小魔法], 地图对象2[游戏对象属性.最大魔法], 地图对象2[游戏对象属性.幸运等级]);
					break;
				case 技能伤害类型.道术:
					num8 = 计算类.计算防御(this[游戏对象属性.最小魔防], this[游戏对象属性.最大魔防]);
					num7 = 计算类.计算攻击(地图对象2[游戏对象属性.最小道术], 地图对象2[游戏对象属性.最大道术], 地图对象2[游戏对象属性.幸运等级]);
					break;
				case 技能伤害类型.刺术:
					num8 = 计算类.计算防御(this[游戏对象属性.最小防御], this[游戏对象属性.最大防御]);
					num7 = 计算类.计算攻击(地图对象2[游戏对象属性.最小刺术], 地图对象2[游戏对象属性.最大刺术], 地图对象2[游戏对象属性.幸运等级]);
					break;
				case 技能伤害类型.弓术:
					num8 = 计算类.计算防御(this[游戏对象属性.最小防御], this[游戏对象属性.最大防御]);
					num7 = 计算类.计算攻击(地图对象2[游戏对象属性.最小弓术], 地图对象2[游戏对象属性.最大弓术], 地图对象2[游戏对象属性.幸运等级]);
					break;
				case 技能伤害类型.毒性:
					num7 = 地图对象2[游戏对象属性.最大道术];
					break;
				case 技能伤害类型.神圣:
					num7 = 计算类.计算攻击(地图对象2[游戏对象属性.最小圣伤], 地图对象2[游戏对象属性.最大圣伤], 0);
					break;
				}
				if (this is 怪物实例)
				{
					num8 = Math.Max(0, num8 - (int)((float)(num8 * 地图对象2[游戏对象属性.怪物破防]) / 10000f));
				}
				int num9 = 0;
				float num10 = 0f;
				int num11 = int.MaxValue;
				foreach (Buff数据 item in 地图对象2.Buff列表.Values.ToList())
				{
					if ((item.Buff效果 & Buff效果类型.伤害增减) == 0 || (item.Buff模板.效果判定方式 != 0 && item.Buff模板.效果判定方式 != Buff判定方式.主动攻击减伤))
					{
						continue;
					}
					bool flag = false;
					switch (参数.技能伤害类型)
					{
					case 技能伤害类型.魔法:
					case 技能伤害类型.道术:
						switch (item.Buff模板.效果判定类型)
						{
						case Buff判定类型.所有技能伤害:
						case Buff判定类型.所有魔法伤害:
							flag = true;
							break;
						case Buff判定类型.所有特定伤害:
							flag = item.Buff模板.特定技能编号?.Contains(技能.技能编号) ?? false;
							break;
						}
						break;
					case 技能伤害类型.攻击:
					case 技能伤害类型.刺术:
					case 技能伤害类型.弓术:
						switch (item.Buff模板.效果判定类型)
						{
						case Buff判定类型.所有特定伤害:
							flag = item.Buff模板.特定技能编号?.Contains(技能.技能编号) ?? false;
							break;
						case Buff判定类型.所有技能伤害:
						case Buff判定类型.所有物理伤害:
							flag = true;
							break;
						}
						break;
					case 技能伤害类型.毒性:
					case 技能伤害类型.神圣:
					case 技能伤害类型.灼烧:
					case 技能伤害类型.撕裂:
						if (item.Buff模板.效果判定类型 == Buff判定类型.所有特定伤害)
						{
							flag = item.Buff模板.特定技能编号?.Contains(技能.技能编号) ?? false;
						}
						break;
					}
					if (!flag)
					{
						continue;
					}
					int num12 = item.当前层数.V * ((item.Buff模板.伤害增减基数?.Length > item.Buff等级.V) ? item.Buff模板.伤害增减基数[item.Buff等级.V] : 0);
					float num13 = (float)(int)item.当前层数.V * ((!(item.Buff模板.伤害增减系数?.Length > item.Buff等级.V)) ? 0f : item.Buff模板.伤害增减系数[item.Buff等级.V]);
					num9 += ((item.Buff模板.效果判定方式 == Buff判定方式.主动攻击增伤) ? num12 : (-num12));
					num10 += ((item.Buff模板.效果判定方式 != 0) ? (0f - num13) : num13);
					if (item.Buff模板.生效后接编号 != 0 && item.Buff来源 != null && 地图处理网关.地图对象表.TryGetValue(item.Buff来源.地图编号, out var value) && value == item.Buff来源)
					{
						if (item.Buff模板.后接技能来源)
						{
							地图对象2.添加Buff时处理(item.Buff模板.生效后接编号, item.Buff来源);
						}
						else
						{
							添加Buff时处理(item.Buff模板.生效后接编号, item.Buff来源);
						}
					}
					if (item.Buff模板.效果生效移除)
					{
						地图对象2.移除Buff时处理(item.Buff编号.V);
					}
				}
				foreach (Buff数据 item2 in Buff列表.Values.ToList())
				{
					if ((item2.Buff效果 & Buff效果类型.伤害增减) == 0 || (item2.Buff模板.效果判定方式 != Buff判定方式.被动受伤增伤 && item2.Buff模板.效果判定方式 != Buff判定方式.被动受伤减伤))
					{
						continue;
					}
					bool flag2 = false;
					switch (参数.技能伤害类型)
					{
					case 技能伤害类型.魔法:
					case 技能伤害类型.道术:
						switch (item2.Buff模板.效果判定类型)
						{
						case Buff判定类型.所有技能伤害:
						case Buff判定类型.所有魔法伤害:
							flag2 = true;
							break;
						case Buff判定类型.所有特定伤害:
							flag2 = item2.Buff模板.特定技能编号.Contains(技能.技能编号);
							break;
						case Buff判定类型.来源特定伤害:
						{
							int num16;
							if (地图对象2 == item2.Buff来源)
							{
								HashSet<ushort> 特定技能编号3 = item2.Buff模板.特定技能编号;
								num16 = ((特定技能编号3 != null && 特定技能编号3.Contains(技能.技能编号)) ? 1 : 0);
							}
							else
							{
								num16 = 0;
							}
							flag2 = (byte)num16 != 0;
							break;
						}
						case Buff判定类型.来源技能伤害:
						case Buff判定类型.来源魔法伤害:
							flag2 = 地图对象2 == item2.Buff来源;
							break;
						}
						break;
					case 技能伤害类型.攻击:
					case 技能伤害类型.刺术:
					case 技能伤害类型.弓术:
						switch (item2.Buff模板.效果判定类型)
						{
						case Buff判定类型.所有特定伤害:
							flag2 = item2.Buff模板.特定技能编号?.Contains(技能.技能编号) ?? false;
							break;
						case Buff判定类型.所有技能伤害:
						case Buff判定类型.所有物理伤害:
							flag2 = true;
							break;
						case Buff判定类型.来源特定伤害:
						{
							int num15;
							if (地图对象2 == item2.Buff来源)
							{
								HashSet<ushort> 特定技能编号2 = item2.Buff模板.特定技能编号;
								num15 = ((特定技能编号2 != null && 特定技能编号2.Contains(技能.技能编号)) ? 1 : 0);
							}
							else
							{
								num15 = 0;
							}
							flag2 = (byte)num15 != 0;
							break;
						}
						case Buff判定类型.来源技能伤害:
						case Buff判定类型.来源物理伤害:
							flag2 = 地图对象2 == item2.Buff来源;
							break;
						}
						break;
					case 技能伤害类型.毒性:
					case 技能伤害类型.神圣:
					case 技能伤害类型.灼烧:
					case 技能伤害类型.撕裂:
						switch (item2.Buff模板.效果判定类型)
						{
						case Buff判定类型.来源特定伤害:
						{
							int num14;
							if (地图对象2 == item2.Buff来源)
							{
								HashSet<ushort> 特定技能编号 = item2.Buff模板.特定技能编号;
								num14 = ((特定技能编号 != null && 特定技能编号.Contains(技能.技能编号)) ? 1 : 0);
							}
							else
							{
								num14 = 0;
							}
							flag2 = (byte)num14 != 0;
							break;
						}
						case Buff判定类型.所有特定伤害:
							flag2 = item2.Buff模板.特定技能编号?.Contains(技能.技能编号) ?? false;
							break;
						}
						break;
					}
					if (!flag2)
					{
						continue;
					}
					int num17 = item2.当前层数.V * ((item2.Buff模板.伤害增减基数?.Length > item2.Buff等级.V) ? item2.Buff模板.伤害增减基数[item2.Buff等级.V] : 0);
					float num18 = (float)(int)item2.当前层数.V * ((item2.Buff模板.伤害增减系数?.Length > item2.Buff等级.V) ? item2.Buff模板.伤害增减系数[item2.Buff等级.V] : 0f);
					num9 += ((item2.Buff模板.效果判定方式 == Buff判定方式.被动受伤增伤) ? num17 : (-num17));
					num10 += ((item2.Buff模板.效果判定方式 == Buff判定方式.被动受伤增伤) ? num18 : (0f - num18));
					if (item2.Buff模板.生效后接编号 != 0 && item2.Buff来源 != null && 地图处理网关.地图对象表.TryGetValue(item2.Buff来源.地图编号, out var value2) && value2 == item2.Buff来源)
					{
						if (item2.Buff模板.后接技能来源)
						{
							地图对象2.添加Buff时处理(item2.Buff模板.生效后接编号, item2.Buff来源);
						}
						else
						{
							添加Buff时处理(item2.Buff模板.生效后接编号, item2.Buff来源);
						}
					}
					if (item2.Buff模板.效果判定方式 == Buff判定方式.被动受伤减伤 && item2.Buff模板.限定伤害上限)
					{
						num11 = Math.Min(num11, item2.Buff模板.限定伤害数值);
					}
					if (item2.Buff模板.效果生效移除)
					{
						移除Buff时处理(item2.Buff编号.V);
					}
				}
				float num19 = (num2 + num4) * (float)num7 + (float)num + (float)num3 + (float)num9;
				float val = (float)(num8 - num5) - (float)num8 * num6;
				float val2 = (num19 - Math.Max(0f, val)) * (1f + num10) * 伤害系数;
				详情.技能伤害 = (int)Math.Min(num11, Math.Max(0f, val2));
			}
			else
			{
				详情.技能伤害 = 当前体力;
			}
		}
		脱战时间 = 主程.当前时间.AddSeconds(10.0);
		地图对象2.脱战时间 = 主程.当前时间.AddSeconds(10.0);
		if ((详情.技能反馈 & 技能命中反馈.闪避) == 0)
		{
			foreach (Buff数据 item3 in Buff列表.Values.ToList())
			{
				if ((item3.Buff效果 & Buff效果类型.状态标志) != 0 && (item3.Buff模板.角色所处状态 & 游戏对象状态.失神状态) != 0)
				{
					移除Buff时处理(item3.Buff编号.V);
				}
			}
		}
		if (this is 怪物实例 怪物实例3)
		{
			怪物实例3.硬直时间 = 主程.当前时间.AddMilliseconds(参数.目标硬直时间);
			if (地图对象2 is 玩家实例 || 地图对象2 is 宠物实例)
			{
				怪物实例3.对象仇恨.添加仇恨(地图对象2, 主程.当前时间.AddMilliseconds(怪物实例3.仇恨时长), 详情.技能伤害);
			}
		}
		else if (this is 玩家实例 玩家实例2)
		{
			if (详情.技能伤害 > 0)
			{
				玩家实例2.装备损失持久(详情.技能伤害);
			}
			if (详情.技能伤害 > 0)
			{
				玩家实例2.扣除护盾时间(详情.技能伤害);
			}
			if (玩家实例2.对象关系(地图对象2) == 游戏对象关系.敌对)
			{
				foreach (宠物实例 item4 in 玩家实例2.宠物列表.ToList())
				{
					if (item4.邻居列表.Contains(地图对象2) && !地图对象2.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
					{
						item4.对象仇恨.添加仇恨(地图对象2, 主程.当前时间.AddMilliseconds(item4.仇恨时长), 0);
					}
				}
			}
			if (地图对象2 is 玩家实例 玩家实例3 && !当前地图.自由区内(当前坐标) && !玩家实例2.灰名玩家 && !玩家实例2.红名玩家)
			{
				if (!玩家实例3.红名玩家)
				{
					玩家实例3.灰名时间 = TimeSpan.FromMinutes(1.0);
				}
				else
				{
					玩家实例3.减PK时间 = TimeSpan.FromMinutes(1.0);
				}
			}
			else if (地图对象2 is 宠物实例 宠物实例2 && !当前地图.自由区内(当前坐标) && !玩家实例2.灰名玩家 && !玩家实例2.红名玩家)
			{
				if (!宠物实例2.宠物主人.红名玩家)
				{
					宠物实例2.宠物主人.灰名时间 = TimeSpan.FromMinutes(1.0);
				}
				else
				{
					宠物实例2.宠物主人.减PK时间 = TimeSpan.FromMinutes(1.0);
				}
			}
		}
		else if (!(this is 宠物实例 宠物实例3))
		{
			if (this is 守卫实例 守卫实例2 && 守卫实例2.对象关系(地图对象2) == 游戏对象关系.敌对)
			{
				守卫实例2.对象仇恨.添加仇恨(地图对象2, default(DateTime), 0);
			}
		}
		else
		{
			if (地图对象2 != 宠物实例3.宠物主人 && 宠物实例3.对象关系(地图对象2) == 游戏对象关系.敌对)
			{
				foreach (宠物实例 item5 in 宠物实例3.宠物主人?.宠物列表.ToList())
				{
					if (item5.邻居列表.Contains(地图对象2) && !地图对象2.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
					{
						item5.对象仇恨.添加仇恨(地图对象2, 主程.当前时间.AddMilliseconds(item5.仇恨时长), 0);
					}
				}
			}
			if (地图对象2 != 宠物实例3.宠物主人 && 地图对象2 is 玩家实例 玩家实例4 && !当前地图.自由区内(当前坐标) && !宠物实例3.宠物主人.灰名玩家 && !宠物实例3.宠物主人.红名玩家)
			{
				玩家实例4.灰名时间 = TimeSpan.FromMinutes(1.0);
			}
		}
		if (地图对象2 is 玩家实例 玩家实例5)
		{
			if (玩家实例5.对象关系(this) == 游戏对象关系.敌对 && !检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
			{
				foreach (宠物实例 item6 in 玩家实例5.宠物列表.ToList())
				{
					if (item6.邻居列表.Contains(this))
					{
						item6.对象仇恨.添加仇恨(this, 主程.当前时间.AddMilliseconds(item6.仇恨时长), 参数.增加宠物仇恨 ? 详情.技能伤害 : 0);
					}
				}
			}
			if (主程.当前时间 > 玩家实例5.战具计时 && !玩家实例5.对象死亡 && 玩家实例5.当前体力 < 玩家实例5[游戏对象属性.最大体力] && 玩家实例5.角色装备.TryGetValue(15, out var v) && v.当前持久.V > 0 && (v.物品编号 == 99999106 || v.物品编号 == 99999107))
			{
				玩家实例5.当前体力 += ((this is 怪物实例) ? 20 : 10);
				玩家实例5.战具损失持久(1);
				玩家实例5.战具计时 = 主程.当前时间.AddMilliseconds(1000.0);
			}
		}
		if ((当前体力 = Math.Max(0, 当前体力 - 详情.技能伤害)) == 0)
		{
			详情.技能反馈 |= 技能命中反馈.死亡;
			自身死亡处理(地图对象2, 技能击杀: true);
		}
	}

	public void 被动受伤时处理(Buff数据 数据)
	{
		int num = 0;
		switch (数据.伤害类型)
		{
		case 技能伤害类型.魔法:
		case 技能伤害类型.道术:
			num = 计算类.计算防御(this[游戏对象属性.最小魔防], this[游戏对象属性.最大魔防]);
			break;
		case 技能伤害类型.攻击:
		case 技能伤害类型.刺术:
		case 技能伤害类型.弓术:
			num = 计算类.计算防御(this[游戏对象属性.最小防御], this[游戏对象属性.最大防御]);
			break;
		}
		int num2 = Math.Max(0, 数据.伤害基数.V * 数据.当前层数.V - num);
		当前体力 = Math.Max(0, 当前体力 - num2);
		触发状态效果 触发状态效果 = new 触发状态效果
		{
			Buff编号 = 数据.Buff编号.V
		};
		触发状态效果.Buff来源 = 数据.Buff来源?.地图编号 ?? 0;
		触发状态效果.Buff目标 = 地图编号;
		触发状态效果.血量变化 = -num2;
		发送封包(触发状态效果);
		if (当前体力 == 0)
		{
			自身死亡处理(数据.Buff来源, 技能击杀: false);
		}
	}

	public void 被动回复时处理(技能实例 技能, C_05_计算目标回复 参数)
	{
		if (!对象死亡 && 当前地图 == 技能.技能来源.当前地图 && (this == 技能.技能来源 || 邻居列表.Contains(技能.技能来源)))
		{
			地图对象 地图对象2 = ((!(技能.技能来源 is 陷阱实例 陷阱实例2)) ? 技能.技能来源 : 陷阱实例2.陷阱来源);
			int num = ((参数.体力回复次数?.Length > 技能.技能等级) ? 参数.体力回复次数[技能.技能等级] : 0);
			int num2 = ((参数.体力回复基数?.Length > 技能.技能等级) ? 参数.体力回复基数[技能.技能等级] : 0);
			float num3 = ((!(参数.道术叠加次数?.Length > 技能.技能等级)) ? 0f : 参数.道术叠加次数[技能.技能等级]);
			float num4 = ((!(参数.道术叠加基数?.Length > 技能.技能等级)) ? 0f : 参数.道术叠加基数[技能.技能等级]);
			int num5 = ((参数.立即回复基数?.Length > 技能.技能等级 && 地图对象2 == this) ? 参数.立即回复基数[技能.技能等级] : 0);
			float num6 = ((!(参数.立即回复系数?.Length > 技能.技能等级) || 地图对象2 != this) ? 0f : 参数.立即回复系数[技能.技能等级]);
			if (num3 > 0f)
			{
				num += (int)(num3 * (float)计算类.计算攻击(地图对象2[游戏对象属性.最小道术], 地图对象2[游戏对象属性.最大道术], 地图对象2[游戏对象属性.幸运等级]));
			}
			if (num4 > 0f)
			{
				num2 += (int)(num4 * (float)计算类.计算攻击(地图对象2[游戏对象属性.最小道术], 地图对象2[游戏对象属性.最大道术], 地图对象2[游戏对象属性.幸运等级]));
			}
			if (num5 > 0)
			{
				当前体力 += num5;
			}
			if (num6 > 0f)
			{
				当前体力 += (int)((float)this[游戏对象属性.最大体力] * num6);
			}
			if (num > 治疗次数 && num2 > 0)
			{
				治疗次数 = (byte)num;
				治疗基数 = num2;
				治疗时间 = 主程.当前时间.AddMilliseconds(500.0);
			}
		}
	}

	public void 被动回复时处理(Buff数据 数据)
	{
		if (数据.Buff模板.体力回复基数 != null && 数据.Buff模板.体力回复基数.Length > 数据.Buff等级.V)
		{
			byte b = 数据.Buff模板.体力回复基数[数据.Buff等级.V];
			当前体力 += b;
			触发状态效果 触发状态效果 = new 触发状态效果
			{
				Buff编号 = 数据.Buff编号.V
			};
			触发状态效果.Buff来源 = 数据.Buff来源?.地图编号 ?? 0;
			触发状态效果.Buff目标 = 地图编号;
			触发状态效果.血量变化 = b;
			发送封包(触发状态效果);
		}
	}

	public void 自身移动时处理(Point 坐标)
	{
		if (!(this is 玩家实例 玩家实例2))
		{
			if (this is 宠物实例)
			{
				foreach (Buff数据 item in Buff列表.Values.ToList())
				{
					if ((item.Buff效果 & Buff效果类型.创建陷阱) != 0 && 技能陷阱.数据表.TryGetValue(item.Buff模板.触发陷阱技能, out var 陷阱模板))
					{
						int num = 0;
						while (true)
						{
							Point point = 计算类.前方坐标(当前坐标, 坐标, num);
							if (point == 坐标)
							{
								break;
							}
							Point[] array = 计算类.技能范围(point, 当前方向, item.Buff模板.触发陷阱数量);
							foreach (Point 坐标2 in array)
							{
								if (!当前地图.地形阻塞(坐标2) && 当前地图[坐标2].FirstOrDefault((地图对象 O) => O is 陷阱实例 陷阱实例3 && 陷阱实例3.陷阱分组编号 != 0 && 陷阱实例3.陷阱分组编号 == 陷阱模板.分组编号) == null)
								{
									陷阱列表.Add(new 陷阱实例(this, 陷阱模板, 当前地图, 坐标2));
								}
							}
							num++;
						}
					}
					if ((item.Buff效果 & Buff效果类型.状态标志) != 0 && (item.Buff模板.角色所处状态 & 游戏对象状态.隐身状态) != 0)
					{
						移除Buff时处理(item.Buff编号.V);
					}
				}
			}
		}
		else
		{
			玩家实例2.当前交易?.结束交易();
			foreach (Buff数据 item2 in Buff列表.Values.ToList())
			{
				if ((item2.Buff效果 & Buff效果类型.创建陷阱) != 0 && 技能陷阱.数据表.TryGetValue(item2.Buff模板.触发陷阱技能, out var 陷阱模板2))
				{
					int num2 = 0;
					while (true)
					{
						Point point2 = 计算类.前方坐标(当前坐标, 坐标, num2);
						if (point2 == 坐标)
						{
							break;
						}
						Point[] array = 计算类.技能范围(point2, 当前方向, item2.Buff模板.触发陷阱数量);
						foreach (Point 坐标3 in array)
						{
							if (!当前地图.地形阻塞(坐标3) && 当前地图[坐标3].FirstOrDefault((地图对象 O) => O is 陷阱实例 陷阱实例2 && 陷阱实例2.陷阱分组编号 != 0 && 陷阱实例2.陷阱分组编号 == 陷阱模板2.分组编号) == null)
							{
								陷阱列表.Add(new 陷阱实例(this, 陷阱模板2, 当前地图, 坐标3));
							}
						}
						num2++;
					}
				}
				if ((item2.Buff效果 & Buff效果类型.状态标志) != 0 && (item2.Buff模板.角色所处状态 & 游戏对象状态.隐身状态) != 0)
				{
					移除Buff时处理(item2.Buff编号.V);
				}
			}
		}
		解绑网格();
		当前坐标 = 坐标;
		绑定网格();
		更新邻居时处理();
		foreach (地图对象 item3 in 邻居列表.ToList())
		{
			item3.对象移动时处理(this);
		}
	}

	public void 清空邻居时处理()
	{
		foreach (地图对象 item in 邻居列表.ToList())
		{
			item.对象消失时处理(this);
		}
		邻居列表.Clear();
		重要邻居.Clear();
		潜行邻居.Clear();
	}

	public void 更新邻居时处理()
	{
		foreach (地图对象 item in 邻居列表.ToList())
		{
			if (当前地图 != item.当前地图 || !在视线内(item))
			{
				item.对象消失时处理(this);
				对象消失时处理(item);
			}
		}
		for (int i = -20; i <= 20; i++)
		{
			for (int j = -20; j <= 20; j++)
			{
				当前地图[new Point(当前坐标.X + i, 当前坐标.Y + j)].ToList();
				try
				{
					foreach (地图对象 item2 in 当前地图[new Point(当前坐标.X + i, 当前坐标.Y + j)])
					{
						if (item2 != this)
						{
							if (!邻居列表.Contains(item2) && 邻居类型(item2))
							{
								对象出现时处理(item2);
							}
							if (!item2.邻居列表.Contains(this) && item2.邻居类型(this))
							{
								item2.对象出现时处理(this);
							}
						}
					}
				}
				catch
				{
				}
			}
		}
	}

	public void 对象移动时处理(地图对象 对象)
	{
		if (!(this is 物品实例))
		{
			if (this is 宠物实例 宠物实例2)
			{
				if (!宠物实例2.主动攻击(对象) || 网格距离(对象) > 宠物实例2.仇恨范围 || 对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
				{
					if (网格距离(对象) > 宠物实例2.仇恨范围 && 宠物实例2.对象仇恨.仇恨列表.TryGetValue(对象, out var value) && value.仇恨时间 < 主程.当前时间)
					{
						宠物实例2.对象仇恨.移除仇恨(对象);
					}
				}
				else
				{
					宠物实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
				}
			}
			else if (!(this is 怪物实例 怪物实例2))
			{
				if (this is 陷阱实例 陷阱实例2)
				{
					if (计算类.技能范围(陷阱实例2.当前坐标, 陷阱实例2.当前方向, 陷阱实例2.对象体型).Contains(对象.当前坐标))
					{
						陷阱实例2.被动触发陷阱(对象);
					}
				}
				else if (this is 守卫实例 守卫实例2)
				{
					if (!守卫实例2.主动攻击(对象) || 网格距离(对象) > 守卫实例2.仇恨范围)
					{
						if (网格距离(对象) > 守卫实例2.仇恨范围)
						{
							守卫实例2.对象仇恨.移除仇恨(对象);
						}
					}
					else
					{
						守卫实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
					}
				}
			}
			else if (网格距离(对象) > 怪物实例2.仇恨范围 || !怪物实例2.主动攻击(对象) || (!怪物实例2.可见隐身目标 && 对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态)))
			{
				if (网格距离(对象) > 怪物实例2.仇恨范围 && 怪物实例2.对象仇恨.仇恨列表.TryGetValue(对象, out var value2) && value2.仇恨时间 < 主程.当前时间)
				{
					怪物实例2.对象仇恨.移除仇恨(对象);
				}
			}
			else
			{
				怪物实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
			}
		}
		if (对象 is 物品实例)
		{
			return;
		}
		if (对象 is 宠物实例 宠物实例3)
		{
			对象仇恨.仇恨详情 value3;
			if (宠物实例3.网格距离(this) <= 宠物实例3.仇恨范围 && 宠物实例3.主动攻击(this) && !检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
			{
				宠物实例3.对象仇恨.添加仇恨(this, default(DateTime), 0);
			}
			else if (宠物实例3.网格距离(this) > 宠物实例3.仇恨范围 && 宠物实例3.对象仇恨.仇恨列表.TryGetValue(this, out value3) && value3.仇恨时间 < 主程.当前时间)
			{
				宠物实例3.对象仇恨.移除仇恨(this);
			}
		}
		else if (对象 is 怪物实例 怪物实例3)
		{
			if (怪物实例3.网格距离(this) > 怪物实例3.仇恨范围 || !怪物实例3.主动攻击(this) || (!怪物实例3.可见隐身目标 && 检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态)))
			{
				if (怪物实例3.网格距离(this) > 怪物实例3.仇恨范围 && 怪物实例3.对象仇恨.仇恨列表.TryGetValue(this, out var value4) && value4.仇恨时间 < 主程.当前时间)
				{
					怪物实例3.对象仇恨.移除仇恨(this);
				}
			}
			else
			{
				怪物实例3.对象仇恨.添加仇恨(this, default(DateTime), 0);
			}
		}
		else if (!(对象 is 陷阱实例 陷阱实例3))
		{
			if (!(对象 is 守卫实例 守卫实例3))
			{
				return;
			}
			if (!守卫实例3.主动攻击(this) || 守卫实例3.网格距离(this) > 守卫实例3.仇恨范围)
			{
				if (守卫实例3.网格距离(this) > 守卫实例3.仇恨范围)
				{
					守卫实例3.对象仇恨.移除仇恨(this);
				}
			}
			else
			{
				守卫实例3.对象仇恨.添加仇恨(this, default(DateTime), 0);
			}
		}
		else if (计算类.技能范围(陷阱实例3.当前坐标, 陷阱实例3.当前方向, 陷阱实例3.对象体型).Contains(当前坐标))
		{
			陷阱实例3.被动触发陷阱(this);
		}
	}

	public void 对象出现时处理(地图对象 对象)
	{
		if (潜行邻居.Remove(对象))
		{
			if (this is 物品实例)
			{
				return;
			}
			if (this is 玩家实例 玩家实例2)
			{
				switch (对象.对象类型)
				{
				case 游戏对象类型.陷阱:
					玩家实例2.网络连接.发送封包(new 陷阱进入视野
					{
						地图编号 = 对象.地图编号,
						陷阱坐标 = 对象.当前坐标,
						陷阱高度 = 对象.当前高度,
						来源编号 = (对象 as 陷阱实例).陷阱来源.地图编号,
						陷阱编号 = (对象 as 陷阱实例).陷阱编号,
						持续时间 = (对象 as 陷阱实例).陷阱剩余时间
					});
					break;
				case 游戏对象类型.物品:
					玩家实例2.网络连接.发送封包(new 对象掉落物品
					{
						对象编号 = 对象.地图编号,
						地图编号 = 对象.地图编号,
						掉落坐标 = 对象.当前坐标,
						掉落高度 = 对象.当前高度,
						物品编号 = (对象 as 物品实例).物品编号,
						物品数量 = (对象 as 物品实例).堆叠数量
					});
					break;
				case 游戏对象类型.宠物:
					玩家实例2.网络连接.发送封包(new 对象角色停止
					{
						对象编号 = 对象.地图编号,
						对象坐标 = 对象.当前坐标,
						对象高度 = 对象.当前高度
					});
					玩家实例2.网络连接.发送封包(new 对象进入视野
					{
						出现方式 = 1,
						对象编号 = 对象.地图编号,
						现身坐标 = 对象.当前坐标,
						现身高度 = 对象.当前高度,
						现身方向 = (ushort)对象.当前方向,
						现身姿态 = (byte)((!对象.对象死亡) ? 1u : 13u),
						体力比例 = (byte)(对象.当前体力 * 100 / 对象[游戏对象属性.最大体力])
					});
					玩家实例2.网络连接.发送封包(new 同步对象体力
					{
						对象编号 = 对象.地图编号,
						当前体力 = 对象.当前体力,
						体力上限 = 对象[游戏对象属性.最大体力]
					});
					玩家实例2.网络连接.发送封包(new 对象变换类型
					{
						改变类型 = 2,
						对象编号 = 对象.地图编号
					});
					break;
				case 游戏对象类型.玩家:
				case 游戏对象类型.怪物:
				case 游戏对象类型.Npcc:
				{
					玩家实例2.网络连接.发送封包(new 对象角色停止
					{
						对象编号 = 对象.地图编号,
						对象坐标 = 对象.当前坐标,
						对象高度 = 对象.当前高度
					});
					客户网络 网络连接 = 玩家实例2.网络连接;
					对象进入视野 对象进入视野 = new 对象进入视野
					{
						出现方式 = 1,
						对象编号 = 对象.地图编号,
						现身坐标 = 对象.当前坐标,
						现身高度 = 对象.当前高度,
						现身方向 = (ushort)对象.当前方向,
						现身姿态 = (byte)((!对象.对象死亡) ? 1u : 13u),
						体力比例 = (byte)(对象.当前体力 * 100 / 对象[游戏对象属性.最大体力])
					};
					玩家实例 玩家实例3 = 对象 as 玩家实例;
					对象进入视野.补充参数 = (byte)((玩家实例3 != null && 玩家实例3.灰名玩家) ? 2u : 0u);
					网络连接.发送封包(对象进入视野);
					玩家实例2.网络连接.发送封包(new 同步对象体力
					{
						对象编号 = 对象.地图编号,
						当前体力 = 对象.当前体力,
						体力上限 = 对象[游戏对象属性.最大体力]
					});
					break;
				}
				}
				if (对象.Buff列表.Count > 0)
				{
					玩家实例2.网络连接.发送封包(new 同步对象Buff
					{
						字节描述 = 对象.对象Buff简述()
					});
				}
			}
			else if (this is 陷阱实例 陷阱实例2)
			{
				if (计算类.技能范围(陷阱实例2.当前坐标, 陷阱实例2.当前方向, 陷阱实例2.对象体型).Contains(对象.当前坐标))
				{
					陷阱实例2.被动触发陷阱(对象);
				}
			}
			else if (this is 宠物实例 宠物实例2)
			{
				对象仇恨.仇恨详情 value;
				if (网格距离(对象) <= 宠物实例2.仇恨范围 && 宠物实例2.主动攻击(对象) && !对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
				{
					宠物实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
				}
				else if (网格距离(对象) > 宠物实例2.仇恨范围 && 宠物实例2.对象仇恨.仇恨列表.TryGetValue(对象, out value) && value.仇恨时间 < 主程.当前时间)
				{
					宠物实例2.对象仇恨.移除仇恨(对象);
				}
			}
			else
			{
				if (!(this is 怪物实例 怪物实例2))
				{
					return;
				}
				if (网格距离(对象) > 怪物实例2.仇恨范围 || !怪物实例2.主动攻击(对象) || (!怪物实例2.可见隐身目标 && 对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态)))
				{
					if (网格距离(对象) > 怪物实例2.仇恨范围 && 怪物实例2.对象仇恨.仇恨列表.TryGetValue(对象, out var value2) && value2.仇恨时间 < 主程.当前时间)
					{
						怪物实例2.对象仇恨.移除仇恨(对象);
					}
				}
				else
				{
					怪物实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
				}
			}
		}
		else
		{
			if (!邻居列表.Add(对象))
			{
				return;
			}
			if (对象 is 玩家实例 || 对象 is 宠物实例)
			{
				重要邻居.Add(对象);
			}
			if (this is 物品实例)
			{
				return;
			}
			if (!(this is 玩家实例 玩家实例4))
			{
				if (!(this is 陷阱实例 陷阱实例3))
				{
					if (this is 宠物实例 宠物实例3 && !对象死亡)
					{
						if (网格距离(对象) > 宠物实例3.仇恨范围 || !宠物实例3.主动攻击(对象) || 对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
						{
							if (网格距离(对象) > 宠物实例3.仇恨范围 && 宠物实例3.对象仇恨.仇恨列表.TryGetValue(对象, out var value3) && value3.仇恨时间 < 主程.当前时间)
							{
								宠物实例3.对象仇恨.移除仇恨(对象);
							}
						}
						else
						{
							宠物实例3.对象仇恨.添加仇恨(对象, default(DateTime), 0);
						}
					}
					else if (!(this is 怪物实例 怪物实例3) || 对象死亡)
					{
						if (this is 守卫实例 守卫实例2 && !对象死亡)
						{
							if (守卫实例2.主动攻击(对象) && 网格距离(对象) <= 守卫实例2.仇恨范围)
							{
								守卫实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
							}
							else if (网格距离(对象) > 守卫实例2.仇恨范围)
							{
								守卫实例2.对象仇恨.移除仇恨(对象);
							}
							if (重要邻居.Count != 0)
							{
								守卫实例2.守卫激活处理();
							}
						}
					}
					else
					{
						对象仇恨.仇恨详情 value4;
						if (网格距离(对象) <= 怪物实例3.仇恨范围 && 怪物实例3.主动攻击(对象) && (怪物实例3.可见隐身目标 || !对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态)))
						{
							怪物实例3.对象仇恨.添加仇恨(对象, default(DateTime), 0);
						}
						else if (网格距离(对象) > 怪物实例3.仇恨范围 && 怪物实例3.对象仇恨.仇恨列表.TryGetValue(对象, out value4) && value4.仇恨时间 < 主程.当前时间)
						{
							怪物实例3.对象仇恨.移除仇恨(对象);
						}
						if (重要邻居.Count != 0)
						{
							怪物实例3.怪物激活处理();
						}
					}
				}
				else if (计算类.技能范围(陷阱实例3.当前坐标, 陷阱实例3.当前方向, 陷阱实例3.对象体型).Contains(对象.当前坐标))
				{
					陷阱实例3.被动触发陷阱(对象);
				}
				return;
			}
			switch (对象.对象类型)
			{
			case 游戏对象类型.宠物:
				玩家实例4.网络连接.发送封包(new 对象角色停止
				{
					对象编号 = 对象.地图编号,
					对象坐标 = 对象.当前坐标,
					对象高度 = 对象.当前高度
				});
				玩家实例4.网络连接.发送封包(new 对象进入视野
				{
					出现方式 = 1,
					对象编号 = 对象.地图编号,
					现身坐标 = 对象.当前坐标,
					现身高度 = 对象.当前高度,
					现身方向 = (ushort)对象.当前方向,
					现身姿态 = (byte)((!对象.对象死亡) ? 1u : 13u),
					体力比例 = (byte)(对象.当前体力 * 100 / 对象[游戏对象属性.最大体力])
				});
				玩家实例4.网络连接.发送封包(new 同步对象体力
				{
					对象编号 = 对象.地图编号,
					当前体力 = 对象.当前体力,
					体力上限 = 对象[游戏对象属性.最大体力]
				});
				玩家实例4.网络连接.发送封包(new 对象变换类型
				{
					改变类型 = 2,
					对象编号 = 对象.地图编号
				});
				break;
			case 游戏对象类型.玩家:
			case 游戏对象类型.怪物:
			case 游戏对象类型.Npcc:
			{
				玩家实例4.网络连接.发送封包(new 对象角色停止
				{
					对象编号 = 对象.地图编号,
					对象坐标 = 对象.当前坐标,
					对象高度 = 对象.当前高度
				});
				客户网络 网络连接2 = 玩家实例4.网络连接;
				对象进入视野 对象进入视野2 = new 对象进入视野
				{
					出现方式 = 1,
					对象编号 = 对象.地图编号,
					现身坐标 = 对象.当前坐标,
					现身高度 = 对象.当前高度,
					现身方向 = (ushort)对象.当前方向,
					现身姿态 = (byte)((!对象.对象死亡) ? 1u : 13u),
					体力比例 = (byte)(对象.当前体力 * 100 / 对象[游戏对象属性.最大体力])
				};
				玩家实例 玩家实例5 = 对象 as 玩家实例;
				对象进入视野2.补充参数 = (byte)((玩家实例5 != null && 玩家实例5.灰名玩家) ? 2u : 0u);
				网络连接2.发送封包(对象进入视野2);
				玩家实例4.网络连接.发送封包(new 同步对象体力
				{
					对象编号 = 对象.地图编号,
					当前体力 = 对象.当前体力,
					体力上限 = 对象[游戏对象属性.最大体力]
				});
				break;
			}
			case 游戏对象类型.陷阱:
				玩家实例4.网络连接.发送封包(new 陷阱进入视野
				{
					地图编号 = 对象.地图编号,
					陷阱坐标 = 对象.当前坐标,
					陷阱高度 = 对象.当前高度,
					来源编号 = (对象 as 陷阱实例).陷阱来源.地图编号,
					陷阱编号 = (对象 as 陷阱实例).陷阱编号,
					持续时间 = (对象 as 陷阱实例).陷阱剩余时间
				});
				break;
			case 游戏对象类型.物品:
				玩家实例4.网络连接.发送封包(new 对象掉落物品
				{
					对象编号 = 对象.地图编号,
					地图编号 = 对象.地图编号,
					掉落坐标 = 对象.当前坐标,
					掉落高度 = 对象.当前高度,
					物品编号 = (对象 as 物品实例).物品编号,
					物品数量 = (对象 as 物品实例).堆叠数量
				});
				break;
			}
			if (对象.Buff列表.Count > 0)
			{
				玩家实例4.网络连接.发送封包(new 同步对象Buff
				{
					字节描述 = 对象.对象Buff简述()
				});
			}
		}
	}

	public void 对象消失时处理(地图对象 对象)
	{
		if (!邻居列表.Remove(对象))
		{
			return;
		}
		潜行邻居.Remove(对象);
		重要邻居.Remove(对象);
		if (this is 物品实例)
		{
			return;
		}
		if (this is 玩家实例 玩家实例2)
		{
			玩家实例2.网络连接.发送封包(new 对象离开视野
			{
				对象编号 = 对象.地图编号
			});
		}
		else if (this is 宠物实例 宠物实例2)
		{
			宠物实例2.对象仇恨.移除仇恨(对象);
		}
		else if (this is 怪物实例 怪物实例2)
		{
			if (!对象死亡)
			{
				怪物实例2.对象仇恨.移除仇恨(对象);
				if (怪物实例2.重要邻居.Count == 0)
				{
					怪物实例2.怪物沉睡处理();
				}
			}
		}
		else if (this is 守卫实例 守卫实例2 && !对象死亡)
		{
			守卫实例2.对象仇恨.移除仇恨(对象);
			if (守卫实例2.重要邻居.Count == 0)
			{
				守卫实例2.守卫沉睡处理();
			}
		}
	}

	public void 对象死亡时处理(地图对象 对象)
	{
		if (this is 怪物实例 怪物实例2)
		{
			怪物实例2.对象仇恨.移除仇恨(对象);
		}
		else if (this is 宠物实例 宠物实例2)
		{
			宠物实例2.对象仇恨.移除仇恨(对象);
		}
		else if (this is 守卫实例 守卫实例2)
		{
			守卫实例2.对象仇恨.移除仇恨(对象);
		}
	}

	public void 对象隐身时处理(地图对象 对象)
	{
		if (this is 宠物实例 宠物实例2 && 宠物实例2.对象仇恨.仇恨列表.ContainsKey(对象))
		{
			宠物实例2.对象仇恨.移除仇恨(对象);
		}
		if (this is 怪物实例 怪物实例2 && 怪物实例2.对象仇恨.仇恨列表.ContainsKey(对象) && !怪物实例2.可见隐身目标)
		{
			怪物实例2.对象仇恨.移除仇恨(对象);
		}
	}

	public void 对象潜行时处理(地图对象 对象)
	{
		if (this is 宠物实例 宠物实例2)
		{
			if (宠物实例2.对象仇恨.仇恨列表.ContainsKey(对象))
			{
				宠物实例2.对象仇恨.移除仇恨(对象);
			}
			潜行邻居.Add(对象);
		}
		if (this is 怪物实例 怪物实例2 && !怪物实例2.可见隐身目标)
		{
			if (怪物实例2.对象仇恨.仇恨列表.ContainsKey(对象))
			{
				怪物实例2.对象仇恨.移除仇恨(对象);
			}
			潜行邻居.Add(对象);
		}
		if (this is 玩家实例 玩家实例2 && (对象关系(对象) == 游戏对象关系.敌对 || 对象.对象关系(this) == 游戏对象关系.敌对))
		{
			潜行邻居.Add(对象);
			玩家实例2.网络连接.发送封包(new 对象离开视野
			{
				对象编号 = 对象.地图编号
			});
		}
	}

	public void 对象显隐时处理(地图对象 对象)
	{
		if (this is 宠物实例 宠物实例2)
		{
			if (网格距离(对象) > 宠物实例2.仇恨范围 || !宠物实例2.主动攻击(对象) || 对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
			{
				if (网格距离(对象) > 宠物实例2.仇恨范围 && 宠物实例2.对象仇恨.仇恨列表.TryGetValue(对象, out var value) && value.仇恨时间 < 主程.当前时间)
				{
					宠物实例2.对象仇恨.移除仇恨(对象);
				}
			}
			else
			{
				宠物实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
			}
		}
		if (!(this is 怪物实例 怪物实例2))
		{
			return;
		}
		if (网格距离(对象) > 怪物实例2.仇恨范围 || !怪物实例2.主动攻击(对象) || (!怪物实例2.可见隐身目标 && 对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态)))
		{
			if (网格距离(对象) > 怪物实例2.仇恨范围 && 怪物实例2.对象仇恨.仇恨列表.TryGetValue(对象, out var value2) && value2.仇恨时间 < 主程.当前时间)
			{
				怪物实例2.对象仇恨.移除仇恨(对象);
			}
		}
		else
		{
			怪物实例2.对象仇恨.添加仇恨(对象, default(DateTime), 0);
		}
	}

	public void 对象显行时处理(地图对象 对象)
	{
		if (潜行邻居.Contains(对象))
		{
			对象出现时处理(对象);
		}
	}

	public byte[] 对象Buff详述()
	{
		using MemoryStream memoryStream = new MemoryStream(34);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)Buff列表.Count);
		foreach (KeyValuePair<ushort, Buff数据> item in Buff列表)
		{
			binaryWriter.Write(item.Value.Buff编号.V);
			binaryWriter.Write((int)item.Value.Buff编号.V);
			binaryWriter.Write(item.Value.当前层数.V);
			binaryWriter.Write((int)item.Value.剩余时间.V.TotalMilliseconds);
			binaryWriter.Write((int)item.Value.持续时间.V.TotalMilliseconds);
		}
		return memoryStream.ToArray();
	}

	public byte[] 对象Buff简述()
	{
		using MemoryStream memoryStream = new MemoryStream(34);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(地图编号);
		int num = 0;
		foreach (KeyValuePair<ushort, Buff数据> item in Buff列表)
		{
			binaryWriter.Write(item.Value.Buff编号.V);
			binaryWriter.Write((int)item.Value.Buff编号.V);
			if (++num >= 5)
			{
				break;
			}
		}
		return memoryStream.ToArray();
	}

	static 地图对象()
	{
	}
}
