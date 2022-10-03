using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
    public sealed class 随机属性 //随机属性 RandomStats
    {
        public static Dictionary<int, 随机属性> DataSheet;  //RandomStats

        public GameObjectStats 对应属性; ///游戏对象属性 Stat
        public int 属性数值;  //Value
        public int 属性编号;  //StatId
        public int 战力加成;  //CombatBonus
        public string 属性描述;  //StatDescription

        public static void LoadData()
        {
            DataSheet = new Dictionary<int, 随机属性>();
            var text = Config.GameDataPath + "\\System\\物品数据\\随机属性\\";

            if (Directory.Exists(text))
            {
                foreach (var obj in Serializer.Deserialize<随机属性>(text))
                    DataSheet.Add(obj.属性编号, obj);

            }
        }
    }
}
