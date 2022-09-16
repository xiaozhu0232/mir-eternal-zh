using System.Collections.Generic;
using System.IO;
using System.Linq;
using 游戏服务器.模板类;

namespace 游戏服务器.数据类;

public class 装备数据 : 物品数据
{
	public readonly 数据监视器<byte> 升级次数;

	public readonly 数据监视器<byte> 升级攻击;

	public readonly 数据监视器<byte> 升级魔法;

	public readonly 数据监视器<byte> 升级道术;

	public readonly 数据监视器<byte> 升级刺术;

	public readonly 数据监视器<byte> 升级弓术;

	public readonly 数据监视器<bool> 灵魂绑定;

	public readonly 数据监视器<byte> 祈祷次数;

	public readonly 数据监视器<sbyte> 幸运等级;

	public readonly 数据监视器<bool> 装备神佑;

	public readonly 数据监视器<byte> 神圣伤害;

	public readonly 数据监视器<ushort> 圣石数量;

	public readonly 数据监视器<bool> 双铭文栏;

	public readonly 数据监视器<byte> 当前铭栏;

	public readonly 数据监视器<int> 洗练数一;

	public readonly 数据监视器<int> 洗练数二;

	public readonly 数据监视器<byte> 物品状态;

	public readonly 列表监视器<随机属性> 随机属性;

	public readonly 列表监视器<装备孔洞颜色> 孔洞颜色;

	public readonly 字典监视器<byte, 铭文技能> 铭文技能;

	public readonly 字典监视器<byte, 游戏物品> 镶嵌灵石;

	public 游戏装备 装备模板 => base.物品模板 as 游戏装备;

	public int 装备战力
	{
		get
		{
			if (装备模板.物品分类 == 物品使用分类.武器)
			{
				int num = (int)((long)(装备模板.基础战力 * (幸运等级.V + 20)) * 1717986919L >> 32 >> 3);
				int num2 = 神圣伤害.V * 3 + 升级攻击.V * 5 + 升级魔法.V * 5 + 升级道术.V * 5 + 升级刺术.V * 5 + 升级弓术.V * 5;
				int num3 = 随机属性.Sum((随机属性 x) => x.战力加成);
				return num + num2 + num3;
			}
			int num4 = 0;
			switch (装备模板.装备套装)
			{
			case 游戏装备套装.祖玛装备:
				switch (装备模板.物品分类)
				{
				case 物品使用分类.腰带:
				case 物品使用分类.鞋子:
				case 物品使用分类.头盔:
					num4 = 2 * 升级次数.V;
					break;
				case 物品使用分类.衣服:
					num4 = 4 * 升级次数.V;
					break;
				}
				break;
			case 游戏装备套装.赤月装备:
				switch (装备模板.物品分类)
				{
				case 物品使用分类.腰带:
				case 物品使用分类.鞋子:
				case 物品使用分类.头盔:
					num4 = 4 * 升级次数.V;
					break;
				case 物品使用分类.衣服:
					num4 = 6 * 升级次数.V;
					break;
				}
				break;
			case 游戏装备套装.魔龙装备:
				switch (装备模板.物品分类)
				{
				case 物品使用分类.腰带:
				case 物品使用分类.鞋子:
				case 物品使用分类.头盔:
					num4 = 5 * 升级次数.V;
					break;
				case 物品使用分类.衣服:
					num4 = 8 * 升级次数.V;
					break;
				}
				break;
			case 游戏装备套装.苍月装备:
				switch (装备模板.物品分类)
				{
				case 物品使用分类.腰带:
				case 物品使用分类.鞋子:
				case 物品使用分类.头盔:
					num4 = 7 * 升级次数.V;
					break;
				case 物品使用分类.衣服:
					num4 = 11 * 升级次数.V;
					break;
				}
				break;
			case 游戏装备套装.星王装备:
				if (装备模板.物品分类 == 物品使用分类.衣服)
				{
					num4 = 13 * 升级次数.V;
				}
				break;
			case 游戏装备套装.神秘装备:
			case 游戏装备套装.城主装备:
				switch (装备模板.物品分类)
				{
				case 物品使用分类.腰带:
				case 物品使用分类.鞋子:
				case 物品使用分类.头盔:
					num4 = 9 * 升级次数.V;
					break;
				case 物品使用分类.衣服:
					num4 = 13 * 升级次数.V;
					break;
				}
				break;
			}
			int num5 = 孔洞颜色.Count * 10;
			using (IEnumerator<游戏物品> enumerator = 镶嵌灵石.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current.物品名字)
					{
					case "纯紫灵石6级":
					case "狂热幻彩灵石6级":
					case "韧紫灵石6级":
					case "守阳灵石6级":
					case "精绿灵石6级":
					case "新阳灵石6级":
					case "驭朱灵石6级":
					case "命朱灵石6级":
					case "盈绿灵石6级":
					case "进击幻彩灵石6级":
					case "蔚蓝灵石6级":
					case "深灰灵石6级":
					case "橙黄灵石6级":
					case "赤褐灵石6级":
					case "抵御幻彩灵石6级":
					case "透蓝灵石6级":
						num5 += 60;
						break;
					case "橙黄灵石7级":
					case "守阳灵石7级":
					case "进击幻彩灵石7级":
					case "透蓝灵石7级":
					case "蔚蓝灵石7级":
					case "新阳灵石7级":
					case "盈绿灵石7级":
					case "深灰灵石7级":
					case "驭朱灵石7级":
					case "韧紫灵石7级":
					case "精绿灵石7级":
					case "纯紫灵石7级":
					case "狂热幻彩灵石7级":
					case "赤褐灵石7级":
					case "抵御幻彩灵石7级":
					case "命朱灵石7级":
						num5 += 70;
						break;
					case "韧紫灵石10级":
					case "新阳灵石10级":
					case "纯紫灵石10级":
					case "驭朱灵石10级":
					case "深灰灵石10级":
					case "盈绿灵石10级":
					case "命朱灵石10级":
					case "守阳灵石10级":
					case "赤褐灵石10级":
					case "进击幻彩灵石10级":
					case "抵御幻彩灵石10级":
					case "橙黄灵石10级":
					case "精绿灵石10级":
					case "透蓝灵石10级":
					case "蔚蓝灵石10级":
					case "狂热幻彩灵石10级":
						num5 += 100;
						break;
					case "命朱灵石8级":
					case "蔚蓝灵石8级":
					case "赤褐灵石8级":
					case "橙黄灵石8级":
					case "深灰灵石8级":
					case "纯紫灵石8级":
					case "新阳灵石8级":
					case "守阳灵石8级":
					case "透蓝灵石8级":
					case "进击幻彩灵石8级":
					case "盈绿灵石8级":
					case "狂热幻彩灵石8级":
					case "韧紫灵石8级":
					case "精绿灵石8级":
					case "抵御幻彩灵石8级":
					case "驭朱灵石8级":
						num5 += 80;
						break;
					case "赤褐灵石3级":
					case "深灰灵石3级":
					case "狂热幻彩灵石3级":
					case "纯紫灵石3级":
					case "透蓝灵石3级":
					case "抵御幻彩灵石3级":
					case "守阳灵石3级":
					case "盈绿灵石3级":
					case "命朱灵石3级":
					case "蔚蓝灵石3级":
					case "橙黄灵石3级":
					case "进击幻彩灵石3级":
					case "新阳灵石3级":
					case "韧紫灵石3级":
					case "精绿灵石3级":
					case "驭朱灵石3级":
						num5 += 30;
						break;
					case "赤褐灵石4级":
					case "纯紫灵石4级":
					case "韧紫灵石4级":
					case "狂热幻彩灵石4级":
					case "透蓝灵石4级":
					case "守阳灵石4级":
					case "精绿灵石4级":
					case "抵御幻彩灵石4级":
					case "新阳灵石4级":
					case "盈绿灵石4级":
					case "命朱灵石4级":
					case "蔚蓝灵石4级":
					case "橙黄灵石4级":
					case "进击幻彩灵石4级":
					case "深灰灵石4级":
					case "驭朱灵石4级":
						num5 += 40;
						break;
					case "橙黄灵石1级":
					case "深灰灵石1级":
					case "命朱灵石1级":
					case "进击幻彩灵石1级":
					case "抵御幻彩灵石1级":
					case "蔚蓝灵石1级":
					case "盈绿灵石1级":
					case "新阳灵石1级":
					case "赤褐灵石1级":
					case "纯紫灵石1级":
					case "精绿灵石1级":
					case "狂热幻彩灵石1级":
					case "守阳灵石1级":
					case "韧紫灵石1级":
					case "驭朱灵石1级":
					case "透蓝灵石1级":
						num5 += 10;
						break;
					case "新阳灵石9级":
					case "盈绿灵石9级":
					case "守阳灵石9级":
					case "狂热幻彩灵石9级":
					case "进击幻彩灵石9级":
					case "韧紫灵石9级":
					case "深灰灵石9级":
					case "赤褐灵石9级":
					case "命朱灵石9级":
					case "纯紫灵石9级":
					case "透蓝灵石9级":
					case "橙黄灵石9级":
					case "抵御幻彩灵石9级":
					case "蔚蓝灵石9级":
					case "驭朱灵石9级":
					case "精绿灵石9级":
						num5 += 90;
						break;
					case "狂热幻彩灵石2级":
					case "盈绿灵石2级":
					case "韧紫灵石2级":
					case "进击幻彩灵石2级":
					case "新阳灵石2级":
					case "命朱灵石2级":
					case "深灰灵石2级":
					case "赤褐灵石2级":
					case "抵御幻彩灵石2级":
					case "守阳灵石2级":
					case "纯紫灵石2级":
					case "透蓝灵石2级":
					case "驭朱灵石2级":
					case "橙黄灵石2级":
					case "蔚蓝灵石2级":
					case "精绿灵石2级":
						num5 += 20;
						break;
					case "狂热幻彩灵石5级":
					case "纯紫灵石5级":
					case "守阳灵石5级":
					case "驭朱灵石5级":
					case "赤褐灵石5级":
					case "抵御幻彩灵石5级":
					case "韧紫灵石5级":
					case "进击幻彩灵石5级":
					case "深灰灵石5级":
					case "盈绿灵石5级":
					case "透蓝灵石5级":
					case "橙黄灵石5级":
					case "命朱灵石5级":
					case "蔚蓝灵石5级":
					case "新阳灵石5级":
					case "精绿灵石5级":
						num5 += 50;
						break;
					}
				}
			}
			int num6 = 随机属性.Sum((随机属性 x) => x.战力加成);
			return 装备模板.基础战力 + num4 + num6 + num5;
		}
	}

	public int 修理费用
	{
		get
		{
			int num = 最大持久.V - 当前持久.V;
			decimal num2 = ((游戏装备)对应模板.V).修理花费;
			decimal num3 = (decimal)((游戏装备)对应模板.V).物品持久 * 1000m;
			return (int)(num2 / num3 * (decimal)num);
		}
	}

	public int 特修费用
	{
		get
		{
			decimal num = (decimal)最大持久.V - (decimal)当前持久.V;
			decimal num2 = ((游戏装备)对应模板.V).特修花费;
			decimal num3 = (decimal)((游戏装备)对应模板.V).物品持久 * 1000m;
			return (int)(num2 / num3 * num * 自定义类.装备特修折扣 * 1.15m);
		}
	}

	public int 需要攻击 => ((游戏装备)base.物品模板).需要攻击;

	public int 需要魔法 => ((游戏装备)base.物品模板).需要魔法;

	public int 需要道术 => ((游戏装备)base.物品模板).需要道术;

	public int 需要刺术 => ((游戏装备)base.物品模板).需要刺术;

	public int 需要弓术 => ((游戏装备)base.物品模板).需要弓术;

	public string 装备名字 => base.物品模板.物品名字;

	public bool 禁止卸下 => ((游戏装备)对应模板.V).禁止卸下;

	public bool 能否修理 => base.持久类型 == 物品持久分类.装备;

	public int 传承材料 => base.物品编号 switch
	{
		99900022 => 21001, 
		99900023 => 21002, 
		99900024 => 21003, 
		99900025 => 21001, 
		99900026 => 21001, 
		99900027 => 21003, 
		99900028 => 21002, 
		99900029 => 21002, 
		99900030 => 21001, 
		99900031 => 21003, 
		99900032 => 21001, 
		99900033 => 21002, 
		99900037 => 21001, 
		99900038 => 21003, 
		99900039 => 21002, 
		99900044 => 21003, 
		99900045 => 21001, 
		99900046 => 21002, 
		99900047 => 21003, 
		99900048 => 21001, 
		99900049 => 21003, 
		99900050 => 21002, 
		99900055 => 21004, 
		99900056 => 21004, 
		99900057 => 21004, 
		99900058 => 21004, 
		99900059 => 21004, 
		99900060 => 21004, 
		99900061 => 21004, 
		99900062 => 21004, 
		99900063 => 21002, 
		99900064 => 21003, 
		99900074 => 21005, 
		99900076 => 21005, 
		99900077 => 21005, 
		99900078 => 21005, 
		99900079 => 21005, 
		99900080 => 21005, 
		99900081 => 21005, 
		99900082 => 21005, 
		99900104 => 21006, 
		99900105 => 21006, 
		99900106 => 21006, 
		99900107 => 21006, 
		99900108 => 21006, 
		99900109 => 21006, 
		99900110 => 21006, 
		99900111 => 21006, 
		_ => 0, 
	};

	public string 属性描述
	{
		get
		{
			string text = "";
			Dictionary<游戏对象属性, int> dictionary = new Dictionary<游戏对象属性, int>();
			foreach (随机属性 item in 随机属性)
			{
				dictionary[item.对应属性] = item.属性数值;
			}
			if (dictionary.ContainsKey(游戏对象属性.最小攻击) || dictionary.ContainsKey(游戏对象属性.最大攻击))
			{
				text += $"\n攻击{(dictionary.TryGetValue(游戏对象属性.最小攻击, out var value) ? value : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大攻击, out var value2) ? value2 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最小魔法) || dictionary.ContainsKey(游戏对象属性.最大魔法))
			{
				text += $"\n魔法{(dictionary.TryGetValue(游戏对象属性.最小魔法, out var value3) ? value3 : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大魔法, out var value4) ? value4 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最小道术) || dictionary.ContainsKey(游戏对象属性.最大道术))
			{
				text += $"\n道术{(dictionary.TryGetValue(游戏对象属性.最小道术, out var value5) ? value5 : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大道术, out var value6) ? value6 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最小刺术) || dictionary.ContainsKey(游戏对象属性.最大刺术))
			{
				text += $"\n刺术{(dictionary.TryGetValue(游戏对象属性.最小刺术, out var value7) ? value7 : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大刺术, out var value8) ? value8 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最小弓术) || dictionary.ContainsKey(游戏对象属性.最大弓术))
			{
				text += $"\n弓术{(dictionary.TryGetValue(游戏对象属性.最小弓术, out var value9) ? value9 : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大弓术, out var value10) ? value10 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最小防御) || dictionary.ContainsKey(游戏对象属性.最大防御))
			{
				text += $"\n防御{(dictionary.TryGetValue(游戏对象属性.最小防御, out var value11) ? value11 : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大防御, out var value12) ? value12 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最小魔防) || dictionary.ContainsKey(游戏对象属性.最大魔防))
			{
				text += $"\n魔防{(dictionary.TryGetValue(游戏对象属性.最小魔防, out var value13) ? value13 : 0)}-{(dictionary.TryGetValue(游戏对象属性.最大魔防, out var value14) ? value14 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.物理准确))
			{
				text += $"\n准确度{(dictionary.TryGetValue(游戏对象属性.物理准确, out var value15) ? value15 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.物理敏捷))
			{
				text += $"\n敏捷度{(dictionary.TryGetValue(游戏对象属性.物理敏捷, out var value16) ? value16 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最大体力))
			{
				text += $"\n体力值{(dictionary.TryGetValue(游戏对象属性.最大体力, out var value17) ? value17 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.最大魔力))
			{
				text += $"\n法力值{(dictionary.TryGetValue(游戏对象属性.最大魔力, out var value18) ? value18 : 0)}";
			}
			if (dictionary.ContainsKey(游戏对象属性.魔法闪避))
			{
				text += $"\n魔法闪避{(dictionary.TryGetValue(游戏对象属性.魔法闪避, out var value19) ? value19 : 0) / 100}%";
			}
			if (dictionary.ContainsKey(游戏对象属性.中毒躲避))
			{
				text += $"\n中毒躲避{(dictionary.TryGetValue(游戏对象属性.中毒躲避, out var value20) ? value20 : 0) / 100}%";
			}
			if (dictionary.ContainsKey(游戏对象属性.幸运等级))
			{
				text += $"\n幸运+{(dictionary.TryGetValue(游戏对象属性.幸运等级, out var value21) ? value21 : 0)}";
			}
			return text;
		}
	}

	public 铭文技能 第一铭文
	{
		get
		{
			if (当前铭栏.V == 0)
			{
				return 铭文技能[0];
			}
			return 铭文技能[2];
		}
		set
		{
			if (当前铭栏.V != 0)
			{
				铭文技能[2] = value;
			}
			else
			{
				铭文技能[0] = value;
			}
		}
	}

	public 铭文技能 第二铭文
	{
		get
		{
			if (当前铭栏.V == 0)
			{
				return 铭文技能[1];
			}
			return 铭文技能[3];
		}
		set
		{
			if (当前铭栏.V == 0)
			{
				铭文技能[1] = value;
			}
			else
			{
				铭文技能[3] = value;
			}
		}
	}

	public 铭文技能 最优铭文
	{
		get
		{
			if (当前铭栏.V != 0)
			{
				if (铭文技能[2].铭文品质 < 铭文技能[3].铭文品质)
				{
					return 铭文技能[3];
				}
				return 铭文技能[2];
			}
			if (铭文技能[0].铭文品质 >= 铭文技能[1].铭文品质)
			{
				return 铭文技能[0];
			}
			return 铭文技能[1];
		}
		set
		{
			if (当前铭栏.V == 0)
			{
				if (铭文技能[0].铭文品质 >= 铭文技能[1].铭文品质)
				{
					铭文技能[0] = value;
				}
				else
				{
					铭文技能[1] = value;
				}
			}
			else if (铭文技能[2].铭文品质 >= 铭文技能[3].铭文品质)
			{
				铭文技能[2] = value;
			}
			else
			{
				铭文技能[3] = value;
			}
		}
	}

	public 铭文技能 最差铭文
	{
		get
		{
			if (当前铭栏.V != 0)
			{
				if (铭文技能[2].铭文品质 >= 铭文技能[3].铭文品质)
				{
					return 铭文技能[3];
				}
				return 铭文技能[2];
			}
			if (铭文技能[0].铭文品质 >= 铭文技能[1].铭文品质)
			{
				return 铭文技能[1];
			}
			return 铭文技能[0];
		}
		set
		{
			if (当前铭栏.V != 0)
			{
				if (铭文技能[2].铭文品质 < 铭文技能[3].铭文品质)
				{
					铭文技能[2] = value;
				}
				else
				{
					铭文技能[3] = value;
				}
			}
			else if (铭文技能[0].铭文品质 < 铭文技能[1].铭文品质)
			{
				铭文技能[0] = value;
			}
			else
			{
				铭文技能[1] = value;
			}
		}
	}

	public int 双铭文点
	{
		get
		{
			if (当前铭栏.V != 0)
			{
				return 洗练数二.V;
			}
			return 洗练数一.V;
		}
		set
		{
			if (当前铭栏.V != 0)
			{
				洗练数二.V = value;
			}
			else
			{
				洗练数一.V = value;
			}
		}
	}

	public Dictionary<游戏对象属性, int> 装备属性
	{
		get
		{
			Dictionary<游戏对象属性, int> dictionary = new Dictionary<游戏对象属性, int>();
			if (装备模板.最小攻击 != 0)
			{
				dictionary[游戏对象属性.最小攻击] = 装备模板.最小攻击;
			}
			if (装备模板.最大攻击 != 0)
			{
				dictionary[游戏对象属性.最大攻击] = 装备模板.最大攻击;
			}
			if (装备模板.最小魔法 != 0)
			{
				dictionary[游戏对象属性.最小魔法] = 装备模板.最小魔法;
			}
			if (装备模板.最大魔法 != 0)
			{
				dictionary[游戏对象属性.最大魔法] = 装备模板.最大魔法;
			}
			if (装备模板.最小道术 != 0)
			{
				dictionary[游戏对象属性.最小道术] = 装备模板.最小道术;
			}
			if (装备模板.最大道术 != 0)
			{
				dictionary[游戏对象属性.最大道术] = 装备模板.最大道术;
			}
			if (装备模板.最小刺术 != 0)
			{
				dictionary[游戏对象属性.最小刺术] = 装备模板.最小刺术;
			}
			if (装备模板.最大刺术 != 0)
			{
				dictionary[游戏对象属性.最大刺术] = 装备模板.最大刺术;
			}
			if (装备模板.最小弓术 != 0)
			{
				dictionary[游戏对象属性.最小弓术] = 装备模板.最小弓术;
			}
			if (装备模板.最大弓术 != 0)
			{
				dictionary[游戏对象属性.最大弓术] = 装备模板.最大弓术;
			}
			if (装备模板.最小防御 != 0)
			{
				dictionary[游戏对象属性.最小防御] = 装备模板.最小防御;
			}
			if (装备模板.最大防御 != 0)
			{
				dictionary[游戏对象属性.最大防御] = 装备模板.最大防御;
			}
			if (装备模板.最小魔防 != 0)
			{
				dictionary[游戏对象属性.最小魔防] = 装备模板.最小魔防;
			}
			if (装备模板.最大魔防 != 0)
			{
				dictionary[游戏对象属性.最大魔防] = 装备模板.最大魔防;
			}
			if (装备模板.最大体力 != 0)
			{
				dictionary[游戏对象属性.最大体力] = 装备模板.最大体力;
			}
			if (装备模板.最大魔力 != 0)
			{
				dictionary[游戏对象属性.最大魔力] = 装备模板.最大魔力;
			}
			if (装备模板.攻击速度 != 0)
			{
				dictionary[游戏对象属性.攻击速度] = 装备模板.攻击速度;
			}
			if (装备模板.魔法闪避 != 0)
			{
				dictionary[游戏对象属性.魔法闪避] = 装备模板.魔法闪避;
			}
			if (装备模板.物理准确 != 0)
			{
				dictionary[游戏对象属性.物理准确] = 装备模板.物理准确;
			}
			if (装备模板.物理敏捷 != 0)
			{
				dictionary[游戏对象属性.物理敏捷] = 装备模板.物理敏捷;
			}
			if (幸运等级.V != 0)
			{
				dictionary[游戏对象属性.幸运等级] = (dictionary.ContainsKey(游戏对象属性.幸运等级) ? (dictionary[游戏对象属性.幸运等级] + 幸运等级.V) : 幸运等级.V);
			}
			if (升级攻击.V != 0)
			{
				dictionary[游戏对象属性.最大攻击] = (dictionary.ContainsKey(游戏对象属性.最大攻击) ? (dictionary[游戏对象属性.最大攻击] + 升级攻击.V) : 升级攻击.V);
			}
			if (升级魔法.V != 0)
			{
				dictionary[游戏对象属性.最大魔法] = (dictionary.ContainsKey(游戏对象属性.最大魔法) ? (dictionary[游戏对象属性.最大魔法] + 升级魔法.V) : 升级魔法.V);
			}
			if (升级道术.V != 0)
			{
				dictionary[游戏对象属性.最大道术] = (dictionary.ContainsKey(游戏对象属性.最大道术) ? (dictionary[游戏对象属性.最大道术] + 升级道术.V) : 升级道术.V);
			}
			if (升级刺术.V != 0)
			{
				dictionary[游戏对象属性.最大刺术] = (dictionary.ContainsKey(游戏对象属性.最大刺术) ? (dictionary[游戏对象属性.最大刺术] + 升级刺术.V) : 升级刺术.V);
			}
			if (升级弓术.V != 0)
			{
				dictionary[游戏对象属性.最大弓术] = (dictionary.ContainsKey(游戏对象属性.最大弓术) ? (dictionary[游戏对象属性.最大弓术] + 升级弓术.V) : 升级弓术.V);
			}
			foreach (随机属性 item in 随机属性.ToList())
			{
				dictionary[item.对应属性] = (dictionary.ContainsKey(item.对应属性) ? (dictionary[item.对应属性] + item.属性数值) : item.属性数值);
			}
			using IEnumerator<游戏物品> enumerator2 = 镶嵌灵石.Values.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				switch (enumerator2.Current.物品编号)
				{
				case 10320:
					dictionary[游戏对象属性.最大魔法] = ((!dictionary.ContainsKey(游戏对象属性.最大魔法)) ? 1 : (dictionary[游戏对象属性.最大魔法] + 1));
					break;
				case 10321:
					dictionary[游戏对象属性.最大魔法] = (dictionary.ContainsKey(游戏对象属性.最大魔法) ? (dictionary[游戏对象属性.最大魔法] + 2) : 2);
					break;
				case 10322:
					dictionary[游戏对象属性.最大魔法] = (dictionary.ContainsKey(游戏对象属性.最大魔法) ? (dictionary[游戏对象属性.最大魔法] + 3) : 3);
					break;
				case 10323:
					dictionary[游戏对象属性.最大魔法] = (dictionary.ContainsKey(游戏对象属性.最大魔法) ? (dictionary[游戏对象属性.最大魔法] + 4) : 4);
					break;
				case 10324:
					dictionary[游戏对象属性.最大魔法] = (dictionary.ContainsKey(游戏对象属性.最大魔法) ? (dictionary[游戏对象属性.最大魔法] + 5) : 5);
					break;
				case 10220:
					dictionary[游戏对象属性.最大防御] = ((!dictionary.ContainsKey(游戏对象属性.最大防御)) ? 1 : (dictionary[游戏对象属性.最大防御] + 1));
					break;
				case 10221:
					dictionary[游戏对象属性.最大防御] = (dictionary.ContainsKey(游戏对象属性.最大防御) ? (dictionary[游戏对象属性.最大防御] + 2) : 2);
					break;
				case 10222:
					dictionary[游戏对象属性.最大防御] = (dictionary.ContainsKey(游戏对象属性.最大防御) ? (dictionary[游戏对象属性.最大防御] + 3) : 3);
					break;
				case 10223:
					dictionary[游戏对象属性.最大防御] = (dictionary.ContainsKey(游戏对象属性.最大防御) ? (dictionary[游戏对象属性.最大防御] + 4) : 4);
					break;
				case 10224:
					dictionary[游戏对象属性.最大防御] = (dictionary.ContainsKey(游戏对象属性.最大防御) ? (dictionary[游戏对象属性.最大防御] + 5) : 5);
					break;
				case 10110:
					dictionary[游戏对象属性.最大道术] = ((!dictionary.ContainsKey(游戏对象属性.最大道术)) ? 1 : (dictionary[游戏对象属性.最大道术] + 1));
					break;
				case 10111:
					dictionary[游戏对象属性.最大道术] = (dictionary.ContainsKey(游戏对象属性.最大道术) ? (dictionary[游戏对象属性.最大道术] + 2) : 2);
					break;
				case 10112:
					dictionary[游戏对象属性.最大道术] = (dictionary.ContainsKey(游戏对象属性.最大道术) ? (dictionary[游戏对象属性.最大道术] + 3) : 3);
					break;
				case 10113:
					dictionary[游戏对象属性.最大道术] = (dictionary.ContainsKey(游戏对象属性.最大道术) ? (dictionary[游戏对象属性.最大道术] + 4) : 4);
					break;
				case 10114:
					dictionary[游戏对象属性.最大道术] = (dictionary.ContainsKey(游戏对象属性.最大道术) ? (dictionary[游戏对象属性.最大道术] + 5) : 5);
					break;
				case 10120:
					dictionary[游戏对象属性.最大体力] = (dictionary.ContainsKey(游戏对象属性.最大体力) ? (dictionary[游戏对象属性.最大体力] + 5) : 5);
					break;
				case 10121:
					dictionary[游戏对象属性.最大体力] = (dictionary.ContainsKey(游戏对象属性.最大体力) ? (dictionary[游戏对象属性.最大体力] + 10) : 10);
					break;
				case 10122:
					dictionary[游戏对象属性.最大体力] = (dictionary.ContainsKey(游戏对象属性.最大体力) ? (dictionary[游戏对象属性.最大体力] + 15) : 15);
					break;
				case 10123:
					dictionary[游戏对象属性.最大体力] = (dictionary.ContainsKey(游戏对象属性.最大体力) ? (dictionary[游戏对象属性.最大体力] + 20) : 20);
					break;
				case 10124:
					dictionary[游戏对象属性.最大体力] = (dictionary.ContainsKey(游戏对象属性.最大体力) ? (dictionary[游戏对象属性.最大体力] + 25) : 25);
					break;
				case 10520:
					dictionary[游戏对象属性.最大魔防] = ((!dictionary.ContainsKey(游戏对象属性.最大魔防)) ? 1 : (dictionary[游戏对象属性.最大魔防] + 1));
					break;
				case 10521:
					dictionary[游戏对象属性.最大魔防] = (dictionary.ContainsKey(游戏对象属性.最大魔防) ? (dictionary[游戏对象属性.最大魔防] + 2) : 2);
					break;
				case 10522:
					dictionary[游戏对象属性.最大魔防] = (dictionary.ContainsKey(游戏对象属性.最大魔防) ? (dictionary[游戏对象属性.最大魔防] + 3) : 3);
					break;
				case 10523:
					dictionary[游戏对象属性.最大魔防] = (dictionary.ContainsKey(游戏对象属性.最大魔防) ? (dictionary[游戏对象属性.最大魔防] + 4) : 4);
					break;
				case 10524:
					dictionary[游戏对象属性.最大魔防] = (dictionary.ContainsKey(游戏对象属性.最大魔防) ? (dictionary[游戏对象属性.最大魔防] + 5) : 5);
					break;
				case 10420:
					dictionary[游戏对象属性.最大攻击] = ((!dictionary.ContainsKey(游戏对象属性.最大攻击)) ? 1 : (dictionary[游戏对象属性.最大攻击] + 1));
					break;
				case 10421:
					dictionary[游戏对象属性.最大攻击] = (dictionary.ContainsKey(游戏对象属性.最大攻击) ? (dictionary[游戏对象属性.最大攻击] + 2) : 2);
					break;
				case 10422:
					dictionary[游戏对象属性.最大攻击] = (dictionary.ContainsKey(游戏对象属性.最大攻击) ? (dictionary[游戏对象属性.最大攻击] + 3) : 3);
					break;
				case 10423:
					dictionary[游戏对象属性.最大攻击] = (dictionary.ContainsKey(游戏对象属性.最大攻击) ? (dictionary[游戏对象属性.最大攻击] + 4) : 4);
					break;
				case 10424:
					dictionary[游戏对象属性.最大攻击] = (dictionary.ContainsKey(游戏对象属性.最大攻击) ? (dictionary[游戏对象属性.最大攻击] + 5) : 5);
					break;
				case 10720:
					dictionary[游戏对象属性.最大弓术] = ((!dictionary.ContainsKey(游戏对象属性.最大弓术)) ? 1 : (dictionary[游戏对象属性.最大弓术] + 1));
					break;
				case 10721:
					dictionary[游戏对象属性.最大弓术] = (dictionary.ContainsKey(游戏对象属性.最大弓术) ? (dictionary[游戏对象属性.最大弓术] + 2) : 2);
					break;
				case 10722:
					dictionary[游戏对象属性.最大弓术] = (dictionary.ContainsKey(游戏对象属性.最大弓术) ? (dictionary[游戏对象属性.最大弓术] + 3) : 3);
					break;
				case 10723:
					dictionary[游戏对象属性.最大弓术] = (dictionary.ContainsKey(游戏对象属性.最大弓术) ? (dictionary[游戏对象属性.最大弓术] + 4) : 4);
					break;
				case 10724:
					dictionary[游戏对象属性.最大弓术] = (dictionary.ContainsKey(游戏对象属性.最大弓术) ? (dictionary[游戏对象属性.最大弓术] + 5) : 5);
					break;
				case 10620:
					dictionary[游戏对象属性.最大刺术] = ((!dictionary.ContainsKey(游戏对象属性.最大刺术)) ? 1 : (dictionary[游戏对象属性.最大刺术] + 1));
					break;
				case 10621:
					dictionary[游戏对象属性.最大刺术] = (dictionary.ContainsKey(游戏对象属性.最大刺术) ? (dictionary[游戏对象属性.最大刺术] + 2) : 2);
					break;
				case 10622:
					dictionary[游戏对象属性.最大刺术] = (dictionary.ContainsKey(游戏对象属性.最大刺术) ? (dictionary[游戏对象属性.最大刺术] + 3) : 3);
					break;
				case 10623:
					dictionary[游戏对象属性.最大刺术] = (dictionary.ContainsKey(游戏对象属性.最大刺术) ? (dictionary[游戏对象属性.最大刺术] + 4) : 4);
					break;
				case 10624:
					dictionary[游戏对象属性.最大刺术] = (dictionary.ContainsKey(游戏对象属性.最大刺术) ? (dictionary[游戏对象属性.最大刺术] + 5) : 5);
					break;
				}
			}
			return dictionary;
		}
	}

	public int 重铸所需灵气
	{
		get
		{
			switch (base.物品类型)
			{
			default:
				return 0;
			case 物品使用分类.武器:
				return 112001;
			case 物品使用分类.衣服:
			case 物品使用分类.披风:
			case 物品使用分类.腰带:
			case 物品使用分类.鞋子:
			case 物品使用分类.护肩:
			case 物品使用分类.护腕:
			case 物品使用分类.头盔:
				return 112003;
			case 物品使用分类.项链:
			case 物品使用分类.戒指:
			case 物品使用分类.手镯:
			case 物品使用分类.勋章:
			case 物品使用分类.玉佩:
				return 112002;
			}
		}
	}

	public 装备数据()
	{
	}

	public 装备数据(游戏装备 模板, 角色数据 来源, byte 容器, byte 位置, bool 随机生成 = false)
	{
		对应模板.V = 模板;
		生成来源.V = 来源;
		物品容器.V = 容器;
		物品位置.V = 位置;
		生成时间.V = 主程.当前时间;
		物品状态.V = 1;
		最大持久.V = ((模板.持久类型 == 物品持久分类.装备) ? (模板.物品持久 * 1000) : 模板.物品持久);
		当前持久.V = ((!随机生成 || 模板.持久类型 != 物品持久分类.装备) ? 最大持久.V : 主程.随机数.Next(0, 最大持久.V));
		if (随机生成 && 模板.持久类型 == 物品持久分类.装备)
		{
			随机属性.SetValue(游戏服务器.模板类.装备属性.生成属性(base.物品类型));
		}
		游戏数据网关.装备数据表.添加数据(this, 分配索引: true);
	}

	public override byte[] 字节描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(物品数据.数据版本);
		binaryWriter.Write(生成来源.V?.数据索引.V ?? 0);
		binaryWriter.Write(计算类.时间转换(生成时间.V));
		binaryWriter.Write(对应模板.V.物品编号);
		binaryWriter.Write(物品容器.V);
		binaryWriter.Write(物品位置.V);
		binaryWriter.Write(当前持久.V);
		binaryWriter.Write(最大持久.V);
		binaryWriter.Write((byte)(base.是否绑定 ? 10u : 0u));
		int num = 256;
		num = 0x100 | 当前铭栏.V;
		if (双铭文栏.V)
		{
			num |= 0x200;
		}
		binaryWriter.Write((short)num);
		int num2 = 0;
		if (物品状态.V == 1)
		{
			if (随机属性.Count == 0)
			{
				if (神圣伤害.V != 0)
				{
					num2 |= 1;
				}
			}
			else
			{
				num2 |= 1;
			}
		}
		else
		{
			num2 |= 1;
		}
		if (随机属性.Count >= 1)
		{
			num2 |= 2;
		}
		if (随机属性.Count >= 2)
		{
			num2 |= 4;
		}
		if (随机属性.Count >= 3)
		{
			num2 |= 8;
		}
		if (随机属性.Count >= 4)
		{
			num2 |= 0x10;
		}
		if (幸运等级.V != 0)
		{
			num2 |= 0x800;
		}
		if (升级次数.V != 0)
		{
			num2 |= 0x1000;
		}
		if (孔洞颜色.Count != 0)
		{
			num2 |= 0x2000;
		}
		if (镶嵌灵石[0] != null)
		{
			num2 |= 0x4000;
		}
		if (镶嵌灵石[1] != null)
		{
			num2 |= 0x8000;
		}
		if (镶嵌灵石[2] != null)
		{
			num2 |= 0x10000;
		}
		if (镶嵌灵石[3] != null)
		{
			num2 |= 0x20000;
		}
		if (神圣伤害.V != 0)
		{
			num2 |= 0x400000;
		}
		else if (圣石数量.V != 0)
		{
			num2 |= 0x400000;
		}
		if (祈祷次数.V != 0)
		{
			num2 |= 0x800000;
		}
		if (装备神佑.V)
		{
			num2 |= 0x2000000;
		}
		binaryWriter.Write(num2);
		if (((uint)num2 & (true ? 1u : 0u)) != 0)
		{
			binaryWriter.Write(物品状态.V);
		}
		if (((uint)num2 & 2u) != 0)
		{
			binaryWriter.Write((ushort)随机属性[0].属性编号);
		}
		if (((uint)num2 & 4u) != 0)
		{
			binaryWriter.Write((ushort)随机属性[1].属性编号);
		}
		if (((uint)num2 & 8u) != 0)
		{
			binaryWriter.Write((ushort)随机属性[2].属性编号);
		}
		if (((uint)num2 & 0x10u) != 0)
		{
			binaryWriter.Write((ushort)随机属性[3].属性编号);
		}
		if (((uint)num & 0x100u) != 0)
		{
			int num3 = 0;
			if (铭文技能[0] != null)
			{
				num3 |= 1;
			}
			if (铭文技能[1] != null)
			{
				num3 |= 2;
			}
			binaryWriter.Write((short)num3);
			binaryWriter.Write(洗练数一.V * 10000);
			if (((uint)num3 & (true ? 1u : 0u)) != 0)
			{
				binaryWriter.Write(铭文技能[0].铭文索引);
			}
			if (((uint)num3 & 2u) != 0)
			{
				binaryWriter.Write(铭文技能[1].铭文索引);
			}
		}
		if (((uint)num & 0x200u) != 0)
		{
			int num4 = 0;
			if (铭文技能[2] != null)
			{
				num4 |= 1;
			}
			if (铭文技能[3] != null)
			{
				num4 |= 2;
			}
			binaryWriter.Write((short)num4);
			binaryWriter.Write(洗练数二.V * 10000);
			if (((uint)num4 & (true ? 1u : 0u)) != 0)
			{
				binaryWriter.Write(铭文技能[2].铭文索引);
			}
			if (((uint)num4 & 2u) != 0)
			{
				binaryWriter.Write(铭文技能[3].铭文索引);
			}
		}
		if (((uint)num2 & 0x800u) != 0)
		{
			binaryWriter.Write(幸运等级.V);
		}
		if (((uint)num2 & 0x1000u) != 0)
		{
			binaryWriter.Write(升级次数.V);
			binaryWriter.Write((byte)0);
			binaryWriter.Write(升级攻击.V);
			binaryWriter.Write(升级魔法.V);
			binaryWriter.Write(升级道术.V);
			binaryWriter.Write(升级刺术.V);
			binaryWriter.Write(升级弓术.V);
			binaryWriter.Write(new byte[3]);
			binaryWriter.Write(灵魂绑定.V);
		}
		if (((uint)num2 & 0x2000u) != 0)
		{
			binaryWriter.Write(new byte[4]
			{
				(byte)孔洞颜色[0],
				(byte)孔洞颜色[1],
				(byte)孔洞颜色[2],
				(byte)孔洞颜色[3]
			});
		}
		if (((uint)num2 & 0x4000u) != 0)
		{
			binaryWriter.Write(镶嵌灵石[0].物品编号);
		}
		if (((uint)num2 & 0x8000u) != 0)
		{
			binaryWriter.Write(镶嵌灵石[1].物品编号);
		}
		if (((uint)num2 & 0x10000u) != 0)
		{
			binaryWriter.Write(镶嵌灵石[2].物品编号);
		}
		if (((uint)num2 & 0x20000u) != 0)
		{
			binaryWriter.Write(镶嵌灵石[3].物品编号);
		}
		if (((uint)num2 & 0x80000u) != 0)
		{
			binaryWriter.Write(0);
		}
		if (((uint)num2 & 0x100000u) != 0)
		{
			binaryWriter.Write(0);
		}
		if (((uint)num2 & 0x200000u) != 0)
		{
			binaryWriter.Write(0);
		}
		if (((uint)num2 & 0x400000u) != 0)
		{
			binaryWriter.Write(神圣伤害.V);
			binaryWriter.Write(圣石数量.V);
		}
		if (((uint)num2 & 0x800000u) != 0)
		{
			binaryWriter.Write((int)祈祷次数.V);
		}
		if (((uint)num2 & 0x2000000u) != 0)
		{
			binaryWriter.Write(装备神佑.V);
		}
		if (((uint)num2 & 0x4000000u) != 0)
		{
			binaryWriter.Write(0);
		}
		return memoryStream.ToArray();
	}

	static 装备数据()
	{
	}
}
