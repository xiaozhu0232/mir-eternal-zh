using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameServer.Templates
{
    public class GameMounts   //游戏坐骑
    {
        public static IDictionary<ushort, GameMounts> DataSheet;  //游戏坐骑 数据表

        public ushort ID;  //ID
        public string Name;  //Name
        public int AuraID;   //AuraID
        public short MountPower;  //MountPower
        public ushort SoulAuraID;   //
        public byte Quality;
        public byte LevelLimit;
        public int SpeedModificationRate;
        public int HitUnmountRate;

        [JsonIgnore]
        public Dictionary<GameObjectStats, int> Stats;

        public static void LoadData()
        {
            DataSheet = new Dictionary<ushort, GameMounts>();

            string text = Config.GameDataPath + "\\System\\Mounts\\";
            if (Directory.Exists(text))
            {
                foreach (var obj in Serializer.Deserialize<GameMounts>(text))
                {
                    obj.Stats = new Dictionary<GameObjectStats, int> {
                        { GameObjectStats.行走速度, obj.SpeedModificationRate / 500 },
                        { GameObjectStats.奔跑速度, obj.SpeedModificationRate / 500 }
                    };
                    DataSheet.Add((obj).ID, obj);
                }
            }
        }
    }
}
