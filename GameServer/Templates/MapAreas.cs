using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GameServer.Templates
{
    public class MapAreas  //地图区域
    {
        public static List<MapAreas> DataSheet; // 地图区域> 数据表

        public byte 所处地图;  //FromMapId
        public string 所处地名;  //FromMapName
        public Point 所处坐标;   //FromCoords
        public string 区域名字;   //RegionName
        public int 区域半径;   //AreaRadius
        public AreaType 区域类型;  //地图区域类型 AreaType
        public HashSet<Point> 范围坐标;  //HashSet<Point> RangeCoords

        private List<Point> _listCoords; //_范围坐标列表

        public static void LoadData()
        {
            DataSheet = new List<MapAreas>();
            string text = Config.GameDataPath + "\\System\\游戏地图\\地图区域\\";
            if (Directory.Exists(text))
            {
                foreach (object obj in Serializer.Deserialize<MapAreas>(text))
                {
                    DataSheet.Add((MapAreas)obj);
                }
            }
        }


        public Point RandomCoords //随机坐标
        {
            get
            {
                return RangeCoordsList[MainProcess.RandomNumber.Next(范围坐标.Count)];
            }
        }

        public List<Point> RangeCoordsList
        {
            get
            {
                if (_listCoords == null)
                    _listCoords = 范围坐标.ToList();
                return _listCoords;
            }
        }
    }
}
