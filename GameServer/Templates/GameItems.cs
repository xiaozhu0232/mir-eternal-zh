using GameServer.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
    public class GameItems
    {
        public static Dictionary<int, GameItems> DataSheet;  //游戏物品  数据表
        public static Dictionary<string, GameItems> DataSheetByName;  //游戏物品 检索表

        public string 物品名字;  //Name
        public int 物品编号;     //Id
        public int 物品持久;   //MaxDura
        public int 物品重量;   //Weight
        public int 物品等级;   //Level
        public int 需要等级;   //NeedLevel
        public int 冷却时间;    //Cooldown
        public byte 物品分组;   //Group
        public int 分组冷却;   //GroupCooling
        public int 出售价格;    //SalePrice
        public ushort 附加技能;   //AdditionalSkill
        public bool 是否绑定;    //IsBound
        public bool 能否掉落;    //CanDrop
        public bool 能否出售;    //CanSold
        public bool 贵重物品;   //ValuableObjects
        public int? 解包物品编号;    //UnpackItemId
        public List<GameItemTreasure> 宝盒物品 = new List<GameItemTreasure>();  //GameItemTreasure   TreasureItems

        public 物品使用分类 物品分类;   //物品使用分类 Type
        public GameObjectRace 需要职业;  //游戏对象职业 NeedRace
        public GameObjectGender 需要性别;  //游戏对象性别 NeedGender
        public PersistentItemType 持久类型;  //物品持久分类 PersistType
        public ItemsForSale 商店类型;   //物品出售分类 StoreType

        public IDictionary<ItemProperty, int> 物品属性 = new Dictionary<ItemProperty, int>();   //Props

        public static GameItems GetItem(int id)
        {
            if (!DataSheet.TryGetValue(id, out GameItems result))
            {
                return null;
            }
            return result;
        }


        public static GameItems GetItem(string name)
        {
            if (!DataSheetByName.TryGetValue(name, out GameItems result))
                return null;
            return result;
        }


        public static void LoadData()
        {
            DataSheet = new Dictionary<int, GameItems>();
            DataSheetByName = new Dictionary<string, GameItems>();

            string text = Config.GameDataPath + "\\System\\物品数据\\普通物品\\";
            if (Directory.Exists(text))
            {
                var array = Serializer.Deserialize<GameItems>(text);
                for (int i = 0; i < array.Length; i++)
                {
                    GameItems 游戏物品 = array[i] as GameItems;
                    DataSheet.Add(游戏物品.物品编号, 游戏物品);
                    DataSheetByName.Add(游戏物品.物品名字, 游戏物品);
                }
            }

            text = Config.GameDataPath + "\\System\\物品数据\\装备物品\\";
            if (Directory.Exists(text))
            {
                var array = Serializer.Deserialize<EquipmentItem>(text);
                for (int i = 0; i < array.Length; i++)
                {
                    EquipmentItem 游戏装备 = array[i] as EquipmentItem;
                    DataSheet.Add(游戏装备.物品编号, 游戏装备);
                    DataSheetByName.Add(游戏装备.物品名字, 游戏装备);
                }
            }
        }
    }
}
