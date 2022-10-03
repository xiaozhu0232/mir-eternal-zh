using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameServer.Templates;

namespace GameServer.Data
{

    public class EquipmentData : ItemData
    {

        public EquipmentItem 装备模板
        {
            get
            {
                return base.物品模板 as EquipmentItem;
            }
        }


        public int 装备战力
        {
            get
            {
                if (装备模板.物品分类 == 物品使用分类.武器)
                {
                    int num = (int)(装备模板.基础战力 * (Luck.V + 20) * 1717986919L >> 32 >> 3);
                    int num2 = Sacred伤害.V * 3 + 升级Attack.V * 5 + 升级Magic.V * 5 + 升级Taoism.V * 5 + 升级Needle.V * 5 + 升级Archery.V * 5;
                    int num3 = 随机属性.Sum((随机属性 x) => x.战力加成);
                    return num + num2 + num3;
                }
                int num4 = 0;
                switch (装备模板.装备套装)
                {
                    case GameEquipmentSet.祖玛装备:
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
                    case GameEquipmentSet.赤月装备:
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
                    case GameEquipmentSet.魔龙装备:
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
                    case GameEquipmentSet.苍月装备:
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
                    case GameEquipmentSet.星王装备:
                        if (装备模板.物品分类 == 物品使用分类.衣服)
                        {
                            num4 = 13 * 升级次数.V;
                        }
                        break;
                    case GameEquipmentSet.神秘装备:
                    case GameEquipmentSet.城主装备:
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
                using (IEnumerator<GameItems> enumerator = 镶嵌灵石.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        switch (enumerator.Current.物品名字)
                        {
                            case "驭朱灵石8级":
                            case "精绿灵石8级":
                            case "韧紫灵石8级":
                            case "抵御幻彩灵石8级":
                            case "进击幻彩灵石8级":
                            case "盈绿灵石8级":
                            case "狂热幻彩灵石8级":
                            case "透蓝灵石8级":
                            case "守阳灵石8级":
                            case "新阳灵石8级":
                            case "命朱灵石8级":
                            case "蔚蓝灵石8级":
                            case "赤褐灵石8级":
                            case "橙黄灵石8级":
                            case "纯紫灵石8级":
                            case "深灰灵石8级":
                                num5 += 80;
                                break;
                            case "精绿灵石5级":
                            case "新阳灵石5级":
                            case "命朱灵石5级":
                            case "蔚蓝灵石5级":
                            case "橙黄灵石5级":
                            case "进击幻彩灵石5级":
                            case "深灰灵石5级":
                            case "盈绿灵石5级":
                            case "透蓝灵石5级":
                            case "韧紫灵石5级":
                            case "抵御幻彩灵石5级":
                            case "驭朱灵石5级":
                            case "赤褐灵石5级":
                            case "守阳灵石5级":
                            case "狂热幻彩灵石5级":
                            case "纯紫灵石5级":
                                num5 += 50;
                                break;
                            case "精绿灵石2级":
                            case "蔚蓝灵石2级":
                            case "驭朱灵石2级":
                            case "橙黄灵石2级":
                            case "守阳灵石2级":
                            case "纯紫灵石2级":
                            case "透蓝灵石2级":
                            case "抵御幻彩灵石2级":
                            case "命朱灵石2级":
                            case "深灰灵石2级":
                            case "赤褐灵石2级":
                            case "新阳灵石2级":
                            case "进击幻彩灵石2级":
                            case "狂热幻彩灵石2级":
                            case "盈绿灵石2级":
                            case "韧紫灵石2级":
                                num5 += 20;
                                break;
                            case "抵御幻彩灵石7级":
                            case "命朱灵石7级":
                            case "赤褐灵石7级":
                            case "狂热幻彩灵石7级":
                            case "精绿灵石7级":
                            case "纯紫灵石7级":
                            case "韧紫灵石7级":
                            case "驭朱灵石7级":
                            case "深灰灵石7级":
                            case "盈绿灵石7级":
                            case "新阳灵石7级":
                            case "蔚蓝灵石7级":
                            case "橙黄灵石7级":
                            case "守阳灵石7级":
                            case "进击幻彩灵石7级":
                            case "透蓝灵石7级":
                                num5 += 70;
                                break;
                            case "精绿灵石9级":
                            case "驭朱灵石9级":
                            case "蔚蓝灵石9级":
                            case "橙黄灵石9级":
                            case "抵御幻彩灵石9级":
                            case "透蓝灵石9级":
                            case "纯紫灵石9级":
                            case "命朱灵石9级":
                            case "赤褐灵石9级":
                            case "深灰灵石9级":
                            case "守阳灵石9级":
                            case "新阳灵石9级":
                            case "盈绿灵石9级":
                            case "进击幻彩灵石9级":
                            case "狂热幻彩灵石9级":
                            case "韧紫灵石9级":
                                num5 += 90;
                                break;
                            case "驭朱灵石4级":
                            case "深灰灵石4级":
                            case "新阳灵石4级":
                            case "盈绿灵石4级":
                            case "蔚蓝灵石4级":
                            case "命朱灵石4级":
                            case "橙黄灵石4级":
                            case "进击幻彩灵石4级":
                            case "抵御幻彩灵石4级":
                            case "透蓝灵石4级":
                            case "守阳灵石4级":
                            case "精绿灵石4级":
                            case "赤褐灵石4级":
                            case "纯紫灵石4级":
                            case "韧紫灵石4级":
                            case "狂热幻彩灵石4级":
                                num5 += 40;
                                break;
                            case "透蓝灵石6级":
                            case "抵御幻彩灵石6级":
                            case "命朱灵石6级":
                            case "盈绿灵石6级":
                            case "深灰灵石6级":
                            case "蔚蓝灵石6级":
                            case "进击幻彩灵石6级":
                            case "橙黄灵石6级":
                            case "赤褐灵石6级":
                            case "驭朱灵石6级":
                            case "精绿灵石6级":
                            case "新阳灵石6级":
                            case "韧紫灵石6级":
                            case "守阳灵石6级":
                            case "纯紫灵石6级":
                            case "狂热幻彩灵石6级":
                                num5 += 60;
                                break;
                            case "透蓝灵石1级":
                            case "驭朱灵石1级":
                            case "韧紫灵石1级":
                            case "守阳灵石1级":
                            case "赤褐灵石1级":
                            case "纯紫灵石1级":
                            case "狂热幻彩灵石1级":
                            case "精绿灵石1级":
                            case "新阳灵石1级":
                            case "盈绿灵石1级":
                            case "蔚蓝灵石1级":
                            case "橙黄灵石1级":
                            case "深灰灵石1级":
                            case "命朱灵石1级":
                            case "进击幻彩灵石1级":
                            case "抵御幻彩灵石1级":
                                num5 += 10;
                                break;
                            case "蔚蓝灵石10级":
                            case "狂热幻彩灵石10级":
                            case "精绿灵石10级":
                            case "透蓝灵石10级":
                            case "橙黄灵石10级":
                            case "抵御幻彩灵石10级":
                            case "进击幻彩灵石10级":
                            case "命朱灵石10级":
                            case "守阳灵石10级":
                            case "赤褐灵石10级":
                            case "盈绿灵石10级":
                            case "深灰灵石10级":
                            case "韧紫灵石10级":
                            case "纯紫灵石10级":
                            case "新阳灵石10级":
                            case "驭朱灵石10级":
                                num5 += 100;
                                break;
                            case "驭朱灵石3级":
                            case "韧紫灵石3级":
                            case "精绿灵石3级":
                            case "新阳灵石3级":
                            case "守阳灵石3级":
                            case "盈绿灵石3级":
                            case "蔚蓝灵石3级":
                            case "命朱灵石3级":
                            case "橙黄灵石3级":
                            case "进击幻彩灵石3级":
                            case "抵御幻彩灵石3级":
                            case "透蓝灵石3级":
                            case "赤褐灵石3级":
                            case "深灰灵石3级":
                            case "狂热幻彩灵石3级":
                            case "纯紫灵石3级":
                                num5 += 30;
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
                int value = 最大持久.V - 当前持久.V;
                decimal d = ((EquipmentItem)对应模板.V).特修花费;
                decimal d2 = ((EquipmentItem)对应模板.V).物品持久 * 1000m;
                return (int)(d / d2 * value);
            }
        }


        public int 特修费用
        {
            get
            {
                decimal d = 最大持久.V - 当前持久.V;
                decimal d2 = ((EquipmentItem)对应模板.V).特修花费;
                decimal d3 = ((EquipmentItem)对应模板.V).物品持久 * 1000m;
                return (int)(d2 / d3 * d * Config.EquipRepairDto * 1.15m);
            }
        }


        public int NeedAttack
        {
            get
            {
                return ((EquipmentItem)base.物品模板).需要攻击;
            }
        }


        public int NeedMagic
        {
            get
            {
                return ((EquipmentItem)base.物品模板).需要魔法;
            }
        }


        public int NeedTaoism
        {
            get
            {
                return ((EquipmentItem)base.物品模板).需要道术;
            }
        }


        public int NeedAcupuncture  //需要刺术
        {
            get
            {
                return ((EquipmentItem)base.物品模板).需要刺术;
            }
        }


        public int NeedArchery  //需要弓术
        {
            get
            {
                return ((EquipmentItem)base.物品模板).需要弓术;
            }
        }


        public string 装备名字
        {
            get
            {
                return base.物品模板.物品名字;
            }
        }


        public bool DisableDismount  //禁止卸下
        {
            get
            {
                return ((EquipmentItem)对应模板.V).禁止卸下;
            }
        }


        public bool CanRepair
        {
            get
            {
                return base.PersistType == PersistentItemType.装备;
            }
        }


        public int 传承材料
        {
            get
            {
                switch (base.Id)
                {
                    case 99900022:
                        return 21001;
                    case 99900023:
                        return 21002;
                    case 99900024:
                        return 21003;
                    case 99900025:
                        return 21001;
                    case 99900026:
                        return 21001;
                    case 99900027:
                        return 21003;
                    case 99900028:
                        return 21002;
                    case 99900029:
                        return 21002;
                    case 99900030:
                        return 21001;
                    case 99900031:
                        return 21003;
                    case 99900032:
                        return 21001;
                    case 99900033:
                        return 21002;
                    case 99900037:
                        return 21001;
                    case 99900038:
                        return 21003;
                    case 99900039:
                        return 21002;
                    case 99900044:
                        return 21003;
                    case 99900045:
                        return 21001;
                    case 99900046:
                        return 21002;
                    case 99900047:
                        return 21003;
                    case 99900048:
                        return 21001;
                    case 99900049:
                        return 21003;
                    case 99900050:
                        return 21002;
                    case 99900055:
                        return 21004;
                    case 99900056:
                        return 21004;
                    case 99900057:
                        return 21004;
                    case 99900058:
                        return 21004;
                    case 99900059:
                        return 21004;
                    case 99900060:
                        return 21004;
                    case 99900061:
                        return 21004;
                    case 99900062:
                        return 21004;
                    case 99900063:
                        return 21002;
                    case 99900064:
                        return 21003;
                    case 99900074:
                        return 21005;
                    case 99900076:
                        return 21005;
                    case 99900077:
                        return 21005;
                    case 99900078:
                        return 21005;
                    case 99900079:
                        return 21005;
                    case 99900080:
                        return 21005;
                    case 99900081:
                        return 21005;
                    case 99900082:
                        return 21005;
                    case 99900104:
                        return 21006;
                    case 99900105:
                        return 21006;
                    case 99900106:
                        return 21006;
                    case 99900107:
                        return 21006;
                    case 99900108:
                        return 21006;
                    case 99900109:
                        return 21006;
                    case 99900110:
                        return 21006;
                    case 99900111:
                        return 21006;
                }
                return 0;
            }
        }


        public string StatDescription  //属性描述
        {
            get
            {
                string text = "";
                Dictionary<GameObjectStats, int> dictionary = new Dictionary<GameObjectStats, int>();
                foreach (随机属性 随机属性 in 随机属性)
                {
                    dictionary[随机属性.对应属性] = 随机属性.属性数值;
                }
                if (dictionary.ContainsKey(GameObjectStats.最小攻击) || dictionary.ContainsKey(GameObjectStats.最大攻击))
                {
                    int num;
                    int num2;
                    text += string.Format("\nAttack{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小攻击, out num) ? num : 0, dictionary.TryGetValue(GameObjectStats.最大攻击, out num2) ? num2 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最小魔法) || dictionary.ContainsKey(GameObjectStats.最大魔法))
                {
                    int num3;
                    int num4;
                    text += string.Format("\nMagic{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小魔法, out num3) ? num3 : 0, dictionary.TryGetValue(GameObjectStats.最大魔法, out num4) ? num4 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最小道术) || dictionary.ContainsKey(GameObjectStats.最大道术))
                {
                    int num5;
                    int num6;
                    text += string.Format("\nTaoism{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小道术, out num5) ? num5 : 0, dictionary.TryGetValue(GameObjectStats.最大道术, out num6) ? num6 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最小刺术) || dictionary.ContainsKey(GameObjectStats.最大刺术))
                {
                    int num7;
                    int num8;
                    text += string.Format("\nNeedle{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小刺术, out num7) ? num7 : 0, dictionary.TryGetValue(GameObjectStats.最大刺术, out num8) ? num8 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最小弓术) || dictionary.ContainsKey(GameObjectStats.最大弓术))
                {
                    int num9;
                    int num10;
                    text += string.Format("\nArchery{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小弓术, out num9) ? num9 : 0, dictionary.TryGetValue(GameObjectStats.最大弓术, out num10) ? num10 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最小防御) || dictionary.ContainsKey(GameObjectStats.最大防御))
                {
                    int num11;
                    int num12;
                    text += string.Format("\nDefence{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小防御, out num11) ? num11 : 0, dictionary.TryGetValue(GameObjectStats.最大防御, out num12) ? num12 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最小魔防) || dictionary.ContainsKey(GameObjectStats.最大魔防))
                {
                    int num13;
                    int num14;
                    text += string.Format("\nMagic Defense{0}-{1}", dictionary.TryGetValue(GameObjectStats.最小魔防, out num13) ? num13 : 0, dictionary.TryGetValue(GameObjectStats.最大魔防, out num14) ? num14 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.物理准确))
                {
                    int num15;
                    text += string.Format("\nAccuracy{0}", dictionary.TryGetValue(GameObjectStats.物理准确, out num15) ? num15 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.物理敏捷))
                {
                    int num16;
                    text += string.Format("\nAgility{0}", dictionary.TryGetValue(GameObjectStats.物理敏捷, out num16) ? num16 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最大体力))
                {
                    int num17;
                    text += string.Format("\nStamina{0}", dictionary.TryGetValue(GameObjectStats.最大体力, out num17) ? num17 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.最大魔力))
                {
                    int num18;
                    text += string.Format("\nMana{0}", dictionary.TryGetValue(GameObjectStats.最大魔力, out num18) ? num18 : 0);
                }
                if (dictionary.ContainsKey(GameObjectStats.魔法闪避))
                {
                    int num19;
                    text += string.Format("\nMagicDodge{0}%", (dictionary.TryGetValue(GameObjectStats.魔法闪避, out num19) ? num19 : 0) / 100);
                }
                if (dictionary.ContainsKey(GameObjectStats.中毒躲避))
                {
                    int num20;
                    text += string.Format("\nPoisoning evasion{0}%", (dictionary.TryGetValue(GameObjectStats.中毒躲避, out num20) ? num20 : 0) / 100);
                }
                if (dictionary.ContainsKey(GameObjectStats.幸运等级))
                {
                    int num21;
                    text += string.Format("\nLuck+{0}", dictionary.TryGetValue(GameObjectStats.幸运等级, out num21) ? num21 : 0);
                }
                return text;
            }
        }


        public InscriptionSkill 第一铭文
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
                if (当前铭栏.V == 0)
                {
                    铭文技能[0] = value;
                    return;
                }
                铭文技能[2] = value;
            }
        }


        public InscriptionSkill 第二铭文
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
                    return;
                }
                铭文技能[3] = value;
            }
        }


        public InscriptionSkill 最优铭文
        {
            get
            {
                if (当前铭栏.V == 0)
                {
                    if (铭文技能[0].铭文品质 < 铭文技能[1].铭文品质)
                    {
                        return 铭文技能[1];
                    }
                    return 铭文技能[0];
                }
                else
                {
                    if (铭文技能[2].铭文品质 < 铭文技能[3].铭文品质)
                    {
                        return 铭文技能[3];
                    }
                    return 铭文技能[2];
                }
            }
            set
            {
                if (当前铭栏.V == 0)
                {
                    if (铭文技能[0].铭文品质 >= 铭文技能[1].铭文品质)
                    {
                        铭文技能[0] = value;
                        return;
                    }
                    铭文技能[1] = value;
                    return;
                }
                else
                {
                    if (铭文技能[2].铭文品质 >= 铭文技能[3].铭文品质)
                    {
                        铭文技能[2] = value;
                        return;
                    }
                    铭文技能[3] = value;
                    return;
                }
            }
        }


        public InscriptionSkill 最差铭文
        {
            get
            {
                if (当前铭栏.V == 0)
                {
                    if (铭文技能[0].铭文品质 >= 铭文技能[1].铭文品质)
                    {
                        return 铭文技能[1];
                    }
                    return 铭文技能[0];
                }
                else
                {
                    if (铭文技能[2].铭文品质 >= 铭文技能[3].铭文品质)
                    {
                        return 铭文技能[3];
                    }
                    return 铭文技能[2];
                }
            }
            set
            {
                if (当前铭栏.V == 0)
                {
                    if (铭文技能[0].铭文品质 < 铭文技能[1].铭文品质)
                    {
                        铭文技能[0] = value;
                        return;
                    }
                    铭文技能[1] = value;
                    return;
                }
                else
                {
                    if (铭文技能[2].铭文品质 < 铭文技能[3].铭文品质)
                    {
                        铭文技能[2] = value;
                        return;
                    }
                    铭文技能[3] = value;
                    return;
                }
            }
        }


        public int 双铭文点
        {
            get
            {
                if (当前铭栏.V == 0)
                {
                    return 洗练数一.V;
                }
                return 洗练数二.V;
            }
            set
            {
                if (当前铭栏.V == 0)
                {
                    洗练数一.V = value;
                    return;
                }
                洗练数二.V = value;
            }
        }


        public Dictionary<GameObjectStats, int> 装备Stat
        {
            get
            {
                Dictionary<GameObjectStats, int> dictionary = new Dictionary<GameObjectStats, int>();
                if (装备模板.最小攻击 != 0)
                {
                    dictionary[GameObjectStats.最小攻击] = 装备模板.最小攻击;
                }
                if (装备模板.最大攻击 != 0)
                {
                    dictionary[GameObjectStats.最大攻击] = 装备模板.最大攻击;
                }
                if (装备模板.最小魔法 != 0)
                {
                    dictionary[GameObjectStats.最小魔法] = 装备模板.最小魔法;
                }
                if (装备模板.最大魔法 != 0)
                {
                    dictionary[GameObjectStats.最大魔法] = 装备模板.最大魔法;
                }
                if (装备模板.最小道术 != 0)
                {
                    dictionary[GameObjectStats.最小道术] = 装备模板.最小道术;
                }
                if (装备模板.最大道术 != 0)
                {
                    dictionary[GameObjectStats.最大道术] = 装备模板.最大道术;
                }
                if (装备模板.最小刺术 != 0)
                {
                    dictionary[GameObjectStats.最小刺术] = 装备模板.最小刺术;
                }
                if (装备模板.最大刺术 != 0)
                {
                    dictionary[GameObjectStats.最大刺术] = 装备模板.最大刺术;
                }
                if (装备模板.最小弓术 != 0)
                {
                    dictionary[GameObjectStats.最小弓术] = 装备模板.最小弓术;
                }
                if (装备模板.最大弓术 != 0)
                {
                    dictionary[GameObjectStats.最大弓术] = 装备模板.最大弓术;
                }
                if (装备模板.最小防御 != 0)
                {
                    dictionary[GameObjectStats.最小防御] = 装备模板.最小防御;
                }
                if (装备模板.最大防御 != 0)
                {
                    dictionary[GameObjectStats.最大防御] = 装备模板.最大防御;
                }
                if (装备模板.最小魔防 != 0)
                {
                    dictionary[GameObjectStats.最小魔防] = 装备模板.最小魔防;
                }
                if (装备模板.最大魔防 != 0)
                {
                    dictionary[GameObjectStats.最大魔防] = 装备模板.最大魔防;
                }
                if (装备模板.最大体力 != 0)
                {
                    dictionary[GameObjectStats.最大体力] = 装备模板.最大体力;
                }
                if (装备模板.最大魔力 != 0)
                {
                    dictionary[GameObjectStats.最大魔力] = 装备模板.最大魔力;
                }
                if (装备模板.攻击速度 != 0)
                {
                    dictionary[GameObjectStats.攻击速度] = 装备模板.攻击速度;
                }
                if (装备模板.魔法闪避 != 0)
                {
                    dictionary[GameObjectStats.魔法闪避] = 装备模板.魔法闪避;
                }
                if (装备模板.物理准确 != 0)
                {
                    dictionary[GameObjectStats.物理准确] = 装备模板.物理准确;
                }
                if (装备模板.物理敏捷 != 0)
                {
                    dictionary[GameObjectStats.物理敏捷] = 装备模板.物理敏捷;
                }
                if (Luck.V != 0)
                {
                    dictionary[GameObjectStats.幸运等级] = (dictionary.ContainsKey(GameObjectStats.幸运等级) ? (dictionary[GameObjectStats.幸运等级] + (int)Luck.V) : ((int)Luck.V));
                }
                if (升级Attack.V != 0)
                {
                    dictionary[GameObjectStats.最大攻击] = (dictionary.ContainsKey(GameObjectStats.最大攻击) ? (dictionary[GameObjectStats.最大攻击] + (int)升级Attack.V) : ((int)升级Attack.V));
                }
                if (升级Magic.V != 0)
                {
                    dictionary[GameObjectStats.最大魔法] = (dictionary.ContainsKey(GameObjectStats.最大魔法) ? (dictionary[GameObjectStats.最大魔法] + (int)升级Magic.V) : ((int)升级Magic.V));
                }
                if (升级Taoism.V != 0)
                {
                    dictionary[GameObjectStats.最大道术] = (dictionary.ContainsKey(GameObjectStats.最大道术) ? (dictionary[GameObjectStats.最大道术] + (int)升级Taoism.V) : ((int)升级Taoism.V));
                }
                if (升级Needle.V != 0)
                {
                    dictionary[GameObjectStats.最大刺术] = (dictionary.ContainsKey(GameObjectStats.最大刺术) ? (dictionary[GameObjectStats.最大刺术] + (int)升级Needle.V) : ((int)升级Needle.V));
                }
                if (升级Archery.V != 0)
                {
                    dictionary[GameObjectStats.最大弓术] = (dictionary.ContainsKey(GameObjectStats.最大弓术) ? (dictionary[GameObjectStats.最大弓术] + (int)升级Archery.V) : ((int)升级Archery.V));
                }
                foreach (随机属性 随机属性 in 随机属性.ToList<随机属性>())
                {
                    dictionary[随机属性.对应属性] = (dictionary.ContainsKey(随机属性.对应属性) ? (dictionary[随机属性.对应属性] + 随机属性.属性数值) : 随机属性.属性数值);
                }
                foreach (GameItems GameItems in 镶嵌灵石.Values)
                {
                    int Id = GameItems.物品编号;
                    if (Id <= 10324)
                    {
                        switch (Id)
                        {
                            case 10110:
                                dictionary[GameObjectStats.最大道术] = (dictionary.ContainsKey(GameObjectStats.最大道术) ? (dictionary[GameObjectStats.最大道术] + 1) : 1);
                                break;
                            case 10111:
                                dictionary[GameObjectStats.最大道术] = (dictionary.ContainsKey(GameObjectStats.最大道术) ? (dictionary[GameObjectStats.最大道术] + 2) : 2);
                                break;
                            case 10112:
                                dictionary[GameObjectStats.最大道术] = (dictionary.ContainsKey(GameObjectStats.最大道术) ? (dictionary[GameObjectStats.最大道术] + 3) : 3);
                                break;
                            case 10113:
                                dictionary[GameObjectStats.最大道术] = (dictionary.ContainsKey(GameObjectStats.最大道术) ? (dictionary[GameObjectStats.最大道术] + 4) : 4);
                                break;
                            case 10114:
                                dictionary[GameObjectStats.最大道术] = (dictionary.ContainsKey(GameObjectStats.最大道术) ? (dictionary[GameObjectStats.最大道术] + 5) : 5);
                                break;
                            case 10115:
                            case 10116:
                            case 10117:
                            case 10118:
                            case 10119:
                                break;
                            case 10120:
                                dictionary[GameObjectStats.最大体力] = (dictionary.ContainsKey(GameObjectStats.最大体力) ? (dictionary[GameObjectStats.最大体力] + 5) : 5);
                                break;
                            case 10121:
                                dictionary[GameObjectStats.最大体力] = (dictionary.ContainsKey(GameObjectStats.最大体力) ? (dictionary[GameObjectStats.最大体力] + 10) : 10);
                                break;
                            case 10122:
                                dictionary[GameObjectStats.最大体力] = (dictionary.ContainsKey(GameObjectStats.最大体力) ? (dictionary[GameObjectStats.最大体力] + 15) : 15);
                                break;
                            case 10123:
                                dictionary[GameObjectStats.最大体力] = (dictionary.ContainsKey(GameObjectStats.最大体力) ? (dictionary[GameObjectStats.最大体力] + 20) : 20);
                                break;
                            case 10124:
                                dictionary[GameObjectStats.最大体力] = (dictionary.ContainsKey(GameObjectStats.最大体力) ? (dictionary[GameObjectStats.最大体力] + 25) : 25);
                                break;
                            default:
                                switch (Id)
                                {
                                    case 10220:
                                        dictionary[GameObjectStats.最大防御] = (dictionary.ContainsKey(GameObjectStats.最大防御) ? (dictionary[GameObjectStats.最大防御] + 1) : 1);
                                        break;
                                    case 10221:
                                        dictionary[GameObjectStats.最大防御] = (dictionary.ContainsKey(GameObjectStats.最大防御) ? (dictionary[GameObjectStats.最大防御] + 2) : 2);
                                        break;
                                    case 10222:
                                        dictionary[GameObjectStats.最大防御] = (dictionary.ContainsKey(GameObjectStats.最大防御) ? (dictionary[GameObjectStats.最大防御] + 3) : 3);
                                        break;
                                    case 10223:
                                        dictionary[GameObjectStats.最大防御] = (dictionary.ContainsKey(GameObjectStats.最大防御) ? (dictionary[GameObjectStats.最大防御] + 4) : 4);
                                        break;
                                    case 10224:
                                        dictionary[GameObjectStats.最大防御] = (dictionary.ContainsKey(GameObjectStats.最大防御) ? (dictionary[GameObjectStats.最大防御] + 5) : 5);
                                        break;
                                    default:
                                        switch (Id)
                                        {
                                            case 10320:
                                                dictionary[GameObjectStats.最大魔法] = (dictionary.ContainsKey(GameObjectStats.最大魔法) ? (dictionary[GameObjectStats.最大魔法] + 1) : 1);
                                                break;
                                            case 10321:
                                                dictionary[GameObjectStats.最大魔法] = (dictionary.ContainsKey(GameObjectStats.最大魔法) ? (dictionary[GameObjectStats.最大魔法] + 2) : 2);
                                                break;
                                            case 10322:
                                                dictionary[GameObjectStats.最大魔法] = (dictionary.ContainsKey(GameObjectStats.最大魔法) ? (dictionary[GameObjectStats.最大魔法] + 3) : 3);
                                                break;
                                            case 10323:
                                                dictionary[GameObjectStats.最大魔法] = (dictionary.ContainsKey(GameObjectStats.最大魔法) ? (dictionary[GameObjectStats.最大魔法] + 4) : 4);
                                                break;
                                            case 10324:
                                                dictionary[GameObjectStats.最大魔法] = (dictionary.ContainsKey(GameObjectStats.最大魔法) ? (dictionary[GameObjectStats.最大魔法] + 5) : 5);
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                    else if (Id <= 10524)
                    {
                        switch (Id)
                        {
                            case 10420:
                                dictionary[GameObjectStats.最大攻击] = (dictionary.ContainsKey(GameObjectStats.最大攻击) ? (dictionary[GameObjectStats.最大攻击] + 1) : 1);
                                break;
                            case 10421:
                                dictionary[GameObjectStats.最大攻击] = (dictionary.ContainsKey(GameObjectStats.最大攻击) ? (dictionary[GameObjectStats.最大攻击] + 2) : 2);
                                break;
                            case 10422:
                                dictionary[GameObjectStats.最大攻击] = (dictionary.ContainsKey(GameObjectStats.最大攻击) ? (dictionary[GameObjectStats.最大攻击] + 3) : 3);
                                break;
                            case 10423:
                                dictionary[GameObjectStats.最大攻击] = (dictionary.ContainsKey(GameObjectStats.最大攻击) ? (dictionary[GameObjectStats.最大攻击] + 4) : 4);
                                break;
                            case 10424:
                                dictionary[GameObjectStats.最大攻击] = (dictionary.ContainsKey(GameObjectStats.最大攻击) ? (dictionary[GameObjectStats.最大攻击] + 5) : 5);
                                break;
                            default:
                                switch (Id)
                                {
                                    case 10520:
                                        dictionary[GameObjectStats.最大魔防] = (dictionary.ContainsKey(GameObjectStats.最大魔防) ? (dictionary[GameObjectStats.最大魔防] + 1) : 1);
                                        break;
                                    case 10521:
                                        dictionary[GameObjectStats.最大魔防] = (dictionary.ContainsKey(GameObjectStats.最大魔防) ? (dictionary[GameObjectStats.最大魔防] + 2) : 2);
                                        break;
                                    case 10522:
                                        dictionary[GameObjectStats.最大魔防] = (dictionary.ContainsKey(GameObjectStats.最大魔防) ? (dictionary[GameObjectStats.最大魔防] + 3) : 3);
                                        break;
                                    case 10523:
                                        dictionary[GameObjectStats.最大魔防] = (dictionary.ContainsKey(GameObjectStats.最大魔防) ? (dictionary[GameObjectStats.最大魔防] + 4) : 4);
                                        break;
                                    case 10524:
                                        dictionary[GameObjectStats.最大魔防] = (dictionary.ContainsKey(GameObjectStats.最大魔防) ? (dictionary[GameObjectStats.最大魔防] + 5) : 5);
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (Id)
                        {
                            case 10620:
                                dictionary[GameObjectStats.最大刺术] = (dictionary.ContainsKey(GameObjectStats.最大刺术) ? (dictionary[GameObjectStats.最大刺术] + 1) : 1);
                                break;
                            case 10621:
                                dictionary[GameObjectStats.最大刺术] = (dictionary.ContainsKey(GameObjectStats.最大刺术) ? (dictionary[GameObjectStats.最大刺术] + 2) : 2);
                                break;
                            case 10622:
                                dictionary[GameObjectStats.最大刺术] = (dictionary.ContainsKey(GameObjectStats.最大刺术) ? (dictionary[GameObjectStats.最大刺术] + 3) : 3);
                                break;
                            case 10623:
                                dictionary[GameObjectStats.最大刺术] = (dictionary.ContainsKey(GameObjectStats.最大刺术) ? (dictionary[GameObjectStats.最大刺术] + 4) : 4);
                                break;
                            case 10624:
                                dictionary[GameObjectStats.最大刺术] = (dictionary.ContainsKey(GameObjectStats.最大刺术) ? (dictionary[GameObjectStats.最大刺术] + 5) : 5);
                                break;
                            default:
                                switch (Id)
                                {
                                    case 10720:
                                        dictionary[GameObjectStats.最大弓术] = (dictionary.ContainsKey(GameObjectStats.最大弓术) ? (dictionary[GameObjectStats.最大弓术] + 1) : 1);
                                        break;
                                    case 10721:
                                        dictionary[GameObjectStats.最大弓术] = (dictionary.ContainsKey(GameObjectStats.最大弓术) ? (dictionary[GameObjectStats.最大弓术] + 2) : 2);
                                        break;
                                    case 10722:
                                        dictionary[GameObjectStats.最大弓术] = (dictionary.ContainsKey(GameObjectStats.最大弓术) ? (dictionary[GameObjectStats.最大弓术] + 3) : 3);
                                        break;
                                    case 10723:
                                        dictionary[GameObjectStats.最大弓术] = (dictionary.ContainsKey(GameObjectStats.最大弓术) ? (dictionary[GameObjectStats.最大弓术] + 4) : 4);
                                        break;
                                    case 10724:
                                        dictionary[GameObjectStats.最大弓术] = (dictionary.ContainsKey(GameObjectStats.最大弓术) ? (dictionary[GameObjectStats.最大弓术] + 5) : 5);
                                        break;
                                }
                                break;
                        }
                    }
                }
                return dictionary;
            }
        }

        public EquipmentData() { }

        public EquipmentData(EquipmentItem item, CharacterData character, byte 容器, byte location, bool randomGenerated = false)
        {
            对应模板.V = item;
            生成来源.V = character;
            物品容器.V = 容器;
            物品位置.V = location;
            生成时间.V = MainProcess.CurrentTime;
            物品状态.V = 1;
            最大持久.V = ((item.持久类型 == PersistentItemType.装备) ? (item.物品持久 * 1000) : item.物品持久);

            if (randomGenerated && item.持久类型 == PersistentItemType.装备)
                当前持久.V = MainProcess.RandomNumber.Next(0, 最大持久.V);
            else
                当前持久.V = 最大持久.V;

            if (randomGenerated && item.持久类型 == PersistentItemType.装备)
                随机属性.SetValue(装备属性.GenerateStats(base.物品类型, false));

            var activeQuests = character.GetInProgressQuests();
            foreach (var quest in activeQuests)
            {
                var missions = quest.GetMissionsOfType(Models.Enums.QuestMissionType.AdquireItem);
                var updated = false;
                foreach (var mission in missions)
                {
                    if (mission.CompletedDate.V != DateTime.MinValue) continue;
                    if (mission.Info.V.Id != item.物品编号) continue;
                    mission.Count.V = (byte)(mission.Count.V + 1);
                    updated = true;
                }
                if (updated) character.ActiveConnection?.Player.UpdateQuestProgress(quest);
            }

            GameDataGateway.EquipmentData表.AddData(this, true);
        }

        public int 重铸所需灵气
        {
            get
            {
                switch (base.物品类型)
                {
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
                    case 物品使用分类.武器:
                        return 112001;
                    default:
                        return 0;
                }
            }
        }


        public override byte[] 字节描述()
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(ItemData.数据版本);
                    BinaryWriter binaryWriter2 = binaryWriter;
                    binaryWriter2.Write(生成来源.V?.Index.V ?? 0);
                    binaryWriter.Write(ComputingClass.TimeShift(生成时间.V));
                    binaryWriter.Write(对应模板.V.物品编号);
                    binaryWriter.Write(物品容器.V);
                    binaryWriter.Write(物品位置.V);
                    binaryWriter.Write(当前持久.V);
                    binaryWriter.Write(最大持久.V);
                    binaryWriter.Write((byte)(IsBound ? 10 : 0));

                    int num = 256;
                    num = 0x100 | 当前铭栏.V;

                    if (双铭文栏.V) num |= 0x200;
                    binaryWriter.Write((short)num);

                    int num2 = 0;

                    if (物品状态.V != 1) num2 |= 1;
                    else if (随机属性.Count != 0) num2 |= 1;
                    else if (Sacred伤害.V != 0) num2 |= 1;

                    if (随机属性.Count >= 1) num2 |= 2;

                    if (随机属性.Count >= 2) num2 |= 4;

                    if (随机属性.Count >= 3) num2 |= 8;

                    if (随机属性.Count >= 4) num2 |= 0x10;

                    if (Luck.V != 0) num2 |= 0x800;

                    if (升级次数.V != 0) num2 |= 0x1000;

                    if (孔洞颜色.Count != 0) num2 |= 0x2000;

                    if (镶嵌灵石[0] != null) num2 |= 0x4000;

                    if (镶嵌灵石[1] != null) num2 |= 0x8000;

                    if (镶嵌灵石[2] != null) num2 |= 0x10000;

                    if (镶嵌灵石[3] != null) num2 |= 0x20000;

                    if (Sacred伤害.V != 0) num2 |= 0x400000;
                    else if (圣石数量.V != 0) num2 |= 0x400000;

                    // unknown flag
                    if (false) num2 |= 0x80000;

                    // expire time
                    if (false) num2 |= 0x100000;

                    // unknown flag
                    if (false) num2 |= 0x200000;

                    if (祈祷次数.V != 0) num2 |= 0x800000;

                    if (装备神佑.V) num2 |= 0x2000000;

                    // unknow flag
                    if (false) num2 |= 0x4000000;

                    binaryWriter.Write(num2);

                    if (((uint)num2 & (true ? 1u : 0u)) != 0)
                        binaryWriter.Write(物品状态.V);

                    if (((uint)num2 & 2u) != 0)
                        binaryWriter.Write((ushort)随机属性[0].属性编号);

                    if (((uint)num2 & 4u) != 0)
                        binaryWriter.Write((ushort)随机属性[1].属性编号);

                    if (((uint)num2 & 8u) != 0)
                        binaryWriter.Write((ushort)随机属性[2].属性编号);

                    if (((uint)num2 & 0x10u) != 0)
                        binaryWriter.Write((ushort)随机属性[3].属性编号);

                    if (((uint)num & 0x100u) != 0)
                    {
                        int num3 = 0;
                        if (铭文技能[0] != null) num3 |= 1;
                        if (铭文技能[1] != null) num3 |= 2;

                        binaryWriter.Write((short)num3);
                        binaryWriter.Write(洗练数一.V * 10000);

                        if (((uint)num3 & (true ? 1u : 0u)) != 0)
                            binaryWriter.Write(铭文技能[0].Index);

                        if (((uint)num3 & 2u) != 0)
                            binaryWriter.Write(铭文技能[1].Index);
                    }
                    if (((uint)num & 0x200u) != 0)
                    {
                        int num4 = 0;
                        if (铭文技能[2] != null) num4 |= 1;
                        if (铭文技能[3] != null) num4 |= 2;
                        binaryWriter.Write((short)num4);
                        binaryWriter.Write(洗练数二.V * 10000);

                        if (((uint)num4 & (true ? 1u : 0u)) != 0)
                            binaryWriter.Write(铭文技能[2].Index);

                        if (((uint)num4 & 2u) != 0)
                            binaryWriter.Write(铭文技能[3].Index);
                    }

                    if (((uint)num2 & 0x800u) != 0)
                        binaryWriter.Write(Luck.V);

                    if (((uint)num2 & 0x1000u) != 0)
                    {
                        binaryWriter.Write(升级次数.V);
                        binaryWriter.Write((byte)0);
                        binaryWriter.Write(升级Attack.V);
                        binaryWriter.Write(升级Magic.V);
                        binaryWriter.Write(升级Taoism.V);
                        binaryWriter.Write(升级Needle.V);
                        binaryWriter.Write(升级Archery.V);
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
                        binaryWriter.Write(镶嵌灵石[0].物品编号);

                    if (((uint)num2 & 0x8000u) != 0)
                        binaryWriter.Write(镶嵌灵石[1].物品编号);

                    if (((uint)num2 & 0x10000u) != 0)
                        binaryWriter.Write(镶嵌灵石[2].物品编号);

                    if (((uint)num2 & 0x20000u) != 0)
                        binaryWriter.Write(镶嵌灵石[3].物品编号);

                    if (((uint)num2 & 0x80000u) != 0)
                        binaryWriter.Write(0);

                    if (((uint)num2 & 0x100000u) != 0)
                        binaryWriter.Write(ComputingClass.TimeShift(DateTime.Now.AddMinutes(10)));

                    if (((uint)num2 & 0x200000u) != 0)
                        binaryWriter.Write(0);

                    if (((uint)num2 & 0x400000u) != 0)
                    {
                        binaryWriter.Write(Sacred伤害.V);
                        binaryWriter.Write(圣石数量.V);
                    }

                    if (((uint)num2 & 0x800000u) != 0)
                        binaryWriter.Write((int)祈祷次数.V);

                    if (((uint)num2 & 0x2000000u) != 0)
                        binaryWriter.Write(装备神佑.V);

                    if (((uint)num2 & 0x4000000u) != 0)
                        binaryWriter.Write(0);

                    result = memoryStream.ToArray();
                }
            }
            return result;
        }


        public readonly DataMonitor<byte> 升级次数;


        public readonly DataMonitor<byte> 升级Attack;  //升级攻击


        public readonly DataMonitor<byte> 升级Magic;   //升级魔法


        public readonly DataMonitor<byte> 升级Taoism;   //升级道术


        public readonly DataMonitor<byte> 升级Needle;   //升级刺术


        public readonly DataMonitor<byte> 升级Archery;   //升级弓术


        public readonly DataMonitor<bool> 灵魂绑定;


        public readonly DataMonitor<byte> 祈祷次数;


        public readonly DataMonitor<sbyte> Luck;  //幸运等级


        public readonly DataMonitor<bool> 装备神佑;


        public readonly DataMonitor<byte> Sacred伤害;  //神圣伤害


        public readonly DataMonitor<ushort> 圣石数量;


        public readonly DataMonitor<bool> 双铭文栏;


        public readonly DataMonitor<byte> 当前铭栏;


        public readonly DataMonitor<int> 洗练数一;


        public readonly DataMonitor<int> 洗练数二;


        public readonly DataMonitor<byte> 物品状态;


        public readonly ListMonitor<随机属性> 随机属性;  //随机属性


        public readonly ListMonitor<EquipHoleColor> 孔洞颜色;


        public readonly MonitorDictionary<byte, InscriptionSkill> 铭文技能;


        public readonly MonitorDictionary<byte, GameItems> 镶嵌灵石;
    }
}
