using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
    public sealed class GameMap  //游戏地图
    {
        public static Dictionary<byte, GameMap> DataSheet;  //游戏地图 数据表

        public byte 地图编号;  //MapId
        public string 地图名字;  //MapName
        public string 地图别名;  //MapFile
        public string 地形文件;  //TerrainFile
        public int 限制人数;  //LimitPlayers
        public byte 限制等级;   //MinLevel
        public byte 分线数量;  //LimitInstances
        public bool 下线传送;   //NoReconnect
        public byte 传送地图;   //NoReconnectMapId
        public bool 副本地图;   //CopyMap
 
        public static void LoadData()
        {
            DataSheet = new Dictionary<byte, GameMap>();
            string text = Config.GameDataPath + "\\System\\游戏地图\\地图数据";
            if (Directory.Exists(text))
            {
                foreach (var obj in Serializer.Deserialize<GameMap>(text))
                    DataSheet.Add(obj.地图编号, obj);
            }
        }

        public override string ToString()
        {
            return 地图名字;
        }
    }
}
